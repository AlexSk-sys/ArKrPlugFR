using System;
using System.Collections.Generic;
using System.Linq;
using ArKrPlugFR.Core.ViewTransfer;
using ArKrPlugFR.Ui.ViewModels;

namespace ArKrPlugFR.Plugins.ViewTemplateTransfer
{
    /// <summary>
    /// One row of the transfer tree: either a group of views or a single view.
    /// </summary>
    public abstract class TreeNodeViewModel : ObservableObject
    {
        /// <summary>Gets the text shown next to the checkbox.</summary>
        public abstract string DisplayName { get; }
    }

    /// <summary>
    /// A view of the source project that the user can tick for transfer.
    /// </summary>
    public sealed class TemplateNodeViewModel : TreeNodeViewModel
    {
        private readonly GroupNodeViewModel _parent;
        private readonly TransferItem _item;
        private bool _isChecked;

        /// <summary>Creates a view node.</summary>
        /// <param name="parent">The group the view belongs to.</param>
        /// <param name="item">The planned view.</param>
        public TemplateNodeViewModel(GroupNodeViewModel parent, TransferItem item)
        {
            _parent = parent ?? throw new ArgumentNullException(nameof(parent));
            _item = item ?? throw new ArgumentNullException(nameof(item));
        }

        /// <summary>Gets the element id of the view in the source project.</summary>
        public long Id => _item.Id;

        /// <summary>Gets the name of the view in the source project.</summary>
        public string Name => _item.Name;

        /// <summary>Gets a value indicating whether the view is ticked for transfer.</summary>
        public bool IsChecked
        {
            get => _isChecked;
            set => SetChecked(value);
        }

        /// <inheritdoc />
        public override string DisplayName => Name;

        /// <summary>Sets the tick without telling the parent, used for ranges and group ticks.</summary>
        /// <param name="value">New tick state.</param>
        internal void SetCheckedSilently(bool value)
        {
            if (_isChecked == value)
            {
                return;
            }

            _isChecked = value;
            RaisePropertyChanged(nameof(IsChecked));
        }

        private void SetChecked(bool value)
        {
            if (_isChecked == value)
            {
                return;
            }

            _isChecked = value;
            RaisePropertyChanged(nameof(IsChecked));
            _parent.OnChildChanged();
        }
    }

    /// <summary>
    /// A group of views that share a display category, with a tri-state tick.
    /// <para>
    /// Ticking the group ticks every view in it; ticking some of the views
    /// leaves the group between the two states, shown as the indeterminate mark.
    /// </para>
    /// </summary>
    public sealed class GroupNodeViewModel : TreeNodeViewModel
    {
        private readonly List<TemplateNodeViewModel> _children;
        private bool? _isChecked;
        private bool _updating;

        /// <summary>Creates a group node.</summary>
        /// <param name="displayName">Localized name of the group.</param>
        /// <param name="items">The planned views of the group.</param>
        public GroupNodeViewModel(string displayName, IReadOnlyList<TransferItem> items)
        {
            if (string.IsNullOrWhiteSpace(displayName))
            {
                throw new ArgumentException("The group needs a name.", nameof(displayName));
            }

            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            DisplayName = displayName;

            _children = items
                .Select(item => new TemplateNodeViewModel(this, item))
                .ToList();
        }

        /// <summary>Gets the localized name of the group.</summary>
        public override string DisplayName { get; }

        /// <summary>Gets the views of the group.</summary>
        public IReadOnlyList<TemplateNodeViewModel> Children => _children;

        /// <summary>Gets the number of views in the group, for the row label.</summary>
        public string CountText => ViewTemplateTransferText.Number(_children.Count);

        /// <summary>
        /// Gets or sets the tick: <c>true</c> when every view is ticked, <c>false</c>
        /// when none is, and <c>null</c> for a partial tick. Setting the value
        /// ticks or unticks every view in the group.
        /// </summary>
        public bool? IsChecked
        {
            get => _isChecked;
            set => SetChecked(value ?? true);
        }

        /// <summary>Recomputes the group tick after one of its views changed.</summary>
        internal void OnChildChanged()
        {
            if (_updating)
            {
                return;
            }

            bool any = _children.Any(child => child.IsChecked);
            bool all = _children.All(child => child.IsChecked);
            bool? next = all ? true : any ? (bool?)null : false;

            if (_isChecked != next)
            {
                _isChecked = next;
                RaisePropertyChanged(nameof(IsChecked));
            }
        }

        /// <summary>Ticks or unticks every view in the group.</summary>
        /// <param name="value">New tick state.</param>
        public void SetChecked(bool value)
        {
            _updating = true;
            _isChecked = value;

            foreach (TemplateNodeViewModel child in _children)
            {
                child.SetCheckedSilently(value);
            }

            _updating = false;
            RaisePropertyChanged(nameof(IsChecked));
        }
    }
}
