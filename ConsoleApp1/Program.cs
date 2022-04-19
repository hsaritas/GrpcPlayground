using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using GrpcProto.Merkez;
using GrpcProto.Saha;
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

            Task.Run(() => MainAsync(args)).Wait();
            Task.Run(() => MainAsync2(args)).Wait();
            Console.WriteLine($"Press any key :)");
            Console.ReadKey();
        }

        static async void MainAsync(string[] args)
        {
            try
            {
                using var channel = GrpcChannel.ForAddress("https://localhost:5001");
                var client = new MerkezSubscription.MerkezSubscriptionClient(channel);

                var cts = new CancellationTokenSource();
                var pingCall = client.Ping(new GrpcProto.Merkez.PingRequest() { RequestStr="Ping From Client"}, cancellationToken: cts.Token);
                Console.WriteLine($"{pingCall.ResponseStr}");
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
                var httpHandler = new HttpClientHandler();
                // Return `true` to allow certificates that are untrusted/invalid
                httpHandler.ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;


                using var channel = GrpcChannel.ForAddress("https://localhost:44327", new GrpcChannelOptions { HttpHandler = new GrpcWebHandler(httpHandler) });
                var client = new SahaSubscription.SahaSubscriptionClient(channel);

                var cts = new CancellationTokenSource();
                var pingCall = client.Ping(new GrpcProto.Saha.PingRequest() { RequestStr = "Ping From Client" }, cancellationToken: cts.Token);
                Console.WriteLine($"{pingCall.ResponseStr}");
            }
            catch (Exception ex)
            {

                Console.WriteLine($"{ex.Message}");
            }
        }
    }
}
