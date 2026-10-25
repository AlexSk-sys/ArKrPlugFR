using System;
using System.Windows;
using ArKrPlugFR.Ui.Views;

namespace ArKrPlugFR.Plugins.AssignWorksets
{
    /// <summary>
    /// The window of the tool: the rule list and the run button.
    /// <para>
    /// The window owns only interaction; everything that reads or changes the model
    /// lives in the view model. The rules are written to the user's settings folder as
    /// they are edited, so closing the window never loses them.
    /// </para>
    /// </summary>
    public partial class WorksetWindow : Window
    {
        private readonly WorksetViewModel _viewModel;

        /// <summary>Creates the window of the tool.</summary>
        /// <param name="viewModel">The state to show.</param>
        public WorksetWindow(WorksetViewModel viewModel)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

            InitializeComponent();

            DataContext = _viewModel;
        }

        private void OnAddRuleClick(object sender, RoutedEventArgs e)
        {
            _viewModel.AddRule();
        }

        private void OnRemoveRuleClick(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is WorksetRow row)
            {
                _viewModel.RemoveRule(row);
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
