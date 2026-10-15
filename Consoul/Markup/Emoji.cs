using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoulLibrary
{
    /// <summary>
    /// Resolves Spectre-style emoji shortcodes such as <c>:check_mark:</c> for markup and reusable output.
    /// </summary>
    public static class Emoji
    {
        private static readonly Dictionary<string, string> Shortcodes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "check_mark", Known.CheckMark },
            { "check_mark_button", Known.CheckMarkButton },
            { "heavy_check_mark", Known.CheckMark },
            { "white_check_mark", Known.CheckMarkButton },
            { "x", Known.CrossMark },
            { "cross_mark", Known.CrossMark },
            { "cross_mark_button", Known.CrossMarkButton },
            { "heavy_multiplication_x", Known.CrossMark },
            { "warning", Known.Warning },
            { "information", Known.Information },
            { "information_source", Known.Information },
            { "rocket", Known.Rocket },
            { "hourglass_done", Known.HourglassDone },
            { "hourglass_not_done", Known.HourglassNotDone },
            { "hourglass", Known.HourglassNotDone },
            { "stop_sign", Known.StopSign },
            { "prohibited", Known.Prohibited },
            { "red_circle", Known.RedCircle },
            { "green_circle", Known.GreenCircle },
            { "yellow_circle", Known.YellowCircle },
            { "blue_circle", Known.BlueCircle },
            { "large_green_circle", Known.GreenCircle },
            { "large_blue_circle", Known.BlueCircle },
            { "black_circle", Known.BlackCircle },
            { "white_circle", Known.WhiteCircle },
            { "sparkles", Known.Sparkles },
            { "high_voltage", Known.HighVoltage },
            { "zap", Known.HighVoltage },
            { "fire", Known.Fire },
            { "gear", Known.Gear },
            { "wrench", Known.Wrench },
            { "hammer_and_wrench", Known.HammerAndWrench },
            { "clipboard", Known.Clipboard },
            { "memo", Known.Memo },
            { "page_facing_up", Known.PageFacingUp },
            { "bar_chart", Known.BarChart },
            { "chart_increasing", Known.ChartIncreasing },
            { "chart_decreasing", Known.ChartDecreasing },
            { "package", Known.Package },
            { "locked", Known.Locked },
            { "unlocked", Known.Unlocked },
            { "key", Known.Key },
            { "bell", Known.Bell },
            { "muted_speaker", Known.MutedSpeaker },
            { "magnifying_glass_tilted_left", Known.Search },
            { "mag", Known.Search },
            { "link", Known.Link },
            { "back_arrow", Known.BackArrow },
            { "up_arrow", Known.UpArrow },
            { "down_arrow", Known.DownArrow },
            { "left_arrow", Known.LeftArrow },
            { "right_arrow", Known.RightArrow },
            { "fast_forward_button", Known.FastForward },
            { "fast_reverse_button", Known.FastReverse }
        };

        /// <summary>
        /// Gets the known emoji shortcode map.
        /// </summary>
        public static IReadOnlyDictionary<string, string> ShortcodeMap => Shortcodes;

        /// <summary>
        /// Gets the replacement text for a shortcode.
        /// </summary>
        /// <param name="name">The shortcode name with or without surrounding colons.</param>
        /// <returns>The emoji text.</returns>
        public static string Get(string name)
        {
            string emoji;
            if (!TryGet(name, out emoji))
            {
                throw new ArgumentException("Emoji shortcode is not known.", nameof(name));
            }

            return emoji;
        }

        /// <summary>
        /// Tries to get the replacement text for a shortcode.
        /// </summary>
        /// <param name="name">The shortcode name with or without surrounding colons.</param>
        /// <param name="emoji">The emoji text when found.</param>
        /// <returns><c>true</c> when the shortcode is known; otherwise, <c>false</c>.</returns>
        public static bool TryGet(string name, out string emoji)
        {
            return Shortcodes.TryGetValue(NormalizeName(name), out emoji);
        }

        /// <summary>
        /// Replaces known <c>:name:</c> shortcodes in text.
        /// </summary>
        /// <param name="text">The text that may contain emoji shortcodes.</param>
        /// <returns>Text with known shortcodes replaced and unknown shortcodes preserved.</returns>
        public static string Replace(string text)
        {
            if (string.IsNullOrEmpty(text) || text.IndexOf(':') < 0)
            {
                return text ?? string.Empty;
            }

            var builder = new StringBuilder(text.Length);
            int index = 0;
            while (index < text.Length)
            {
                char current = text[index];
                if (current != ':')
                {
                    builder.Append(current);
                    index++;
                    continue;
                }

                int closeIndex = text.IndexOf(':', index + 1);
                if (closeIndex <= index + 1)
                {
                    builder.Append(current);
                    index++;
                    continue;
                }

                string name = text.Substring(index + 1, closeIndex - index - 1);
                string emoji;
                if (IsShortcodeName(name) && TryGet(name, out emoji))
                {
                    builder.Append(emoji);
                    index = closeIndex + 1;
                    continue;
                }

                builder.Append(current);
                index++;
            }

            return builder.ToString();
        }

        private static string NormalizeName(string name)
        {
            name = (name ?? string.Empty).Trim();
            if (name.Length >= 2 && name[0] == ':' && name[name.Length - 1] == ':')
            {
                name = name.Substring(1, name.Length - 2);
            }

            return name.Trim().Replace('-', '_');
        }

        private static bool IsShortcodeName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            for (int index = 0; index < name.Length; index++)
            {
                char character = name[index];
                if (!(char.IsLetterOrDigit(character) || character == '_' || character == '-'))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Provides common emoji values used by migration-friendly markup snippets.
        /// </summary>
        public static class Known
        {
            /// <summary>Gets a check mark.</summary>
            public const string CheckMark = "\u2714";

            /// <summary>Gets a check mark button.</summary>
            public const string CheckMarkButton = "\u2705";

            /// <summary>Gets a cross mark.</summary>
            public const string CrossMark = "\u274C";

            /// <summary>Gets a cross mark button.</summary>
            public const string CrossMarkButton = "\u274E";

            /// <summary>Gets a warning sign.</summary>
            public const string Warning = "\u26A0\uFE0F";

            /// <summary>Gets an information sign.</summary>
            public const string Information = "\u2139\uFE0F";

            /// <summary>Gets a rocket.</summary>
            public const string Rocket = "\uD83D\uDE80";

            /// <summary>Gets an hourglass done symbol.</summary>
            public const string HourglassDone = "\u231B";

            /// <summary>Gets an hourglass not done symbol.</summary>
            public const string HourglassNotDone = "\u23F3";

            /// <summary>Gets a stop sign.</summary>
            public const string StopSign = "\uD83D\uDED1";

            /// <summary>Gets a prohibited sign.</summary>
            public const string Prohibited = "\uD83D\uDEAB";

            /// <summary>Gets a red circle.</summary>
            public const string RedCircle = "\uD83D\uDD34";

            /// <summary>Gets a green circle.</summary>
            public const string GreenCircle = "\uD83D\uDFE2";

            /// <summary>Gets a yellow circle.</summary>
            public const string YellowCircle = "\uD83D\uDFE1";

            /// <summary>Gets a blue circle.</summary>
            public const string BlueCircle = "\uD83D\uDD35";

            /// <summary>Gets a black circle.</summary>
            public const string BlackCircle = "\u26AB";

            /// <summary>Gets a white circle.</summary>
            public const string WhiteCircle = "\u26AA";

            /// <summary>Gets sparkles.</summary>
            public const string Sparkles = "\u2728";

            /// <summary>Gets a high voltage sign.</summary>
            public const string HighVoltage = "\u26A1";

            /// <summary>Gets a fire symbol.</summary>
            public const string Fire = "\uD83D\uDD25";

            /// <summary>Gets a gear.</summary>
            public const string Gear = "\u2699\uFE0F";

            /// <summary>Gets a wrench.</summary>
            public const string Wrench = "\uD83D\uDD27";

            /// <summary>Gets a hammer and wrench.</summary>
            public const string HammerAndWrench = "\uD83D\uDEE0\uFE0F";

            /// <summary>Gets a clipboard.</summary>
            public const string Clipboard = "\uD83D\uDCCB";

            /// <summary>Gets a memo.</summary>
            public const string Memo = "\uD83D\uDCDD";

            /// <summary>Gets a page facing up.</summary>
            public const string PageFacingUp = "\uD83D\uDCC4";

            /// <summary>Gets a bar chart.</summary>
            public const string BarChart = "\uD83D\uDCCA";

            /// <summary>Gets an increasing chart.</summary>
            public const string ChartIncreasing = "\uD83D\uDCC8";

            /// <summary>Gets a decreasing chart.</summary>
            public const string ChartDecreasing = "\uD83D\uDCC9";

            /// <summary>Gets a package.</summary>
            public const string Package = "\uD83D\uDCE6";

            /// <summary>Gets a locked symbol.</summary>
            public const string Locked = "\uD83D\uDD12";

            /// <summary>Gets an unlocked symbol.</summary>
            public const string Unlocked = "\uD83D\uDD13";

            /// <summary>Gets a key.</summary>
            public const string Key = "\uD83D\uDD11";

            /// <summary>Gets a bell.</summary>
            public const string Bell = "\uD83D\uDD14";

            /// <summary>Gets a muted speaker.</summary>
            public const string MutedSpeaker = "\uD83D\uDD07";

            /// <summary>Gets a search symbol.</summary>
            public const string Search = "\uD83D\uDD0D";

            /// <summary>Gets a link symbol.</summary>
            public const string Link = "\uD83D\uDD17";

            /// <summary>Gets a back arrow.</summary>
            public const string BackArrow = "\uD83D\uDD19";

            /// <summary>Gets an up arrow.</summary>
            public const string UpArrow = "\u2B06";

            /// <summary>Gets a down arrow.</summary>
            public const string DownArrow = "\u2B07";

            /// <summary>Gets a left arrow.</summary>
            public const string LeftArrow = "\u2B05";

            /// <summary>Gets a right arrow.</summary>
            public const string RightArrow = "\u27A1";

            /// <summary>Gets a fast forward symbol.</summary>
            public const string FastForward = "\u23E9";

            /// <summary>Gets a fast reverse symbol.</summary>
            public const string FastReverse = "\u23EA";
        }
    }
}
