using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using GrpcProto.Merkez;
using GrpcProto.Saha;
using GrpcProto;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DS.GTS.Application;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

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

                using var channel = GrpcChannel.ForAddress("https://localhost:44327", new GrpcChannelOptions { HttpHandler = new GrpcWebHandler(httpHandler) });
                var client = new SahaSubscription.SahaSubscriptionClient(channel);
                var cts = new CancellationTokenSource();
                
                Console.WriteLine($"Single Call");
                var pingcall = client.Ping(new GrpcProto.Saha.PingRequest() { RequestStr = "Ping From Client" }, cancellationToken: cts.Token);
                Console.WriteLine($"{pingcall.ResponseStr}");

                Console.WriteLine($"//Server Side Streaming");
                var pingStramCall = client.PingServerStream(new GrpcProto.Saha.PingRequest() { RequestStr = "Ping From Client" }, cancellationToken: cts.Token);
                while (pingStramCall.ResponseStream.MoveNext(cts.Token).Result)
                    Console.WriteLine($"{pingStramCall.ResponseStream.Current.ResponseStr}");

                Console.WriteLine($"//Bidirectional Streaming");
                using var pingBothStreamCall = client.PingBothStream();
                for (int i = 0; i < 15; i++)
                {
                    await pingBothStreamCall.RequestStream.WriteAsync(new GrpcProto.Saha.PingRequest() { RequestStr = $"Ping from client #{i}" });
                }
                await pingBothStreamCall.RequestStream.CompleteAsync();
                while (pingBothStreamCall.ResponseStream.MoveNext(cts.Token).Result)
                    Console.WriteLine($"{pingBothStreamCall.ResponseStream.Current.ResponseStr}");

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
                //using var channel = GrpcChannel.ForAddress("https://gtsraporapi.dstrace.com/");
                using var channel = GrpcChannel.ForAddress("https://localhost:44325/");
                
                var client = new DS.GTS.Application.GrpcStream.GrpcStreamClient(channel);

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
