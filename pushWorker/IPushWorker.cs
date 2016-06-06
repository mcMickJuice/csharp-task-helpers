using System.Threading.Tasks;
using EagleOne.Request;

namespace EagleOne.Distributor
{
    public interface IPushWorker<in TRequest, TPushResult>
    {
        Task<TPushResult> Push(TRequest itemToPush);
    }
}