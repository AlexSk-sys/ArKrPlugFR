using System;
using System.Windows;
using ArKrPlugFR.Ui.Views;

namespace ArKrPlugFR.Plugins.AnalyticalAlignment
{
    /// <summary>
    /// The window of the tool: the target, the elements and the nodes, and the two
    /// alignments.
    /// <para>
    /// The window owns only interaction; the geometry and the moves live in the view model
    /// and the engine. The window hides itself while the user picks in the model and returns
    /// after the pick, and the result of an action is shown in the shared result window.
    /// </para>
    /// </summary>
    public partial class AlignmentWindow : Window
    {
        private readonly AlignmentViewModel _viewModel;

        /// <summary>Creates the window of the tool.</summary>
        /// <param name="viewModel">The state to show.</param>
        public AlignmentWindow(AlignmentViewModel viewModel)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

            InitializeComponent();

            DataContext = _viewModel;
        }

        private void OnPickTargetClick(object sender, RoutedEventArgs e)
        {
            Pick(_viewModel.PickTarget);
        }

        private void OnPickMembersClick(object sender, RoutedEventArgs e)
        {
            Pick(_viewModel.PickMembers);
        }

        private void OnPickNodesClick(object sender, RoutedEventArgs e)
        {
            Pick(_viewModel.PickNodes);
        }

        private void OnAlignMembersClick(object sender, RoutedEventArgs e)
        {
            ShowReport(_viewModel.ApplyMembers());
        }

        private void OnAlignNodesClick(object sender, RoutedEventArgs e)
        {
            ShowReport(_viewModel.ApplyNodes());
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Pick(Action pick)
        {
            Hide();

            try
            {
                pick();
            }
            finally
            {
                Show();
                Activate();
            }
        }

        private void ShowReport(string report)
        {
            ToolWindowPresenter.Show(
                _viewModel.WindowTitle,
                _viewModel.Subtitle,
                _viewModel.ReportCaption,
                report);
        }
    }
}
