using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using EagleOne.AppMonitoring;
using iSynaptic.Commons;

namespace EagleOne.Retry
{
    public interface IRetryStrategy
    {
        Task<Maybe<TResult>> RunAndRetry<TResult>(Func<Maybe<TResult>> actionToRun);
        Task<Maybe<TResult>> RunAndRetry<TResult>(Func<Task<Maybe<TResult>>> actionToRun);

        Task<Result<TValue, TObservation>> RunAndRetry<TValue, TObservation>(
            Func<Result<TValue, TObservation>> actionToRun);
        Task<Result<TValue, TObservation>> RunAndRetry<TValue, TObservation>(
            Func<Task<Result<TValue, TObservation>>> actionToRun);

        Task<Result<TValue, TObservation>> RunAndCheckShouldRetry<TValue, TObservation>(
            Func<Result<TValue, TObservation>> actionToRun) where TObservation : IRetryObservation;
        Task<Result<TValue, TObservation>> RunAndCheckShouldRetry<TValue, TObservation>(
            Func<Task<Result<TValue, TObservation>>> actionToRun) where TObservation : IRetryObservation;

        Task<bool> RunAndRetry(Func<bool> actionToRun);
        Task<bool> RunAndRetry(Func<Task<bool>> actionToRun);

        Task<Outcome<TObservation>> RunAndRetry<TObservation>(Func<Outcome<TObservation>> actionToRun);
        Task<Outcome<TObservation>> RunAndRetry<TObservation>(Func<Task<Outcome<TObservation>>> actionToRun);
        Task<Outcome<TObservation>> RunAndCheckShouldRetry<TObservation>(Func<Outcome<TObservation>> actionToRun) where TObservation : IRetryObservation;
        Task<Outcome<TObservation>> RunAndCheckShouldRetry<TObservation>(Func<Task<Outcome<TObservation>>> actionToRun) where TObservation : IRetryObservation;
    }
}
