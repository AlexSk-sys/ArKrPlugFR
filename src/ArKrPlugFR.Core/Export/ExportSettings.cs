namespace ArKrPlugFR.Core.Export
{
    /// <summary>
    /// The remembered state of the export: the scope and the last file.
    /// </summary>
    public sealed class ExportSettings
    {
        /// <summary>Gets or sets a value indicating whether only the selected elements are exported.</summary>
        public bool OnlySelection { get; set; }

        /// <summary>Gets or sets the path of the last file; empty when none was chosen yet.</summary>
        public string FilePath { get; set; } = string.Empty;
    }
}
