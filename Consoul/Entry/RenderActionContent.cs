using System;
using System.Globalization;

namespace ConsoulLibrary
{
    internal static class RenderActionContent
    {
        public static string FormatValue(object value)
        {
            return value == null
                ? string.Empty
                : Convert.ToString(value, CultureInfo.CurrentCulture) ?? string.Empty;
        }

        public static string Capture(Action render, int? width, int height)
        {
            if (render == null)
            {
                throw new ArgumentNullException(nameof(render));
            }

            return Consoul.Capture(render, PanelEntry.ResolveWidth(width), Math.Max(1, height));
        }

        public static string Capture(IConsoulRenderable renderable, int? width, int height)
        {
            if (renderable == null)
            {
                throw new ArgumentNullException(nameof(renderable));
            }

            return Capture(renderable.Render, width, height);
        }
    }
}
