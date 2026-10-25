using System;
using System.Collections.Generic;

namespace ArKrPlugFR.Core.Marking
{
    /// <summary>
    /// The decision behind the marking: what an element is — its kind from the category
    /// and the shape, its material family from the material names — and which mark that
    /// pair asks for.
    /// </summary>
    public static class MarkingRules
    {
        private static readonly string[] ConcreteWords = { "бетон", "железобетон", "concrete", "reinforced" };
        private static readonly string[] SteelWords = { "сталь", "металл", "steel", "metal" };

        /// <summary>Decides the kind of an element, or <c>null</c> for anything that is not a structure.</summary>
        /// <param name="facts">What the marking may look at.</param>
        public static StructureKind? Detect(StructureFacts facts)
        {
            if (facts == null)
            {
                throw new ArgumentNullException(nameof(facts));
            }

            string? identifier = facts.CategoryIdentifier?.Trim();

            if (string.IsNullOrEmpty(identifier))
            {
                return null;
            }

            if (Matches(identifier!, "OST_StructuralColumns") || Matches(identifier!, "OST_Columns"))
            {
                return StructureKind.Column;
            }

            if (Matches(identifier!, "OST_StructuralFraming"))
            {
                return StructureKind.Beam;
            }

            if (Matches(identifier!, "OST_Floors"))
            {
                return StructureKind.Floor;
            }

            if (Matches(identifier!, "OST_Walls"))
            {
                return StructureKind.Wall;
            }

            if (Matches(identifier!, "OST_StructuralFoundation"))
            {
                // A foundation whose largest size is its height is a pile; a broad slab or a
                // strip footing is a foundation.
                bool deep = facts.HeightMillimeters > facts.WidthMillimeters
                    && facts.HeightMillimeters > facts.DepthMillimeters;

                return deep ? StructureKind.Pile : StructureKind.Foundation;
            }

            return null;
        }

        /// <summary>
        /// Decides the material family from the names of the materials. Concrete wins over
        /// steel when an element names both, because that is what the structure is made of.
        /// </summary>
        /// <param name="names">Names of the materials of the element and its type.</param>
        public static MaterialClass Material(IReadOnlyList<string>? names)
        {
            if (names == null)
            {
                return MaterialClass.None;
            }

            bool steel = false;

            foreach (string name in names)
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                if (ContainsAny(name, ConcreteWords))
                {
                    return MaterialClass.Concrete;
                }

                if (ContainsAny(name, SteelWords))
                {
                    steel = true;
                }
            }

            return steel ? MaterialClass.Steel : MaterialClass.None;
        }

        /// <summary>
        /// Builds the mark of a kind and a material family: the code of the kind followed
        /// by the suffix of the family. An empty code yields an empty mark, which the run
        /// never writes.
        /// </summary>
        /// <param name="settings">Codes and suffixes.</param>
        /// <param name="kind">The kind of structure.</param>
        /// <param name="material">The material family.</param>
        public static string Mark(MarkingSettings settings, StructureKind kind, MaterialClass material)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            string code = (settings.MarkOf(kind) ?? string.Empty).Trim();

            if (code.Length == 0)
            {
                return string.Empty;
            }

            string suffix = (settings.SuffixOf(material) ?? string.Empty).Trim();

            return suffix.Length == 0 ? code : code + suffix;
        }

        private static bool Matches(string identifier, string wanted)
        {
            return string.Equals(identifier, wanted, StringComparison.OrdinalIgnoreCase);
        }

        private static bool ContainsAny(string value, string[] words)
        {
            foreach (string word in words)
            {
                if (value.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
