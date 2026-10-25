using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ArKrPlugFR.Core.Diagnostics;
using ArKrPlugFR.Ui.Views;

namespace ArKrPlugFR.Plugins.DuplicateViews
{
    /// <summary>
    /// The window of the tool: the three trees with their ticks and counters, the
    /// template picker and the run.
    /// <para>
    /// The window owns only interaction — which row was clicked, which range a
    /// Shift-click covers. Everything that reads or changes the model lives in the
    /// view model.
    /// </para>
    /// </summary>
    public partial class DuplicateViewsWindow : Window
    {
        private readonly DuplicateViewsViewModel _viewModel;
        private CopyLeafViewModel? _anchor;

        /// <summary>Creates the window of the tool.</summary>
        /// <param name="viewModel">The state to show.</param>
        public DuplicateViewsWindow(DuplicateViewsViewModel viewModel)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

            InitializeComponent();

            DataContext = _viewModel;
        }

        private void OnWindowPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                // Escape clears the ticks rather than closing the window, so a mistaken
                // selection costs nothing.
                _viewModel.ClearAll();
                _anchor = null;
                e.Handled = true;
            }
        }

        private void OnIncreaseClick(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is CopyLeafViewModel leaf)
            {
                leaf.Increase();
            }
        }

        private void OnDecreaseClick(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is CopyLeafViewModel leaf)
            {
                leaf.Decrease();
            }
        }

        private void OnTreePreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is not DependencyObject source)
            {
                return;
            }

            // The counter buttons and the count field belong to the row's counter, not
            // to its tick.
            if (FindAncestor<Button>(source) != null || FindAncestor<TextBox>(source) != null)
            {
                return;
            }

            object? node = FindNode(source);

            if (node is CopyGroupViewModel group)
            {
                group.IsChecked = group.IsChecked != true;
                e.Handled = true;
                return;
            }

            if (node is not CopyLeafViewModel leaf)
            {
                return;
            }

            if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift
                && _anchor != null
                && _anchor.Tree == leaf.Tree)
            {
                IReadOnlyList<CopyLeafViewModel> ordered = _viewModel.VisibleLeaves(leaf.Tree);

                int from = IndexOf(ordered, _anchor);
                int to = IndexOf(ordered, leaf);

                if (from >= 0 && to >= 0)
                {
                    bool state = _anchor.IsChecked;

                    for (int index = Math.Min(from, to); index <= Math.Max(from, to); index++)
                    {
                        ordered[index].IsChecked = state;
                    }

                    e.Handled = true;
                    return;
                }
            }

            leaf.IsChecked = !leaf.IsChecked;
            _anchor = leaf;
            e.Handled = true;
        }

        private void OnDuplicateClick(object sender, RoutedEventArgs e)
        {
            if (!_viewModel.HasSelection)
            {
                MessageBox.Show(
                    this,
                    _viewModel.Text.NothingSelected + Environment.NewLine + Environment.NewLine +
                        _viewModel.Text.LogFolder(AppPaths.LogDirectory),
                    _viewModel.WindowTitle,
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            bool applyTemplate = true;

            if (!_viewModel.ChosenTemplateFits())
            {
                // The chosen template cannot shape every ticked kind; the user decides
                // whether the run goes on without it.
                MessageBoxResult answer = MessageBox.Show(
                    this,
                    _viewModel.Text.TemplateMismatch,
                    _viewModel.Text.TemplateSection,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (answer != MessageBoxResult.Yes)
                {
                    return;
                }

                applyTemplate = false;
            }

            DuplicateRunResult result;

            try
            {
                result = _viewModel.Run(applyTemplate);
            }
            catch (Exception exception)
            {
                _viewModel.SetReport(_viewModel.Text.LogFolder(AppPaths.LogDirectory));

                MessageBox.Show(
                    this,
                    exception.Message,
                    _viewModel.Text.ReportCaption,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return;
            }

            string report = _viewModel.BuildReport(result);
            _viewModel.SetReport(report);

            MessageBox.Show(
                this,
                report,
                _viewModel.Text.ReportCaption,
                MessageBoxButton.OK,
                result.Failures.Count > 0 || result.Created == 0
                    ? MessageBoxImage.Warning
                    : MessageBoxImage.Information);
        }

        private void OnHelpClick(object sender, RoutedEventArgs e)
        {
            ToolWindowPresenter.Show(
                _viewModel.Text.HelpTitle,
                string.Empty,
                _viewModel.Text.ReportCaption,
                _viewModel.Text.HelpText);
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private static object? FindNode(DependencyObject? source)
        {
            DependencyObject? current = source;

            while (current != null)
            {
                if (current is FrameworkElement element
                    && (element.DataContext is CopyLeafViewModel || element.DataContext is CopyGroupViewModel))
                {
                    return element.DataContext;
                }

                current = VisualTreeHelper.GetParent(current);
            }

            return null;
        }

        private static T? FindAncestor<T>(DependencyObject? source)
            where T : DependencyObject
        {
            DependencyObject? current = source;

            while (current != null)
            {
                if (current is T match)
                {
                    return match;
                }

                current = VisualTreeHelper.GetParent(current);
            }

            return null;
        }

        private static int IndexOf(IReadOnlyList<CopyLeafViewModel> leaves, CopyLeafViewModel leaf)
        {
            for (int index = 0; index < leaves.Count; index++)
            {
                if (ReferenceEquals(leaves[index], leaf))
                {
                    return index;
                }
            }

            return -1;
        }
    }
}
