using Grpc.Core;
using GrpcProto.Merkez;
using System.Threading.Tasks;

namespace GrpcService1.Services
{
    public class MerkezService : MerkezSubscription.MerkezSubscriptionBase
    {
        public override async Task<PingResponse> Ping(PingRequest request, ServerCallContext context)
        {
            return new PingResponse() { ResponseStr = $"PONG from MerkezService [{request.RequestStr}]" };
        }
    }
}
