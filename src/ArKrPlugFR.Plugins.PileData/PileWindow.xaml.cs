using System;
using System.Windows;
using ArKrPlugFR.Ui.Views;

namespace ArKrPlugFR.Plugins.PileData
{
    /// <summary>
    /// The window of the tool: the picks, the settings and the three actions.
    /// <para>
    /// The window owns only interaction; everything that reads or changes the model lives
    /// in the view model. The window hides itself while the user picks in the model and
    /// returns after the pick, and the result of an action is shown in the shared result
    /// window.
    /// </para>
    /// </summary>
    public partial class PileWindow : Window
    {
        private readonly PileViewModel _viewModel;

        /// <summary>Creates the window of the tool.</summary>
        /// <param name="viewModel">The state to show.</param>
        public PileWindow(PileViewModel viewModel)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

            InitializeComponent();

            DataContext = _viewModel;
        }

        private void OnPickPilesClick(object sender, RoutedEventArgs e)
        {
            Pick(_viewModel.PickPiles);
        }

        private void OnPickSlabsClick(object sender, RoutedEventArgs e)
        {
            Pick(_viewModel.PickSlabs);
        }

        private void OnNumberClick(object sender, RoutedEventArgs e)
        {
            ShowReport(_viewModel.ApplyNumber());
        }

        private void OnElevationsClick(object sender, RoutedEventArgs e)
        {
            ShowReport(_viewModel.ApplyElevations());
        }

        private void OnCutClick(object sender, RoutedEventArgs e)
        {
            ShowReport(_viewModel.ApplyCut());
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
