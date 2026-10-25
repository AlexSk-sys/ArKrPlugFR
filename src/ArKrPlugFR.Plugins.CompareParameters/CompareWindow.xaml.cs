using System;
using System.Windows;
using ArKrPlugFR.Ui.Views;

namespace ArKrPlugFR.Plugins.CompareParameters
{
    /// <summary>
    /// The window of the tool: the two picks, the comparison and the copy button.
    /// <para>
    /// The window owns only interaction; everything that reads or changes the model lives
    /// in the view model. The window hides itself while the user picks in the model and
    /// returns after the pick, and the result of a run is shown in the shared result
    /// window.
    /// </para>
    /// </summary>
    public partial class CompareWindow : Window
    {
        private readonly CompareViewModel _viewModel;

        /// <summary>Creates the window of the tool.</summary>
        /// <param name="viewModel">The state to show.</param>
        public CompareWindow(CompareViewModel viewModel)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

            InitializeComponent();

            DataContext = _viewModel;
        }

        private void OnPickSourceClick(object sender, RoutedEventArgs e)
        {
            Pick(_viewModel.PickSource);
        }

        private void OnPickTargetClick(object sender, RoutedEventArgs e)
        {
            Pick(_viewModel.PickTarget);
        }

        private void OnCopyClick(object sender, RoutedEventArgs e)
        {
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
