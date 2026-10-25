using ArKrPlugFR.Core.Views;

namespace ArKrPlugFR.Plugins.DuplicateViews
{
    /// <summary>Which of the three trees of the window an element belongs to.</summary>
    public enum CopyTree
    {
        /// <summary>Ordinary views and drafting views.</summary>
        Views,

        /// <summary>Schedules that can be duplicated.</summary>
        Schedules,

        /// <summary>View templates, including schedule templates.</summary>
        Templates
    }

    /// <summary>
    /// One element the tool can copy, described with plain values, so the window can
    /// build its tree and the run can be planned without holding on to Revit elements.
    /// </summary>
    public sealed class CopyCandidate
    {
        /// <summary>Creates a candidate.</summary>
        /// <param name="id">Element id in the document.</param>
        /// <param name="name">Name of the element.</param>
        /// <param name="tree">The tree the element is listed in.</param>
        /// <param name="group">Localized group the element is shown under.</param>
        /// <param name="kind">Kind the template fit rules work with.</param>
        public CopyCandidate(long id, string name, CopyTree tree, string group, TemplateKind kind)
        {
            Id = id;
            Name = name ?? string.Empty;
            Tree = tree;
            Group = group ?? string.Empty;
            Kind = kind;
        }

        /// <summary>Gets the element id in the document.</summary>
        public long Id { get; }

        /// <summary>Gets the name of the element.</summary>
        public string Name { get; }

        /// <summary>Gets the tree the element is listed in.</summary>
        public CopyTree Tree { get; }

        /// <summary>Gets the localized group the element is shown under.</summary>
        public string Group { get; }

        /// <summary>Gets the kind the template fit rules work with.</summary>
        public TemplateKind Kind { get; }
    }
}
