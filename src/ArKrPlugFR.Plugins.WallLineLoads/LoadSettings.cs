namespace ArKrPlugFR.Plugins.WallLineLoads
{
    /// <summary>
    /// The options of the tool, remembered between sessions as one JSON file in the
    /// user's settings folder.
    /// </summary>
    public sealed class LoadSettings
    {
        /// <summary>Gets or sets the scope of the run: <c>selected</c>, <c>view</c> or <c>workset</c>.</summary>
        public string Scope { get; set; } = "view";

        /// <summary>Gets or sets the name of the workset the scope reads.</summary>
        public string WorksetName { get; set; } = string.Empty;

        /// <summary>Gets or sets the name of the load case the loads are attached to.</summary>
        public string CaseName { get; set; } = "Постоянная";

        /// <summary>Gets or sets the force per metre along the vertical axis, in kilonewtons per metre.</summary>
        public double Force { get; set; } = 1.5;

        /// <summary>Gets or sets the moment per metre about the vertical axis, in kilonewton metres per metre.</summary>
        public double Moment { get; set; }

        /// <summary>Gets or sets a value indicating whether the load is uniform along the wall.</summary>
        public bool Uniform { get; set; } = true;

        /// <summary>Gets or sets the comment written into every created load.</summary>
        public string Comment { get; set; } = string.Empty;
    }
}
