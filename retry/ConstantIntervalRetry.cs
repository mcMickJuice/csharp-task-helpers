using System;
using System.Threading;
using System.Threading.Tasks;
using EagleOne.AppMonitoring;

namespace EagleOne.Retry
{
    public class ConstantIntervalRetry : RetryStrategyBase, IConstantIntervalRetryStrategy
    {
        private readonly TimeSpan _waitInterval;
        private readonly CancellationToken _cancelToken;
        private readonly Func<TimeSpan, bool> _withinTimeAllottedFunc;

        /// <summary>
        /// Constructs ConstantIntervalRetry
        /// </summary>
        /// <param name="withinTimeAllotedFunc">Predicate to determine if, based on time elapsed, if action is still within allotted time. Returns true if
        /// retry action should still be performed</param>
        /// <param name="waitInterval">Interval at which action should be retried</param>
        /// <param name="cancelToken">Cancellation Token to exit Retry loop</param>
        public ConstantIntervalRetry(Func<TimeSpan, bool> withinTimeAllotedFunc, TimeSpan waitInterval, CancellationToken cancelToken)
        {
            //TODO add guards
            _withinTimeAllottedFunc = withinTimeAllotedFunc;
            _waitInterval = waitInterval;
            _cancelToken = cancelToken;
        }
        
//        private static bool MaybeResultCheck<TResult>(Maybe<TResult> maybe)
//        {
//            return maybe.HasValue;
//        }

        private bool ShouldRetry(int numberOfRetries)
        {
            return _withinTimeAllottedFunc(TimeSpan.FromMilliseconds(_waitInterval.TotalMilliseconds * numberOfRetries));
        }

        protected override async Task<TResult> RunAndRetryImplementation<TResult>(Func<Task<TResult>> actionToRun
            , Func<TResult, bool> isSuccess, Func<TResult, TResult> failureResult, Func<TResult, bool> shouldRetryFunction = null)
        {
            TResult result;
            var numberOfRetries = 0;
            do
            {
                result = await actionToRun();
                if (isSuccess(result))
                {
                    return result;
                }
                else if (shouldRetryFunction != null && shouldRetryFunction(result) == false)
                {
                    break;
                }

                numberOfRetries++;
                //we want cancel exception to propogate to caller. This token will be cancelled due to app stopping. Failed result
                //here is not what we want therefore we won't catch the exception here and break
                await Task.Delay(_waitInterval, _cancelToken);
            } while (ShouldRetry(numberOfRetries));

            return failureResult(result);
        }
    }
}