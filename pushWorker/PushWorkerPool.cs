using System;
using System.Threading;
using System.Threading.Tasks;
using iSynaptic.Commons;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace EagleOne.Distributor
{
    public abstract class PushWorkerPool<TPushRequest,TPushResult> : IPushWorkerPool<TPushRequest, TPushResult>
    {
        private readonly IPushWorkerFactory<TPushRequest, TPushResult> _factory;
        private readonly int _maxWorkerCount;
        private int _currentWorkersInUseCount;
        private ConcurrentQueue<TaskCompletionSource<IPushWorker<TPushRequest, TPushResult>>> _isAvailableTasks;
        private CancellationToken _cancelToken;

        private bool CanIssueNewWorker => _currentWorkersInUseCount < _maxWorkerCount;

        protected PushWorkerPool(int maxWorkerCount
            ,  IPushWorkerFactory<TPushRequest, TPushResult> factory)
        {
            Guard.Ensure(maxWorkerCount != 0, nameof(maxWorkerCount), "You must provide a number greater than 0");
            _isAvailableTasks = new ConcurrentQueue<TaskCompletionSource<IPushWorker<TPushRequest, TPushResult>>>();
            _maxWorkerCount = maxWorkerCount;
            _factory = factory;
        }

        public Task<IPushWorker<TPushRequest, TPushResult>> GetWorkerAsync(CancellationToken cancelToken)
        {
            _cancelToken = cancelToken;
            _cancelToken.Register(OnCancellation);

            if (CanIssueNewWorker)
            {
                IncrementWorkersInUse();

                var worker = CreateWorker();
                return Task.FromResult(worker);
            }

            //No workers are available. So create blocking task that returns when a worker is available
            var isAvailableTask = new TaskCompletionSource<IPushWorker<TPushRequest, TPushResult>>();
            _isAvailableTasks.Enqueue(isAvailableTask);
            return isAvailableTask.Task;

        }

        private IPushWorker<TPushRequest, TPushResult> CreateWorker()
        {
            return _factory.GetWorker(WorkerActionCompleted);
        }

        private void OnCancellation()
        {
            TaskCompletionSource<IPushWorker<TPushRequest, TPushResult>> taskCompletionSource = null;
            while ( _isAvailableTasks.TryDequeue(out taskCompletionSource))
            {
                taskCompletionSource.TrySetCanceled();
            }
            
        }

        private void WorkerActionCompleted(TPushResult result)
        {
            TaskCompletionSource<IPushWorker<TPushRequest, TPushResult>> taskCompletionSource = null;
            if (_isAvailableTasks.TryDequeue(out taskCompletionSource))
            {
                taskCompletionSource.TrySetResult(CreateWorker());
            } else
            {
                DecrementWorkersInUse();
            }
        }

        private void IncrementWorkersInUse()
        {
            _currentWorkersInUseCount++;
        }

        private void DecrementWorkersInUse()
        {
            _currentWorkersInUseCount--;
        }
    }
}