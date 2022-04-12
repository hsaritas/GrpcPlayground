
using Grpc.Core;
using Grpc.Net.Client;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Task.Run(() => MainAsync(args)).Wait();
            Console.WriteLine($"Press any key :)");
            Console.ReadKey();
        }

        static async void MainAsync(string[] args)
        {
            try
            {
                using var channel = GrpcChannel.ForAddress("https://localhost:44381/");
                var client = new GrpcStream.GrpcStreamClient(channel);
                var cts = new CancellationTokenSource();
                using var streamingCall = client.GetMongoStream(new  MongoStreamRequest() { MethodFilter = "" }, cancellationToken: cts.Token);

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

        static async void MainAsync2(string[] args)
        {
            try
            {
                using var channel = GrpcChannel.ForAddress("https://localhost:5001");
                var client = new MongoSubscription.MongoSubscriptionClient(channel);
                //var reply = await client.SayHelloAsync(
                //                  new HelloRequest { Name = "GreeterClient" });
                //Console.WriteLine($"Greeting: Message:{reply.Message}, Name: {reply.Name} ");

                var cts = new CancellationTokenSource();
                using var streamingCall = client.GetMongoStream(new MongoSubscriptionRequest() { ClientId = 12345, FilterMethod= "oRtak" }, cancellationToken: cts.Token);

                while (streamingCall.ResponseStream.MoveNext(cts.Token).Result)
                {
                    BsonDocument doc = BsonSerializer.Deserialize<BsonDocument>(streamingCall.ResponseStream.Current.Message);
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
