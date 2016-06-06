using System.Threading;
using System.Threading.Tasks;

namespace EagleOne.Distributor
{
    public interface IPushWorkerPool<TPushRequest,TPushResult>
    {
        Task<IPushWorker<TPushRequest, TPushResult>> GetWorkerAsync(CancellationToken cancelToken);
    }
}