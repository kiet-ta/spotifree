using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotifree.Helpers
{
    public static class TimeFormatHelper
    {
        public static string ToHhMmSs(double seconds)
        {
            if (seconds < 0)
                seconds = 0;

            var ts = TimeSpan.FromSeconds(seconds);

            return ts.ToString(@"hh\:mm\:ss");
        }
    }
}
