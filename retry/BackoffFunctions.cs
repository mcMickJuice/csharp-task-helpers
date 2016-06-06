using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EagleOne.Retry
{
    public class BackoffFunctions
    {
        public static TimeSpan ExponentialBackoff(int count, TimeSpan interval)
        {
            var ms = interval.TotalMilliseconds * Math.Pow(10, count);
            return TimeSpan.FromMilliseconds(ms);
        }

        public static TimeSpan LinearBackoff(int count, TimeSpan interval)
        {
            var ms = interval.TotalMilliseconds*count;
            return TimeSpan.FromMilliseconds(ms);
        }
    }
}
