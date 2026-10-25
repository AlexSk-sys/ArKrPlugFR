namespace ArKrPlugFR.Plugins.CageMetres
{
    /// <summary>
    /// The options of the tool, remembered between sessions as one JSON file in the
    /// user's settings folder.
    /// </summary>
    public sealed class CageMetresSettings
    {
        /// <summary>Gets or sets the parameter that holds the number of longitudinal bars.</summary>
        public string LongitudinalCountParameter { get; set; } = "Колво_горизонт";

        /// <summary>Gets or sets the parameter that holds the step of the inclined bars.</summary>
        public string InclinedStepParameter { get; set; } = "Шаг наклонных";

        /// <summary>Gets or sets the parameter that holds the step of the transverse bars.</summary>
        public string TransverseStepParameter { get; set; } = "Шаг поперечных";

        /// <summary>Gets or sets the parameter that holds the length of a bar.</summary>
        public string BarLengthParameter { get; set; } = "ADSK_Длина арматуры";

        /// <summary>Gets or sets the flag that marks a longitudinal bar.</summary>
        public string LongitudinalFlagParameter { get; set; } = "Продольный стержень";

        /// <summary>Gets or sets the flag that marks an inclined bar.</summary>
        public string InclinedFlagParameter { get; set; } = "Вертикальный стержень";

        /// <summary>Gets or sets the flag that marks a transverse bar.</summary>
        public string TransverseFlagParameter { get; set; } = "Поперечный стержень";

        /// <summary>Gets or sets the parameter the calculated length is written into.</summary>
        public string ResultParameter { get; set; } = "Длина ПМ расчетная";

        /// <summary>Gets or sets a value indicating whether the calculated length is written into the cage.</summary>
        public bool WriteResult { get; set; } = true;
    }
}
