using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Win32;
using PowerPointCountdown.Properties;

namespace PowerPointCountdown
{
    public class CountdownWindowViewModel : BindableBase, IDisposable
    {
        private string _RemainingTimeText;
        private double _CountdownMinutes = Settings.Default.LastCountdownMinutes;
        private bool _IsMonitorEnabled = true;
        private bool _IsCountdownStarted;
        private string _PresentationName;

        private ImageSource _BackgroundImageSource;
        private DelegateCommand _StartCountdownCommand;
        private DelegateCommand _StopCountdownCommand;
        private DelegateCommand _OptionsBackgroundImageCommand;
        private DelegateCommand _OptionsClearBackgroundImageCommand;

        private Timer myTimer = new Timer();
        private CountdownTimer cdt = new CountdownTimer();
        private PowerPointManager ppt = new PowerPointManager();

        public Dispatcher Dispatcher { get; set; }

        #region Countdown Logic

        public void StartCountdown()
        {
            if (cdt.TimeRemains == TimeSpan.Zero)
                cdt.TimeRemains = TimeSpan.FromMinutes(CountdownMinutes);
            IsCountdownPaused = false;
            IsCountdownStarted = true;
        }

        public void StopCountdown()
        {
            IsCountdownPaused = true;
            cdt.TimeRemains = TimeSpan.Zero;
            UpdateTimerDisplay();
            IsCountdownStarted = false;
        }

        public bool IsCountdownStarted
        {
            get { return _IsCountdownStarted; }
            private set { SetProperty(ref _IsCountdownStarted, value); }
        }

        public bool IsCountdownPaused
        {
            get { return !cdt.IsRunnning; }
            set
            {
                // i.e. cdt.IsRunnning != !value
                Debug.Print("Paused : " + value);
                if (cdt.IsRunnning == value)
                {
                    Debug.Print("Status Switch");
                    if (value)
                    {
                        cdt.Stop();
                        myTimer.Enabled = false;
                    }
                    else
                    {
                        cdt.Start();
                        myTimer.Enabled = true;
                    }
                    OnPropertyChanged(nameof(IsCountdownPaused));
                }
            }
        }

        public TimeSpan RemainingTime => cdt.TimeRemains;

        #endregion

        #region UI

        public ImageSource BackgroundImageSource => _BackgroundImageSource;

        /// <summary>
        /// The time, set by user, in minutes to be counted down.
        /// </summary>
        public double CountdownMinutes
        {
            get { return _CountdownMinutes; }
            set
            {
                if (value < 0) value = 0;
                SetProperty(ref _CountdownMinutes, value);
            }
        }

        /// <summary>
        /// Decides whether to monitor the PowerPoint for any presentation to be played.
        /// </summary>
        public bool IsMonitorEnabled
        {
            get { return _IsMonitorEnabled; }
            set
            {
                if (SetProperty(ref _IsMonitorEnabled, value))
                {
                    ppt.ActiveSlideShow = value ? ppt.SlideShows.FirstOrDefault() : null;
                }
            }
        }

        public string PresentationName
        {
            get { return _PresentationName; }
            private set { SetProperty(ref _PresentationName, value); }
        }


        public DelegateCommand StartCountdownCommand
        {
            get
            {
                if (_StartCountdownCommand == null)
                {
                    _StartCountdownCommand = new DelegateCommand(() =>
                    {
                        if (!cdt.IsRunnning) StartCountdown();
                    });
                }
                return _StartCountdownCommand;
            }
        }

        public DelegateCommand StopCountdownCommand
        {
            get
            {
                if (_StopCountdownCommand == null)
                {
                    _StopCountdownCommand = new DelegateCommand(() =>
                    {
                        StopCountdown();
                    });
                }
                return _StopCountdownCommand;
            }
        }

        public DelegateCommand OptionsBackgroundImageCommand
        {
            get
            {
                if (_OptionsBackgroundImageCommand == null)
                {
                    _OptionsBackgroundImageCommand = new DelegateCommand(() =>
                    {
                        var ofd = new OpenFileDialog
                        {
                            Filter = "*.bmp;*.jpg;*.jpeg;*.png;*.tif;*.tiff|*.bmp;*.jpg;*.jpeg;*.png;*.tif;*.tiff"
                        };
                        if (ofd.ShowDialog() == true)
                        {
                            var newUri = new Uri(ofd.FileName, UriKind.Absolute);
                            var startupUri = new Uri(Directory.GetCurrentDirectory() + "\\", UriKind.Absolute);
                            var relativeUri = startupUri.MakeRelativeUri(newUri);
                            Settings.Default.BackgroundImageSource = relativeUri;
                            ApplySettings();
                        }
                    });
                }
                return _OptionsBackgroundImageCommand;
            }
        }

        public DelegateCommand OptionsClearBackgroundImageCommand
        {
            get
            {
                if (_OptionsClearBackgroundImageCommand == null)
                {
                    _OptionsClearBackgroundImageCommand = new DelegateCommand(() =>
                    {
                        Settings.Default.BackgroundImageSource = null;
                        ApplySettings();
                    });
                }
                return _OptionsClearBackgroundImageCommand;
            }
        }

        #endregion

        private bool postInitializeCalled;

        public void PostInitialize()
        {
            if (postInitializeCalled) throw new InvalidOperationException();
            postInitializeCalled = true;
            ApplySettings();
        }

        public void ApplySettings()
        {
            try
            {
                var imageUri = Settings.Default.BackgroundImageSource;
                var startupUri = new Uri(Directory.GetCurrentDirectory() + "\\", UriKind.Absolute);
                if (imageUri == null)
                {
                    _BackgroundImageSource = null;
                }
                else
                {
                    if (!imageUri.IsAbsoluteUri) imageUri = new Uri(startupUri, imageUri);
                    _BackgroundImageSource = new BitmapImage(imageUri);
                }
                OnPropertyChanged(nameof(BackgroundImageSource));
            }
            catch (Exception ex)
            {
                Utility.ReportException(ex);
            }
        }

        private void UpdateTimerDisplay()
        {
            OnPropertyChanged(nameof(RemainingTime));
            if (cdt.IsRunnning)
            {
                if (cdt.IsTimeUp)
                {
                    //StatusLabel.Text = "时间到";
                }
                else
                {
                    //StatusLabel.Text = "倒计时";
                }
            }
            else
            {
                //StatusLabel.Text = "就绪";
            }
        }

        private void InvokeAsync(Action act)
        {
            Dispatcher.BeginInvoke(act);
        }

        public CountdownWindowViewModel()
        {
            Dispatcher = Dispatcher.CurrentDispatcher;
            ppt.SlideShowBegin += (_, e) =>
            {
                if (!IsMonitorEnabled) return;
                ppt.ActiveSlideShow = e.SlideShowWindow;
            };
            ppt.ActiveSlideShowChanged += (_, e) =>
            {
                InvokeAsync(() =>
                {
                    if (ppt.ActiveSlideShow != null)
                    {
                        PresentationName = ppt.ActiveSlideShow.Presentation.Name;
                        StartCountdown();
                    }
                    else
                    {
                        PresentationName = null;
                    }
                });
            };
            ppt.ActiveSlideShowEnd += (_, e) =>
            {
                Debug.Print("Active Slideshow Exited.");
                InvokeAsync(() =>
                {
                    IsCountdownPaused = true;
                });
            };
            myTimer.Elapsed += (_, e) =>
            {
                Debug.Print("Timer Pulse");
                InvokeAsync(() =>
                {
                    Debug.Print("Async Pulse");
                    UpdateTimerDisplay();
                    if (cdt.IsTimeUp)
                    {
                        Debug.Print("Time Up");
                        StopCountdown();
                        if (IsMonitorEnabled && ppt.ActiveSlideShow != null)
                        {
                            Debug.Print("Exit Presentation");
                            ppt.ExitPresentation();
                        }
                    }
                });
            };
            StopCountdown();
            myTimer.Interval = 200;
        }

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)。
                    myTimer.Dispose();
                    ppt.Dispose();
                    // Save settings.
                    Settings.Default.LastCountdownMinutes = CountdownMinutes;
                }
                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。
                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~CountdownWindowViewModel() {
        //   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
