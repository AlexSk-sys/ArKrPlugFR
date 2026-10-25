using System;
using ArKrPlugFR.Core.Worksets;
using ArKrPlugFR.Ui.ViewModels;

namespace ArKrPlugFR.Plugins.AssignWorksets
{
    /// <summary>
    /// One line of the rule list: the kind of match, what it matches and the workset the
    /// elements go to.
    /// <para>
    /// The row is what the window binds to; the rule itself is built when a run starts or
    /// when the settings are written. A parameter rule takes its workset from the
    /// parameter value, so the workset field of such a row is switched off instead of
    /// pretending to matter.
    /// </para>
    /// </summary>
    public sealed class WorksetRow : ObservableObject
    {
        private readonly WorksetText _text;
        private int _kindIndex;
        private string _pattern = string.Empty;
        private string _target = string.Empty;

        /// <summary>Creates an empty row of the default kind.</summary>
        /// <param name="text">Text resolved for the current language.</param>
        public WorksetRow(WorksetText text)
            : this(text, null)
        {
        }

        /// <summary>Creates a row that shows a stored rule.</summary>
        /// <param name="text">Text resolved for the current language.</param>
        /// <param name="rule">Stored rule, or <c>null</c> for an empty row.</param>
        public WorksetRow(WorksetText text, WorksetRule? rule)
        {
            _text = text ?? throw new ArgumentNullException(nameof(text));

            _kindIndex = IndexOfKind(rule?.Kind);
            _pattern = rule?.Pattern ?? string.Empty;
            _target = rule?.Target ?? string.Empty;
        }

        /// <summary>Raised when the row changes; the view model writes the settings then.</summary>
        internal event Action? Changed;

        /// <summary>Gets the four choices of the kind list.</summary>
        public string[] Kinds => _text.Kinds;

        /// <summary>Gets the caption of the button that removes the row.</summary>
        public string RemoveText => _text.RemoveRule;

        /// <summary>Gets or sets the chosen kind, as an index into <see cref="WorksetRuleKinds.All"/>.</summary>
        public int KindIndex
        {
            get => _kindIndex;

            set
            {
                if (SetProperty(ref _kindIndex, value))
                {
                    RaisePropertyChanged(nameof(TargetEnabled));
                    Changed?.Invoke();
                }
            }
        }

        /// <summary>Gets a value indicating whether the workset field applies to the row.</summary>
        public bool TargetEnabled => !IsParameterKind;

        /// <summary>Gets or sets what the rule matches.</summary>
        public string Pattern
        {
            get => _pattern;

            set
            {
                if (SetProperty(ref _pattern, value ?? string.Empty))
                {
                    Changed?.Invoke();
                }
            }
        }

        /// <summary>Gets or sets the workset the elements go to.</summary>
        public string Target
        {
            get => _target;

            set
            {
                if (SetProperty(ref _target, value ?? string.Empty))
                {
                    Changed?.Invoke();
                }
            }
        }

        /// <summary>Builds the rule the row describes.</summary>
        internal WorksetRule ToRule()
        {
            // A parameter row takes its workset from the parameter value; an old workset
            // name left in the disabled field is not written down.
            return new WorksetRule(
                WorksetRuleKinds.All[SafeKindIndex],
                Pattern,
                IsParameterKind ? string.Empty : Target);
        }

        private bool IsParameterKind =>
            string.Equals(WorksetRuleKinds.All[SafeKindIndex], WorksetRuleKinds.ParameterValue, StringComparison.OrdinalIgnoreCase);

        private int SafeKindIndex =>
            _kindIndex >= 0 && _kindIndex < WorksetRuleKinds.All.Length ? _kindIndex : 0;

        private static int IndexOfKind(string? kind)
        {
            if (WorksetRuleKinds.IsKnown(kind))
            {
                for (int index = 0; index < WorksetRuleKinds.All.Length; index++)
                {
                    if (string.Equals(WorksetRuleKinds.All[index], kind, StringComparison.OrdinalIgnoreCase))
                    {
                        return index;
                    }
                }
            }

            return 0;
        }
    }
}
