namespace ArKrPlugFR.Core.Marking
{
    /// <summary>
    /// The remembered state of the marking: the switch that protects existing marks, the
    /// code of every kind and the suffix of every material family.
    /// </summary>
    public sealed class MarkingSettings
    {
        /// <summary>Gets or sets a value indicating whether an element that already carries a mark is left alone.</summary>
        public bool OnlyEmpty { get; set; } = true;

        /// <summary>Gets or sets the code of a column.</summary>
        public string ColumnMark { get; set; } = "К";

        /// <summary>Gets or sets the code of a beam.</summary>
        public string BeamMark { get; set; } = "Б";

        /// <summary>Gets or sets the code of a floor.</summary>
        public string FloorMark { get; set; } = "П";

        /// <summary>Gets or sets the code of a wall.</summary>
        public string WallMark { get; set; } = "СТ";

        /// <summary>Gets or sets the code of a foundation.</summary>
        public string FoundationMark { get; set; } = "Ф";

        /// <summary>Gets or sets the code of a pile.</summary>
        public string PileMark { get; set; } = "С";

        /// <summary>Gets or sets the suffix appended to the mark of a concrete element.</summary>
        public string ConcreteSuffix { get; set; } = "м";

        /// <summary>Gets or sets the suffix appended to the mark of a steel element.</summary>
        public string SteelSuffix { get; set; } = "ст";

        /// <summary>Reads the code of a kind.</summary>
        /// <param name="kind">The kind of structure.</param>
        public string MarkOf(StructureKind kind)
        {
            switch (kind)
            {
                case StructureKind.Column:
                    return ColumnMark;

                case StructureKind.Beam:
                    return BeamMark;

                case StructureKind.Floor:
                    return FloorMark;

                case StructureKind.Wall:
                    return WallMark;

                case StructureKind.Foundation:
                    return FoundationMark;

                default:
                    return PileMark;
            }
        }

        /// <summary>Reads the suffix of a material family.</summary>
        /// <param name="material">The material family.</param>
        public string SuffixOf(MaterialClass material)
        {
            switch (material)
            {
                case MaterialClass.Concrete:
                    return ConcreteSuffix;

                case MaterialClass.Steel:
                    return SteelSuffix;

                default:
                    return string.Empty;
            }
        }
    }
}
