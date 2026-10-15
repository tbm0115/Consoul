using System;
using System.ComponentModel;
using System.Reflection;

namespace ConsoulLibrary
{
    internal static class CommandDescriptionMetadata
    {
        public static string FromTypes(params Type[] types)
        {
            if (types == null)
            {
                return string.Empty;
            }

            foreach (Type type in types)
            {
                string description = FromType(type);
                if (!string.IsNullOrWhiteSpace(description))
                {
                    return description;
                }
            }

            return string.Empty;
        }

        public static string FromType(Type type)
        {
            if (type == null)
            {
                return string.Empty;
            }

            DescriptionAttribute attribute = type.GetCustomAttribute<DescriptionAttribute>(true);
            return attribute == null || string.IsNullOrWhiteSpace(attribute.Description)
                ? string.Empty
                : attribute.Description;
        }
    }
}
