using System;
using System.Windows;
using ArKrPlugFR.Ui.Views;

namespace ArKrPlugFR.Plugins.AnalyticalExtension
{
    /// <summary>
    /// The window of the tool: the sources, the targets, the reach and the run.
    /// <para>
    /// The window owns only interaction; everything that reads or changes the model lives in
    /// the view model and the engine. The window hides itself while the user picks in the
    /// model and returns after the pick, and the result of the run is shown in the shared
    /// result window.
    /// </para>
    /// </summary>
    public partial class ExtensionWindow : Window
    {
        private readonly ExtensionViewModel _viewModel;

        /// <summary>Creates the window of the tool.</summary>
        /// <param name="viewModel">The state to show.</param>
        public ExtensionWindow(ExtensionViewModel viewModel)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

            InitializeComponent();

            DataContext = _viewModel;
        }

        private void OnPickSourcesClick(object sender, RoutedEventArgs e)
        {
            Pick(_viewModel.PickSources);
        }

        private void OnPickTargetsClick(object sender, RoutedEventArgs e)
        {
            Pick(_viewModel.PickTargets);
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
    }
}
