using System;
using System.Globalization;
namespace Alexander.CharacterSheet.Core
{
    /// <summary>Original display policy. Inputs are supplied values; this class knows no game mechanics.</summary>
    public static class NumberFormatting
    {
        public const string Unavailable = "Unavailable";
        static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;
        public static bool IsFinite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
        public static string Display(double? value)
        {
            if (!value.HasValue || !IsFinite(value.Value)) return Unavailable;
            double number = value.Value, magnitude = Math.Abs(number);
            if (number == 0) return "0";
            if (magnitude < .01 || magnitude >= 1e15) return number.ToString("0.###e+0", Invariant);
            if (magnitude < 1e6) return number.ToString("#,##0.##", Invariant);
            string[] suffixes = { "M", "B", "T" };
            double divisor = magnitude >= 1e12 ? 1e12 : magnitude >= 1e9 ? 1e9 : 1e6;
            int index = divisor == 1e12 ? 2 : divisor == 1e9 ? 1 : 0;
            // Carry at rounded suffix boundaries rather than showing 1000M or 1000B.
            if (Math.Round(magnitude / divisor, 2, MidpointRounding.AwayFromZero) >= 1000 && index < 2)
            { divisor *= 1000; index++; }
            return (number / divisor).ToString("0.##", Invariant) + suffixes[index];
        }
        public static string Exact(double? value)
        {
            if (!value.HasValue || !IsFinite(value.Value)) return Unavailable;
            if (value.Value == 0) return "0";
            string text = value.Value.ToString("R", Invariant);
            if (text.IndexOf('E') >= 0) return text.Replace('E', 'e');
            int point = text.IndexOf('.');
            string integer = point < 0 ? text : text.Substring(0, point);
            string fraction = point < 0 ? "" : text.Substring(point);
            bool negative = integer.StartsWith("-", StringComparison.Ordinal);
            string digits = negative ? integer.Substring(1) : integer;
            for (int i = digits.Length - 3; i > 0; i -= 3) digits = digits.Insert(i, ",");
            return (negative ? "-" : "") + digits + fraction;
        }
        public static string Signed(double? value)
        {
            string result = Display(value);
            return value.HasValue && IsFinite(value.Value) && value.Value > 0 ? "+" + result : result;
        }
        public static string Count(long? value) => value.HasValue && value.Value >= 0 ? value.Value.ToString("#,##0", Invariant) : Unavailable;
    }
}
