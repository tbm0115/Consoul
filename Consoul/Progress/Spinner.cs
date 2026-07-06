using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoulLibrary
{
    /// <summary>
    /// Represents a reusable set of spinner frames for status and spinner scopes.
    /// </summary>
    public sealed class Spinner
    {
        private readonly string[] _frames;

        /// <summary>
        /// Initializes a new instance of the <see cref="Spinner"/> class.
        /// </summary>
        /// <param name="frames">The frames rendered by successive refreshes.</param>
        public Spinner(params string[] frames)
            : this(null, frames)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Spinner"/> class.
        /// </summary>
        /// <param name="name">Optional friendly name for diagnostics and documentation.</param>
        /// <param name="frames">The frames rendered by successive refreshes.</param>
        public Spinner(string name, params string[] frames)
        {
            Name = name ?? string.Empty;
            _frames = frames == null || frames.Length == 0
                ? new[] { "|", "/", "-", "\\" }
                : frames.Select(frame => frame ?? string.Empty).ToArray();
        }

        /// <summary>
        /// Gets the optional friendly spinner name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the spinner frames.
        /// </summary>
        public IReadOnlyList<string> Frames => _frames;

        internal string[] ToFrameArray()
        {
            return _frames.ToArray();
        }

        /// <summary>
        /// Provides common spinner frame sets for migration-friendly snippets.
        /// </summary>
        public static class Known
        {
            /// <summary>
            /// Gets the default line spinner.
            /// </summary>
            public static Spinner Default { get; } = new Spinner("Default", "|", "/", "-", "\\");

            /// <summary>
            /// Gets a simple line spinner.
            /// </summary>
            public static Spinner Line { get; } = new Spinner("Line", "|", "/", "-", "\\");

            /// <summary>
            /// Gets an alternate simple line spinner.
            /// </summary>
            public static Spinner Line2 { get; } = new Spinner("Line2", "-", "\\", "|", "/");

            /// <summary>
            /// Gets a pipe spinner alias used by common Spectre-style snippets.
            /// </summary>
            public static Spinner Pipe { get; } = Line;

            /// <summary>
            /// Gets a compact dots spinner.
            /// </summary>
            public static Spinner Dots { get; } = new Spinner("Dots", ".", "..", "...");

            /// <summary>
            /// Gets a compact dots spinner alias used by common Spectre-style snippets.
            /// </summary>
            public static Spinner SimpleDots { get; } = Dots;

            /// <summary>
            /// Gets a scrolling dots spinner.
            /// </summary>
            public static Spinner SimpleDotsScrolling { get; } = new Spinner("SimpleDotsScrolling", ".  ", ".. ", "...", " ..", "  .", "   ");

            /// <summary>
            /// Gets an alternate scrolling dots spinner.
            /// </summary>
            public static Spinner Dots2 { get; } = new Spinner("Dots2", ".  ", ".. ", "...", " ..", "  .");

            /// <summary>
            /// Gets a pulsing dots spinner.
            /// </summary>
            public static Spinner Dots3 { get; } = new Spinner("Dots3", ".  ", ".. ", "...", " ..", "  .", "   ");

            /// <summary>
            /// Gets a compact pulsing dots spinner.
            /// </summary>
            public static Spinner Dots4 { get; } = new Spinner("Dots4", ".", "o", "O", "o");

            /// <summary>
            /// Gets a horizontal dots spinner.
            /// </summary>
            public static Spinner Dots5 { get; } = new Spinner("Dots5", ".  ", " . ", "  .", " . ");

            /// <summary>
            /// Gets a growing dots spinner.
            /// </summary>
            public static Spinner Dots6 { get; } = new Spinner("Dots6", ".", "..", "...", "....", "...");

            /// <summary>
            /// Gets a fading dots spinner.
            /// </summary>
            public static Spinner Dots7 { get; } = new Spinner("Dots7", "   ", ".  ", ".. ", "...", " ..", "  .");

            /// <summary>
            /// Gets a tight dots spinner.
            /// </summary>
            public static Spinner Dots8 { get; } = new Spinner("Dots8", ".", ":", "*", ":");

            /// <summary>
            /// Gets a wide dots spinner.
            /// </summary>
            public static Spinner Dots9 { get; } = new Spinner("Dots9", "[.  ]", "[.. ]", "[...]", "[ ..]", "[  .]");

            /// <summary>
            /// Gets a bracketed dots spinner.
            /// </summary>
            public static Spinner Dots10 { get; } = new Spinner("Dots10", "(.)", "(..)", "(...)", "(..)");

            /// <summary>
            /// Gets an alternating dots spinner.
            /// </summary>
            public static Spinner Dots11 { get; } = new Spinner("Dots11", ". .", ".. ", " ..", ". .");

            /// <summary>
            /// Gets a second alternating dots spinner.
            /// </summary>
            public static Spinner Dots12 { get; } = new Spinner("Dots12", ".--", "-.-", "--.", "-.-");

            /// <summary>
            /// Gets a star spinner.
            /// </summary>
            public static Spinner Star { get; } = new Spinner("Star", "*", "+", "x", "+");

            /// <summary>
            /// Gets an alternate star spinner.
            /// </summary>
            public static Spinner Star2 { get; } = new Spinner("Star2", "+", "x", "*", "x");

            /// <summary>
            /// Gets a flip-style ASCII spinner.
            /// </summary>
            public static Spinner Flip { get; } = new Spinner("Flip", "_", "_", "-", "`", "'", "-", "_", "_");

            /// <summary>
            /// Gets a compact menu-style spinner.
            /// </summary>
            public static Spinner Hamburger { get; } = new Spinner("Hamburger", "-", "=", "#", "=");

            /// <summary>
            /// Gets a vertical growth spinner.
            /// </summary>
            public static Spinner GrowVertical { get; } = new Spinner("GrowVertical", ".", ":", "|", ":", ".");

            /// <summary>
            /// Gets a horizontal growth spinner.
            /// </summary>
            public static Spinner GrowHorizontal { get; } = new Spinner("GrowHorizontal", ".", "-", "=", "-", ".");

            /// <summary>
            /// Gets a balloon-style spinner.
            /// </summary>
            public static Spinner Balloon { get; } = new Spinner("Balloon", ".", "o", "O", "o");

            /// <summary>
            /// Gets an alternate balloon-style spinner.
            /// </summary>
            public static Spinner Balloon2 { get; } = new Spinner("Balloon2", ".", "o", "@", "o");

            /// <summary>
            /// Gets a noise-style spinner.
            /// </summary>
            public static Spinner Noise { get; } = new Spinner("Noise", ".", ":", "*", "#", "*", ":");

            /// <summary>
            /// Gets a bouncing marker spinner.
            /// </summary>
            public static Spinner Bounce { get; } = new Spinner("Bounce", "(_)", "(-)", "(_)");

            /// <summary>
            /// Gets a boxed bouncing marker spinner.
            /// </summary>
            public static Spinner BoxBounce { get; } = new Spinner("BoxBounce", "[=   ]", "[ =  ]", "[  = ]", "[   =]", "[  = ]", "[ =  ]");

            /// <summary>
            /// Gets an alternate boxed bouncing marker spinner.
            /// </summary>
            public static Spinner BoxBounce2 { get; } = new Spinner("BoxBounce2", "[#   ]", "[ #  ]", "[  # ]", "[   #]", "[  # ]", "[ #  ]");

            /// <summary>
            /// Gets a bouncing bar spinner.
            /// </summary>
            public static Spinner BouncingBar { get; } = new Spinner("BouncingBar", "[=   ]", "[ =  ]", "[  = ]", "[   =]", "[  = ]", "[ =  ]");

            /// <summary>
            /// Gets a bouncing ball spinner.
            /// </summary>
            public static Spinner BouncingBall { get; } = new Spinner("BouncingBall", "(o   )", "( o  )", "(  o )", "(   o)", "(  o )", "( o  )");

            /// <summary>
            /// Gets a directional arrow spinner.
            /// </summary>
            public static Spinner Arrow { get; } = new Spinner("Arrow", "<-", "<\\", "^", "/>", "->", "\\>", "v", "</");

            /// <summary>
            /// Gets an alternate directional arrow spinner.
            /// </summary>
            public static Spinner Arrow2 { get; } = new Spinner("Arrow2", "<", "^", ">", "v");

            /// <summary>
            /// Gets a compact directional arrow spinner.
            /// </summary>
            public static Spinner Arrow3 { get; } = new Spinner("Arrow3", "<", "/", ">", "\\");

            /// <summary>
            /// Gets a triangle-style spinner.
            /// </summary>
            public static Spinner Triangle { get; } = new Spinner("Triangle", "/\\", "|>", "\\/", "<|");

            /// <summary>
            /// Gets an arc-style spinner.
            /// </summary>
            public static Spinner Arc { get; } = new Spinner("Arc", "(", ")", "(", ")");

            /// <summary>
            /// Gets a circle-style spinner.
            /// </summary>
            public static Spinner Circle { get; } = new Spinner("Circle", "o", "O", "0", "O");

            /// <summary>
            /// Gets a square-corner spinner.
            /// </summary>
            public static Spinner SquareCorners { get; } = new Spinner("SquareCorners", "[   ]", "[=  ]", "[== ]", "[===]", "[ ==]", "[  =]");

            /// <summary>
            /// Gets a circle-quarters spinner.
            /// </summary>
            public static Spinner CircleQuarters { get; } = new Spinner("CircleQuarters", "o", "O", "0", "O");

            /// <summary>
            /// Gets a circle-halves spinner.
            /// </summary>
            public static Spinner CircleHalves { get; } = new Spinner("CircleHalves", "o", "0", "o", "0");

            /// <summary>
            /// Gets a squish-style spinner.
            /// </summary>
            public static Spinner Squish { get; } = new Spinner("Squish", "====", "-===", "--==", "---=", "----", "=---", "==--", "===-");

            /// <summary>
            /// Gets a toggle-style spinner.
            /// </summary>
            public static Spinner Toggle { get; } = new Spinner("Toggle", "[ ]", "[x]");

            /// <summary>
            /// Gets an alternate toggle-style spinner.
            /// </summary>
            public static Spinner Toggle2 { get; } = new Spinner("Toggle2", "( )", "(*)");

            /// <summary>
            /// Gets a third toggle-style spinner.
            /// </summary>
            public static Spinner Toggle3 { get; } = new Spinner("Toggle3", "[ ]", "[-]", "[=]", "[-]");

            /// <summary>
            /// Gets a fourth toggle-style spinner.
            /// </summary>
            public static Spinner Toggle4 { get; } = new Spinner("Toggle4", "< >", "<->", "<=>", "<->");

            /// <summary>
            /// Gets a fifth toggle-style spinner.
            /// </summary>
            public static Spinner Toggle5 { get; } = new Spinner("Toggle5", "off", "on ");

            /// <summary>
            /// Gets a sixth toggle-style spinner.
            /// </summary>
            public static Spinner Toggle6 { get; } = new Spinner("Toggle6", "no ", "yes");

            /// <summary>
            /// Gets a seventh toggle-style spinner.
            /// </summary>
            public static Spinner Toggle7 { get; } = new Spinner("Toggle7", "[_]", "[^]");

            /// <summary>
            /// Gets an eighth toggle-style spinner.
            /// </summary>
            public static Spinner Toggle8 { get; } = new Spinner("Toggle8", "[.]", "[o]", "[O]", "[o]");

            /// <summary>
            /// Gets a ninth toggle-style spinner.
            /// </summary>
            public static Spinner Toggle9 { get; } = new Spinner("Toggle9", "(.)", "(o)", "(O)", "(o)");

            /// <summary>
            /// Gets a tenth toggle-style spinner.
            /// </summary>
            public static Spinner Toggle10 { get; } = new Spinner("Toggle10", "-  ", "-- ", "---", " --", "  -");

            /// <summary>
            /// Gets an eleventh toggle-style spinner.
            /// </summary>
            public static Spinner Toggle11 { get; } = new Spinner("Toggle11", "*  ", "** ", "***", " **", "  *");

            /// <summary>
            /// Gets a twelfth toggle-style spinner.
            /// </summary>
            public static Spinner Toggle12 { get; } = new Spinner("Toggle12", "0", "1");

            /// <summary>
            /// Gets a thirteenth toggle-style spinner.
            /// </summary>
            public static Spinner Toggle13 { get; } = new Spinner("Toggle13", "?", "!", "?");

            /// <summary>
            /// Gets a smiley-style spinner.
            /// </summary>
            public static Spinner Smiley { get; } = new Spinner("Smiley", ":)", ":D", ":)", ":|");

            /// <summary>
            /// Gets a monkey-style spinner.
            /// </summary>
            public static Spinner Monkey { get; } = new Spinner("Monkey", "o_o", "-_-", "O_O", "-_-");

            /// <summary>
            /// Gets a hearts-style spinner.
            /// </summary>
            public static Spinner Hearts { get; } = new Spinner("Hearts", "<3", "</3", "<3");

            /// <summary>
            /// Gets a clock-style spinner.
            /// </summary>
            public static Spinner Clock { get; } = new Spinner("Clock", "12", "3 ", "6 ", "9 ");

            /// <summary>
            /// Gets an earth-style spinner.
            /// </summary>
            public static Spinner Earth { get; } = new Spinner("Earth", "E", "W", "N", "S");

            /// <summary>
            /// Gets a moon-style spinner.
            /// </summary>
            public static Spinner Moon { get; } = new Spinner("Moon", "new", "wax", "full", "wan");

            /// <summary>
            /// Gets a runner-style spinner.
            /// </summary>
            public static Spinner Runner { get; } = new Spinner("Runner", "run ", "run.", "run:", "run'");

            /// <summary>
            /// Gets a pong-style spinner.
            /// </summary>
            public static Spinner Pong { get; } = new Spinner("Pong", "[o   ]", "[ o  ]", "[  o ]", "[   o]", "[  o ]", "[ o  ]");

            /// <summary>
            /// Gets a shark-style spinner.
            /// </summary>
            public static Spinner Shark { get; } = new Spinner("Shark", "><>", " <><", "  ><", " <><");

            /// <summary>
            /// Gets a dqpb-style spinner.
            /// </summary>
            public static Spinner Dqpb { get; } = new Spinner("Dqpb", "d", "q", "p", "b");

            /// <summary>
            /// Gets a weather-style spinner.
            /// </summary>
            public static Spinner Weather { get; } = new Spinner("Weather", "sun", "cld", "rny", "cld");

            /// <summary>
            /// Gets a Christmas-style spinner.
            /// </summary>
            public static Spinner Christmas { get; } = new Spinner("Christmas", "*", "+", "x", "+");

            /// <summary>
            /// Gets a grenade-style spinner.
            /// </summary>
            public static Spinner Grenade { get; } = new Spinner("Grenade", "o", "O", "@", "*");

            /// <summary>
            /// Gets a point-style spinner.
            /// </summary>
            public static Spinner Point { get; } = new Spinner("Point", "<", "^", ">", "v");

            /// <summary>
            /// Gets a layer-style spinner.
            /// </summary>
            public static Spinner Layer { get; } = new Spinner("Layer", "-", "=", "#", "=");

            /// <summary>
            /// Gets a beta-wave-style spinner.
            /// </summary>
            public static Spinner BetaWave { get; } = new Spinner("BetaWave", "~   ", "~~  ", "~~~ ", "~~~~", " ~~~", "  ~~", "   ~");
        }
    }
}
