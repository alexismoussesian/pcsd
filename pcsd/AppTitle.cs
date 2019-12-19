using System;
using System.Threading;
using System.Threading.Tasks;

namespace pcsd
{
    class AppTitle
    {
        private static bool _inProgress = false;

        public static void StartProgress()
        {
            if (_inProgress) return;
            _inProgress = true;
            var t = Task.Run(() => AnimateProgressBar());
        }

        private static void AnimateProgressBar()
        {
            const string title = "PureCloud Stat";
            const int progressLength = 20;
            var progressValue = 0;
            while (_inProgress)
            {
                if (progressValue >= progressLength) progressValue = 0;
                var progressDisplay = $"{title} - ";
                for (var i = 0; i < progressLength; i++)
                {
                    if (i < progressValue) progressDisplay += "|";
                }
                Console.Title = progressDisplay;
                progressValue ++;
                Thread.Sleep(200);
            }
            Console.Title = $"{title} - finished";
        }

        public static void StopProgress()
        {
            if (!_inProgress) return;
            _inProgress = false;

        }
    }
}
