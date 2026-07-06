using ConsoulLibrary.Color;
using System;
using System.Text;

namespace ConsoulLibrary
{
    internal static class TerminalLinkWriter
    {
        private const string Escape = "\u001b";
        private const string StringTerminator = Escape + "\\";

        public static void WriteText(string text, ColorScheme scheme, Link link)
        {
            bool writeLink = CanWrite(link);
            if (writeLink)
            {
                Consoul.WriteControl(Escape + "]8;;" + SanitizeUrl(link.Url) + StringTerminator);
            }

            Consoul.Write(text, scheme, false);

            if (writeLink)
            {
                Consoul.WriteControl(Escape + "]8;;" + StringTerminator);
            }
        }

        public static bool SameTarget(Link left, Link right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left == null || right == null)
            {
                return false;
            }

            return string.Equals(left.Url, right.Url, StringComparison.Ordinal);
        }

        private static bool CanWrite(Link link)
        {
            return link != null &&
                !string.IsNullOrWhiteSpace(link.Url) &&
                Consoul.Terminal.Capabilities.Links;
        }

        private static string SanitizeUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return string.Empty;
            }

            var builder = new StringBuilder(url.Length);
            foreach (char character in url)
            {
                if (character == '\u001b' || character == '\a' || character == '\r' || character == '\n')
                {
                    continue;
                }

                builder.Append(character);
            }

            return builder.ToString();
        }
    }
}
