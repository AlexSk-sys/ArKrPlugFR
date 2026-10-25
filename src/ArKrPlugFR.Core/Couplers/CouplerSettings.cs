namespace ArKrPlugFR.Core.Couplers
{
    /// <summary>
    /// The remembered state of the tool: the tolerance of the search and the names of the
    /// two parameters a coupler receives.
    /// <para>
    /// The default parameter names are the usual names of the shared parameters that carry
    /// the mark and the category of the host; a project that named them differently keeps
    /// its names, because the fields are editable and remembered.
    /// </para>
    /// </summary>
    public sealed class CouplerSettings
    {
        /// <summary>Name of the parameter that receives the mark of the host.</summary>
        public const string DefaultMarkParameter = "ADSK_Метка основы";

        /// <summary>Name of the parameter that receives the category of the host.</summary>
        public const string DefaultCategoryParameter = "ADSK_Категория основы";

        /// <summary>Gets or sets the tolerance of the search, in millimetres.</summary>
        public double ToleranceMillimeters { get; set; } = CouplerRules.DefaultToleranceMillimeters;

        /// <summary>Gets or sets the name of the parameter that receives the mark of the host.</summary>
        public string MarkParameter { get; set; } = DefaultMarkParameter;

        /// <summary>Gets or sets the name of the parameter that receives the category of the host.</summary>
        public string CategoryParameter { get; set; } = DefaultCategoryParameter;
    }
}
