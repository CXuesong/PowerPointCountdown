using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PowerPointCountdown
{
    static class Utility
    {
        public static void ReportException(Exception ex)
        {
            MessageBox.Show(ex.ToString(), null, MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }
    }
}
