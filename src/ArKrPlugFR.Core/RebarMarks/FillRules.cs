namespace ArKrPlugFR.Core.RebarMarks
{
    /// <summary>
    /// The decision behind filling bars from their field: whether a bar receives the value
    /// of its area reinforcement.
    /// </summary>
    public static class FillRules
    {
        /// <summary>
        /// Determines whether a bar receives the value.
        /// <para>
        /// A source without a value fills nothing — the tool fills, it never clears — and a
        /// bar that already carries a value is kept when the user asks to keep existing
        /// values.
        /// </para>
        /// </summary>
        /// <param name="sourceValue">Value of the area reinforcement; may be empty.</param>
        /// <param name="currentValue">Value the bar already carries; may be empty.</param>
        /// <param name="onlyEmpty">Whether a bar with a value is kept.</param>
        public static bool ShouldFill(string? sourceValue, string? currentValue, bool onlyEmpty)
        {
            if (string.IsNullOrWhiteSpace(sourceValue))
            {
                return false;
            }

            return !onlyEmpty || string.IsNullOrWhiteSpace(currentValue);
        }
    }
}
