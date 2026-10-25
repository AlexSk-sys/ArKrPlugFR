using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using ArKrPlugFR.Ui.Views;

namespace ArKrPlugFR.Plugins.ViewTemplateTransfer
{
    /// <summary>
    /// The transfer window: the tree of transferable views, the list of projects the
    /// views can move into, and the transfer action.
    /// <para>
    /// The window owns only interaction: which row was clicked, which range a
    /// Shift-click covers, what the user picked as a target. Everything that reads or
    /// changes the model lives in the view model.
    /// </para>
    /// </summary>
    public partial class ViewTemplateTransferWindow : Window
    {
        private readonly TransferViewModel _viewModel;
        private TemplateNodeViewModel? _anchor;

        /// <summary>Creates the window for a view model.</summary>
        /// <param name="viewModel">The state to show.</param>
        public ViewTemplateTransferWindow(TransferViewModel viewModel)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

            InitializeComponent();

            DataContext = _viewModel;
        }

        private void OnCaptionMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                try
                {
                    DragMove();
                }
                catch (InvalidOperationException)
                {
                    // The button was released before the drag started; nothing to do.
                }
            }
        }

        private void OnHelpClick(object sender, RoutedEventArgs e)
        {
            ToolWindowPresenter.Show(
                _viewModel.Text.HelpTitle,
                string.Empty,
                _viewModel.Text.ResultCaption,
                _viewModel.Text.HelpText);
        }

        private void OnMinimizeClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void OnCloseCaptionClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnWindowPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
                e.Handled = true;
            }
        }

        private void OnTransferClick(object sender, RoutedEventArgs e)
        {
            TransferOutcome? outcome = _viewModel.TryTransfer(out string? validationError);

            if (outcome == null)
            {
                ToolWindowPresenter.Show(
                    _viewModel.WindowTitle,
                    _viewModel.TargetLine,
                    _viewModel.Text.ResultCaption,
                    validationError ?? string.Empty);

                return;
            }

            ToolWindowPresenter.Show(
                _viewModel.WindowTitle,
                _viewModel.TargetLine,
                _viewModel.Text.ResultCaption,
                _viewModel.Report);
        }

        private void OnTreePreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is not DependencyObject source)
            {
                return;
            }

            // The expander toggle of a group row belongs to the tree, not to the
            // selection logic; a click on it only expands or collapses.
            if (FindAncestor<ToggleButton>(source) != null)
            {
                return;
            }

            if (FindAncestor<TreeViewItem>(source)?.DataContext is not TreeNodeViewModel node)
            {
                return;
            }

            if (node is GroupNodeViewModel group)
            {
                group.IsChecked = group.IsChecked != true;
                e.Handled = true;
                return;
            }

            if (node is not TemplateNodeViewModel leaf)
            {
                return;
            }

            if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift && _anchor != null)
            {
                IReadOnlyList<TemplateNodeViewModel> visible = VisibleLeaves();

                int from = IndexOf(visible, _anchor);
                int to = IndexOf(visible, leaf);

                if (from >= 0 && to >= 0)
                {
                    bool value = _anchor.IsChecked;

                    for (int index = Math.Min(from, to); index <= Math.Max(from, to); index++)
                    {
                        visible[index].IsChecked = value;
                    }

                    e.Handled = true;
                    return;
                }
            }

            leaf.IsChecked = !leaf.IsChecked;
            _anchor = leaf;
            e.Handled = true;
        }

        /// <summary>
        /// Returns the view rows of the tree in visible flattened order; groups
        /// are skipped, and a collapsed group contributes nothing.
        /// </summary>
        private IReadOnlyList<TemplateNodeViewModel> VisibleLeaves()
        {
            var leaves = new List<TemplateNodeViewModel>();

            foreach (GroupNodeViewModel group in _viewModel.Groups)
            {
                if (Tree.ItemContainerGenerator.ContainerFromItem(group) is not TreeViewItem container)
                {
                    continue;
                }

                if (!container.IsExpanded)
                {
                    continue;
                }

                foreach (TemplateNodeViewModel child in group.Children)
                {
                    leaves.Add(child);
                }
            }

            return leaves;
        }

        private static int IndexOf(IReadOnlyList<TemplateNodeViewModel> leaves, TemplateNodeViewModel leaf)
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

        private static T? FindAncestor<T>(DependencyObject? current)
            where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T typed)
                {
                    return typed;
                }

                current = VisualTreeHelper.GetParent(current);
            }

            return null;
        }
    }
}
