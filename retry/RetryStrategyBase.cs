using System;
using System.Threading.Tasks;
using iSynaptic.Commons;
using System.Linq;

namespace EagleOne.Retry
{
    public abstract class RetryStrategyBase : IRetryStrategy
    {
        protected static bool MaybeResultCheck<TResult>(Maybe<TResult> maybe)
        {
            return maybe.HasValue;
        }

        protected static bool ResultResultCheck<TValue, TObservation>(Result<TValue, TObservation> result)
        {
            return result.WasSuccessful;
        }

        protected static bool OutcomeResultCheck<TObservation>(Outcome<TObservation> result)
        {
            return result.WasSuccessful;
        }

        protected Func<Task<T>> WrapSyncAction<T>(Func<T> actionToWrap)
        {
            return () => Task.FromResult(actionToWrap());
        }

        public Task<Maybe<TResult>> RunAndRetry<TResult>(Func<Maybe<TResult>> actionToRun)
        {
            var asyncWrap = WrapSyncAction(actionToRun);

            return RunAndRetryImplementation(asyncWrap, MaybeResultCheck, result => Maybe.NoValue);
        }

        public Task<Maybe<TResult>> RunAndRetry<TResult>(Func<Task<Maybe<TResult>>> actionToRun)
        {
            return RunAndRetryImplementation(actionToRun, MaybeResultCheck, result => Maybe.NoValue);
        }

        public Task<Result<TValue, TObservation>> RunAndRetry<TValue, TObservation>(Func<Task<Result<TValue, TObservation>>> actionToRun)
        {
            return RunAndRetryImplementation(actionToRun, ResultResultCheck, result => result.Fail());
        }
        public Task<Result<TValue, TObservation>> RunAndRetry<TValue, TObservation>(Func<Result<TValue, TObservation>> actionToRun)
        {
            var asyncWrap = WrapSyncAction(actionToRun);
            return RunAndRetryImplementation(asyncWrap, ResultResultCheck, result => result.Fail());
        }

        public Task<Result<TValue, TObservation>> RunAndCheckShouldRetry<TValue, TObservation>(Func<Task<Result<TValue, TObservation>>> actionToRun)
             where TObservation : IRetryObservation
        {
            return RunAndRetryImplementation(actionToRun, ResultResultCheck, result => result.Fail(), result => result.Observations.All(x => x.ShouldRetry() == true));
        }
        public Task<Result<TValue, TObservation>> RunAndCheckShouldRetry<TValue, TObservation>(Func<Result<TValue, TObservation>> actionToRun)
             where TObservation : IRetryObservation
        {
            var asyncWrap = WrapSyncAction(actionToRun);
            return RunAndRetryImplementation(asyncWrap, ResultResultCheck, result => result.Fail(), result => result.Observations.All(x => x.ShouldRetry() == true));
        }

        public Task<bool> RunAndRetry(Func<bool> actionToRun)
        {
            var asyncWrap = WrapSyncAction(actionToRun);

            return RunAndRetryImplementation(asyncWrap, result => result, result => false);
        }

        public async Task<bool> RunAndRetry(Func<Task<bool>> actionToRun)
        {
            return await RunAndRetryImplementation(actionToRun, result => result, result => false);
        }

        public Task<Outcome<TObservation>> RunAndRetry<TObservation>(Func<Outcome<TObservation>> actionToRun)
        {
            var asyncWrap = WrapSyncAction(actionToRun);
            return RunAndRetryImplementation(asyncWrap, OutcomeResultCheck, result => result.Fail());
        }

        public Task<Outcome<TObservation>> RunAndCheckShouldRetry<TObservation>(Func<Outcome<TObservation>> actionToRun)
             where TObservation : IRetryObservation
        {
            var asyncWrap = WrapSyncAction(actionToRun);
            return RunAndRetryImplementation(asyncWrap, OutcomeResultCheck, result => result.Fail(), result => result.Observations.All(x => x.ShouldRetry() == true));
        }

        public Task<Outcome<TObservation>> RunAndRetry<TObservation>(Func<Task<Outcome<TObservation>>> actionToRun)
        {
            return RunAndRetryImplementation(actionToRun, OutcomeResultCheck, result => result.Fail());
        }

        public Task<Outcome<TObservation>> RunAndCheckShouldRetry<TObservation>(Func<Task<Outcome<TObservation>>> actionToRun)
             where TObservation : IRetryObservation
        {
            return RunAndRetryImplementation(actionToRun, OutcomeResultCheck, result => result.Fail(), result => result.Observations.All(x => x.ShouldRetry() == true));
        }

        protected abstract Task<TResult> RunAndRetryImplementation<TResult>(Func<Task<TResult>> actionToRun
            , Func<TResult, bool> isSuccess, Func<TResult, TResult> failureResult, Func<TResult, bool> shouldRetry = null);
    }
}