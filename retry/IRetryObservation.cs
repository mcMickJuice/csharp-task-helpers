using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EagleOne.Retry
{
    public interface IRetryObservation
    {
        bool ShouldRetry();
    }
}
