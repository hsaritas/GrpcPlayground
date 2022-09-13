using Grpc.Core;
using GrpcProto.Saha;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrpcClient.Services
{
    public class SahaService : GrpcStreamSahaSubscription.GrpcStreamSahaSubscriptionBase
    {
        public override async Task<SahaResponse> BasicRequest(SahaRequest request, ServerCallContext context)
        {
            return new SahaResponse() { Message = "basic response" };
        }

        public override async Task<SahaResponse> StreamingRequest(IAsyncStreamReader<SahaRequest> requestStream, ServerCallContext context)
        {
            var requests = new List<SahaRequest>();
            while (await requestStream.MoveNext()
                && !context.CancellationToken.IsCancellationRequested)
            {
                requests.Add(requestStream.Current);
            }

            return new SahaResponse() { Message = $"request stream received count:[{requests.Count}]" };
        }

        public override async Task StreamingResponse(SahaRequest request, IServerStreamWriter<SahaResponse> responseStream, ServerCallContext context)
        {
            for (int i = 0; i < 5; i++)
            {
                await responseStream.WriteAsync(new SahaResponse() { Message = $"response # [{i}]" });
            }
        }

        public override async Task StreamingBoth(IAsyncStreamReader<SahaRequest> requestStream, IServerStreamWriter<SahaResponse> responseStream, ServerCallContext context)
        {
            var requests = new List<SahaRequest>();
            while (await requestStream.MoveNext()
                && !context.CancellationToken.IsCancellationRequested)
            {
                requests.Add(requestStream.Current);
            }

            for (int i = 0; i < 5; i++)
            {
                await responseStream.WriteAsync(new SahaResponse() { Message = $"request stream received count:[{requests.Count}] response # [{i}]" });
            }
        }

        public override async Task<TaskManagerResponse> TaskManager(TaskManagerRequest request, ServerCallContext context)
        {
            return new TaskManagerResponse() { CPU = 1, DISC = 2, RAM = 3 };
        }
    }
}
