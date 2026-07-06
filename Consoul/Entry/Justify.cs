namespace ConsoulLibrary
{
    /// <summary>
    /// Describes horizontal justification for Spectre-style builder APIs.
    /// </summary>
    public enum Justify
    {
        /// <summary>
        /// Aligns content to the left side.
        /// </summary>
        Left,

        /// <summary>
        /// Centers content.
        /// </summary>
        Center,

        /// <summary>
        /// Aligns content to the right side.
        /// </summary>
        Right
    }

    internal static class JustifyMap
    {
        public static TextBlockAlignment ToTextBlockAlignment(Justify justify)
        {
            switch (justify)
            {
                case Justify.Center:
                    return TextBlockAlignment.Center;
                case Justify.Right:
                    return TextBlockAlignment.Right;
                default:
                    return TextBlockAlignment.Left;
            }
        }

        public static RuleTitleAlignment ToRuleTitleAlignment(Justify justify)
        {
            switch (justify)
            {
                case Justify.Center:
                    return RuleTitleAlignment.Center;
                case Justify.Right:
                    return RuleTitleAlignment.Right;
                default:
                    return RuleTitleAlignment.Left;
            }
        }

        public static TableCellAlignment ToTableCellAlignment(Justify justify)
        {
            switch (justify)
            {
                case Justify.Center:
                    return TableCellAlignment.Center;
                case Justify.Right:
                    return TableCellAlignment.Right;
                default:
                    return TableCellAlignment.Left;
            }
        }

        public static Justify FromRuleTitleAlignment(RuleTitleAlignment alignment)
        {
            switch (alignment)
            {
                case RuleTitleAlignment.Center:
                    return Justify.Center;
                case RuleTitleAlignment.Right:
                    return Justify.Right;
                default:
                    return Justify.Left;
            }
        }
    }
}
