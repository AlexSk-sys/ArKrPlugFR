using System.Collections.ObjectModel;
using ArKrPlugFR.Core.Types;
using ArKrPlugFR.Ui.ViewModels;

namespace ArKrPlugFR.Plugins.SelectTypes
{
    /// <summary>
    /// A row of the type tree: a category, a family or a type.
    /// <para>
    /// Ticking a row ticks everything under it; a parent shows a tick when every child is
    /// ticked, a cross when none is, and the third state in between. The rows know nothing
    /// about Revit; the view model builds them from the facts the reader returns.
    /// </para>
    /// </summary>
    public abstract class FilterNode : ObservableObject
    {
        private bool? _isChecked = false;
        private bool _isExpanded = true;

        /// <summary>Creates a row.</summary>
        /// <param name="title">Text shown in the tree.</param>
        protected FilterNode(string title)
        {
            Title = title;
        }

        /// <summary>Gets the text shown in the tree.</summary>
        public string Title { get; }

        /// <summary>Gets the rows under this one.</summary>
        public ObservableCollection<FilterNode> Children { get; } = new ObservableCollection<FilterNode>();

        /// <summary>Gets or sets the row above this one.</summary>
        internal FilterNode? Parent { get; set; }

        /// <summary>Gets or sets a value indicating whether the row is open.</summary>
        public bool IsExpanded
        {
            get => _isExpanded;

            set => SetProperty(ref _isExpanded, value);
        }

        /// <summary>
        /// Gets or sets the tick of the row; parents show the third state in between.
        /// <para>
        /// The third state is computed from the children, never typed by the user: the tick
        /// boxes of the window are not three-state, so a click always produces a tick or a
        /// cross and only a recomputed row can read as <c>null</c>. Setting <c>null</c>
        /// programmatically therefore marks this row alone — it is not pushed down, because
        /// there is nothing below a parent that an in-between state could mean.
        /// </para>
        /// </summary>
        public bool? IsChecked
        {
            get => _isChecked;

            set
            {
                if (_isChecked == value)
                {
                    return;
                }

                _isChecked = value;
                RaisePropertyChanged();

                if (value != null)
                {
                    foreach (FilterNode child in Children)
                    {
                        child.ApplyCheck(value.Value);
                    }
                }

                Parent?.RefreshFromChildren();
            }
        }

        /// <summary>Ticks or unticks the row and everything under it without walking back up.</summary>
        /// <param name="value">State to apply.</param>
        internal void ApplyCheck(bool value)
        {
            if (_isChecked == value)
            {
                return;
            }

            _isChecked = value;
            RaisePropertyChanged(nameof(IsChecked));

            foreach (FilterNode child in Children)
            {
                child.ApplyCheck(value);
            }
        }

        /// <summary>Recomputes the tick of the row from its children and walks up.</summary>
        internal void RefreshFromChildren()
        {
            if (Children.Count == 0)
            {
                return;
            }

            bool all = true;
            bool none = true;

            foreach (FilterNode child in Children)
            {
                if (child.IsChecked != true)
                {
                    all = false;
                }

                if (child.IsChecked != false)
                {
                    none = false;
                }
            }

            bool? state = all ? true : none ? (bool?)false : null;

            if (_isChecked == state)
            {
                return;
            }

            _isChecked = state;
            RaisePropertyChanged(nameof(IsChecked));
            Parent?.RefreshFromChildren();
        }
    }

    /// <summary>A category of the tree: a family folder with everything inside it.</summary>
    public sealed class CategoryNode : FilterNode
    {
        /// <summary>Creates a category row.</summary>
        /// <param name="name">Name of the category.</param>
        /// <param name="typeCount">How many types the category holds.</param>
        public CategoryNode(string name, int typeCount)
            : base(name + " (" + typeCount + ")")
        {
        }
    }

    /// <summary>A family of the tree, with its types.</summary>
    public sealed class FamilyNode : FilterNode
    {
        /// <summary>Creates a family row.</summary>
        /// <param name="name">Name of the family.</param>
        /// <param name="typeCount">How many types the family holds.</param>
        public FamilyNode(string name, int typeCount)
            : base(name + " (" + typeCount + ")")
        {
        }
    }

    /// <summary>A type of the tree; the row the run works from.</summary>
    public sealed class TypeNode : FilterNode
    {
        /// <summary>Creates a type row.</summary>
        /// <param name="fact">The type the row shows.</param>
        public TypeNode(TypeFact fact)
            : base(fact.Type)
        {
            TypeId = fact.Id;
        }

        /// <summary>Gets the element id of the type.</summary>
        public long TypeId { get; }
    }
}
