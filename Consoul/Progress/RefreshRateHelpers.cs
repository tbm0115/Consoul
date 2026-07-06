using System;

namespace ConsoulLibrary
{
    internal static class RefreshRateHelpers
    {
        public static int Normalize(int milliseconds)
        {
            return Math.Max(0, milliseconds);
        }

        public static int Normalize(TimeSpan interval)
        {
            if (interval <= TimeSpan.Zero)
            {
                return 0;
            }

            if (interval.TotalMilliseconds >= int.MaxValue)
            {
                return int.MaxValue;
            }

            return Math.Max(0, (int)Math.Round(interval.TotalMilliseconds, MidpointRounding.AwayFromZero));
        }
    }
}
