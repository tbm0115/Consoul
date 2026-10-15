using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ConsoulLibrary
{
    internal static class CommandSettingsBinder
    {
        public static Type GetSettingsType(Type commandType)
        {
            if (commandType == null)
            {
                return null;
            }

            foreach (Type interfaceType in commandType.GetInterfaces())
            {
                if (!interfaceType.IsGenericType)
                {
                    continue;
                }

                Type definition = interfaceType.GetGenericTypeDefinition();
                if (definition == typeof(ICommand<>) || definition == typeof(IAsyncCommand<>))
                {
                    Type settingsType = interfaceType.GetGenericArguments()[0];
                    if (typeof(CommandSettings).IsAssignableFrom(settingsType))
                    {
                        return settingsType;
                    }
                }
            }

            return null;
        }

        public static CommandSettingsDescriptor Describe(Type settingsType)
        {
            if (settingsType == null)
            {
                return CommandSettingsDescriptor.Empty;
            }

            var arguments = new List<CommandArgumentDescriptor>();
            var options = new List<CommandOptionDescriptor>();
            foreach (PropertyInfo property in settingsType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                CommandArgumentAttribute argument = GetCommandAttribute<CommandArgumentAttribute>(property);
                if (argument != null)
                {
                    arguments.Add(new CommandArgumentDescriptor(property, argument));
                }

                CommandOptionAttribute option = GetCommandAttribute<CommandOptionAttribute>(property);
                if (option != null)
                {
                    options.Add(new CommandOptionDescriptor(property, option));
                }
            }

            CommandArgumentDescriptor[] orderedArguments = arguments.OrderBy(argument => argument.Position).ToArray();
            CommandOptionDescriptor[] orderedOptions = options.ToArray();
            ValidateDescriptor(settingsType, orderedArguments, orderedOptions);
            return new CommandSettingsDescriptor(orderedArguments, orderedOptions);
        }

        private static T GetCommandAttribute<T>(PropertyInfo property)
            where T : Attribute
        {
            try
            {
                return property.GetCustomAttribute<T>();
            }
            catch (Exception ex)
            {
                CommandTemplateException templateException = FindTemplateException(ex);
                if (templateException != null)
                {
                    throw templateException;
                }

                throw;
            }
        }

        private static CommandTemplateException FindTemplateException(Exception exception)
        {
            while (exception != null)
            {
                var templateException = exception as CommandTemplateException;
                if (templateException != null)
                {
                    return templateException;
                }

                exception = exception.InnerException;
            }

            return null;
        }

        private static void ValidateDescriptor(Type settingsType, IReadOnlyList<CommandArgumentDescriptor> arguments, IReadOnlyList<CommandOptionDescriptor> options)
        {
            ValidateArgumentPositions(settingsType, arguments);
            ValidateOptionAliases(settingsType, options);
        }

        private static void ValidateArgumentPositions(Type settingsType, IReadOnlyList<CommandArgumentDescriptor> arguments)
        {
            foreach (IGrouping<int, CommandArgumentDescriptor> group in arguments.GroupBy(argument => argument.Position))
            {
                if (group.Count() > 1)
                {
                    throw new CommandTemplateException(
                        "Argument position '" +
                        group.Key.ToString(CultureInfo.InvariantCulture) +
                        "' is used by multiple properties on '" +
                        settingsType.Name +
                        "': " +
                        string.Join(", ", group.Select(argument => argument.Property.Name)) +
                        ".");
                }
            }

            CommandArgumentDescriptor firstOptionalArgument = null;
            for (int index = 0; index < arguments.Count; index++)
            {
                CommandArgumentDescriptor argument = arguments[index];
                if (firstOptionalArgument != null && argument.IsRequired)
                {
                    throw new CommandTemplateException(
                        "Required argument '" +
                        argument.Template +
                        "' cannot appear after optional argument '" +
                        firstOptionalArgument.Template +
                        "' on '" +
                        settingsType.Name +
                        "'.");
                }

                if (!argument.IsRequired && firstOptionalArgument == null)
                {
                    firstOptionalArgument = argument;
                }

                if ((argument.IsArray || argument.IsCollection || argument.IsRemainingArguments) && index < arguments.Count - 1)
                {
                    throw new CommandTemplateException(
                        "Argument '" +
                        argument.Template +
                        "' on '" +
                        settingsType.Name +
                        "' captures remaining values and must be the final positional argument.");
                }
            }
        }

        private static void ValidateOptionAliases(Type settingsType, IReadOnlyList<CommandOptionDescriptor> options)
        {
            var aliases = new Dictionary<string, CommandOptionDescriptor>(StringComparer.Ordinal);
            foreach (CommandOptionDescriptor option in options)
            {
                foreach (string alias in option.Aliases)
                {
                    CommandOptionDescriptor existing;
                    if (aliases.TryGetValue(alias, out existing))
                    {
                        throw new CommandTemplateException(
                            "Option alias '" +
                            alias +
                            "' is used by multiple properties on '" +
                            settingsType.Name +
                            "': " +
                            existing.Property.Name +
                            ", " +
                            option.Property.Name +
                            ".");
                    }

                    aliases.Add(alias, option);
                }
            }
        }

        public static CommandSettings Bind(Type settingsType, string[] arguments, CommandAppSettings settings)
        {
            if (settingsType == null)
            {
                return null;
            }

            if (!typeof(CommandSettings).IsAssignableFrom(settingsType))
            {
                throw new CommandParameterException("Settings type must inherit from CommandSettings.");
            }

            var descriptor = Describe(settingsType);
            var instance = (CommandSettings)Activator.CreateInstance(settingsType);
            CultureInfo culture = settings == null || settings.ApplicationCulture == null
                ? CultureInfo.InvariantCulture
                : settings.ApplicationCulture;
            ApplyDefaultValues(instance, descriptor, culture);

            var positionalValues = new List<string>();
            var providedOptions = new HashSet<CommandOptionDescriptor>();
            HashSet<CommandOptionDescriptor> environmentOptions = ApplyEnvironmentOptionValues(
                instance,
                descriptor.Options,
                providedOptions,
                culture);
            var comparison = settings != null && settings.CaseSensitive
                ? StringComparison.Ordinal
                : StringComparison.OrdinalIgnoreCase;

            for (int index = 0; index < (arguments ?? new string[0]).Length; index++)
            {
                string token = arguments[index] ?? string.Empty;
                if (token == "--")
                {
                    for (index++; index < arguments.Length; index++)
                    {
                        positionalValues.Add(arguments[index]);
                    }

                    break;
                }

                OptionToken optionToken;
                if (TryParseOptionToken(token, descriptor.Options, comparison, out optionToken))
                {
                    CommandOptionDescriptor option = optionToken.Option;
                    if (environmentOptions.Remove(option))
                    {
                        ClearEnvironmentSeededValue(instance, option, culture);
                    }

                    providedOptions.Add(option);

                    if (option.IsFlagValue)
                    {
                        string flagValue = optionToken.Value;
                        if (!optionToken.HasValue)
                        {
                            if (index + 1 < arguments.Length && !IsRecognizedOption(arguments[index + 1], descriptor.Options, comparison))
                            {
                                flagValue = arguments[++index];
                            }
                        }

                        SetFlagValue(instance, option, flagValue, culture, token);
                        continue;
                    }

                    if (option.IsBoolean)
                    {
                        string boolValue = optionToken.HasValue ? optionToken.Value : "true";
                        SetValue(instance, BindingTarget.ForOption(option, token), boolValue, culture);
                        continue;
                    }

                    string value = optionToken.Value;
                    if (!optionToken.HasValue)
                    {
                        if (index + 1 >= arguments.Length || IsRecognizedOption(arguments[index + 1], descriptor.Options, comparison))
                        {
                            if (option.Attribute.ValueIsOptional)
                            {
                                continue;
                            }

                            throw new CommandParameterException(BuildMissingOptionValueMessage(token, option));
                        }

                        value = arguments[++index];
                    }

                    AddOrSetValue(instance, BindingTarget.ForOption(option, token), value, culture);
                    continue;
                }

                IReadOnlyList<ClusteredFlag> clusteredFlags;
                if (TryParseShortBooleanFlagCluster(token, descriptor.Options, comparison, out clusteredFlags))
                {
                    foreach (ClusteredFlag flag in clusteredFlags)
                    {
                        CommandOptionDescriptor option = flag.Option;
                        if (environmentOptions.Remove(option))
                        {
                            ClearEnvironmentSeededValue(instance, option, culture);
                        }

                        providedOptions.Add(option);
                        SetValue(instance, BindingTarget.ForOption(option, flag.Alias), "true", culture);
                    }

                    continue;
                }

                if (LooksLikeOption(token))
                {
                    if (settings == null || settings.StrictParsing)
                    {
                        throw new CommandParameterException(BuildUnknownOptionMessage(token, descriptor.Options));
                    }
                }

                positionalValues.Add(token);
            }

            BindArguments(instance, descriptor.Arguments, positionalValues, culture);
            EnsureRequiredOptions(descriptor.Options, providedOptions);
            ValidateParameters(instance, descriptor);
            return instance;
        }

        private static void ApplyDefaultValues(CommandSettings instance, CommandSettingsDescriptor descriptor, CultureInfo culture)
        {
            foreach (CommandArgumentDescriptor argument in descriptor.Arguments)
            {
                ApplyDefaultValue(instance, argument.Property, culture);
            }

            foreach (CommandOptionDescriptor option in descriptor.Options)
            {
                ApplyDefaultValue(instance, option.Property, culture);
            }
        }

        private static void ApplyDefaultValue(CommandSettings instance, PropertyInfo property, CultureInfo culture)
        {
            if (IsFlagValueType(property.PropertyType))
            {
                return;
            }

            DefaultValueAttribute defaultValue = property.GetCustomAttribute<DefaultValueAttribute>();
            if (defaultValue != null && property.CanWrite)
            {
                object value = ConvertValue(defaultValue.Value, property.PropertyType, BindingTarget.ForProperty(property), culture);
                property.SetValue(instance, value, null);
            }
        }

        private static HashSet<CommandOptionDescriptor> ApplyEnvironmentOptionValues(
            CommandSettings instance,
            IReadOnlyList<CommandOptionDescriptor> options,
            HashSet<CommandOptionDescriptor> providedOptions,
            CultureInfo culture)
        {
            var appliedOptions = new HashSet<CommandOptionDescriptor>();
            foreach (CommandOptionDescriptor option in options ?? new CommandOptionDescriptor[0])
            {
                string variableName = option.EnvironmentVariable;
                if (string.IsNullOrWhiteSpace(variableName))
                {
                    continue;
                }

                string value = Environment.GetEnvironmentVariable(variableName);
                if (value == null)
                {
                    continue;
                }

                BindingTarget target = BindingTarget.ForEnvironmentVariable(option, variableName);
                if (option.IsFlagValue)
                {
                    SetFlagValue(instance, option, string.IsNullOrEmpty(value) ? null : value, culture, target);
                }
                else if (option.IsBoolean)
                {
                    SetValue(instance, target, string.IsNullOrEmpty(value) ? "true" : value, culture);
                }
                else
                {
                    AddOrSetValue(instance, target, value, culture);
                }

                providedOptions.Add(option);
                appliedOptions.Add(option);
            }

            return appliedOptions;
        }

        private static void ClearEnvironmentSeededValue(CommandSettings instance, CommandOptionDescriptor option, CultureInfo culture)
        {
            PropertyInfo property = option.Property;
            BindingTarget target = BindingTarget.ForOption(option, option.PrimaryName);

            if (property.PropertyType.IsArray)
            {
                SetArrayValue(instance, target, new string[0], culture);
                return;
            }

            if (option.IsCollection)
            {
                SetCollectionValue(instance, target, new string[0], culture);
                return;
            }

            Type[] dictionaryTypes = GetDictionaryTypes(property.PropertyType);
            if (dictionaryTypes != null)
            {
                ClearDictionaryValue(instance, property, dictionaryTypes[0], dictionaryTypes[1]);
                return;
            }

            Type[] lookupTypes = GetLookupTypes(property.PropertyType);
            if (lookupTypes != null)
            {
                if (!property.CanWrite)
                {
                    throw new CommandParameterException("Property '" + property.Name + "' is not writable.");
                }

                property.SetValue(instance, CreateLookup(new List<KeyValuePair<object, object>>(), lookupTypes[0], lookupTypes[1]), null);
            }
        }

        private static void BindArguments(CommandSettings instance, IReadOnlyList<CommandArgumentDescriptor> arguments, List<string> values, CultureInfo culture)
        {
            int valueIndex = 0;
            var missingRequiredArguments = new List<string>();
            foreach (CommandArgumentDescriptor argument in arguments)
            {
                if (argument.IsArray || argument.IsCollection)
                {
                    var remaining = values.Skip(valueIndex).ToArray();
                    if (argument.Attribute.IsRequired && remaining.Length == 0)
                    {
                        missingRequiredArguments.Add(argument.Name);
                        valueIndex = values.Count;
                        continue;
                    }

                    if (argument.IsArray)
                    {
                        SetArrayValue(instance, BindingTarget.ForArgument(argument), remaining, culture);
                    }
                    else
                    {
                        SetCollectionValue(instance, BindingTarget.ForArgument(argument), remaining, culture);
                    }

                    valueIndex = values.Count;
                    continue;
                }

                if (argument.IsRemainingArguments)
                {
                    var remaining = values.Skip(valueIndex).ToArray();
                    if (argument.Attribute.IsRequired && remaining.Length == 0)
                    {
                        missingRequiredArguments.Add(argument.Name);
                        valueIndex = values.Count;
                        continue;
                    }

                    SetRemainingArgumentsValue(instance, argument.Property, remaining);
                    valueIndex = values.Count;
                    continue;
                }

                if (valueIndex >= values.Count)
                {
                    if (argument.Attribute.IsRequired)
                    {
                        missingRequiredArguments.Add(argument.Name);
                    }

                    continue;
                }

                SetValue(instance, BindingTarget.ForArgument(argument), values[valueIndex++], culture);
            }

            if (missingRequiredArguments.Count == 1)
            {
                throw new CommandParameterException("Missing required argument '" + missingRequiredArguments[0] + "'.");
            }

            if (missingRequiredArguments.Count > 1)
            {
                throw new CommandParameterException("Missing required arguments " + string.Join(", ", missingRequiredArguments.Select(argument => "'" + argument + "'")) + ".");
            }

            if (valueIndex < values.Count)
            {
                throw new CommandParameterException("Unexpected argument '" + values[valueIndex] + "'.");
            }
        }

        private static void EnsureRequiredOptions(IReadOnlyList<CommandOptionDescriptor> options, HashSet<CommandOptionDescriptor> providedOptions)
        {
            string[] missingOptions = (options ?? new CommandOptionDescriptor[0])
                .Where(option => option.Attribute.IsRequired && !providedOptions.Contains(option))
                .Select(option => option.PrimaryName)
                .ToArray();

            if (missingOptions.Length == 0)
            {
                return;
            }

            if (missingOptions.Length == 1)
            {
                throw new CommandParameterException("Missing required option '" + missingOptions[0] + "'.");
            }

            throw new CommandParameterException("Missing required options " + string.Join(", ", missingOptions.Select(option => "'" + option + "'")) + ".");
        }

        private static void ValidateParameters(CommandSettings instance, CommandSettingsDescriptor descriptor)
        {
            foreach (CommandArgumentDescriptor argument in descriptor.Arguments)
            {
                ValidateParameter(instance, argument);
            }

            foreach (CommandOptionDescriptor option in descriptor.Options)
            {
                ValidateParameter(instance, option);
            }

            ValidateObject(instance);
        }

        private static void ValidateParameter(CommandSettings instance, ICommandParameterInfo parameter)
        {
            PropertyInfo property = parameter.Property;
            foreach (ParameterValidationAttribute attribute in property.GetCustomAttributes<ParameterValidationAttribute>(true))
            {
                object value = property.GetValue(instance, null);
                var context = new CommandParameterContext(instance, parameter, value);
                CommandValidationResult result = attribute.Validate(context);
                if (result != null && !result.Successful)
                {
                    string message = string.IsNullOrWhiteSpace(result.Message)
                        ? "Parameter '" + parameter.Name + "' is invalid."
                        : result.Message;
                    throw new CommandParameterException(message);
                }
            }

            foreach (object attribute in property.GetCustomAttributes(true))
            {
                if (!IsValidationAttribute(attribute))
                {
                    continue;
                }

                object value = property.GetValue(instance, null);
                string displayName = ResolveParameterDisplayName(parameter);
                string message = ValidateAnnotationAttribute(attribute, value, instance, property.Name, displayName);
                if (!string.IsNullOrWhiteSpace(message))
                {
                    throw new CommandParameterException(message);
                }
            }
        }

        private static void ValidateObject(CommandSettings instance)
        {
            if (instance == null)
            {
                return;
            }

            Type settingsType = instance.GetType();
            Type validatableInterface = settingsType
                .GetInterfaces()
                .FirstOrDefault(type => string.Equals(
                    type.FullName,
                    "System.ComponentModel.DataAnnotations.IValidatableObject",
                    StringComparison.Ordinal));

            if (validatableInterface == null)
            {
                return;
            }

            MethodInfo validateMethod = validatableInterface.GetMethod("Validate");
            if (validateMethod == null)
            {
                return;
            }

            ParameterInfo[] parameters = validateMethod.GetParameters();
            if (parameters.Length != 1)
            {
                return;
            }

            object validationContext = CreateValidationContext(parameters[0].ParameterType, instance);
            if (validationContext == null)
            {
                return;
            }

            IEnumerable results;
            try
            {
                results = validateMethod.Invoke(instance, new[] { validationContext }) as IEnumerable;
            }
            catch
            {
                return;
            }

            if (results == null)
            {
                return;
            }

            foreach (object result in results)
            {
                if (result == null)
                {
                    continue;
                }

                string message = GetValidationResultMessage(result, "Command settings are invalid.");
                if (!string.IsNullOrWhiteSpace(message))
                {
                    throw new CommandParameterException(message);
                }
            }
        }

        private static string ValidateAnnotationAttribute(object attribute, object value, object instance, string memberName, string displayName)
        {
            object validationResult;
            if (TryGetValidationResult(attribute, value, instance, memberName, displayName, out validationResult))
            {
                return IsValidationResultSuccess(validationResult)
                    ? null
                    : GetValidationResultMessage(validationResult, ResolveValidationMessage(attribute, displayName));
            }

            return IsValidationAttributeValid(attribute, value) ? null : ResolveValidationMessage(attribute, displayName);
        }

        private static bool TryGetValidationResult(object attribute, object value, object instance, string memberName, string displayName, out object validationResult)
        {
            validationResult = null;
            MethodInfo method = FindGetValidationResultMethod(attribute);
            if (method == null)
            {
                return false;
            }

            ParameterInfo[] parameters = method.GetParameters();
            object validationContext = CreateValidationContext(parameters[1].ParameterType, instance);
            if (validationContext == null)
            {
                return false;
            }

            SetValidationContextMemberName(validationContext, memberName);
            SetValidationContextDisplayName(validationContext, displayName);

            try
            {
                validationResult = method.Invoke(attribute, new[] { value, validationContext });
                return true;
            }
            catch
            {
                validationResult = null;
                return false;
            }
        }

        private static MethodInfo FindGetValidationResultMethod(object attribute)
        {
            if (attribute == null)
            {
                return null;
            }

            return attribute
                .GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(method =>
                {
                    if (!string.Equals(method.Name, "GetValidationResult", StringComparison.Ordinal))
                    {
                        return false;
                    }

                    ParameterInfo[] parameters = method.GetParameters();
                    return parameters.Length == 2 &&
                        parameters[0].ParameterType == typeof(object) &&
                        string.Equals(
                            parameters[1].ParameterType.FullName,
                            "System.ComponentModel.DataAnnotations.ValidationContext",
                            StringComparison.Ordinal);
                });
        }

        private static object CreateValidationContext(Type validationContextType, object instance)
        {
            if (validationContextType == null)
            {
                return null;
            }

            ConstructorInfo constructor = validationContextType.GetConstructor(new[] { typeof(object) });
            if (constructor == null)
            {
                return null;
            }

            try
            {
                return constructor.Invoke(new[] { instance });
            }
            catch
            {
                return null;
            }
        }

        private static void SetValidationContextMemberName(object validationContext, string memberName)
        {
            SetValidationContextStringProperty(validationContext, "MemberName", memberName);
        }

        private static void SetValidationContextDisplayName(object validationContext, string displayName)
        {
            SetValidationContextStringProperty(validationContext, "DisplayName", displayName);
        }

        private static void SetValidationContextStringProperty(object validationContext, string propertyName, string value)
        {
            if (validationContext == null || string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            PropertyInfo property = validationContext.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (property == null || !property.CanWrite)
            {
                return;
            }

            try
            {
                property.SetValue(validationContext, value, null);
            }
            catch
            {
                // ValidationContext implementations may reject changes; validation can still run without it.
            }
        }

        private static bool IsValidationResultSuccess(object validationResult)
        {
            return validationResult == null;
        }

        private static string GetValidationResultMessage(object validationResult, string fallbackMessage)
        {
            if (validationResult == null)
            {
                return null;
            }

            PropertyInfo property = validationResult.GetType().GetProperty("ErrorMessage", BindingFlags.Public | BindingFlags.Instance);
            string message = property == null ? null : property.GetValue(validationResult, null) as string;
            return !string.IsNullOrWhiteSpace(message) ? message : fallbackMessage;
        }

        private static bool IsValidationAttributeValid(object attribute, object value)
        {
            MethodInfo method = attribute.GetType().GetMethod("IsValid", new[] { typeof(object) });
            if (method == null || method.ReturnType != typeof(bool))
            {
                return true;
            }

            try
            {
                return (bool)method.Invoke(attribute, new[] { value });
            }
            catch
            {
                return true;
            }
        }

        private static string ResolveValidationMessage(object attribute, string displayName)
        {
            MethodInfo formatMethod = attribute.GetType().GetMethod("FormatErrorMessage", new[] { typeof(string) });
            if (formatMethod != null && formatMethod.ReturnType == typeof(string))
            {
                try
                {
                    string formatted = formatMethod.Invoke(attribute, new object[] { displayName }) as string;
                    if (!string.IsNullOrWhiteSpace(formatted))
                    {
                        return formatted;
                    }
                }
                catch
                {
                    // Fall back to ErrorMessage below.
                }
            }

            PropertyInfo errorMessageProperty = attribute.GetType().GetProperty("ErrorMessage", BindingFlags.Public | BindingFlags.Instance);
            string errorMessage = errorMessageProperty == null ? null : errorMessageProperty.GetValue(attribute, null) as string;
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                return errorMessage;
            }

            return displayName + " is invalid.";
        }

        private static bool IsValidationAttribute(object attribute)
        {
            Type type = attribute == null ? null : attribute.GetType();
            while (type != null)
            {
                if (string.Equals(
                    type.FullName,
                    "System.ComponentModel.DataAnnotations.ValidationAttribute",
                    StringComparison.Ordinal))
                {
                    return true;
                }

                type = type.BaseType;
            }

            return false;
        }

        private static string ResolveParameterDisplayName(ICommandParameterInfo parameter)
        {
            string displayName = ResolveDisplayAttributeName(parameter.Property);
            if (!string.IsNullOrWhiteSpace(displayName))
            {
                return displayName;
            }

            var displayNameAttribute = parameter.Property.GetCustomAttribute<DisplayNameAttribute>(true);
            if (displayNameAttribute != null && !string.IsNullOrWhiteSpace(displayNameAttribute.DisplayName))
            {
                return displayNameAttribute.DisplayName;
            }

            return string.IsNullOrWhiteSpace(parameter.Name) ? parameter.Property.Name : parameter.Name;
        }

        private static string ResolveDisplayAttributeName(MemberInfo member)
        {
            object displayAttribute = member
                .GetCustomAttributes(true)
                .FirstOrDefault(attribute => string.Equals(
                    attribute.GetType().FullName,
                    "System.ComponentModel.DataAnnotations.DisplayAttribute",
                    StringComparison.Ordinal));

            if (displayAttribute == null)
            {
                return null;
            }

            MethodInfo getNameMethod = displayAttribute.GetType().GetMethod("GetName", Type.EmptyTypes);
            if (getNameMethod != null && getNameMethod.ReturnType == typeof(string))
            {
                try
                {
                    string name = getNameMethod.Invoke(displayAttribute, null) as string;
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        return name;
                    }
                }
                catch
                {
                    // Fall back to Name below.
                }
            }

            PropertyInfo nameProperty = displayAttribute.GetType().GetProperty("Name", BindingFlags.Public | BindingFlags.Instance);
            return nameProperty == null ? null : nameProperty.GetValue(displayAttribute, null) as string;
        }

        private static bool TryParseOptionToken(string token, IReadOnlyList<CommandOptionDescriptor> options, StringComparison comparison, out OptionToken optionToken)
        {
            optionToken = default(OptionToken);
            if (!LooksLikeOption(token))
            {
                return false;
            }

            string name = token;
            string value = null;
            int equalsIndex = token.IndexOf('=');
            if (equalsIndex >= 0)
            {
                name = token.Substring(0, equalsIndex);
                value = token.Substring(equalsIndex + 1);
            }

            foreach (CommandOptionDescriptor option in options)
            {
                if (option.Aliases.Any(alias => string.Equals(alias, name, comparison)))
                {
                    optionToken = new OptionToken(option, equalsIndex >= 0, value);
                    return true;
                }
            }

            foreach (CommandOptionDescriptor option in options)
            {
                if (!option.AcceptsBooleanNegation)
                {
                    continue;
                }

                foreach (string alias in option.LongNames)
                {
                    string negatedAlias = "--no-" + alias.Substring(2);
                    if (string.Equals(negatedAlias, name, comparison))
                    {
                        optionToken = new OptionToken(option, true, "false");
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool TryParseShortBooleanFlagCluster(
            string token,
            IReadOnlyList<CommandOptionDescriptor> options,
            StringComparison comparison,
            out IReadOnlyList<ClusteredFlag> clusteredFlags)
        {
            clusteredFlags = Array.Empty<ClusteredFlag>();
            if (string.IsNullOrEmpty(token) ||
                token.Length <= 2 ||
                token[0] != '-' ||
                token[1] == '-' ||
                token.IndexOf('=') >= 0)
            {
                return false;
            }

            var flags = new List<ClusteredFlag>();
            for (int index = 1; index < token.Length; index++)
            {
                string alias = "-" + token[index];
                CommandOptionDescriptor option = FindOptionByAlias(options, alias, comparison);
                if (option == null || !option.IsBoolean)
                {
                    clusteredFlags = Array.Empty<ClusteredFlag>();
                    return false;
                }

                flags.Add(new ClusteredFlag(option, alias));
            }

            clusteredFlags = flags;
            return flags.Count > 0;
        }

        private static CommandOptionDescriptor FindOptionByAlias(
            IReadOnlyList<CommandOptionDescriptor> options,
            string alias,
            StringComparison comparison)
        {
            foreach (CommandOptionDescriptor option in options ?? new CommandOptionDescriptor[0])
            {
                if (option.Aliases.Any(candidate => string.Equals(candidate, alias, comparison)))
                {
                    return option;
                }
            }

            return null;
        }

        private static bool IsRecognizedOption(string token, IReadOnlyList<CommandOptionDescriptor> options, StringComparison comparison)
        {
            OptionToken optionToken;
            return TryParseOptionToken(token, options, comparison, out optionToken);
        }

        internal static string BuildUnknownOptionMessage(string token, IReadOnlyList<CommandOptionDescriptor> options)
        {
            string optionName = GetOptionName(token);
            IReadOnlyList<string> suggestions = SuggestionHelper.FindSuggestions(
                optionName,
                EnumerateOptionSuggestionCandidates(options));
            return SuggestionHelper.AppendSuggestion("Unknown option '" + token + "'.", suggestions);
        }

        internal static string BuildMissingOptionValueMessage(string token, CommandOptionDescriptor option)
        {
            if (option == null)
            {
                throw new ArgumentNullException(nameof(option));
            }

            string optionName = GetOptionName(token);
            if (string.IsNullOrWhiteSpace(optionName))
            {
                optionName = option.PrimaryName;
            }

            string valueName = string.IsNullOrWhiteSpace(option.ValueName) ? "VALUE" : option.ValueName;
            return "Missing value for option '" + optionName + "'. Expected <" + valueName + ">.";
        }

        private static IEnumerable<SuggestionHelper.Candidate> EnumerateOptionSuggestionCandidates(IReadOnlyList<CommandOptionDescriptor> options)
        {
            foreach (CommandOptionDescriptor option in (options ?? new CommandOptionDescriptor[0]).Where(option => !option.IsHidden))
            {
                foreach (string alias in option.Aliases)
                {
                    yield return new SuggestionHelper.Candidate(alias, new[] { alias });
                }
            }
        }

        private static string GetOptionName(string token)
        {
            string optionName = token ?? string.Empty;
            int equalsIndex = optionName.IndexOf('=');
            return equalsIndex >= 0 ? optionName.Substring(0, equalsIndex) : optionName;
        }

        private static bool LooksLikeOption(string value)
        {
            return !string.IsNullOrEmpty(value) && value.Length > 1 && value[0] == '-';
        }

        private static void AddOrSetValue(CommandSettings instance, BindingTarget target, string value, CultureInfo culture)
        {
            PropertyInfo property = target.Property;
            if (TryAddDictionaryValue(instance, target, value, culture))
            {
                return;
            }

            if (TryAddLookupValue(instance, target, value, culture))
            {
                return;
            }

            if (property.PropertyType.IsArray)
            {
                Array existing = property.GetValue(instance, null) as Array;
                Type elementType = property.PropertyType.GetElementType();
                int existingLength = existing == null ? 0 : existing.Length;
                Array array = Array.CreateInstance(elementType, existingLength + 1);
                if (existing != null)
                {
                    Array.Copy(existing, array, existingLength);
                }

                array.SetValue(ConvertValue(value, elementType, target, culture), existingLength);
                property.SetValue(instance, array, null);
                return;
            }

            if (TryAddCollectionValue(instance, target, value, culture))
            {
                return;
            }

            SetValue(instance, target, value, culture);
        }

        private static bool TryAddCollectionValue(CommandSettings instance, BindingTarget target, string value, CultureInfo culture)
        {
            PropertyInfo property = target.Property;
            Type elementType = GetCollectionElementType(property.PropertyType);
            if (elementType == null)
            {
                return false;
            }

            object convertedValue = ConvertValue(value, elementType, target, culture);
            object existing = property.GetValue(instance, null);
            if (existing != null && TryAddCollectionItem(existing, elementType, convertedValue))
            {
                return true;
            }

            if (!property.CanWrite)
            {
                throw new CommandParameterException("Property '" + property.Name + "' is not writable.");
            }

            IList values = CreateList(elementType);
            if (existing != null)
            {
                foreach (object item in (IEnumerable)existing)
                {
                    values.Add(item);
                }
            }

            values.Add(convertedValue);
            property.SetValue(instance, CreateCollectionInstance(property.PropertyType, elementType, values), null);
            return true;
        }

        private static bool TryAddDictionaryValue(CommandSettings instance, BindingTarget target, string value, CultureInfo culture)
        {
            PropertyInfo property = target.Property;
            Type[] keyValueTypes = GetDictionaryTypes(property.PropertyType);
            if (keyValueTypes == null)
            {
                return false;
            }

            if (!property.CanWrite && property.GetValue(instance, null) == null)
            {
                throw new CommandParameterException("Property '" + property.Name + "' is not writable.");
            }

            object dictionary = property.GetValue(instance, null);
            if (dictionary == null)
            {
                dictionary = CreateDictionaryInstance(property.PropertyType, keyValueTypes[0], keyValueTypes[1]);
                property.SetValue(instance, dictionary, null);
            }

            KeyValuePair<object, object> pair = ParseKeyValue(value, keyValueTypes[0], keyValueTypes[1], target, culture);
            Type dictionaryInterface = typeof(IDictionary<,>).MakeGenericType(keyValueTypes[0], keyValueTypes[1]);
            dictionaryInterface.GetProperty("Item").SetValue(dictionary, pair.Value, new[] { pair.Key });
            return true;
        }

        private static bool TryAddLookupValue(CommandSettings instance, BindingTarget target, string value, CultureInfo culture)
        {
            PropertyInfo property = target.Property;
            Type[] keyValueTypes = GetLookupTypes(property.PropertyType);
            if (keyValueTypes == null)
            {
                return false;
            }

            if (!property.CanWrite)
            {
                throw new CommandParameterException("Property '" + property.Name + "' is not writable.");
            }

            var pairs = new List<KeyValuePair<object, object>>();
            object existing = property.GetValue(instance, null);
            if (existing != null)
            {
                foreach (object group in (System.Collections.IEnumerable)existing)
                {
                    object key = group.GetType().GetProperty("Key").GetValue(group, null);
                    foreach (object item in (System.Collections.IEnumerable)group)
                    {
                        pairs.Add(new KeyValuePair<object, object>(key, item));
                    }
                }
            }

            pairs.Add(ParseKeyValue(value, keyValueTypes[0], keyValueTypes[1], target, culture));
            object lookup = CreateLookup(pairs, keyValueTypes[0], keyValueTypes[1]);
            property.SetValue(instance, lookup, null);
            return true;
        }

        private static object CreateDictionaryInstance(Type propertyType, Type keyType, Type valueType)
        {
            if (!propertyType.IsInterface && !propertyType.IsAbstract)
            {
                return Activator.CreateInstance(propertyType);
            }

            Type dictionaryType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
            return Activator.CreateInstance(dictionaryType);
        }

        private static void ClearDictionaryValue(CommandSettings instance, PropertyInfo property, Type keyType, Type valueType)
        {
            object dictionary = property.GetValue(instance, null);
            var nonGenericDictionary = dictionary as IDictionary;
            if (nonGenericDictionary != null)
            {
                nonGenericDictionary.Clear();
                return;
            }

            Type pairType = typeof(KeyValuePair<,>).MakeGenericType(keyType, valueType);
            Type collectionInterface = typeof(ICollection<>).MakeGenericType(pairType);
            if (dictionary != null && collectionInterface.IsInstanceOfType(dictionary))
            {
                collectionInterface.GetMethod("Clear").Invoke(dictionary, null);
                return;
            }

            if (!property.CanWrite)
            {
                throw new CommandParameterException("Property '" + property.Name + "' is not writable.");
            }

            property.SetValue(instance, CreateDictionaryInstance(property.PropertyType, keyType, valueType), null);
        }

        private static object CreateLookup(List<KeyValuePair<object, object>> pairs, Type keyType, Type valueType)
        {
            Type helperType = typeof(LookupBuilder<,>).MakeGenericType(keyType, valueType);
            return helperType.GetMethod(nameof(LookupBuilder<object, object>.Create)).Invoke(null, new object[] { pairs });
        }

        private static KeyValuePair<object, object> ParseKeyValue(string text, Type keyType, Type valueType, BindingTarget target, CultureInfo culture)
        {
            PropertyInfo property = target.Property;
            IPairDeconstructor deconstructor = CreatePairDeconstructor(property, keyType, valueType);
            if (deconstructor != null)
            {
                object deconstructedKey;
                object deconstructedValue;
                if (!deconstructor.TryDeconstruct(text, out deconstructedKey, out deconstructedValue))
                {
                    throw new CommandParameterException("Value '" + (text ?? string.Empty) + "' for " + target.Kind + " '" + target.Name + "' could not be deconstructed by '" + deconstructor.GetType().Name + "'.");
                }

                return new KeyValuePair<object, object>(deconstructedKey, deconstructedValue);
            }

            string value = text ?? string.Empty;
            int separatorIndex = value.IndexOf('=');
            if (separatorIndex < 0)
            {
                separatorIndex = value.IndexOf(':');
            }

            if (separatorIndex < 0)
            {
                throw new CommandParameterException("Value '" + value + "' for " + target.Kind + " '" + target.Name + "' must use KEY=VALUE syntax.");
            }

            string keyText = value.Substring(0, separatorIndex);
            string valueText = value.Substring(separatorIndex + 1);
            object key = ConvertValue(keyText, keyType, target, culture);
            object convertedValue = ConvertValue(valueText, valueType, target, culture);
            return new KeyValuePair<object, object>(key, convertedValue);
        }

        private static IPairDeconstructor CreatePairDeconstructor(PropertyInfo property, Type keyType, Type valueType)
        {
            TypeConverterAttribute converterAttribute = property.GetCustomAttribute<TypeConverterAttribute>(true);
            if (converterAttribute == null || string.IsNullOrWhiteSpace(converterAttribute.ConverterTypeName))
            {
                return null;
            }

            Type converterType = Type.GetType(converterAttribute.ConverterTypeName, false);
            if (converterType == null || !typeof(IPairDeconstructor).IsAssignableFrom(converterType))
            {
                return null;
            }

            var deconstructor = (IPairDeconstructor)Activator.CreateInstance(converterType);
            if (deconstructor.KeyType != keyType || deconstructor.ValueType != valueType)
            {
                throw new CommandParameterException(
                    "Pair deconstructor '" +
                    converterType.Name +
                    "' for '" +
                    property.Name +
                    "' must derive from PairDeconstructor<" +
                    keyType.Name +
                    ", " +
                    valueType.Name +
                    ">.");
            }

            return deconstructor;
        }

        private static Type[] GetDictionaryTypes(Type type)
        {
            return GetGenericCollectionTypes(type, typeof(IDictionary<,>), typeof(IReadOnlyDictionary<,>), typeof(Dictionary<,>));
        }

        private static Type[] GetLookupTypes(Type type)
        {
            return GetGenericCollectionTypes(type, typeof(ILookup<,>));
        }

        internal static Type GetCollectionElementType(Type type)
        {
            if (type == null ||
                type == typeof(string) ||
                type.IsArray ||
                IsFlagValueType(type) ||
                GetDictionaryTypes(type) != null ||
                GetLookupTypes(type) != null)
            {
                return null;
            }

            Type[] collectionTypes = GetGenericCollectionTypes(
                type,
                typeof(IEnumerable<>),
                typeof(IReadOnlyCollection<>),
                typeof(IReadOnlyList<>),
                typeof(ICollection<>),
                typeof(IList<>),
                typeof(List<>),
                typeof(HashSet<>));
            return collectionTypes == null ? null : collectionTypes[0];
        }

        private static Type[] GetGenericCollectionTypes(Type type, params Type[] definitions)
        {
            if (type == null)
            {
                return null;
            }

            if (type.IsGenericType && definitions.Contains(type.GetGenericTypeDefinition()))
            {
                return type.GetGenericArguments();
            }

            foreach (Type interfaceType in type.GetInterfaces())
            {
                if (interfaceType.IsGenericType && definitions.Contains(interfaceType.GetGenericTypeDefinition()))
                {
                    return interfaceType.GetGenericArguments();
                }
            }

            return null;
        }

        private static void SetCollectionValue(CommandSettings instance, BindingTarget target, string[] values, CultureInfo culture)
        {
            PropertyInfo property = target.Property;
            Type elementType = GetCollectionElementType(property.PropertyType);
            if (elementType == null)
            {
                throw new CommandParameterException("Property '" + property.Name + "' is not a supported generic collection.");
            }

            IList convertedValues = CreateList(elementType);
            foreach (string value in values ?? new string[0])
            {
                convertedValues.Add(ConvertValue(value, elementType, target, culture));
            }

            if (property.CanWrite)
            {
                property.SetValue(instance, CreateCollectionInstance(property.PropertyType, elementType, convertedValues), null);
                return;
            }

            object existing = property.GetValue(instance, null);
            if (existing != null && TryReplaceCollectionItems(existing, elementType, convertedValues))
            {
                return;
            }

            throw new CommandParameterException("Property '" + property.Name + "' is not writable.");
        }

        private static IList CreateList(Type elementType)
        {
            Type listType = typeof(List<>).MakeGenericType(elementType);
            return (IList)Activator.CreateInstance(listType);
        }

        private static object CreateCollectionInstance(Type propertyType, Type elementType, IList values)
        {
            if (propertyType.IsAssignableFrom(values.GetType()))
            {
                return values;
            }

            if (!propertyType.IsInterface && !propertyType.IsAbstract)
            {
                object collection = Activator.CreateInstance(propertyType);
                if (TryReplaceCollectionItems(collection, elementType, values))
                {
                    return collection;
                }
            }

            throw new CommandParameterException(
                "Property type '" +
                propertyType.Name +
                "' must be assignable from List<" +
                elementType.Name +
                "> or implement ICollection<" +
                elementType.Name +
                "> with a parameterless constructor.");
        }

        private static bool TryReplaceCollectionItems(object collection, Type elementType, IEnumerable values)
        {
            if (!TryClearCollection(collection, elementType))
            {
                return false;
            }

            foreach (object value in values)
            {
                if (!TryAddCollectionItem(collection, elementType, value))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool TryClearCollection(object collection, Type elementType)
        {
            if (collection == null)
            {
                return false;
            }

            Type collectionType = typeof(ICollection<>).MakeGenericType(elementType);
            if (!collectionType.IsInstanceOfType(collection))
            {
                return false;
            }

            try
            {
                collectionType.GetMethod("Clear").Invoke(collection, null);
                return true;
            }
            catch (TargetInvocationException)
            {
                return false;
            }
            catch (NotSupportedException)
            {
                return false;
            }
        }

        private static bool TryAddCollectionItem(object collection, Type elementType, object value)
        {
            if (collection == null)
            {
                return false;
            }

            Type collectionType = typeof(ICollection<>).MakeGenericType(elementType);
            if (!collectionType.IsInstanceOfType(collection))
            {
                return false;
            }

            try
            {
                collectionType.GetMethod("Add").Invoke(collection, new[] { value });
                return true;
            }
            catch (TargetInvocationException)
            {
                return false;
            }
            catch (NotSupportedException)
            {
                return false;
            }
        }

        private static void SetFlagValue(CommandSettings instance, CommandOptionDescriptor option, string value, CultureInfo culture, string token)
        {
            SetFlagValue(instance, option, value, culture, BindingTarget.ForOption(option, token));
        }

        private static void SetFlagValue(CommandSettings instance, CommandOptionDescriptor option, string value, CultureInfo culture, BindingTarget target)
        {
            if (!option.Property.CanWrite)
            {
                throw new CommandParameterException("Property '" + option.Property.Name + "' is not writable.");
            }

            Type valueType = option.FlagValueType;
            object rawValue = value;
            if (rawValue == null)
            {
                rawValue = option.DefaultValue ?? GetDefaultValue(valueType);
            }

            object convertedValue = ConvertValue(rawValue, valueType, target, culture);
            object flagValue = Activator.CreateInstance(option.Property.PropertyType, true, convertedValue);
            option.Property.SetValue(instance, flagValue, null);
        }

        private static void SetArrayValue(CommandSettings instance, BindingTarget target, string[] values, CultureInfo culture)
        {
            PropertyInfo property = target.Property;
            Type elementType = property.PropertyType.GetElementType();
            Array array = Array.CreateInstance(elementType, values.Length);
            for (int index = 0; index < values.Length; index++)
            {
                array.SetValue(ConvertValue(values[index], elementType, target, culture), index);
            }

            property.SetValue(instance, array, null);
        }

        private static void SetRemainingArgumentsValue(CommandSettings instance, PropertyInfo property, string[] values)
        {
            if (!property.CanWrite)
            {
                throw new CommandParameterException("Property '" + property.Name + "' is not writable.");
            }

            if (!property.PropertyType.IsAssignableFrom(typeof(RemainingArguments)))
            {
                throw new CommandParameterException("Property '" + property.Name + "' must be assignable from RemainingArguments.");
            }

            property.SetValue(instance, new RemainingArguments(values), null);
        }

        private static void SetValue(CommandSettings instance, BindingTarget target, string value, CultureInfo culture)
        {
            PropertyInfo property = target.Property;
            if (!property.CanWrite)
            {
                throw new CommandParameterException("Property '" + property.Name + "' is not writable.");
            }

            property.SetValue(instance, ConvertValue(value, property.PropertyType, target, culture), null);
        }

        private static object ConvertValue(object value, Type targetType, BindingTarget target, CultureInfo culture)
        {
            return ConvertValue(value, targetType, target, culture, CreatePropertyTypeConverter(target.Property));
        }

        private static object ConvertValue(object value, Type targetType, BindingTarget target, CultureInfo culture, TypeConverter propertyConverter)
        {
            culture = culture ?? CultureInfo.InvariantCulture;
            Type conversionType = Nullable.GetUnderlyingType(targetType) ?? targetType;
            if (value == null)
            {
                return null;
            }

            if (conversionType.IsInstanceOfType(value))
            {
                return value;
            }

            string text = Convert.ToString(value, culture);
            try
            {
                if (conversionType == typeof(string))
                {
                    return text;
                }

                if (conversionType == typeof(bool))
                {
                    if (string.Equals(text, "1", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(text, "yes", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(text, "y", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }

                    if (string.Equals(text, "0", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(text, "no", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(text, "n", StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }

                    return bool.Parse(text);
                }

                if (conversionType.IsEnum)
                {
                    return ConvertEnumValue(conversionType, text);
                }

                TypeConverter converter = TypeDescriptor.GetConverter(conversionType);
                if (propertyConverter != null && propertyConverter.CanConvertFrom(typeof(string)))
                {
                    return propertyConverter.ConvertFrom(null, culture, text);
                }

                if (converter != null && converter.CanConvertFrom(typeof(string)))
                {
                    return converter.ConvertFrom(null, culture, text);
                }

                return Convert.ChangeType(value, conversionType, culture);
            }
            catch (Exception ex)
            {
                throw new CommandParameterException(BuildConversionFailureMessage(text, target, ex, conversionType), ex);
            }
        }

        private static object ConvertEnumValue(Type enumType, string text)
        {
            try
            {
                return Enum.Parse(enumType, text, ignoreCase: true);
            }
            catch
            {
                // Try CLI-friendly spellings such as release-candidate for ReleaseCandidate below.
            }

            if (text != null &&
                text.IndexOf(',') >= 0 &&
                enumType.GetCustomAttribute<FlagsAttribute>() != null)
            {
                long combined = 0;
                foreach (string segment in text.Split(','))
                {
                    object parsed = ConvertNormalizedEnumValue(enumType, segment);
                    combined |= Convert.ToInt64(parsed, CultureInfo.InvariantCulture);
                }

                return Enum.ToObject(enumType, combined);
            }

            return ConvertNormalizedEnumValue(enumType, text);
        }

        private static object ConvertNormalizedEnumValue(Type enumType, string text)
        {
            string normalized = NormalizeEnumValue(text);
            if (normalized.Length > 0)
            {
                foreach (string name in Enum.GetNames(enumType))
                {
                    if (IsEnumValueMatch(enumType, name, normalized))
                    {
                        return Enum.Parse(enumType, name, ignoreCase: false);
                    }
                }
            }

            throw new ArgumentException("Requested value '" + (text ?? string.Empty) + "' was not found.");
        }

        private static bool IsEnumValueMatch(Type enumType, string name, string normalized)
        {
            if (string.Equals(NormalizeEnumValue(name), normalized, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            foreach (string alias in GetEnumValueAliases(enumType, name))
            {
                if (string.Equals(NormalizeEnumValue(alias), normalized, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static IEnumerable<string> GetEnumValueAliases(Type enumType, string name)
        {
            FieldInfo field = enumType.GetField(name, BindingFlags.Public | BindingFlags.Static);
            if (field == null)
            {
                yield break;
            }

            var descriptionAttribute = field.GetCustomAttribute<DescriptionAttribute>(false);
            if (descriptionAttribute != null && !string.IsNullOrWhiteSpace(descriptionAttribute.Description))
            {
                yield return descriptionAttribute.Description;
            }

            var displayNameAttribute = field.GetCustomAttribute<DisplayNameAttribute>(false);
            if (displayNameAttribute != null && !string.IsNullOrWhiteSpace(displayNameAttribute.DisplayName))
            {
                yield return displayNameAttribute.DisplayName;
            }

            string displayName = ResolveDisplayAttributeName(field);
            if (!string.IsNullOrWhiteSpace(displayName))
            {
                yield return displayName;
            }

            foreach (object attribute in field.GetCustomAttributes(false))
            {
                if (!string.Equals(
                    attribute.GetType().FullName,
                    "System.Runtime.Serialization.EnumMemberAttribute",
                    StringComparison.Ordinal))
                {
                    continue;
                }

                PropertyInfo valueProperty = attribute.GetType().GetProperty("Value", BindingFlags.Public | BindingFlags.Instance);
                string value = valueProperty == null ? null : valueProperty.GetValue(attribute, null) as string;
                if (!string.IsNullOrWhiteSpace(value))
                {
                    yield return value;
                }
            }
        }

        internal static string[] GetEnumValueHelpNames(Type enumType)
        {
            var values = new List<string>();
            var observed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string name in Enum.GetNames(enumType))
            {
                AddEnumValueHelpName(values, observed, ToCliEnumValue(name));
                foreach (string alias in GetEnumValueAliases(enumType, name))
                {
                    AddEnumValueHelpName(values, observed, ToCliEnumValue(alias));
                }
            }

            return values.ToArray();
        }

        private static void AddEnumValueHelpName(List<string> values, HashSet<string> observed, string value)
        {
            if (string.IsNullOrWhiteSpace(value) || !observed.Add(value))
            {
                return;
            }

            values.Add(value);
        }

        private static string ToCliEnumValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var builder = new StringBuilder();
            char previous = '\0';
            for (int index = 0; index < value.Length; index++)
            {
                char current = value[index];
                if (char.IsWhiteSpace(current) || current == '_')
                {
                    AppendEnumValueSeparator(builder);
                    previous = current;
                    continue;
                }

                if (current == '-')
                {
                    AppendEnumValueSeparator(builder);
                    previous = current;
                    continue;
                }

                if (char.IsUpper(current) && builder.Length > 0 && previous != '-' && previous != '_' && !char.IsWhiteSpace(previous))
                {
                    bool previousIsLowerOrDigit = char.IsLower(previous) || char.IsDigit(previous);
                    bool nextIsLower = index + 1 < value.Length && char.IsLower(value[index + 1]);
                    if (previousIsLowerOrDigit || nextIsLower)
                    {
                        AppendEnumValueSeparator(builder);
                    }
                }

                builder.Append(char.ToLowerInvariant(current));
                previous = current;
            }

            return builder.ToString().Trim('-');
        }

        private static void AppendEnumValueSeparator(StringBuilder builder)
        {
            if (builder.Length == 0 || builder[builder.Length - 1] == '-')
            {
                return;
            }

            builder.Append('-');
        }

        private static string NormalizeEnumValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            return new string(value.Where(character =>
                character != '-' &&
                character != '_' &&
                !char.IsWhiteSpace(character)).ToArray());
        }

        private static string BuildConversionFailureMessage(string text, BindingTarget target, Exception exception, Type conversionType)
        {
            string message = "Could not bind value '" + text + "' to " + target.Kind + " '" + target.Name + "'.";
            if (!string.IsNullOrWhiteSpace(target.ExpectedValueName))
            {
                message += " Expected <" + target.ExpectedValueName + ">.";
            }

            bool hasEnumHint = false;
            if (conversionType != null && conversionType.IsEnum)
            {
                string[] values = GetEnumValueHelpNames(conversionType);
                if (values.Length > 0)
                {
                    message += " Allowed values: " + string.Join(", ", values) + ".";
                    hasEnumHint = true;
                }
            }

            if (!hasEnumHint && exception != null && !string.IsNullOrWhiteSpace(exception.Message))
            {
                message += " " + exception.Message;
            }

            return message;
        }

        private static TypeConverter CreatePropertyTypeConverter(PropertyInfo property)
        {
            TypeConverterAttribute converterAttribute = property.GetCustomAttribute<TypeConverterAttribute>(true);
            if (converterAttribute == null || string.IsNullOrWhiteSpace(converterAttribute.ConverterTypeName))
            {
                return null;
            }

            Type converterType = Type.GetType(converterAttribute.ConverterTypeName, false);
            if (converterType == null || !typeof(TypeConverter).IsAssignableFrom(converterType))
            {
                return null;
            }

            return (TypeConverter)Activator.CreateInstance(converterType);
        }

        private static bool IsFlagValueType(Type type)
        {
            return type != null &&
                type.IsGenericType &&
                type.GetGenericTypeDefinition() == typeof(FlagValue<>);
        }

        internal static Type GetFlagValueType(Type type)
        {
            return IsFlagValueType(type) ? type.GetGenericArguments()[0] : null;
        }

        private static object GetDefaultValue(Type type)
        {
            if (type == null)
            {
                return null;
            }

            if (!type.IsValueType || Nullable.GetUnderlyingType(type) != null)
            {
                return null;
            }

            return Activator.CreateInstance(type);
        }

        private readonly struct BindingTarget
        {
            private BindingTarget(PropertyInfo property, string kind, string name, string expectedValueName)
            {
                Property = property;
                Kind = string.IsNullOrWhiteSpace(kind) ? "property" : kind;
                Name = string.IsNullOrWhiteSpace(name) && property != null ? property.Name : name;
                ExpectedValueName = expectedValueName ?? string.Empty;
            }

            public PropertyInfo Property { get; }

            public string Kind { get; }

            public string Name { get; }

            public string ExpectedValueName { get; }

            public static BindingTarget ForArgument(CommandArgumentDescriptor argument)
            {
                return new BindingTarget(argument.Property, "argument", argument.Name, argument.Name);
            }

            public static BindingTarget ForOption(CommandOptionDescriptor option, string token)
            {
                string optionName = GetOptionName(token);
                if (string.IsNullOrWhiteSpace(optionName))
                {
                    optionName = option.PrimaryName;
                }

                return new BindingTarget(option.Property, "option", optionName, option.ValueName);
            }

            public static BindingTarget ForEnvironmentVariable(CommandOptionDescriptor option, string variableName)
            {
                return new BindingTarget(option.Property, "environment variable", variableName, option.ValueName);
            }

            public static BindingTarget ForProperty(PropertyInfo property)
            {
                return new BindingTarget(property, "property", property == null ? string.Empty : property.Name, string.Empty);
            }
        }

        private readonly struct OptionToken
        {
            public OptionToken(CommandOptionDescriptor option, bool hasValue, string value)
            {
                Option = option;
                HasValue = hasValue;
                Value = value;
            }

            public CommandOptionDescriptor Option { get; }

            public bool HasValue { get; }

            public string Value { get; }
        }

        private readonly struct ClusteredFlag
        {
            public ClusteredFlag(CommandOptionDescriptor option, string alias)
            {
                Option = option;
                Alias = alias;
            }

            public CommandOptionDescriptor Option { get; }

            public string Alias { get; }
        }

        private static class LookupBuilder<TKey, TValue>
        {
            public static ILookup<TKey, TValue> Create(IEnumerable<KeyValuePair<object, object>> pairs)
            {
                return pairs
                    .Select(pair => new KeyValuePair<TKey, TValue>((TKey)pair.Key, (TValue)pair.Value))
                    .ToLookup(pair => pair.Key, pair => pair.Value);
            }
        }
    }

    internal sealed class CommandSettingsDescriptor
    {
        public static readonly CommandSettingsDescriptor Empty = new CommandSettingsDescriptor(
            new CommandArgumentDescriptor[0],
            new CommandOptionDescriptor[0]);

        public CommandSettingsDescriptor(IReadOnlyList<CommandArgumentDescriptor> arguments, IReadOnlyList<CommandOptionDescriptor> options)
        {
            Arguments = arguments;
            Options = options;
        }

        public IReadOnlyList<CommandArgumentDescriptor> Arguments { get; }

        public IReadOnlyList<CommandOptionDescriptor> Options { get; }
    }

    internal sealed class CommandArgumentDescriptor : ICommandArgument
    {
        public CommandArgumentDescriptor(PropertyInfo property, CommandArgumentAttribute attribute)
        {
            Property = property;
            Attribute = attribute;
            Description = GetDescription(property);
            DefaultValue = property.GetCustomAttribute<DefaultValueAttribute>()?.Value;
        }

        public PropertyInfo Property { get; }

        public CommandArgumentAttribute Attribute { get; }

        public int Position => Attribute.Position;

        public string Template => Attribute.Template;

        public string Name => Attribute.Name;

        public string Description { get; }

        public object DefaultValue { get; }

        public bool IsRequired => Attribute.IsRequired;

        public bool IsOption => false;

        public Type ValueType => Property.PropertyType;

        public bool IsArray => Property.PropertyType.IsArray;

        public bool IsCollection => CollectionElementType != null;

        public Type CollectionElementType => CommandSettingsBinder.GetCollectionElementType(Property.PropertyType);

        public bool IsRemainingArguments => typeof(IRemainingArguments).IsAssignableFrom(Property.PropertyType);

        private static string GetDescription(PropertyInfo property)
        {
            DescriptionAttribute attribute = property.GetCustomAttribute<DescriptionAttribute>();
            return attribute == null ? string.Empty : attribute.Description;
        }
    }

    internal sealed class CommandOptionDescriptor : ICommandOption
    {
        public CommandOptionDescriptor(PropertyInfo property, CommandOptionAttribute attribute)
        {
            Property = property;
            Attribute = attribute;
            Description = GetDescription(property);
            DefaultValue = property.GetCustomAttribute<DefaultValueAttribute>()?.Value;
        }

        public PropertyInfo Property { get; }

        public CommandOptionAttribute Attribute { get; }

        public string Description { get; }

        public object DefaultValue { get; }

        public IReadOnlyList<string> Aliases => Attribute.Aliases;

        public IReadOnlyList<string> LongNames => Attribute.LongNames;

        public string PrimaryName => Aliases.Count == 0 ? Property.Name : Aliases[0];

        public string Name => PrimaryName;

        public string Template => Attribute.Template;

        public bool IsRequired => Attribute.IsRequired;

        public bool IsOption => true;

        public Type ValueType => Property.PropertyType;

        public bool IsArray => Property.PropertyType.IsArray;

        public bool IsCollection => CollectionElementType != null;

        public Type CollectionElementType => CommandSettingsBinder.GetCollectionElementType(Property.PropertyType);

        public bool IsBoolean
        {
            get
            {
                if (IsArray || IsCollection)
                {
                    return false;
                }

                Type type = FlagValueType ?? Property.PropertyType;
                type = Nullable.GetUnderlyingType(type) ?? type;
                return type == typeof(bool);
            }
        }

        public bool IsFlagValue => FlagValueType != null;

        public Type FlagValueType => CommandSettingsBinder.GetFlagValueType(Property.PropertyType);

        public bool AcceptsBooleanNegation => IsBoolean && LongNames.Count > 0;

        public bool IsHidden => Attribute.IsHidden;

        public string EnvironmentVariable => Attribute.EnvironmentVariable;

        public bool ValueIsOptional => Attribute.ValueIsOptional;

        public string ValueName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(Attribute.ValueName))
                {
                    return Attribute.ValueName;
                }

                Type type = IsArray
                    ? Property.PropertyType.GetElementType()
                    : CollectionElementType ?? FlagValueType ?? Property.PropertyType;
                type = Nullable.GetUnderlyingType(type) ?? type;
                return type.IsEnum ? "VALUE" : type.Name.ToUpperInvariant();
            }
        }

        private static string GetDescription(PropertyInfo property)
        {
            DescriptionAttribute attribute = property.GetCustomAttribute<DescriptionAttribute>();
            return attribute == null ? string.Empty : attribute.Description;
        }
    }
}
