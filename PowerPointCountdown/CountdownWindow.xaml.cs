using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace PowerPointCountdown
{
    /// <summary>
    /// CountdownWindow.xaml 的交互逻辑
    /// </summary>
    public partial class CountdownWindow : Window
    {
        private CountdownWindowViewModel ViewModel => (CountdownWindowViewModel) DataContext;

        public CountdownWindow()
        {
            InitializeComponent();
            ViewModel.PostInitialize();
            VisualStateManager.GoToElementState((FrameworkElement)this.Content, "Stopped", true);
            PropertyChangedEventManager.AddHandler(ViewModel, (_, e) =>
            {
                if (ViewModel.IsCountdownStarted)
                {
                    VisualStateManager.GoToElementState((FrameworkElement) this.Content, "Started", true);
                    //Dispatcher.InvokeAsync(UpdateDigitHeight, DispatcherPriority.Background);
                }
                else
                {
                    VisualStateManager.GoToElementState((FrameworkElement) this.Content, "Stopped", true);
                }
            }, nameof(ViewModel.IsCountdownStarted));
            this.Height = 70;
        }

        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            this.Left += e.HorizontalChange;
            this.Top += e.VerticalChange;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            ViewModel.Dispose();
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            CountdownMinutesTextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            ((Control)sender).ContextMenu.IsOpen = true;
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            // Fix data context.
            ((Control) sender).DataContext = this.DataContext;
        }

        private void ThisWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.HeightChanged)
            {
                UpdateDigitHeight();
            }
        }

        private void UpdateDigitHeight()
        {
            var h = CounterDigitPanel.ActualHeight;
            //Debug.Print(h.ToString());
            if (h > 0)
            {
                CounterDigitTransform.ScaleX =
                    CounterDigitTransform.ScaleY =
                        Math.Max(0.1, (this.Height - PresentationIndicatorLabel.ActualHeight)/h);
            }
        }
    }
}
