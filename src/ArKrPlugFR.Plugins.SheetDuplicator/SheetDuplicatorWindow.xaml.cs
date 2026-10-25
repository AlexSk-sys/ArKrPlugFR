using System;
using System.Windows;

namespace ArKrPlugFR.Plugins.SheetDuplicator
{
    /// <summary>
    /// The sheet-duplicator window: the sheets of the current project with their
    /// tick boxes and copy counts, the content switches, and the single Duplicate
    /// button.
    /// <para>
    /// The window owns only interaction — which buttons were clicked and how the
    /// result is shown. Everything that reads or changes the model lives in the
    /// view model.
    /// </para>
    /// </summary>
    public partial class SheetDuplicatorWindow : Window
    {
        private readonly SheetDuplicatorViewModel _viewModel;

        /// <summary>Creates the window for the sheets of the current project.</summary>
        /// <param name="viewModel">The state to show.</param>
        public SheetDuplicatorWindow(SheetDuplicatorViewModel viewModel)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

            InitializeComponent();

            DataContext = _viewModel;
        }

        private void OnIncreaseClick(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is SheetRowViewModel row)
            {
                row.Increase();
            }
        }

        private void OnDecreaseClick(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is SheetRowViewModel row)
            {
                row.Decrease();
            }
        }

        private void OnDuplicateClick(object sender, RoutedEventArgs e)
        {
            // Nothing ticked means no transaction at all: the run is not even
            // started, and the message says what is missing.
            if (!_viewModel.HasSelection)
            {
                MessageBox.Show(
                    this,
                    _viewModel.Text.NothingSelected,
                    _viewModel.Text.WindowTitle,
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            SheetRunOutcome outcome;

            try
            {
                outcome = _viewModel.Duplicate();
            }
            catch (Exception exception)
            {
                _viewModel.SetReport(_viewModel.Text.GeneralFailure(exception.Message));

                MessageBox.Show(
                    this,
                    _viewModel.Text.GeneralFailure(exception.Message),
                    _viewModel.ReportCaption,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return;
            }

            _viewModel.SetReport(outcome.Message);

            MessageBox.Show(
                this,
                outcome.Message,
                _viewModel.ReportCaption,
                MessageBoxButton.OK,
                outcome.HasFailures ? MessageBoxImage.Warning : MessageBoxImage.Information);
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
