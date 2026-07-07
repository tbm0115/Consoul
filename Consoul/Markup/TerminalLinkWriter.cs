using ConsoulLibrary.Color;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoulLibrary
{
    internal static class TerminalLinkWriter
    {
        private const string Escape = "\u001b";
        private const string StringTerminator = Escape + "\\";
        private static readonly int[] ColorCubeLevels = { 0, 95, 135, 175, 215, 255 };

        public static void WriteText(
            string text,
            ColorScheme scheme,
            Link link,
            Decoration decoration = Decoration.None,
            RgbColor? foregroundRgb = null,
            RgbColor? backgroundRgb = null)
        {
            text = text ?? string.Empty;
            using (new ColorScope(scheme.Color, scheme.BackgroundColor))
            {
                bool writeLink = CanWrite(link);
                if (writeLink)
                {
                    Consoul.WriteControl(Escape + "]8;;" + SanitizeUrl(link.Url) + StringTerminator);
                }

                WriteStyleStart(decoration, foregroundRgb, backgroundRgb);
                Consoul.ConsoleDriver.Write(text);
                WriteStyleEnd(decoration, foregroundRgb, backgroundRgb);

                if (writeLink)
                {
                    Consoul.WriteControl(Escape + "]8;;" + StringTerminator);
                }
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

        private static void WriteStyleStart(Decoration decoration, RgbColor? foregroundRgb, RgbColor? backgroundRgb)
        {
            if (!CanWriteStyle(decoration, foregroundRgb, backgroundRgb))
            {
                return;
            }

            var codes = new List<string>();
            if ((decoration & Decoration.Bold) == Decoration.Bold)
            {
                codes.Add("1");
            }

            if ((decoration & Decoration.Dim) == Decoration.Dim)
            {
                codes.Add("2");
            }

            if ((decoration & Decoration.Italic) == Decoration.Italic)
            {
                codes.Add("3");
            }

            if ((decoration & Decoration.Underline) == Decoration.Underline)
            {
                codes.Add("4");
            }

            if ((decoration & Decoration.Blink) == Decoration.Blink)
            {
                codes.Add("5");
            }

            if ((decoration & Decoration.Reverse) == Decoration.Reverse)
            {
                codes.Add("7");
            }

            if ((decoration & Decoration.Conceal) == Decoration.Conceal)
            {
                codes.Add("8");
            }

            if ((decoration & Decoration.Strikethrough) == Decoration.Strikethrough)
            {
                codes.Add("9");
            }

            AddColorStart(codes, foreground: true, foregroundRgb);
            AddColorStart(codes, foreground: false, backgroundRgb);

            if (codes.Count > 0)
            {
                Consoul.WriteControl(Escape + "[" + string.Join(";", codes) + "m");
            }
        }

        private static void WriteStyleEnd(Decoration decoration, RgbColor? foregroundRgb, RgbColor? backgroundRgb)
        {
            if (!CanWriteStyle(decoration, foregroundRgb, backgroundRgb))
            {
                return;
            }

            var codes = new List<string>();
            if ((decoration & (Decoration.Bold | Decoration.Dim)) != Decoration.None)
            {
                codes.Add("22");
            }

            if ((decoration & Decoration.Italic) == Decoration.Italic)
            {
                codes.Add("23");
            }

            if ((decoration & Decoration.Underline) == Decoration.Underline)
            {
                codes.Add("24");
            }

            if ((decoration & Decoration.Blink) == Decoration.Blink)
            {
                codes.Add("25");
            }

            if ((decoration & Decoration.Reverse) == Decoration.Reverse)
            {
                codes.Add("27");
            }

            if ((decoration & Decoration.Conceal) == Decoration.Conceal)
            {
                codes.Add("28");
            }

            if ((decoration & Decoration.Strikethrough) == Decoration.Strikethrough)
            {
                codes.Add("29");
            }

            if (CanWriteExtendedColor(foregroundRgb))
            {
                codes.Add("39");
            }

            if (CanWriteExtendedColor(backgroundRgb))
            {
                codes.Add("49");
            }

            if (codes.Count > 0)
            {
                Consoul.WriteControl(Escape + "[" + string.Join(";", codes) + "m");
            }
        }

        private static bool CanWriteStyle(Decoration decoration, RgbColor? foregroundRgb, RgbColor? backgroundRgb)
        {
            return Consoul.Terminal.Capabilities.Ansi &&
                (decoration != Decoration.None ||
                 CanWriteExtendedColor(foregroundRgb) ||
                 CanWriteExtendedColor(backgroundRgb));
        }

        private static void AddColorStart(List<string> codes, bool foreground, RgbColor? color)
        {
            ExtendedColorMode mode = GetExtendedColorMode(color);
            if (mode == ExtendedColorMode.None)
            {
                return;
            }

            string target = foreground ? "38" : "48";
            RgbColor value = color.Value;
            if (mode == ExtendedColorMode.TrueColor)
            {
                codes.Add(target + ";2;" + value.R + ";" + value.G + ";" + value.B);
                return;
            }

            codes.Add(target + ";5;" + ToAnsi256Color(value));
        }

        private static bool CanWriteExtendedColor(RgbColor? color)
        {
            return GetExtendedColorMode(color) != ExtendedColorMode.None;
        }

        private static ExtendedColorMode GetExtendedColorMode(RgbColor? color)
        {
            if (!color.HasValue || !Consoul.Terminal.Capabilities.Ansi)
            {
                return ExtendedColorMode.None;
            }

            switch (Consoul.Terminal.Capabilities.ColorSystem)
            {
                case ColorSystem.TrueColor:
                    return ExtendedColorMode.TrueColor;
                case ColorSystem.EightBit:
                    return ExtendedColorMode.EightBit;
                default:
                    return ExtendedColorMode.None;
            }
        }

        private static int ToAnsi256Color(RgbColor color)
        {
            int redIndex = ToColorCubeIndex(color.R);
            int greenIndex = ToColorCubeIndex(color.G);
            int blueIndex = ToColorCubeIndex(color.B);
            int cubeRed = ColorCubeLevels[redIndex];
            int cubeGreen = ColorCubeLevels[greenIndex];
            int cubeBlue = ColorCubeLevels[blueIndex];
            int cubeCode = 16 + (36 * redIndex) + (6 * greenIndex) + blueIndex;
            int cubeDistance = GetDistanceSquared(color, cubeRed, cubeGreen, cubeBlue);

            int grayIndex = ToGrayIndex(color);
            int grayLevel = 8 + (10 * grayIndex);
            int grayDistance = GetDistanceSquared(color, grayLevel, grayLevel, grayLevel);

            return grayDistance < cubeDistance
                ? 232 + grayIndex
                : cubeCode;
        }

        private static int ToColorCubeIndex(byte value)
        {
            int bestIndex = 0;
            int bestDistance = int.MaxValue;
            for (int index = 0; index < ColorCubeLevels.Length; index++)
            {
                int distance = Math.Abs(value - ColorCubeLevels[index]);
                if (distance < bestDistance)
                {
                    bestIndex = index;
                    bestDistance = distance;
                }
            }

            return bestIndex;
        }

        private static int ToGrayIndex(RgbColor color)
        {
            int luminance = ((30 * color.R) + (59 * color.G) + (11 * color.B) + 50) / 100;
            return Math.Max(0, Math.Min(23, (luminance - 8 + 5) / 10));
        }

        private static int GetDistanceSquared(RgbColor color, int red, int green, int blue)
        {
            int redDelta = color.R - red;
            int greenDelta = color.G - green;
            int blueDelta = color.B - blue;
            return (redDelta * redDelta) + (greenDelta * greenDelta) + (blueDelta * blueDelta);
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

        private enum ExtendedColorMode
        {
            None,
            EightBit,
            TrueColor
        }
    }
}
