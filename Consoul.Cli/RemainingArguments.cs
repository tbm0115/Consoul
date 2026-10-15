using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoulLibrary
{
    /// <summary>
    /// Captures command-line arguments that remain after known arguments and options are bound.
    /// </summary>
    public sealed class RemainingArguments : IRemainingArguments
    {
        private readonly string[] _raw;
        private readonly ILookup<string, string> _parsed;

        /// <summary>
        /// Initializes a new instance of the <see cref="RemainingArguments"/> class.
        /// </summary>
        public RemainingArguments() : this(new string[0])
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RemainingArguments"/> class.
        /// </summary>
        /// <param name="raw">The raw remaining arguments.</param>
        public RemainingArguments(IEnumerable<string> raw)
        {
            _raw = (raw ?? new string[0]).Select(value => value ?? string.Empty).ToArray();
            _parsed = Parse(_raw);
        }

        /// <summary>
        /// Gets an empty remaining-arguments instance.
        /// </summary>
        public static RemainingArguments Empty { get; } = new RemainingArguments();

        /// <summary>
        /// Gets the captured arguments exactly as they appeared after command parsing.
        /// </summary>
        public IReadOnlyList<string> Raw => _raw;

        /// <summary>
        /// Gets option-shaped captured arguments grouped by option name.
        /// </summary>
        public ILookup<string, string> Parsed => _parsed;

        private static ILookup<string, string> Parse(IReadOnlyList<string> raw)
        {
            var parsed = new List<KeyValuePair<string, string>>();
            for (int index = 0; index < raw.Count; index++)
            {
                string token = raw[index] ?? string.Empty;
                if (!LooksLikeOption(token))
                {
                    continue;
                }

                string name = token;
                string value = null;
                int equalsIndex = token.IndexOf('=');
                if (equalsIndex >= 0)
                {
                    name = token.Substring(0, equalsIndex);
                    value = token.Substring(equalsIndex + 1);
                }
                else if (index + 1 < raw.Count && !LooksLikeOption(raw[index + 1]))
                {
                    value = raw[++index];
                }

                parsed.Add(new KeyValuePair<string, string>(name, value));
            }

            return parsed.ToLookup(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase);
        }

        private static bool LooksLikeOption(string value)
        {
            return !string.IsNullOrEmpty(value) && value.Length > 1 && value[0] == '-';
        }
    }
}
