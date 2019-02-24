using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

namespace DraggableTree.Controls
{
    [TemplatePart(Name = "Part_TextBlock", Type = typeof(TextBlock)),
     TemplatePart(Name = "Part_TextBox", Type = typeof(TextBox))]
    public class ExtendedTextBlock : Control
    {
        static ExtendedTextBlock()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ExtendedTextBlock), new FrameworkPropertyMetadata(typeof(ExtendedTextBlock)));
        }
        public override void OnApplyTemplate()
        {
            PreviewMouseDoubleClick += PreviewMouseDoubleClickEventHandler;
            base.OnApplyTemplate();
            _textBlock = GetTemplateChild("Part_TextBlock") as TextBlock;
            _textBox = GetTemplateChild("Part_TextBox") as TextBox;
            _textBlock.DataContext = this;
            _textBox.DataContext = this;
            _textBox.LostFocus += TextBox_LostFocus;
            _textBox.KeyDown += TextBox_KeyDown;
            ChangeVisibleLayout();
        }
        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            IsEditing = false;
            e.Handled = true;
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                _textChanged = false;
                _textBox.Text = _initialText;

                IsEditing = false;
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Enter)
            {
                IsEditing = false;
                e.Handled = true;
            }
        }
        private void PreviewMouseDoubleClickEventHandler(object sender, MouseButtonEventArgs e)
        {
            IsEditing = true;
            e.Handled = true;
        }
        private void ChangeVisibleLayout()
        {
            if (IsEditing)
            {
                _textBox.Visibility = Visibility.Visible;
                _textBox.SelectAll();
                _textBox.Focus();
                _textChanged = true;
                _textBlock.Visibility = Visibility.Collapsed;
                _initialText = Text;
                return;
            }

            if (_textChanged)
            {
                _textChanged = false;
                if (string.IsNullOrEmpty(_textBox.Text))
                {
                    _textBox.Text = _initialText;
                    Text = _initialText;
                }
            }
            _textBlock.Visibility = Visibility.Visible;
            _textBox.Visibility = Visibility.Collapsed;
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string),
            typeof(ExtendedTextBlock));

        public static readonly DependencyProperty IsEditingProperty =
            DependencyProperty.Register("IsEditing", typeof(bool), typeof(ExtendedTextBlock),
            new PropertyMetadata(false, IsEditingPropertyChanged));

        public static void IsEditingPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var s = sender as ExtendedTextBlock;
            if (s != null && s.IsInitialized)
                s.ChangeVisibleLayout();
        }

        public string Text
        {
            get { return GetValue(TextProperty).ToString(); }
            set { SetValue(TextProperty, value); }
        }
        public bool IsEditing
        {
            get { return (bool)GetValue(IsEditingProperty); }
            set { SetValue(IsEditingProperty, value); }
        }

        private string _initialText;
        private bool _textChanged = false;
        private TextBlock _textBlock;
        private TextBox _textBox;
    }
}
