using System;

namespace EagleOne.Distributor
{
    public interface IPushWorkerFactory<in T, TResult>
    {
        IPushWorker<T, TResult> GetWorker(Action<TResult> onCompletion);
    }
}