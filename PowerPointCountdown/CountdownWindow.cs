using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PowerPointCountdown
{
    public partial class CountdownWindow : Form
    {
        private CountdownTimer cdt = new CountdownTimer();
        private PowerPointManager ppt = new PowerPointManager();
        private double dpiY;

        public CountdownWindow()
        {
            InitializeComponent();
            using (var bmp = this.CreateGraphics())
            {
                dpiY = bmp.DpiY;
            }
            CountdownTimerLabel_Resize(CountdownTimerLabel, EventArgs.Empty);
            UpdateTimerDisplay();
            ppt.SlideShowBegin += (_, e) =>
            {
                ppt.ActiveSlideShow = e.SlideShowWindow;
            };
            ppt.ActiveSlideShowChanged += (_, e) =>
            {
                this.BeginInvoke((Action) (() =>
                {
                     if (ppt.ActiveSlideShow != null)
                    {
                        this.Text = "正在演示 - " + ppt.ActiveSlideShow.Presentation.Name;
                        if (StartButton.Enabled) StartButton.PerformClick();
                    }
                    else
                    {
                        this.Text = "";
                    }
                }));
            };
        }

        private void UpdateTimerDisplay()
        {
            CountdownTimerLabel.Text = cdt.TimeRemains.ToString(@"hh\:mm\:ss");
            if (cdt.IsRunnning)
            {
                if (cdt.IsTimeUp)
                {
                    StatusLabel.Text = "时间到";
                }
                else
                {
                    StatusLabel.Text = "倒计时";
                }
            }
            else
            {
                StatusLabel.Text = "就绪";
            }
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            cdt.TimeRemains = TimeSpan.FromMinutes((double)PresetMinutesUpDown.Value);
            cdt.Start();
            CountdownTimer.Enabled = true;
            StartButton.Enabled = false;
            PauseButton.Enabled = StopButton.Enabled = true;
        }

        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            UpdateTimerDisplay();
            if (cdt.IsTimeUp)
            {
                StopButton.PerformClick();
                if (ppt.ActiveSlideShow != null) ppt.ExitPresentation();
            }
        }

        private void PauseButton_Click(object sender, EventArgs e)
        {
            if (cdt.IsRunnning)
            {
                cdt.Stop();
                CountdownTimer.Enabled = false;
                PauseButton.Text = "继续";
            }
            else
            {
                cdt.Start();
                CountdownTimer.Enabled = true;
                PauseButton.Text = "暂停";
            }
            UpdateTimerDisplay();
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            cdt.Stop();
            cdt.TimeRemains = TimeSpan.Zero;
            CountdownTimer.Enabled = false;
            UpdateTimerDisplay();
            StartButton.Enabled = true;
            PauseButton.Enabled = StopButton.Enabled = false;
        }

        private void CountdownTimerLabel_Resize(object sender, EventArgs e)
        {
            CountdownTimerLabel.Font = new Font(CountdownTimerLabel.Font.FontFamily, (float) (CountdownTimerLabel.Height/dpiY*72));
        }
    }
}
