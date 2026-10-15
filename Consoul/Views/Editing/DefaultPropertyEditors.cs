using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;
using ConsoulLibrary.Color;

namespace ConsoulLibrary.Views.Editing
{
    /// <summary>
    /// Provides default editors for property editing scenarios.
    /// </summary>
    public sealed class DefaultPropertyEditor : IPropertyEditor
    {
        /// <inheritdoc />
        public bool TryEdit(PropertyEditContext context, out object value)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var instruction = context.Documentation.DisplayDescription;
            if (string.IsNullOrWhiteSpace(instruction))
            {
                instruction = context.Documentation.XmlSummary;
            }

            if (!string.IsNullOrWhiteSpace(instruction))
            {
                Consoul.WriteCore(NormalizeWhitespace(instruction), RenderOptions.SubnoteColor);
            }

            Type nullableType = Nullable.GetUnderlyingType(context.Property.PropertyType);
            var expectedType = nullableType ?? context.Property.PropertyType;
            var message = "Enter new " + GetPropertyDisplayName(context) + " (" + expectedType.Name + ")";
            if (context.Property.PropertyType == typeof(bool) || nullableType == typeof(bool))
            {
                return TryEditBoolean(context, message, nullableType != null && !IsRequiredProperty(context.Property), out value);
            }

            if (expectedType.IsEnum)
            {
                if (IsFlagsEnum(expectedType))
                {
                    return TryEditFlagsEnum(context, expectedType, message, nullableType != null && !IsRequiredProperty(context.Property), out value);
                }

                return TryEditEnum(context, expectedType, message, nullableType != null && !IsRequiredProperty(context.Property), out value);
            }

            return TryEditWithPrompt(context, context.Property.PropertyType, message, out value);
        }

        private static string GetPropertyDisplayName(PropertyEditContext context)
        {
            var displayName = context.Documentation.DisplayName;
            if (!string.IsNullOrWhiteSpace(displayName))
            {
                return displayName;
            }

            return context.Property.Name;
        }

        private static bool TryEditBoolean(PropertyEditContext context, string message, bool allowClear, out object value)
        {
            if (allowClear)
            {
                return TryEditNullableBoolean(context, message, out value);
            }

            var prompt = Consoul.Confirmation(message);
            if (context.CurrentValue is bool currentValue)
            {
                prompt.WithDefault(currentValue);
            }

            ConfirmResult result = prompt.Render(context.CancellationToken);
            if (result.IsCanceled)
            {
                value = null;
                return false;
            }

            value = result.Value;
            return true;
        }

        private static bool TryEditNullableBoolean(PropertyEditContext context, string message, out object value)
        {
            var prompt = new SelectionPrompt(message);
            bool hasCurrentValue = context.CurrentValue is bool;
            bool currentValue = hasCurrentValue && (bool)context.CurrentValue;

            prompt.Add("Yes", ConsoleColor.Green, isDefault: hasCurrentValue && currentValue);
            prompt.Add("No", ConsoleColor.DarkYellow, isDefault: hasCurrentValue && !currentValue);
            prompt.Add("Clear value", RenderOptions.SubnoteColor, isDefault: !hasCurrentValue);

            PromptResult result = prompt.Render(context.CancellationToken);
            if (result.IsCanceled)
            {
                value = null;
                return false;
            }

            if (!result.HasSelection)
            {
                value = null;
                return false;
            }

            switch (result.Index)
            {
                case 0:
                    value = true;
                    return true;
                case 1:
                    value = false;
                    return true;
                case 2:
                    value = null;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        private static bool TryEditEnum(PropertyEditContext context, Type enumType, string message, bool allowClear, out object value)
        {
            Array enumValues = Enum.GetValues(enumType);
            var values = enumValues.Cast<object>().ToArray();
            var prompt = new SelectionPrompt(message);
            foreach (object enumValue in values)
            {
                bool isDefault = context.CurrentValue != null && Equals(context.CurrentValue, enumValue);
                prompt.Add(FormatEnumValue(enumType, enumValue), isDefault: isDefault);
            }

            if (allowClear)
            {
                prompt.Add("Clear value", RenderOptions.SubnoteColor, isDefault: context.CurrentValue == null);
            }

            PromptResult result = prompt.Render(context.CancellationToken);
            if (result.IsCanceled)
            {
                value = null;
                return false;
            }

            if (result.HasSelection && result.Index >= 0 && result.Index < values.Length)
            {
                value = values[result.Index];
                return true;
            }

            if (allowClear && result.HasSelection && result.Index == values.Length)
            {
                value = null;
                return true;
            }

            value = null;
            return false;
        }

        private static bool TryEditFlagsEnum(PropertyEditContext context, Type enumType, string message, bool allowClear, out object value)
        {
            object[] values = GetFlagChoices(enumType);
            var prompt = new MultiSelectionPrompt(message, false, true);
            foreach (object enumValue in values)
            {
                bool isDefault = IsFlagSelected(enumType, context.CurrentValue, enumValue);
                prompt.Add(FormatEnumValue(enumType, enumValue), isDefault: isDefault);
            }

            int clearIndex = -1;
            if (allowClear)
            {
                clearIndex = prompt.Add("Clear value", RenderOptions.SubnoteColor, isDefault: context.CurrentValue == null);
            }

            MultiSelectionResult result = prompt.Render(context.CancellationToken);
            if (result.IsCanceled)
            {
                value = null;
                return false;
            }

            if (allowClear && result.Indexes.Contains(clearIndex))
            {
                value = null;
                return true;
            }

            ulong combinedValue = 0;
            foreach (int index in result.Indexes)
            {
                if (index >= 0 && index < values.Length)
                {
                    combinedValue |= ConvertEnumValueToUInt64(enumType, values[index]);
                }
            }

            value = Enum.ToObject(enumType, combinedValue);
            return true;
        }

        private static bool TryEditWithPrompt(PropertyEditContext context, Type promptType, string message, out object value)
        {
            MethodInfo method = typeof(DefaultPropertyEditor).GetMethod(nameof(TryEditWithPromptCore), BindingFlags.Static | BindingFlags.NonPublic);
            if (method == null)
            {
                throw new InvalidOperationException("The default property editor prompt helper could not be resolved.");
            }

            object[] parameters = new object[] { context, message, null };
            try
            {
                bool edited = (bool)method.MakeGenericMethod(promptType).Invoke(null, parameters);
                value = parameters[2];
                return edited;
            }
            catch (TargetInvocationException exception) when (exception.InnerException != null)
            {
                ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
                throw;
            }
        }

        private static bool TryEditWithPromptCore<T>(PropertyEditContext context, string message, out object value)
        {
            TextPrompt<T> prompt = Consoul.Text<T>(message)
                .TrimInput()
                .WithInvalidMessage("Invalid input. Please enter a value of type " + GetPromptTypeName(typeof(T)) + ".");

            if (Nullable.GetUnderlyingType(typeof(T)) != null)
            {
                prompt.AllowEmptyInput();
            }

            if (context.CurrentValue != null && TryConvertCurrentValue(context.CurrentValue, out T currentValue))
            {
                prompt.WithDefault(currentValue);
            }

            ApplyValidationMetadata(prompt, context);

            TextPromptResult<T> result = prompt.Render(context.CancellationToken);
            if (result.IsCanceled)
            {
                value = null;
                return false;
            }

            value = result.Value;
            return true;
        }

        private static void ApplyValidationMetadata<T>(TextPrompt<T> prompt, PropertyEditContext context)
        {
            string displayName = GetPropertyDisplayName(context);
            foreach (object attribute in context.Property.GetCustomAttributes(true))
            {
                string attributeName = attribute.GetType().FullName;
                string validationMessage = ResolveValidationMessage(attribute, displayName);

                if (string.Equals(attributeName, "System.ComponentModel.DataAnnotations.RequiredAttribute", StringComparison.Ordinal))
                {
                    prompt.Required(validationMessage);
                }
                else if (string.Equals(attributeName, "System.ComponentModel.DataAnnotations.RangeAttribute", StringComparison.Ordinal))
                {
                    object minimumObject = GetPropertyValue(attribute, "Minimum");
                    object maximumObject = GetPropertyValue(attribute, "Maximum");
                    T minimum;
                    T maximum;
                    if (TryConvertBoundary(minimumObject, out minimum) && TryConvertBoundary(maximumObject, out maximum))
                    {
                        prompt.Range(minimum, maximum, validationMessage);
                    }
                }
                else if (typeof(T) == typeof(string) &&
                    string.Equals(attributeName, "System.ComponentModel.DataAnnotations.StringLengthAttribute", StringComparison.Ordinal))
                {
                    ApplyStringLengthAttribute((TextPrompt<string>)(object)prompt, attribute, validationMessage);
                }
                else if (typeof(T) == typeof(string) &&
                    string.Equals(attributeName, "System.ComponentModel.DataAnnotations.MinLengthAttribute", StringComparison.Ordinal))
                {
                    int? length = GetIntPropertyValue(attribute, "Length");
                    if (length.HasValue)
                    {
                        ((TextPrompt<string>)(object)prompt).MinimumLength(length.Value, validationMessage);
                    }
                }
                else if (typeof(T) == typeof(string) &&
                    string.Equals(attributeName, "System.ComponentModel.DataAnnotations.MaxLengthAttribute", StringComparison.Ordinal))
                {
                    int? length = GetIntPropertyValue(attribute, "Length");
                    if (length.HasValue && length.Value >= 0)
                    {
                        ((TextPrompt<string>)(object)prompt).MaximumLength(length.Value, validationMessage);
                    }
                }

                if (IsValidationAttribute(attribute))
                {
                    object validationAttribute = attribute;
                    string message = validationMessage;
                    prompt.Validate(value => ValidateAttribute(validationAttribute, value, context.Model, context.Property.Name, message));
                }
            }
        }

        private static void ApplyStringLengthAttribute(TextPrompt<string> prompt, object attribute, string validationMessage)
        {
            int? maximumLength = GetIntPropertyValue(attribute, "MaximumLength");
            int minimumLength = GetIntPropertyValue(attribute, "MinimumLength") ?? 0;
            if (!maximumLength.HasValue || maximumLength.Value < 0)
            {
                return;
            }

            if (minimumLength > 0)
            {
                prompt.LengthBetween(minimumLength, maximumLength.Value, validationMessage);
            }
            else
            {
                prompt.MaximumLength(maximumLength.Value, validationMessage);
            }
        }

        private static bool TryConvertCurrentValue<T>(object value, out T convertedValue)
        {
            return TryConvertValue(value, out convertedValue);
        }

        private static bool TryConvertBoundary<T>(object value, out T convertedValue)
        {
            return TryConvertValue(value, out convertedValue);
        }

        private static bool TryConvertValue<T>(object value, out T convertedValue)
        {
            if (value == null)
            {
                convertedValue = default(T);
                return true;
            }

            try
            {
                Type targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
                object converted = value;
                if (!targetType.IsInstanceOfType(value))
                {
                    if (targetType.IsEnum)
                    {
                        converted = Enum.Parse(targetType, value.ToString(), ignoreCase: true);
                    }
                    else
                    {
                        converted = Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
                    }
                }

                convertedValue = (T)converted;
                return true;
            }
            catch
            {
                convertedValue = default(T);
                return false;
            }
        }

        private static object GetPropertyValue(object source, string propertyName)
        {
            PropertyInfo property = source.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            return property == null ? null : property.GetValue(source);
        }

        private static int? GetIntPropertyValue(object source, string propertyName)
        {
            object value = GetPropertyValue(source, propertyName);
            if (value == null)
            {
                return null;
            }

            try
            {
                return Convert.ToInt32(value, CultureInfo.InvariantCulture);
            }
            catch
            {
                return null;
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
                    // Fall back to a simple invalid message below.
                }
            }

            return displayName + " is invalid.";
        }

        private static bool IsRequiredProperty(PropertyInfo property)
        {
            return property != null && property.GetCustomAttributes(true).Any(IsRequiredAttribute);
        }

        private static bool IsRequiredAttribute(object attribute)
        {
            return attribute != null &&
                string.Equals(
                    attribute.GetType().FullName,
                    "System.ComponentModel.DataAnnotations.RequiredAttribute",
                    StringComparison.Ordinal);
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

        private static string ValidateAttribute(object attribute, object value, object model, string memberName, string fallbackMessage)
        {
            object validationResult;
            if (TryGetValidationResult(attribute, value, model, memberName, out validationResult))
            {
                return IsValidationResultSuccess(validationResult) ? null : GetValidationResultMessage(validationResult, fallbackMessage);
            }

            return IsValidationAttributeValid(attribute, value) ? null : fallbackMessage;
        }

        private static bool TryGetValidationResult(object attribute, object value, object model, string memberName, out object validationResult)
        {
            validationResult = null;
            MethodInfo method = FindGetValidationResultMethod(attribute);
            if (method == null)
            {
                return false;
            }

            ParameterInfo[] parameters = method.GetParameters();
            object validationContext = CreateValidationContext(parameters[1].ParameterType, model);
            if (validationContext == null)
            {
                return false;
            }

            SetValidationContextMemberName(validationContext, memberName);

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

        private static object CreateValidationContext(Type validationContextType, object model)
        {
            ConstructorInfo constructor = validationContextType.GetConstructor(new[] { typeof(object) });
            if (constructor == null)
            {
                return null;
            }

            try
            {
                return constructor.Invoke(new[] { model });
            }
            catch
            {
                return null;
            }
        }

        private static void SetValidationContextMemberName(object validationContext, string memberName)
        {
            if (validationContext == null || string.IsNullOrWhiteSpace(memberName))
            {
                return;
            }

            PropertyInfo memberNameProperty = validationContext.GetType().GetProperty("MemberName", BindingFlags.Public | BindingFlags.Instance);
            if (memberNameProperty == null || !memberNameProperty.CanWrite)
            {
                return;
            }

            try
            {
                memberNameProperty.SetValue(validationContext, memberName);
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
            string message = property == null ? null : property.GetValue(validationResult) as string;
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

        private static string FormatEnumValue(Type enumType, object value)
        {
            string name = Enum.GetName(enumType, value);
            if (string.IsNullOrEmpty(name))
            {
                return value.ToString();
            }

            FieldInfo field = enumType.GetField(name);
            string label = ResolveEnumDisplayName(field);
            return !string.IsNullOrWhiteSpace(label) ? label : SplitPascalCase(name);
        }

        private static bool IsFlagsEnum(Type enumType)
        {
            return enumType != null && enumType.GetCustomAttributes(typeof(FlagsAttribute), false).Length > 0;
        }

        private static object[] GetFlagChoices(Type enumType)
        {
            object[] values = Enum.GetValues(enumType).Cast<object>().ToArray();
            object[] simpleFlags = values
                .Where(value =>
                {
                    ulong bits = ConvertEnumValueToUInt64(enumType, value);
                    return bits == 0 || IsSingleFlag(bits);
                })
                .GroupBy(value => ConvertEnumValueToUInt64(enumType, value))
                .Select(group => group.First())
                .ToArray();

            return simpleFlags.Length == 0 ? values : simpleFlags;
        }

        private static bool IsFlagSelected(Type enumType, object currentValue, object flagValue)
        {
            if (currentValue == null)
            {
                return false;
            }

            ulong currentBits = ConvertEnumValueToUInt64(enumType, currentValue);
            ulong flagBits = ConvertEnumValueToUInt64(enumType, flagValue);
            if (flagBits == 0)
            {
                return currentBits == 0;
            }

            return (currentBits & flagBits) == flagBits;
        }

        private static bool IsSingleFlag(ulong value)
        {
            return value != 0 && (value & (value - 1)) == 0;
        }

        private static ulong ConvertEnumValueToUInt64(Type enumType, object value)
        {
            Type underlyingType = Enum.GetUnderlyingType(enumType);
            switch (Type.GetTypeCode(underlyingType))
            {
                case TypeCode.SByte:
                    return unchecked((ulong)Convert.ToSByte(value, CultureInfo.InvariantCulture));
                case TypeCode.Int16:
                    return unchecked((ulong)Convert.ToInt16(value, CultureInfo.InvariantCulture));
                case TypeCode.Int32:
                    return unchecked((ulong)Convert.ToInt32(value, CultureInfo.InvariantCulture));
                case TypeCode.Int64:
                    return unchecked((ulong)Convert.ToInt64(value, CultureInfo.InvariantCulture));
                case TypeCode.Byte:
                    return Convert.ToByte(value, CultureInfo.InvariantCulture);
                case TypeCode.UInt16:
                    return Convert.ToUInt16(value, CultureInfo.InvariantCulture);
                case TypeCode.UInt32:
                    return Convert.ToUInt32(value, CultureInfo.InvariantCulture);
                case TypeCode.UInt64:
                    return Convert.ToUInt64(value, CultureInfo.InvariantCulture);
                default:
                    return Convert.ToUInt64(value, CultureInfo.InvariantCulture);
            }
        }

        private static string ResolveEnumDisplayName(FieldInfo field)
        {
            if (field == null)
            {
                return null;
            }

            object displayAttribute = field
                .GetCustomAttributes(true)
                .FirstOrDefault(attribute => string.Equals(
                    attribute.GetType().FullName,
                    "System.ComponentModel.DataAnnotations.DisplayAttribute",
                    StringComparison.Ordinal));

            if (displayAttribute != null)
            {
                MethodInfo getName = displayAttribute.GetType().GetMethod("GetName", Type.EmptyTypes);
                if (getName != null && getName.ReturnType == typeof(string))
                {
                    string name = getName.Invoke(displayAttribute, null) as string;
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        return name;
                    }
                }
            }

            object displayNameAttribute = field
                .GetCustomAttributes(true)
                .FirstOrDefault(attribute => string.Equals(
                    attribute.GetType().FullName,
                    "System.ComponentModel.DisplayNameAttribute",
                    StringComparison.Ordinal));

            string displayName = GetAttributeStringProperty(displayNameAttribute, "DisplayName");
            if (!string.IsNullOrWhiteSpace(displayName))
            {
                return displayName;
            }

            object descriptionAttribute = field
                .GetCustomAttributes(true)
                .FirstOrDefault(attribute => string.Equals(
                    attribute.GetType().FullName,
                    "System.ComponentModel.DescriptionAttribute",
                    StringComparison.Ordinal));

            string description = GetAttributeStringProperty(descriptionAttribute, "Description");
            return !string.IsNullOrWhiteSpace(description) ? description : null;
        }

        private static string GetAttributeStringProperty(object attribute, string propertyName)
        {
            if (attribute == null)
            {
                return null;
            }

            PropertyInfo property = attribute.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            return property == null ? null : property.GetValue(attribute) as string;
        }

        private static string SplitPascalCase(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            var builder = new StringBuilder(value.Length + 8);
            for (int index = 0; index < value.Length; index++)
            {
                if (index > 0 && ShouldInsertWordBoundary(value, index))
                {
                    builder.Append(' ');
                }

                builder.Append(value[index]);
            }

            return builder.ToString();
        }

        private static bool ShouldInsertWordBoundary(string value, int index)
        {
            char current = value[index];
            char previous = value[index - 1];
            if (char.IsWhiteSpace(current) || char.IsWhiteSpace(previous))
            {
                return false;
            }

            if (char.IsUpper(current) && (char.IsLower(previous) || char.IsDigit(previous)))
            {
                return true;
            }

            if (char.IsUpper(current) &&
                char.IsUpper(previous) &&
                index + 1 < value.Length &&
                char.IsLower(value[index + 1]))
            {
                return true;
            }

            return char.IsDigit(current) && !char.IsDigit(previous);
        }

        private static string GetPromptTypeName(Type type)
        {
            Type promptType = Nullable.GetUnderlyingType(type) ?? type;
            return promptType.Name;
        }

        private static string NormalizeWhitespace(string value)
        {
            var builder = new StringBuilder();
            var space = false;
            foreach (var ch in value)
            {
                if (char.IsWhiteSpace(ch))
                {
                    if (!space)
                    {
                        builder.Append(' ');
                        space = true;
                    }
                }
                else
                {
                    builder.Append(ch);
                    space = false;
                }
            }

            return builder.ToString().Trim();
        }
    }

    /// <summary>
    /// Prompts the user for an existing file path.
    /// </summary>
    public sealed class FilePathPropertyEditor : IPropertyEditor
    {
        /// <inheritdoc />
        public bool TryEdit(PropertyEditContext context, out object value)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var prompt = context.Documentation.DisplayDescription;
            if (string.IsNullOrWhiteSpace(prompt))
            {
                prompt = "Please type or paste a filepath below:";
            }

            var summary = context.Documentation.XmlSummary;
            if (!string.IsNullOrWhiteSpace(summary))
            {
                Consoul.WriteCore(NormalizeWhitespace(summary), RenderOptions.SubnoteColor);
            }

            Consoul.WriteCore(NormalizeWhitespace(prompt), RenderOptions.SubnoteColor);
            var displayName = context.Documentation.DisplayName;
            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = context.Property.Name;
            }

            var currentValue = context.Property.GetValue(context.Model);
            var currentText = currentValue != null ? currentValue.ToString() : string.Empty;
            value = Consoul.PromptForFilepath(currentText, "Select " + displayName, checkExists: false, cancellationToken: context.CancellationToken);
            return value != null && !context.CancellationToken.IsCancellationRequested;
        }

        private static string NormalizeWhitespace(string value)
        {
            var builder = new StringBuilder();
            var space = false;
            foreach (var ch in value)
            {
                if (char.IsWhiteSpace(ch))
                {
                    if (!space)
                    {
                        builder.Append(' ');
                        space = true;
                    }
                }
                else
                {
                    builder.Append(ch);
                    space = false;
                }
            }

            return builder.ToString().Trim();
        }
    }
}
