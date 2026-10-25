using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ArKrPlugFR.Core.Views;
using ArKrPlugFR.Ui.ViewModels;

namespace ArKrPlugFR.Plugins.DuplicateViews
{
    /// <summary>One row of a tree: either a group of elements or a single element.</summary>
    public abstract class CopyNodeViewModel : ObservableObject
    {
        private bool _isVisible = true;

        /// <summary>Gets the text shown next to the checkbox.</summary>
        public abstract string DisplayName { get; }

        /// <summary>
        /// Gets a value indicating whether the row passes the current search. The row
        /// is only hidden, never removed, so ticks and counters survive a search.
        /// </summary>
        public bool IsVisible
        {
            get => _isVisible;

            internal set
            {
                if (_isVisible == value)
                {
                    return;
                }

                _isVisible = value;
                RaisePropertyChanged(nameof(IsVisible));
            }
        }
    }

    /// <summary>
    /// One element that can be copied, with its tick and its copy counter.
    /// </summary>
    public sealed class CopyLeafViewModel : CopyNodeViewModel
    {
        private readonly CopyGroupViewModel _parent;
        private readonly CopyCandidate _candidate;
        private bool _isChecked;
        private string _countText = CopyCounter.DefaultText;

        /// <summary>Creates a leaf row.</summary>
        /// <param name="parent">The group the element is listed under.</param>
        /// <param name="candidate">The element to show.</param>
        public CopyLeafViewModel(CopyGroupViewModel parent, CopyCandidate candidate)
        {
            _parent = parent ?? throw new ArgumentNullException(nameof(parent));
            _candidate = candidate ?? throw new ArgumentNullException(nameof(candidate));
        }

        /// <summary>Gets the element id in the document.</summary>
        public long Id => _candidate.Id;

        /// <summary>Gets the name of the element.</summary>
        public string Name => _candidate.Name;

        /// <summary>Gets the kind the template fit rules work with.</summary>
        public TemplateKind Kind => _candidate.Kind;

        /// <summary>Gets the tree the element belongs to.</summary>
        public CopyTree Tree => _candidate.Tree;

        /// <inheritdoc />
        public override string DisplayName => Name;

        /// <summary>Gets or sets a value indicating whether the element is ticked for copying.</summary>
        public bool IsChecked
        {
            get => _isChecked;

            set
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

        /// <summary>Gets or sets the text of the copy counter, normalised as the user types.</summary>
        public string CountText
        {
            get => _countText;

            set
            {
                string normalized = CopyCounter.Normalize(value);

                if (string.Equals(_countText, normalized, StringComparison.Ordinal))
                {
                    return;
                }

                _countText = normalized;
                RaisePropertyChanged(nameof(CountText));
                RaisePropertyChanged(nameof(Count));
            }
        }

        /// <summary>Gets the number of copies the row asks for, never below one.</summary>
        public int Count => CopyCounter.ValueOf(_countText);

        /// <summary>Adds one copy, up to the cap.</summary>
        public void Increase()
        {
            CountText = CopyCounter.Increase(_countText);
        }

        /// <summary>Removes one copy, down to one copy.</summary>
        public void Decrease()
        {
            CountText = CopyCounter.Decrease(_countText);
        }

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
    }

    /// <summary>
    /// A group of elements that share a display category, with a tri-state tick.
    /// <para>
    /// Ticking the group ticks every element in it; ticking some of them leaves the
    /// group between the two states, shown as the indeterminate mark.
    /// </para>
    /// </summary>
    public sealed class CopyGroupViewModel : CopyNodeViewModel
    {
        private readonly List<CopyLeafViewModel> _children;
        private bool? _isChecked;
        private bool _updating;

        /// <summary>Creates a group row.</summary>
        /// <param name="displayName">Localized name of the group.</param>
        /// <param name="candidates">The elements of the group.</param>
        public CopyGroupViewModel(string displayName, IReadOnlyList<CopyCandidate> candidates)
        {
            if (string.IsNullOrWhiteSpace(displayName))
            {
                throw new ArgumentException("The group needs a name.", nameof(displayName));
            }

            if (candidates == null)
            {
                throw new ArgumentNullException(nameof(candidates));
            }

            DisplayName = displayName;

            _children = candidates
                .Select(candidate => new CopyLeafViewModel(this, candidate))
                .ToList();
        }

        /// <summary>Gets the localized name of the group.</summary>
        public override string DisplayName { get; }

        /// <summary>Gets the elements of the group.</summary>
        public IReadOnlyList<CopyLeafViewModel> Children => _children;

        /// <summary>Gets the number of elements in the group, for the row label.</summary>
        public string CountText => _children.Count.ToString(CultureInfo.InvariantCulture);

        /// <summary>
        /// Gets or sets the tick: <c>true</c> when every element is ticked,
        /// <c>false</c> when none is, and <c>null</c> for a partial tick. Setting the
        /// value ticks or unticks every element in the group.
        /// </summary>
        public bool? IsChecked
        {
            get => _isChecked;
            set => SetChecked(value ?? true);
        }

        /// <summary>Recomputes the group tick after one of its elements changed.</summary>
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

        /// <summary>Ticks or unticks every element in the group.</summary>
        /// <param name="value">New tick state.</param>
        public void SetChecked(bool value)
        {
            _updating = true;
            _isChecked = value;

            foreach (CopyLeafViewModel child in _children)
            {
                child.SetCheckedSilently(value);
            }

            _updating = false;
            RaisePropertyChanged(nameof(IsChecked));
        }

        /// <summary>
        /// Hides or shows the group and its elements for a search text. A group whose
        /// own name matches shows all of its elements; otherwise only the matching ones
        /// stay, and the group disappears when none of them does.
        /// </summary>
        /// <param name="groupMatches">Whether the group name itself matches.</param>
        /// <param name="matchesName">Tells whether an element name matches.</param>
        internal void ApplyFilter(bool groupMatches, Func<string, bool> matchesName)
        {
            if (matchesName == null)
            {
                throw new ArgumentNullException(nameof(matchesName));
            }

            bool anyVisible = false;

            foreach (CopyLeafViewModel child in _children)
            {
                child.IsVisible = groupMatches || matchesName(child.Name);
                anyVisible |= child.IsVisible;
            }

            IsVisible = groupMatches || anyVisible;
        }
    }
}
