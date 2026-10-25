using System;
using System.Collections.Generic;

namespace ArKrPlugFR.Core.Marking
{
    /// <summary>The kinds of structure the marking understands.</summary>
    public enum StructureKind
    {
        /// <summary>A column, structural or architectural.</summary>
        Column,

        /// <summary>A structural framing member: a beam, a joist, a brace.</summary>
        Beam,

        /// <summary>A floor slab.</summary>
        Floor,

        /// <summary>A wall.</summary>
        Wall,

        /// <summary>A foundation that is not a pile.</summary>
        Foundation,

        /// <summary>A foundation whose largest size is its height.</summary>
        Pile
    }

    /// <summary>What the materials of an element say it is made of.</summary>
    public enum MaterialClass
    {
        /// <summary>No word for concrete or steel was found.</summary>
        None,

        /// <summary>The materials name concrete.</summary>
        Concrete,

        /// <summary>The materials name steel.</summary>
        Steel
    }

    /// <summary>
    /// What the marking may look at on one element: the category, the materials and the
    /// three sizes of its bounding box. Everything in millimetres.
    /// </summary>
    public sealed class StructureFacts
    {
        /// <summary>Creates the facts of one element.</summary>
        /// <param name="categoryIdentifier">Built-in category name such as <c>OST_Walls</c>; may be absent.</param>
        /// <param name="materialNames">Names of the materials of the element and its type.</param>
        /// <param name="widthMillimeters">Size along X of the bounding box.</param>
        /// <param name="depthMillimeters">Size along Y of the bounding box.</param>
        /// <param name="heightMillimeters">Size along Z of the bounding box.</param>
        public StructureFacts(
            string? categoryIdentifier,
            IReadOnlyList<string>? materialNames,
            double widthMillimeters,
            double depthMillimeters,
            double heightMillimeters)
        {
            CategoryIdentifier = categoryIdentifier;
            MaterialNames = materialNames ?? Array.Empty<string>();
            WidthMillimeters = widthMillimeters;
            DepthMillimeters = depthMillimeters;
            HeightMillimeters = heightMillimeters;
        }

        /// <summary>Gets the built-in category name, or <c>null</c>.</summary>
        public string? CategoryIdentifier { get; }

        /// <summary>Gets the names of the materials.</summary>
        public IReadOnlyList<string> MaterialNames { get; }

        /// <summary>Gets the size along X of the bounding box in millimetres.</summary>
        public double WidthMillimeters { get; }

        /// <summary>Gets the size along Y of the bounding box in millimetres.</summary>
        public double DepthMillimeters { get; }

        /// <summary>Gets the size along Z of the bounding box in millimetres.</summary>
        public double HeightMillimeters { get; }
    }
}
