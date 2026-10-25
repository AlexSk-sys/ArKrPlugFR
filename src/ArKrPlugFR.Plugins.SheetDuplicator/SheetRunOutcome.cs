namespace ArKrPlugFR.Plugins.SheetDuplicator
{
    /// <summary>
    /// What one press of the run button did.
    /// <para>
    /// The window shows the message and picks the icon from these flags, so the
    /// decision how to word the outcome stays with the run itself.
    /// </para>
    /// </summary>
    public sealed class SheetRunOutcome
    {
        private SheetRunOutcome(bool nothingSelected, bool hasFailures, string message)
        {
            NothingSelected = nothingSelected;
            HasFailures = hasFailures;
            Message = message;
        }

        /// <summary>Gets a value indicating whether no sheet was ticked and no run took place.</summary>
        public bool NothingSelected { get; }

        /// <summary>Gets a value indicating whether at least one line of the report is a failure.</summary>
        public bool HasFailures { get; }

        /// <summary>Gets the text to show: the success line or the list of failures.</summary>
        public string Message { get; }

        /// <summary>Creates the outcome for a run that never started.</summary>
        /// <returns>The outcome that asks the user to tick a sheet.</returns>
        public static SheetRunOutcome NoneSelected()
        {
            return new SheetRunOutcome(true, false, string.Empty);
        }

        /// <summary>Creates the outcome of a finished run.</summary>
        /// <param name="message">The text to show.</param>
        /// <param name="hasFailures">Whether anything went wrong on the way.</param>
        /// <returns>The outcome.</returns>
        public static SheetRunOutcome Completed(string message, bool hasFailures)
        {
            return new SheetRunOutcome(false, hasFailures, message ?? string.Empty);
        }
    }
}
