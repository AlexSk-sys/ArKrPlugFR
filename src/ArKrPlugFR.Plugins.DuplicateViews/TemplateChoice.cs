using ArKrPlugFR.Core.Views;

namespace ArKrPlugFR.Plugins.DuplicateViews
{
    /// <summary>
    /// One entry of the template picker: a view template of the project, or the
    /// "No template" entry that leaves every copy as it was duplicated.
    /// </summary>
    public sealed class TemplateChoice
    {
        private TemplateChoice(long id, string name, string description, TemplateKind kind, bool isNone)
        {
            Id = id;
            Name = name;
            Description = description ?? string.Empty;
            Kind = kind;
            IsNone = isNone;
        }

        /// <summary>Gets the element id of the template in the document.</summary>
        public long Id { get; }

        /// <summary>Gets the name shown in the picker.</summary>
        public string Name { get; }

        /// <summary>Gets the group the template belongs to, shown as its kind.</summary>
        public string Description { get; }

        /// <summary>Gets the kind the template fit rules work with.</summary>
        public TemplateKind Kind { get; }

        /// <summary>Gets a value indicating whether this entry applies no template at all.</summary>
        public bool IsNone { get; }

        /// <summary>Creates the entry that applies no template.</summary>
        /// <param name="name">Localized caption of the entry.</param>
        /// <returns>The "No template" entry.</returns>
        public static TemplateChoice None(string name)
        {
            return new TemplateChoice(0L, name, string.Empty, TemplateKind.None, true);
        }

        /// <summary>Creates the entry of one view template.</summary>
        /// <param name="id">Element id of the template.</param>
        /// <param name="name">Name of the template.</param>
        /// <param name="kind">Kind of the template.</param>
        /// <param name="description">Group the template is shown under.</param>
        /// <returns>The picker entry.</returns>
        public static TemplateChoice Of(long id, string name, TemplateKind kind, string description)
        {
            return new TemplateChoice(id, name, description, kind, false);
        }
    }
}
