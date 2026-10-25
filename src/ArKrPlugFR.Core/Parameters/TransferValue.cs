using System;

namespace ArKrPlugFR.Core.Parameters
{
    /// <summary>
    /// The value one element hands to another.
    /// <para>
    /// A transfer carries text: a value is read as text, trimmed and written as text, and
    /// an empty value is nothing to transfer rather than an instruction to clear the
    /// target.
    /// </para>
    /// </summary>
    public static class TransferValue
    {
        /// <summary>Trims a value and treats an empty result as nothing to transfer.</summary>
        /// <param name="value">Value read from the source element.</param>
        /// <returns>The trimmed value, or <c>null</c> when there is nothing to transfer.</returns>
        public static string? Normalize(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value!.Trim();
        }
    }
}
