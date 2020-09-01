using WpfDotNetDemoSamples.DataModels;
using WpfDotNetDemoSamples.ViewModels;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WpfDotNetDemoSamples.DataModels.Models;

namespace WpfDotNetDemoSamples.CustomControls
{
    /// <summary>
    /// Interaction logic for TreeViewInPlaceEditControl.xaml
    /// </summary>
    public partial class TreeViewInPlaceEditControl : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private TreeViewDemoViewModel _viewModel;
        Point _lastMouseDown;
        EventTypes draggedItem, _target;

        bool isInEditMode = false;
        public bool IsInEditMode
        {
            get { return isInEditMode; }
            set
            {
                isInEditMode = value;
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null)
                    handler(this, new PropertyChangedEventArgs("IsInEditMode"));
            }
        }

        // text in a text box before editing - to enable cancelling changes
        string oldText;

        public TreeViewInPlaceEditControl()
        {
            InitializeComponent();
            _viewModel = new TreeViewDemoViewModel();
            DataContext = _viewModel;
        }

        private void treeView_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _lastMouseDown = e.GetPosition(treeView);
            }
        }

        private bool CheckGridSplitter(UIElement element)
        {
            if (element is GridSplitter)
            {
                return true;
            }

            GridSplitter GridSplitter = FindParent<GridSplitter>(element);

            if (GridSplitter != null)
            {
                return true;
            }
            return false;

        }

        private void treeView_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    UIElement element = e.OriginalSource as UIElement;
                    Point currentPosition = e.GetPosition(treeView);

                    //if ((Math.Abs(currentPosition.X - _lastMouseDown.X) > 10.0) ||
                    //    (Math.Abs(currentPosition.Y - _lastMouseDown.Y) > 10.0))
                    //{
                    draggedItem = (EventTypes)treeView.SelectedItem;

                    if (draggedItem != null)
                    {
                        DragDropEffects finalDropEffect = DragDrop.DoDragDrop(treeView, treeView.SelectedValue,
                            DragDropEffects.Move);
                        //Checking target is not null and item is dragging(moving)
                        if ((finalDropEffect == DragDropEffects.Move) && (_target != null))
                        {
                            // A Move drop was accepted
                            if (draggedItem.EventTypeName != _target.EventTypeName)
                            {
                                MoveItem(draggedItem, _target);
                                _viewModel.EventTypesUI.EventTypes.Remove(draggedItem);
                                _target = null;
                                draggedItem = null;
                            }
                        }
                    }
                    //}
                }
            }
            catch (Exception)
            {
            }
        }

        private void treeView_DragOver(object sender, DragEventArgs e)
        {
            try
            {
                Point currentPosition = e.GetPosition(treeView);

                if ((Math.Abs(currentPosition.X - _lastMouseDown.X) > 10.0) ||
                    (Math.Abs(currentPosition.Y - _lastMouseDown.Y) > 10.0))
                {
                    // Verify that this is a valid drop and then store the drop target
                    EventTypes item = GetNearestContainer(e.OriginalSource as UIElement);
                    if (CheckDropTarget(draggedItem, item))
                    {
                        e.Effects = DragDropEffects.Move;
                    }
                    else
                    {
                        e.Effects = DragDropEffects.None;
                    }
                }
                e.Handled = true;
            }
            catch (Exception)
            {
            }
        }

        private void treeView_Drop(object sender, DragEventArgs e)
        {
            try
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;

                // Verify that this is a valid drop and then store the drop target
                EventTypes TargetItem = GetNearestContainer(e.OriginalSource as UIElement);
                if (TargetItem != null && draggedItem != null)
                {
                    _target = TargetItem;
                    e.Effects = DragDropEffects.Move;

                }
            }
            catch (Exception)
            {
            }
        }

        private bool CheckDropTarget(EventTypes _sourceItem, EventTypes _targetItem)
        {
            //Check whether the target item is meeting your condition
            bool _isEqual = false;

            if (_sourceItem.EventTypeName != _targetItem.EventTypeName)
            {
                _isEqual = true;
            }

            return _isEqual;
        }

        private void MoveItem(EventTypes _sourceItem, EventTypes _targetItem)
        {
            //Asking user wether he want to drop the dragged TreeViewItem here or not
            if (MessageBox.Show("Would you like to drop " + _sourceItem.EventTypeName + " into " + _targetItem.EventTypeName + "", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    _targetItem.Children.Add(_sourceItem);
                }
                catch (Exception)
                {

                }
            }
        }

        private EventTypes GetNearestContainer(UIElement element)
        {
            // Walk up the element tree to the nearest tree view item.
            TreeViewItem UIContainer = FindParent<TreeViewItem>(element);
            EventTypes NVContainer = null;

            if (UIContainer != null)
            {
                NVContainer = UIContainer.DataContext as EventTypes;
            }
            return NVContainer;
        }

        private void treeView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F2)
            {
                IsInEditMode = true;
            }
        }

        private void treeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            IsInEditMode = false;
        }

        private void textBlockHeader_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (FindTreeItem(e.OriginalSource as DependencyObject).IsSelected)
            {
                IsInEditMode = true;
                e.Handled = true;       // otherwise the newly activated control will immediately loose focus
            }
        }

        static TreeViewItem FindTreeItem(DependencyObject source)
        {
            while (source != null && !(source is TreeViewItem))
                source = VisualTreeHelper.GetParent(source);
            return source as TreeViewItem;
        }

        private void editableTextBoxHeader_LostFocus(object sender, RoutedEventArgs e)
        {
            IsInEditMode = false;
        }

        private void editableTextBoxHeader_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb.IsVisible)
            {
                tb.Focus();
                tb.SelectAll();
                oldText = tb.Text;      // back up - for possible cancelling
            }
        }
        
        private void editableTextBoxHeader_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                IsInEditMode = false;
            }
            if (e.Key == Key.Escape)
            {
                var tb = sender as TextBox;
                tb.Text = oldText;
                IsInEditMode = false;
            }
        }

        private static Parent FindParent<Parent>(DependencyObject child)
                where Parent : DependencyObject
        {
            DependencyObject parentObject = child;
            parentObject = VisualTreeHelper.GetParent(parentObject);

            //check if the parent matches the type we're looking for
            if (parentObject is Parent || parentObject == null)
            {
                return parentObject as Parent;
            }
            else
            {
                //use recursion to proceed with next level
                return FindParent<Parent>(parentObject);
            }
        }

    }
}
