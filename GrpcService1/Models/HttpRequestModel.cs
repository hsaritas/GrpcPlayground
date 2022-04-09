using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrpcService1
{
    public class HttpRequestModel
    {
        public string Scheme { get; set; }
        public string Host { get; set; }
        public string Method { get; set; }
        public string Path { get; set; }
        public string QueryString { get; set; }
        public JObject Headers { get; set; }
        public JObject Body { get; set; }
        public string LocalAddress { get; set; }
        public string RemoteAddress { get; set; }
        public string User { get; set; }
    }
}
