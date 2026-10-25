using System;
using System.Windows;
using ArKrPlugFR.Ui.Views;

namespace ArKrPlugFR.Plugins.BarSketches
{
    /// <summary>
    /// The window of the tool: the bars to sketch and the run.
    /// <para>
    /// The window owns only interaction; everything that creates views and sheets lives in
    /// the view model and the engine. The window hides itself while the user picks in the
    /// model and returns after the pick, and the result of the run is shown in the shared
    /// result window.
    /// </para>
    /// </summary>
    public partial class SketchWindow : Window
    {
        private readonly SketchViewModel _viewModel;

        /// <summary>Creates the window of the tool.</summary>
        /// <param name="viewModel">The state to show.</param>
        public SketchWindow(SketchViewModel viewModel)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

            InitializeComponent();

            DataContext = _viewModel;
        }

        private void OnPickBarsClick(object sender, RoutedEventArgs e)
        {
            Hide();

            try
            {
                _viewModel.PickBars();
            }
            finally
            {
                Show();
                Activate();
            }
        }

        private void OnApplyClick(object sender, RoutedEventArgs e)
        {
            ToolWindowPresenter.Show(
                _viewModel.WindowTitle,
                _viewModel.Subtitle,
                _viewModel.ReportCaption,
                _viewModel.Apply());
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
