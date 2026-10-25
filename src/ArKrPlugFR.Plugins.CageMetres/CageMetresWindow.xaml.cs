using System;
using System.Windows;
using ArKrPlugFR.Ui.Views;

namespace ArKrPlugFR.Plugins.CageMetres
{
    /// <summary>
    /// The window of the tool: the picks, the parameter names and the run.
    /// <para>
    /// The window owns only interaction; everything that reads or changes the model lives
    /// in the view model. The window hides itself while the user picks in the model and
    /// returns after the pick, and the result of a run is shown in the shared result window.
    /// </para>
    /// </summary>
    public partial class CageMetresWindow : Window
    {
        private readonly CageMetresViewModel _viewModel;

        /// <summary>Creates the window of the tool.</summary>
        /// <param name="viewModel">The state to show.</param>
        public CageMetresWindow(CageMetresViewModel viewModel)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

            InitializeComponent();

            DataContext = _viewModel;
        }

        private void OnPickLinesClick(object sender, RoutedEventArgs e)
        {
            Pick(_viewModel.PickLines);
        }

        private void OnPickCageClick(object sender, RoutedEventArgs e)
        {
            Pick(_viewModel.PickCage);
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
