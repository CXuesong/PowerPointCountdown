using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.PowerPoint;

namespace PowerPointCountdown
{

    public class PowerPointManager
    {
        private Application app = new Application();
        private SlideShowWindow _ActiveSlideShow;
        public event EventHandler<SlideShowBeginEventArgs> SlideShowBegin;
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
                    OnActiveSlideShowChanged();
                }
            }
        }

        public void ExitPresentation()
        {
            if (ActiveSlideShow == null) throw new InvalidOperationException();
            ActiveSlideShow.View.Exit();
        }

        public PowerPointManager()
        {
            app.SlideShowBegin += wnd =>
            {
                Debug.Print(app.SlideShowWindows.Count + "");
                OnSlideShowBegin(new SlideShowBeginEventArgs(wnd));
            };
            app.SlideShowEnd += presentation =>
            {
                if (ActiveSlideShow?.View?.State == PpSlideShowState.ppSlideShowDone) ActiveSlideShow = null;
            };
        }

        protected virtual void OnSlideShowBegin(SlideShowBeginEventArgs e)
        {
            SlideShowBegin?.Invoke(this, e);
        }

        protected virtual void OnActiveSlideShowChanged()
        {
            ActiveSlideShowChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public class SlideShowBeginEventArgs : EventArgs
    {
        public SlideShowBeginEventArgs(SlideShowWindow slideShowWindow)
        {
            SlideShowWindow = slideShowWindow;
        }

        public SlideShowWindow SlideShowWindow { get; }
    }
}
