using System.Diagnostics;

namespace Backend
{
    public static class GlobalClock
    {
        #region Private Static Properties

        private static Stopwatch stopwatch;

        #endregion

        #region Static Constructor

        static GlobalClock()
        {
            stopwatch = new Stopwatch();
        }

        #endregion

        #region Public Static Properties

        public static long ElapsedMilliseconds
        {
            get
            {
                return stopwatch.ElapsedMilliseconds;
            }
        }

        #endregion

        #region Public Static Methods

        public static void Start()
        {
            stopwatch.Start();
        }

        #endregion
    }
}