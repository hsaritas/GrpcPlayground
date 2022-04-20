using Grpc.Core;
using GrpcProto.Saha;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrpcClient.Services
{
    public class SahaService : SahaSubscription.SahaSubscriptionBase
    {
        public override async Task<PingResponse> Ping(PingRequest request, ServerCallContext context)
        {
            return new PingResponse() { ResponseStr = $"PONG from SahaService [{request.RequestStr}] " };
        }

        public override async Task PingBothStream(IAsyncStreamReader<PingRequest> requestStream, IServerStreamWriter<PingResponse> responseStream, ServerCallContext context)
        {
            var requests = new List<PingRequest>();
            while (await requestStream.MoveNext()
                && !context.CancellationToken.IsCancellationRequested)
            {
                requests.Add(requestStream.Current);               
            }

            for (int i = 0; i < requests.Count; i++)
            {
                await responseStream.WriteAsync(new PingResponse() { ResponseStr = $"PONG #{i} from SahaService [{requests[i].RequestStr}]" });
            }
        }
        public override async Task PingServerStream(PingRequest request, IServerStreamWriter<PingResponse> responseStream, ServerCallContext context)
        {
            for (int i = 0; i < 5; i++) 
            {
                await responseStream.WriteAsync(new PingResponse() { ResponseStr = $"PONG #{i} from SahaService [{request.RequestStr}]" } );
            }
            
        }
    }
}
