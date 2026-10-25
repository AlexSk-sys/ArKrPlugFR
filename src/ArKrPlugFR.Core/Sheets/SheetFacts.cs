namespace ArKrPlugFR.Core.Sheets
{
    /// <summary>
    /// The facts one sheet contributes to the decision whether to duplicate it.
    /// <para>
    /// The facts are read from the document once, when the window is built; the
    /// rules that consume them live here, away from the modelling software, so
    /// they can be tested on their own.
    /// </para>
    /// </summary>
    public sealed record SheetFacts
    {
        /// <summary>Creates the facts of one sheet.</summary>
        /// <param name="id">Element id of the sheet.</param>
        /// <param name="number">Sheet number as shown in the project browser.</param>
        /// <param name="name">Sheet name.</param>
        /// <param name="isPlaceholder">Whether the sheet is a sheet-list placeholder row.</param>
        /// <param name="isAlive">Whether the element still resolves in the document.</param>
        public SheetFacts(long id, string? number, string? name, bool isPlaceholder, bool isAlive)
        {
            Id = id;
            Number = number ?? string.Empty;
            Name = name ?? string.Empty;
            IsPlaceholder = isPlaceholder;
            IsAlive = isAlive;
        }

        /// <summary>Gets the element id of the sheet.</summary>
        public long Id { get; }

        /// <summary>Gets the sheet number; unique per document.</summary>
        public string Number { get; }

        /// <summary>Gets the sheet name; not unique per document.</summary>
        public string Name { get; }

        /// <summary>Gets a value indicating whether the sheet is a sheet-list placeholder.</summary>
        public bool IsPlaceholder { get; }

        /// <summary>Gets a value indicating whether the sheet still exists in the document.</summary>
        public bool IsAlive { get; }
    }
}
