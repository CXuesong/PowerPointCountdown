﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
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
            VisualStateManager.GoToElementState((FrameworkElement)this.Content, "Stopped", true);
            PropertyChangedEventManager.AddHandler(ViewModel, (_, e) =>
            {
                if (ViewModel.IsCountdownStarted)
                {
                    VisualStateManager.GoToElementState((FrameworkElement) this.Content, "Started", true);
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

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            ViewModel.Dispose();
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            CountdownMinutesTextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
        }
    }
}
