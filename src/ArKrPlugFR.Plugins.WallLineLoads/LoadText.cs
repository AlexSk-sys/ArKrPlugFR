using System.Globalization;

namespace ArKrPlugFR.Plugins.WallLineLoads
{
    /// <summary>
    /// The strings of the tool resolved for one language.
    /// </summary>
    public sealed class LoadText
    {
        private readonly string _culture;

        /// <summary>Creates the text for a culture; an unknown culture falls back to English.</summary>
        /// <param name="culture">Two-letter culture code.</param>
        public LoadText(string? culture)
        {
            _culture = culture ?? "en";
        }

        /// <summary>Gets the window title.</summary>
        public string WindowTitle => LoadStrings.WindowTitle.Resolve(_culture);

        /// <summary>Gets the line under the title.</summary>
        public string Subtitle => LoadStrings.Subtitle.Resolve(_culture);

        /// <summary>Gets the caption of the scope.</summary>
        public string ScopeLabel => LoadStrings.ScopeLabel.Resolve(_culture);

        /// <summary>Gets the caption of the scope that reads the selection.</summary>
        public string ScopeSelected => LoadStrings.ScopeSelected.Resolve(_culture);

        /// <summary>Gets the caption of the scope that reads the active view.</summary>
        public string ScopeView => LoadStrings.ScopeView.Resolve(_culture);

        /// <summary>Gets the caption of the scope that reads a workset.</summary>
        public string ScopeWorkset => LoadStrings.ScopeWorkset.Resolve(_culture);

        /// <summary>Gets the caption of the workset list.</summary>
        public string WorksetLabel => LoadStrings.WorksetLabel.Resolve(_culture);

        /// <summary>Gets the caption of the load case field.</summary>
        public string CaseLabel => LoadStrings.CaseLabel.Resolve(_culture);

        /// <summary>Gets the hint about a load case that does not exist yet.</summary>
        public string CaseHint => LoadStrings.CaseHint.Resolve(_culture);

        /// <summary>Gets the caption of the force field.</summary>
        public string ForceLabel => LoadStrings.ForceLabel.Resolve(_culture);

        /// <summary>Gets the caption of the moment field.</summary>
        public string MomentLabel => LoadStrings.MomentLabel.Resolve(_culture);

        /// <summary>Gets the caption of the uniformity switch.</summary>
        public string UniformLabel => LoadStrings.UniformLabel.Resolve(_culture);

        /// <summary>Gets the caption of the comment field.</summary>
        public string CommentLabel => LoadStrings.CommentLabel.Resolve(_culture);

        /// <summary>Gets the caption of the run button.</summary>
        public string Apply => LoadStrings.Apply.Resolve(_culture);

        /// <summary>Gets the caption of the close button.</summary>
        public string Close => LoadStrings.Close.Resolve(_culture);

        /// <summary>Gets the caption of the report area.</summary>
        public string ReportCaption => LoadStrings.ReportCaption.Resolve(_culture);

        /// <summary>Gets the name of the run in the undo list.</summary>
        public string TransactionName => LoadStrings.TransactionName.Resolve(_culture);

        /// <summary>Gets the message shown when the force and the moment are both zero.</summary>
        public string NoForce => LoadStrings.NoForce.Resolve(_culture);

        /// <summary>Gets the message shown when the scope holds no walls.</summary>
        public string NoWalls => LoadStrings.NoWalls.Resolve(_culture);

        /// <summary>Gets the message shown when no wall has a straight location line.</summary>
        public string NoStraightWalls => LoadStrings.NoStraightWalls.Resolve(_culture);

        /// <summary>Gets the message shown when the workset does not exist.</summary>
        /// <param name="name">Name of the workset.</param>
        public string NoWorkset(string name) => LoadStrings.NoWorkset.Resolve(_culture, name);

        /// <summary>Gets the message shown in a project without worksets.</summary>
        public string NotAWorksharedModel => LoadStrings.NotAWorksharedModel.Resolve(_culture);

        /// <summary>Gets the message shown for a wall without an analytical model.</summary>
        /// <param name="wall">Description of the wall.</param>
        public string NoAnalyticalHost(string wall) => LoadStrings.NoAnalyticalHost.Resolve(_culture, wall);

        /// <summary>Gets the line that reports a wall without a straight location line.</summary>
        /// <param name="wall">Description of the wall.</param>
        public string NotStraight(string wall) => LoadStrings.NotStraight.Resolve(_culture, wall);

        /// <summary>Gets the line that reports a refused wall.</summary>
        /// <param name="wall">Description of the wall.</param>
        /// <param name="reason">What the platform reported.</param>
        public string Refused(string wall, string reason) => LoadStrings.Refused.Resolve(_culture, wall, reason);

        /// <summary>Gets the line that counts the run.</summary>
        /// <param name="created">Loads that were created.</param>
        /// <param name="walls">Walls of the scope.</param>
        /// <param name="skipped">Walls that were skipped.</param>
        /// <param name="refused">Walls the platform refused.</param>
        /// <param name="loadCase">Name of the load case.</param>
        /// <param name="force">Force per metre.</param>
        /// <param name="moment">Moment per metre.</param>
        public string Result(int created, int walls, int skipped, int refused, string loadCase, double force, double moment) =>
            LoadStrings.Result.Resolve(
                _culture,
                created,
                walls,
                skipped,
                refused,
                loadCase,
                Number(force),
                Number(moment));

        /// <summary>Gets the note about loads created on an analytical host.</summary>
        public string HostedNote => LoadStrings.HostedNote.Resolve(_culture);

        /// <summary>Gets the line that reports a load case that was created.</summary>
        /// <param name="name">Name of the load case.</param>
        public string CaseCreated(string name) => LoadStrings.CaseCreated.Resolve(_culture, name);

        /// <summary>Formats a number the way the report shows it.</summary>
        /// <param name="value">The value to show.</param>
        /// <returns>The value without trailing zeros.</returns>
        public static string Number(double value)
        {
            return value.ToString("0.###", CultureInfo.InvariantCulture);
        }
    }
}
