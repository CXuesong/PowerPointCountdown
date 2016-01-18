using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Core;
using Microsoft.Office.Interop.PowerPoint;
using PptApplication = Microsoft.Office.Interop.PowerPoint.Application;

namespace PowerPointCountdown
{

    public class PowerPointManager : IDisposable
    {
        private PptApplication app = new PptApplication();
        private SlideShowWindow _ActiveSlideShow;
        private Presentation _ActivePresentation;
        public event EventHandler<SlideShowEventArgs> SlideShowBegin;
        public event EventHandler ActiveSlideShowEnd;
        public event EventHandler ActiveSlideShowChanged;

        /// <summary>
        /// Gets/sets the target slideshow window of operations.
        /// </summary>
        public SlideShowWindow ActiveSlideShow
        {
            get { return _ActiveSlideShow; }
            set
            {
                if (_ActiveSlideShow != value)
                {
                    _ActiveSlideShow = value;
                    // Holds the corresponding presentation to the specified slideshow window.
                    // This object will later be used to determine if ActiveSlideShow has finished,
                    // because when SlideShowWindow has finised, it will be destructed and cannot be accessed.
                    _ActivePresentation = _ActiveSlideShow?.Presentation;
                    OnActiveSlideShowChanged();
                }
            }
        }

        public IEnumerable<SlideShowWindow> SlideShows => app.SlideShowWindows.Cast<SlideShowWindow>();

        public void ExitPresentation()
        {
            if (ActiveSlideShow == null) throw new InvalidOperationException();
            try
            {
                Debug.Print("Exit presentation called.");
                ActiveSlideShow.View.Exit();
            }
            catch (COMException)
            {
                // Handles COMException with error code 0x80004005
                // object does not exist
                ActiveSlideShow = null;
            }
        }

        public PowerPointManager()
        {
            app.SlideShowBegin += OnAppOnSlideShowBegin;
            app.SlideShowEnd += OnAppOnSlideShowEnd;
        }

        private void OnAppOnSlideShowEnd(Presentation presentation)
        {
            if (_ActivePresentation == presentation)
            {
                OnActiveSlideShowEnd();
                ActiveSlideShow = null;
            }
        }

        private void OnAppOnSlideShowBegin(SlideShowWindow wnd)
        {
            //Debug.Print(app.SlideShowWindows.Count + "");
            OnSlideShowBegin(new SlideShowEventArgs(wnd));
        }

        protected virtual void OnSlideShowBegin(SlideShowEventArgs e)
        {
            SlideShowBegin?.Invoke(this, e);
        }

        protected virtual void OnActiveSlideShowChanged()
        {
            ActiveSlideShowChanged?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnActiveSlideShowEnd()
        {
            ActiveSlideShowEnd?.Invoke(this, EventArgs.Empty);
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
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。
                if (app != null)
                {
                    try
                    {
                        app.SlideShowBegin -= OnAppOnSlideShowBegin;
                        app.SlideShowEnd -= OnAppOnSlideShowEnd;
                        if (app.Visible == MsoTriState.msoFalse) app.Quit();
                    }
                    catch (COMException)
                    {

                    }
                }
                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~PowerPointManager() {
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

    public class SlideShowEventArgs : EventArgs
    {
        public SlideShowEventArgs(SlideShowWindow slideShowWindow)
        {
            SlideShowWindow = slideShowWindow;
        }

        public SlideShowWindow SlideShowWindow { get; }
    }
}
