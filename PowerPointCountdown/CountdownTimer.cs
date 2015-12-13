using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPointCountdown
{
    public class CountdownTimer
    {
        private Stopwatch myStopwatch = new Stopwatch();
        private TimeSpan _CachedTimeRemains;

        /// <summary>
        /// Gets/sets the time remaining to be counted.
        /// </summary>
        public TimeSpan TimeRemains
        {
            get { return _CachedTimeRemains - myStopwatch.Elapsed; }
            set
            {
                _CachedTimeRemains = value;
                if (myStopwatch.IsRunning)
                    myStopwatch.Restart();
                else
                    myStopwatch.Reset();
            }
        }

        /// <summary>
        /// Starts or continues the countdown.
        /// </summary>
        public void Start()
        {
            myStopwatch.Start();
        }

        /// <summary>
        /// Pauses or stops the countdown.
        /// </summary>
        public void Stop()
        {
            myStopwatch.Stop();
            // Flush the remaining time into TimeRemains cache.
            TimeRemains = TimeRemains;
        }

        public bool IsRunnning => myStopwatch.IsRunning;

        /// <summary>
        /// Determines whether <see cref="TimeRemains"/> is or is less than zero.
        /// </summary>
        public bool IsTimeUp => TimeRemains <= TimeSpan.Zero;
    }
}
