using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ConsoulLibrary.Color;
using ConsoulLibrary.Views.Editing;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Validates a model with dependency-light DataAnnotations-style reflection and renders a responsive summary.
    /// </summary>
    public class ValidationSummaryEntry : IRenderable
    {
        private readonly object _model;
        private readonly ValidationSummaryOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationSummaryEntry"/> class.
        /// </summary>
        /// <param name="model">The model object to validate.</param>
        /// <param name="options">Optional validation summary options.</param>
        public ValidationSummaryEntry(object model, ValidationSummaryOptions options = null)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _options = options != null ? options.Clone() : new ValidationSummaryOptions();
        }

        /// <summary>
        /// Gets the model object validated by the summary.
        /// </summary>
        public object Model => _model;

        /// <summary>
        /// Gets the options used by the validation summary.
        /// </summary>
        public ValidationSummaryOptions Options => _options;

        /// <summary>
        /// Sets the requested validation summary width.
        /// </summary>
        /// <param name="width">The requested width. The actual width is clamped to the active terminal.</param>
        /// <returns>The current <see cref="ValidationSummaryEntry"/> instance.</returns>
        public ValidationSummaryEntry Width(int? width)
        {
            _options.Width = width;
            return this;
        }

        /// <summary>
        /// Sets the title used when validation issues are present.
        /// </summary>
        /// <param name="title">The invalid model title.</param>
        /// <returns>The current <see cref="ValidationSummaryEntry"/> instance.</returns>
        public ValidationSummaryEntry InvalidTitle(string title)
        {
            _options.InvalidTitle = title;
            return this;
        }

        /// <summary>
        /// Sets the title used when a valid model is rendered.
        /// </summary>
        /// <param name="title">The valid model title.</param>
        /// <returns>The current <see cref="ValidationSummaryEntry"/> instance.</returns>
        public ValidationSummaryEntry ValidTitle(string title)
        {
            _options.ValidTitle = title;
            return this;
        }

        /// <summary>
        /// Sets the message rendered when a valid model is shown.
        /// </summary>
        /// <param name="message">The valid model message.</param>
        /// <returns>The current <see cref="ValidationSummaryEntry"/> instance.</returns>
        public ValidationSummaryEntry ValidMessage(string message)
        {
            _options.ValidMessage = message;
            return this;
        }

        /// <summary>
        /// Shows or hides the valid model message.
        /// </summary>
        /// <param name="show">Whether the valid model message should render.</param>
        /// <returns>The current <see cref="ValidationSummaryEntry"/> instance.</returns>
        public ValidationSummaryEntry ShowValidMessage(bool show = true)
        {
            _options.ShowValidMessage = show;
            return this;
        }

        /// <summary>
        /// Sets the invalid summary border and title color scheme.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <param name="backgroundColor">Optional background color.</param>
        /// <returns>The current <see cref="ValidationSummaryEntry"/> instance.</returns>
        public ValidationSummaryEntry InvalidColor(ConsoleColor color, ConsoleColor? backgroundColor = null)
        {
            _options.InvalidColor(color, backgroundColor);
            return this;
        }

        /// <summary>
        /// Sets the invalid summary border and title foreground color.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="ValidationSummaryEntry"/> instance.</returns>
        public ValidationSummaryEntry InvalidColor(SpectreColor color)
        {
            _options.InvalidColor(color);
            return this;
        }

        /// <summary>
        /// Sets the invalid summary border and title foreground color.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="ValidationSummaryEntry"/> instance.</returns>
        public ValidationSummaryEntry InvalidColor(RgbColor color)
        {
            _options.InvalidColor(color);
            return this;
        }

        /// <summary>
        /// Sets the invalid summary border and title color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="ValidationSummaryEntry"/> instance.</returns>
        public ValidationSummaryEntry InvalidColor(Style style)
        {
            _options.InvalidColor(style);
            return this;
        }

        /// <summary>
        /// Sets the invalid summary border and title color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>red</c>, <c>grey</c>, <c>#dc2626</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="ValidationSummaryEntry"/> instance.</returns>
        public ValidationSummaryEntry InvalidColor(string style)
        {
            _options.InvalidColor(style);
            return this;
        }

        /// <summary>
        /// Sets the invalid summary issue content color scheme.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <param name="backgroundColor">Optional background color.</param>
        /// <returns>The current <see cref="ValidationSummaryEntry"/> instance.</returns>
        public ValidationSummaryEntry IssueColor(ConsoleColor color, ConsoleColor? backgroundColor = null)
        {
            _options.IssueColor(color, backgroundColor);
            return this;
        }

        /// <summary>
        /// Sets the invalid summary issue content foreground color.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="ValidationSummaryEntry"/> instance.</returns>
        public ValidationSummaryEntry IssueColor(SpectreColor color)
        {
            _options.IssueColor(color);
            return this;
        }

        /// <summary>
        /// Sets the invalid summary issue content foreground color.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="ValidationSummaryEntry"/> instance.</returns>
        public ValidationSummaryEntry IssueColor(RgbColor color)
        {
            _options.IssueColor(color);
            return this;
        }

        /// <summary>
        /// Sets the invalid summary issue content color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="ValidationSummaryEntry"/> instance.</returns>
        public ValidationSummaryEntry IssueColor(Style style)
        {
            _options.IssueColor(style);
            return this;
        }

        /// <summary>
        /// Sets the invalid summary issue content color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>yellow</c>, <c>grey</c>, <c>#f59e0b</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="ValidationSummaryEntry"/> instance.</returns>
        public ValidationSummaryEntry IssueColor(string style)
        {
            _options.IssueColor(style);
            return this;
        }

        /// <summary>
        /// Sets the valid summary border and title color scheme.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <param name="backgroundColor">Optional background color.</param>
        /// <returns>The current <see cref="ValidationSummaryEntry"/> instance.</returns>
        public ValidationSummaryEntry ValidColor(ConsoleColor color, ConsoleColor? backgroundColor = null)
        {
            _options.ValidColor(color, backgroundColor);
            return this;
        }

        /// <summary>
        /// Sets the valid summary border and title foreground color.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="ValidationSummaryEntry"/> instance.</returns>
        public ValidationSummaryEntry ValidColor(SpectreColor color)
        {
            _options.ValidColor(color);
            return this;
        }

        /// <summary>
        /// Sets the valid summary border and title foreground color.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="ValidationSummaryEntry"/> instance.</returns>
        public ValidationSummaryEntry ValidColor(RgbColor color)
        {
            _options.ValidColor(color);
            return this;
        }

        /// <summary>
        /// Sets the valid summary border and title color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="ValidationSummaryEntry"/> instance.</returns>
        public ValidationSummaryEntry ValidColor(Style style)
        {
            _options.ValidColor(style);
            return this;
        }

        /// <summary>
        /// Sets the valid summary border and title color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="ValidationSummaryEntry"/> instance.</returns>
        public ValidationSummaryEntry ValidColor(string style)
        {
            _options.ValidColor(style);
            return this;
        }

        /// <summary>
        /// Sets the valid summary message color scheme.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <param name="backgroundColor">Optional background color.</param>
        /// <returns>The current <see cref="ValidationSummaryEntry"/> instance.</returns>
        public ValidationSummaryEntry ValidMessageColor(ConsoleColor color, ConsoleColor? backgroundColor = null)
        {
            _options.ValidMessageColor(color, backgroundColor);
            return this;
        }

        /// <summary>
        /// Sets the valid summary message foreground color.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="ValidationSummaryEntry"/> instance.</returns>
        public ValidationSummaryEntry ValidMessageColor(SpectreColor color)
        {
            _options.ValidMessageColor(color);
            return this;
        }

        /// <summary>
        /// Sets the valid summary message foreground color.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <returns>The current <see cref="ValidationSummaryEntry"/> instance.</returns>
        public ValidationSummaryEntry ValidMessageColor(RgbColor color)
        {
            _options.ValidMessageColor(color);
            return this;
        }

        /// <summary>
        /// Sets the valid summary message color from a reusable style object.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        /// <returns>The current <see cref="ValidationSummaryEntry"/> instance.</returns>
        public ValidationSummaryEntry ValidMessageColor(Style style)
        {
            _options.ValidMessageColor(style);
            return this;
        }

        /// <summary>
        /// Sets the valid summary message color from Spectre-style style text.
        /// </summary>
        /// <param name="style">Style text such as <c>green</c>, <c>grey</c>, <c>#22c55e</c>, or <c>white on blue</c>.</param>
        /// <returns>The current <see cref="ValidationSummaryEntry"/> instance.</returns>
        public ValidationSummaryEntry ValidMessageColor(string style)
        {
            _options.ValidMessageColor(style);
            return this;
        }

        /// <summary>
        /// Validates the model without rendering output.
        /// </summary>
        /// <returns>The validation result for the model.</returns>
        public ValidationSummaryResult Validate()
        {
            return Validate(_model, _options);
        }

        /// <summary>
        /// Validates the model and renders a responsive validation summary.
        /// </summary>
        /// <returns>The validation result for the model.</returns>
        public ValidationSummaryResult Render()
        {
            ValidationSummaryResult result = Validate();
            RenderResult(result, _options);
            return result;
        }

        void IConsoulRenderable.Render()
        {
            Render();
        }

        /// <summary>
        /// Validates a model without rendering output.
        /// </summary>
        /// <param name="model">The model object to validate.</param>
        /// <param name="options">Optional validation summary options.</param>
        /// <returns>The validation result for the model.</returns>
        public static ValidationSummaryResult Validate(object model, ValidationSummaryOptions options = null)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            ValidationSummaryOptions resolvedOptions = options != null ? options.Clone() : new ValidationSummaryOptions();
            var issues = new List<ValidationIssue>();
            PropertyInfo[] properties = GetModelProperties(model.GetType(), resolvedOptions.BindingAttr).ToArray();
            foreach (PropertyInfo property in properties)
            {
                object value = property.GetValue(model);
                string displayName = ResolvePropertyLabel(property);
                foreach (object attribute in property.GetCustomAttributes(true))
                {
                    if (!IsValidationAttribute(attribute))
                    {
                        continue;
                    }

                    string message = ValidateAttribute(attribute, value, model, property.Name, displayName);
                    if (string.IsNullOrWhiteSpace(message))
                    {
                        continue;
                    }

                    issues.Add(new ValidationIssue(
                        property.Name,
                        displayName,
                        message));
                }
            }

            AddObjectValidationIssues(model, properties, issues);
            return new ValidationSummaryResult(issues);
        }

        /// <summary>
        /// Validates and renders a temporary validation summary.
        /// </summary>
        /// <param name="model">The model object to validate.</param>
        /// <param name="options">Optional validation summary options.</param>
        /// <returns>The validation result for the model.</returns>
        public static ValidationSummaryResult Render(object model, ValidationSummaryOptions options = null)
        {
            return new ValidationSummaryEntry(model, options).Render();
        }

        private static void RenderResult(ValidationSummaryResult result, ValidationSummaryOptions options)
        {
            if (result.IsValid)
            {
                if (!options.ShowValidMessage)
                {
                    return;
                }

                CalloutEntry.Render(
                    options.ValidMessage,
                    CalloutKind.Success,
                    new CalloutOptions
                    {
                        Width = options.Width,
                        Title = options.ValidTitle,
                        SuccessScheme = options.ValidScheme,
                        SuccessDecoration = options.ValidDecoration,
                        SuccessForegroundRgb = options.ValidForegroundRgb,
                        SuccessBackgroundRgb = options.ValidBackgroundRgb,
                        ContentScheme = options.ValidMessageScheme,
                        ContentDecoration = options.ValidMessageDecoration,
                        ContentForegroundRgb = options.ValidMessageForegroundRgb,
                        ContentBackgroundRgb = options.ValidMessageBackgroundRgb
                    });
                return;
            }

            int width = PanelEntry.ResolveWidth(options.Width);
            int checklistWidth = Math.Max(1, width - 4);
            CalloutEntry.Render(
                () => Consoul.Checklist(list =>
                {
                    foreach (ValidationIssue issue in result.Issues)
                    {
                        list.Add(issue.DisplayName, ChecklistItemState.Error, issue.Message);
                    }
                }, new ChecklistOptions
                {
                    Width = checklistWidth,
                    LabelScheme = options.IssueScheme,
                    LabelDecoration = options.IssueDecoration,
                    LabelForegroundRgb = options.IssueForegroundRgb,
                    LabelBackgroundRgb = options.IssueBackgroundRgb,
                    DescriptionScheme = options.IssueScheme,
                    DescriptionDecoration = options.IssueDecoration,
                    DescriptionForegroundRgb = options.IssueForegroundRgb,
                    DescriptionBackgroundRgb = options.IssueBackgroundRgb,
                    ErrorScheme = options.InvalidScheme,
                    ErrorDecoration = options.InvalidDecoration,
                    ErrorForegroundRgb = options.InvalidForegroundRgb,
                    ErrorBackgroundRgb = options.InvalidBackgroundRgb
                }),
                CalloutKind.Error,
                new CalloutOptions
                {
                    Width = options.Width,
                    Title = options.InvalidTitle,
                    ErrorScheme = options.InvalidScheme,
                    ErrorDecoration = options.InvalidDecoration,
                    ErrorForegroundRgb = options.InvalidForegroundRgb,
                    ErrorBackgroundRgb = options.InvalidBackgroundRgb,
                    ContentScheme = options.IssueScheme,
                    ContentDecoration = options.IssueDecoration,
                    ContentForegroundRgb = options.IssueForegroundRgb,
                    ContentBackgroundRgb = options.IssueBackgroundRgb
                },
                captureWidth: checklistWidth);
        }

        private static IEnumerable<PropertyInfo> GetModelProperties(Type type, BindingFlags bindingAttr)
        {
            return type
                .GetProperties(bindingAttr)
                .Where(property =>
                    property.CanRead &&
                    property.GetIndexParameters().Length == 0 &&
                    !PropertyUiMetadata.IsIgnored(property))
                .OrderBy(PropertyUiMetadata.GetOrder);
        }

        private static bool IsValidationAttribute(object attribute)
        {
            Type type = attribute.GetType();
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

        private static void AddObjectValidationIssues(object model, IEnumerable<PropertyInfo> properties, IList<ValidationIssue> issues)
        {
            Type modelType = model.GetType();
            Type validatableInterface = modelType
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

            object validationContext = CreateValidationContext(parameters[0].ParameterType, model);
            if (validationContext == null)
            {
                return;
            }

            IEnumerable results;
            try
            {
                results = validateMethod.Invoke(model, new[] { validationContext }) as IEnumerable;
            }
            catch
            {
                return;
            }

            if (results == null)
            {
                return;
            }

            var propertyLookup = properties
                .GroupBy(property => property.Name, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

            foreach (object result in results)
            {
                if (result == null)
                {
                    continue;
                }

                string message = GetValidationResultMessage(result);
                string propertyName = GetValidationResultMemberNames(result).FirstOrDefault(name => !string.IsNullOrWhiteSpace(name)) ?? string.Empty;
                string displayName = ResolveValidationIssueDisplayName(propertyName, propertyLookup, modelType);
                issues.Add(new ValidationIssue(propertyName, displayName, message));
            }
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

        private static string GetValidationResultMessage(object validationResult)
        {
            PropertyInfo property = validationResult.GetType().GetProperty("ErrorMessage", BindingFlags.Public | BindingFlags.Instance);
            string message = property == null ? null : property.GetValue(validationResult) as string;
            return !string.IsNullOrWhiteSpace(message) ? message : "Model is invalid.";
        }

        private static IEnumerable<string> GetValidationResultMemberNames(object validationResult)
        {
            PropertyInfo property = validationResult.GetType().GetProperty("MemberNames", BindingFlags.Public | BindingFlags.Instance);
            IEnumerable values = property == null ? null : property.GetValue(validationResult) as IEnumerable;
            if (values == null)
            {
                yield break;
            }

            foreach (object value in values)
            {
                string name = value as string;
                if (!string.IsNullOrWhiteSpace(name))
                {
                    yield return name;
                }
            }
        }

        private static string ResolveValidationIssueDisplayName(string propertyName, IDictionary<string, PropertyInfo> properties, Type modelType)
        {
            if (!string.IsNullOrWhiteSpace(propertyName) && properties.TryGetValue(propertyName, out PropertyInfo property))
            {
                return ResolvePropertyLabel(property);
            }

            return !string.IsNullOrWhiteSpace(propertyName) ? SplitPascalCase(propertyName) : SplitPascalCase(modelType.Name);
        }

        private static string ValidateAttribute(object attribute, object value, object model, string memberName, string displayName)
        {
            object validationResult;
            if (TryGetValidationResult(attribute, value, model, memberName, out validationResult))
            {
                return IsValidationResultSuccess(validationResult) ? null : GetValidationResultMessage(validationResult);
            }

            return IsValidationAttributeValid(attribute, value) ? null : ResolveValidationMessage(attribute, displayName);
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
            string errorMessage = errorMessageProperty?.GetValue(attribute) as string;
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                return errorMessage;
            }

            return displayName + " is invalid.";
        }

        private static string ResolvePropertyLabel(PropertyInfo property)
        {
            var documentation = new PropertyDocumentation(property);
            if (!string.IsNullOrWhiteSpace(documentation.DisplayName))
            {
                return documentation.DisplayName;
            }

            if (!string.IsNullOrWhiteSpace(documentation.DisplayDescription))
            {
                return documentation.DisplayDescription;
            }

            return SplitPascalCase(property.Name);
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

            if (char.IsDigit(current) && char.IsLetter(previous))
            {
                return true;
            }

            return char.IsLetter(current) && char.IsDigit(previous);
        }
    }
}
