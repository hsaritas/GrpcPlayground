using Grpc.Core;
using GrpcProto.Saha;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GrpcClient.Services
{
    public class SahaService : SahaSubscription.SahaSubscriptionBase
    {
        public override Task<PingResponse> Ping(PingRequest request, ServerCallContext context)
        {
            return Task.FromResult(new PingResponse() { ResponseStr = $"PONG from SahaService [{request.RequestStr}]" });
        }
    }
}
