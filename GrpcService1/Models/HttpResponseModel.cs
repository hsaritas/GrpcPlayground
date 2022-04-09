using Newtonsoft.Json.Linq;

namespace GrpcService1
{
    public class HttpResponseModel
    {
        public int? StatusCode { get; set; }
        public JObject Headers { get; set; }
        public JObject Body { get; set; }
    }
}