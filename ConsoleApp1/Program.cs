using DS.GTS.Application;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using GrpcProto.Merkez;
using GrpcProto.Saha;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {

            //Task.Run(() => GrpcMerkez(args)).Wait();
            Task.Run(() => GrpcSaha(args)).Wait();
            //Task.Run(() => GrpcGTS(args)).Wait();
            Console.WriteLine($"Press any key :)");
            Console.ReadKey();
        }

        static async void GrpcMerkez(string[] args)
        {
            try
            {
                using var channel = GrpcChannel.ForAddress("https://localhost:5001");
                var client = new MerkezSubscription.MerkezSubscriptionClient(channel);

                var cts = new CancellationTokenSource();
                var pingCall = await client.PingAsync(new GrpcProto.Merkez.PingRequest() { RequestStr="Ping From Client"}, cancellationToken: cts.Token);
                Console.WriteLine($"{pingCall.ResponseStr}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
            }
        }

        static async void GrpcSaha(string[] args)
        {
            try
            {
                var httpHandler = new HttpClientHandler();
                // Return `true` to allow certificates that are untrusted/invalid
                httpHandler.ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

                //using var channel = GrpcChannel.ForAddress("https://localhost:44327/", new GrpcChannelOptions { HttpHandler = new GrpcWebHandler(httpHandler) });
                //using var channel = GrpcChannel.ForAddress("https://localhost:44327/");
                
                using var channel = GrpcChannel.ForAddress("https://localhost:55623/", new GrpcChannelOptions { HttpHandler = new GrpcWebHandler(httpHandler) });
                //using var channel = GrpcChannel.ForAddress("https://localhost:55623/");

                var client = new GrpcStreamSahaSubscription.GrpcStreamSahaSubscriptionClient(channel);
                var cts = new CancellationTokenSource();
                
                Console.WriteLine($"Single Call");
                var pingcall = client.BasicRequest(new GrpcProto.Saha.SahaRequest() { PageIndex = 0, PageSize = 10, IsDescending = true }, cancellationToken: cts.Token);
                Console.WriteLine($"{pingcall.Message}");

                Console.WriteLine($"//Server Side Streaming");
                var pingStramCall = client.StreamingResponse(new GrpcProto.Saha.SahaRequest() { PageIndex = 0, PageSize = 10, IsDescending = true }, cancellationToken: cts.Token);
                while (pingStramCall.ResponseStream.MoveNext(cts.Token).Result)
                    Console.WriteLine($"{pingStramCall.ResponseStream.Current.Message}");

                Console.WriteLine($"//Bidirectional Streaming");
                using var pingBothStreamCall = client.StreamingBoth();
                for (int i = 0; i < 15; i++)
                {
                    await pingBothStreamCall.RequestStream.WriteAsync(new GrpcProto.Saha.SahaRequest() { PageIndex = i, PageSize = 10, IsDescending = true });
                }
                await pingBothStreamCall.RequestStream.CompleteAsync();
                while (pingBothStreamCall.ResponseStream.MoveNext(cts.Token).Result)
                    Console.WriteLine($"{pingBothStreamCall.ResponseStream.Current.Message}");

            }
            catch (Exception ex)
            {

                Console.WriteLine($"{ex.Message}");
            }
        }

        static async void GrpcGTS(string[] args)
        {
            try
            {
                using var channel = GrpcChannel.ForAddress("https://gtsraporapi.dstrace.com/");
                //using var channel = GrpcChannel.ForAddress("http://localhost:55620/");
                
                var client = new GrpcStream.GrpcStreamClient(channel);

                var cts = new CancellationTokenSource();
                using var streamingCall = client.GetMongoStream(new MongoStreamRequest() { MethodFilter = "", Database = "DSGTS", Collection = "DS.GTS.API" }, cancellationToken: cts.Token);

                while (streamingCall.ResponseStream.MoveNext(cts.Token).Result)
                {
                    BsonDocument doc = BsonSerializer.Deserialize<BsonDocument>(streamingCall.ResponseStream.Current.JsonData);
                    Console.WriteLine($"{doc["Properties"]["HttpLogModel"]["Request"]["Path"]}");
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine($"{ex.Message}");
            }
        }
    }
}
