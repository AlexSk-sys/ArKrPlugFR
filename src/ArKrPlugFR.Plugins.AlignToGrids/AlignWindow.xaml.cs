using System;
using System.Windows;
using ArKrPlugFR.Ui.Views;

namespace ArKrPlugFR.Plugins.AlignToGrids
{
    /// <summary>
    /// The options window of the tool: the picked sets, the capture distance and the
    /// apply button.
    /// <para>
    /// The window owns only interaction. Everything that reads or changes the model
    /// lives in the view model. The window hides itself while the user picks in the
    /// model and returns after the pick, and the result of a run is shown in the shared
    /// result window.
    /// </para>
    /// </summary>
    public partial class AlignWindow : Window
    {
        private readonly AlignViewModel _viewModel;

        /// <summary>Creates the window of the tool.</summary>
        /// <param name="viewModel">The state to show.</param>
        public AlignWindow(AlignViewModel viewModel)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

            InitializeComponent();

            DataContext = _viewModel;
        }

        private void OnPickColumnsClick(object sender, RoutedEventArgs e)
        {
            Hide();

            try
            {
                _viewModel.PickColumns();
            }
            finally
            {
                Show();
                Activate();
            }
        }

        private void OnPickGridsClick(object sender, RoutedEventArgs e)
        {
            Hide();

            try
            {
                _viewModel.PickGrids();
            }
            finally
            {
                Show();
                Activate();
            }
        }

        private void OnApplyClick(object sender, RoutedEventArgs e)
        {
            if (!_viewModel.TryReadCapture(out string captureError))
            {
                _viewModel.SetReport(captureError);

                return;
            }

            string report = _viewModel.Apply();

            ToolWindowPresenter.Show(
                _viewModel.WindowTitle,
                _viewModel.Subtitle,
                _viewModel.ReportCaption,
                report);
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
