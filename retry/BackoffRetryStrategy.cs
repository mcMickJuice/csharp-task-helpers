using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iSynaptic.Commons;

namespace EagleOne.Retry
{
    public class BackoffRetryStrategy : RetryStrategyBase, IBackoffRetryStrategy
    {
        private readonly Func<int, TimeSpan, TimeSpan> _intervalToWaitFunc;
        private readonly TimeSpan _waitInterval;
        private readonly int _timesToRetry;

        public BackoffRetryStrategy(Func<int, TimeSpan, TimeSpan> intervalToWaitFunc, TimeSpan waitInterval, int timesToRetry)
        {
            Guard.NotNull(intervalToWaitFunc, $"{nameof(intervalToWaitFunc)}");
            Guard.NotNull(waitInterval, $"{nameof(waitInterval)}");

            _intervalToWaitFunc = intervalToWaitFunc;
            _waitInterval = waitInterval;
            _timesToRetry = timesToRetry;
        }

        private bool ShouldRetry(int numberOfRetries)
        {
            return numberOfRetries <= _timesToRetry;
        }

        protected override async Task<TResult> RunAndRetryImplementation<TResult>(Func<Task<TResult>> actionToRun, Func<TResult, bool> isSuccess, Func<TResult, TResult> failureResult, Func<TResult, bool> shouldRetryFunction = null)
        {
            TResult result;
            var numberOfRetries = 0;
            bool shouldRetry;

            do
            {
                result = await actionToRun();
                if (isSuccess(result))
                {
                    return result;
                }
                else if(shouldRetryFunction != null && shouldRetryFunction(result) == false)
                {
                    break;
                }

                var interval = _intervalToWaitFunc(numberOfRetries, _waitInterval);
                numberOfRetries++;
                shouldRetry = ShouldRetry(numberOfRetries);
                if (shouldRetry) await Task.Delay(interval);
            } while (shouldRetry);

            return failureResult(result);
        }
    }
}
