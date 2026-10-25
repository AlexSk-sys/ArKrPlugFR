namespace ArKrPlugFR.Core.RebarMarks
{
    /// <summary>
    /// The remembered state of the tool: the scope, the parameter and the switch that
    /// protects the values the bars already carry.
    /// </summary>
    public sealed class AreaRebarSettings
    {
        /// <summary>Gets or sets a value indicating whether the whole project is the scope.</summary>
        public bool WholeProject { get; set; } = true;

        /// <summary>Gets or sets the name of the parameter that is transported to the bars.</summary>
        public string ParameterName { get; set; } = string.Empty;

        /// <summary>Gets or sets a value indicating whether a bar with a value is kept.</summary>
        public bool OnlyEmpty { get; set; } = true;
    }
}
