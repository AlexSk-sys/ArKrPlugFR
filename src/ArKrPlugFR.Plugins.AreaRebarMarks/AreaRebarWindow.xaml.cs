using System;
using System.Windows;
using ArKrPlugFR.Ui.Views;

namespace ArKrPlugFR.Plugins.AreaRebarMarks
{
    /// <summary>
    /// The window of the tool: the scope, the parameter and the run button.
    /// <para>
    /// The window owns only interaction; everything that reads or changes the model lives
    /// in the view model. The window hides itself while the user picks in the model and
    /// returns after the pick, and the result of a run is shown in the shared result
    /// window.
    /// </para>
    /// </summary>
    public partial class AreaRebarWindow : Window
    {
        private readonly AreaRebarViewModel _viewModel;

        /// <summary>Creates the window of the tool.</summary>
        /// <param name="viewModel">The state to show.</param>
        public AreaRebarWindow(AreaRebarViewModel viewModel)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

            InitializeComponent();

            DataContext = _viewModel;
        }

        private void OnPickSourcesClick(object sender, RoutedEventArgs e)
        {
            Hide();

            try
            {
                _viewModel.PickSources();
            }
            finally
            {
                Show();
                Activate();
            }
        }

        private void OnApplyClick(object sender, RoutedEventArgs e)
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
    }
}
