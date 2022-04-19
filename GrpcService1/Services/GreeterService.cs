using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcProto;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrpcService1
{
    public class GreeterService : MongoSubscription.MongoSubscriptionBase
    {
        private readonly ILogger<GreeterService> _logger;
        public GreeterService(ILogger<GreeterService> logger)
        {
            _logger = logger;
        }

        public override async Task GetMongoStream(MongoSubscriptionRequest request, IServerStreamWriter<MongoSubscriberEvent> responseStream, ServerCallContext context)
        {
            try
            {
                var client = new MongoClient("mongodb://192.168.141.111:27017");
                var database = client.GetDatabase("DSGTS");
                var collection = database.GetCollection<BsonDocument>("DS.GTS.API");

                var builder = Builders<ChangeStreamDocument<BsonDocument>>.Filter;
                var filter = builder.Where(x => x.OperationType == ChangeStreamOperationType.Insert);

                if (string.IsNullOrEmpty(request.FilterMethod) == false)
                {
                    filter &= builder.Regex("FullDocument.Properties.HttpLogModel.Request.Path", new BsonRegularExpression(new Regex(request.FilterMethod, RegexOptions.IgnoreCase)));
                }

                var pipeline = new IPipelineStageDefinition[]
                    {
                        PipelineStageDefinitionBuilder.Match(filter)
                    };
                var options = new ChangeStreamOptions { FullDocument = ChangeStreamFullDocumentOption.UpdateLookup };

                using var cursor = collection.Watch<ChangeStreamDocument<BsonDocument>>(pipeline, options);

                var enumerator = cursor.ToEnumerable().GetEnumerator();
                while (enumerator.MoveNext() && !context.CancellationToken.IsCancellationRequested)
                {
                    ChangeStreamDocument<BsonDocument> doc = enumerator.Current;

                    Console.WriteLine();
                    var mongoEvent = new MongoSubscriberEvent
                    {
                        DateTimeStamp = Timestamp.FromDateTime(DateTime.UtcNow),
                        Id = request.ClientId,
                        Message = doc.FullDocument.ToJson()
                    };

                    _logger.LogInformation("MongoEvent Sent");

                    await responseStream.WriteAsync(mongoEvent);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HelloReply
            {
                Message = "Hello " + request.Name,
                Name = "My Name is " + request.Name,
            });
        }
    }
}
