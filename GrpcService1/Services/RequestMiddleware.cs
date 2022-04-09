using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrpcService1
{
    public class RequestMiddleware
    {
        private readonly RequestDelegate _next;
        private bool isFileUpload = false;

        public RequestMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Protocol == "HTTP/2")
            {
                await _next(context);
                return;
            }

            //DevMode da request başlangıç ve bitiş sürelerini hesaplayıp loglamak için StopWatch
            var sw = new Stopwatch();

            try
            {
                sw.Start();

                //var request = await GetHttpRequestModel(context);

                // Response objesinden Body okuyabilmek için geçici MemoryStream kullanıyoruz
                var originalResponseBodyStream = context.Response.Body;
                using (var ms = new MemoryStream())
                {
                    context.Response.Body = ms;

                    await _next(context);

                    var response = await GetHttpResponseModel(context, originalResponseBodyStream, ms);
                }
            }
            catch (Exception ex)
            {
                await context.Response.WriteAsync(ex.Message);
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
            finally
            {
                sw.Stop();
            }
        }

        /// <summary>
        /// Loglama için Request modelini sağlar
        /// </summary>
        /// <param name="context">HttpContext</param>
        private async Task<HttpRequestModel> GetHttpRequestModel(HttpContext context)
        {
            string body = null;
            if (context.Request.ContentType != null && context.Request.ContentType.Contains("multipart/form-data") && context.Request.Form.Files.Any())
                body = "";
            else
                body = await GetRequestBody(context);

            return new HttpRequestModel
            {
                Scheme = context?.Request?.Scheme,
                Host = context?.Request?.Host.Value,
                Method = context?.Request?.Method,
                Path = context?.Request?.Path,
                QueryString = context?.Request?.QueryString.Value,
                Headers = ObjectSerializerHelper<Dictionary<string, string>>.ToJObject(GetRequestHeaders(context)),
                Body = ObjectSerializerHelper.JsonToJObject(body),
                LocalAddress = context?.Connection?.LocalIpAddress?.MapToIPv4()?.ToString() + ":" + context?.Connection?.LocalPort.ToString(),
                RemoteAddress = context?.Connection?.RemoteIpAddress?.MapToIPv4()?.ToString() + ":" + context?.Connection?.RemotePort.ToString(),
                User = context?.User?.Identity?.Name,
            };
        }

        /// <summary>
        /// Loglama için Response modelini sağlar
        /// </summary>
        /// <param name="context">HttpContext</param>
        /// <param name="originalResponseBodyStream">Original response body stream</param>
        /// <param name="ms"></param>
        private async Task<HttpResponseModel> GetHttpResponseModel(HttpContext context, Stream originalResponseBodyStream, MemoryStream ms)
        {
            var body = await GetResponseBody(context, originalResponseBodyStream, ms);
            return new HttpResponseModel
            {
                StatusCode = context?.Response?.StatusCode,
                Headers = ObjectSerializerHelper<Dictionary<string, string>>.ToJObject(GetResponseHeaders(context)),
                Body = ObjectSerializerHelper.JsonToJObject(body)
            };
        }

        /// <summary>
        /// Request objesinden body streamini okuyup string olarak dönüyoruz
        /// </summary>
        /// <param name="context">HttpContext</param>
        /// <returns>String olarak Request Body</returns>
        private async Task<string> GetRequestBody(HttpContext context)
        {
            string bodyAsText = null;
            HttpRequestRewindExtensions.EnableBuffering(context.Request);
            if (context.Request.ContentLength != null && context.Request.ContentLength > 1024 * 100)
            {
                bodyAsText = "{'data':'request body length exceeded 100K limit'}";
            }
            else
            {
                var sr = new StreamReader(context.Request.Body, Encoding.UTF8);
                bodyAsText = await sr.ReadToEndAsync();
            }

            context.Request.Body.Seek(0, SeekOrigin.Begin);
            return bodyAsText;
        }

        /// <summary>
        /// Request objesinden header'daki key-value pair'ları dönüyoruz
        /// </summary>
        /// <param name="context">HttpContext</param>
        /// <returns>Request Header objesinden dictionary dönüyor</returns>
        private Dictionary<string, string> GetRequestHeaders(HttpContext context)
        {
            var headers = new Dictionary<string, string>();
            foreach (string key in context.Request.Headers.Keys)
            {
                context.Request.Headers.TryGetValue(key, out StringValues values);
                headers.Add(key, string.Join(",", values));
            }
            return headers;
        }

        /// <summary>
        /// Response objesinden body streamini okuyup string olarak dönüyoruz
        /// </summary>
        /// <param name="context">HttpContext</param>
        /// <param name="originalResponseBodyStream">Orijinal Response Body Stream objesi</param>
        /// <param name="ms"></param>
        /// <returns>String olarak Response Body</returns>
        private async Task<string> GetResponseBody(HttpContext context, Stream originalResponseBodyStream, MemoryStream ms)
        {
            string bodyAsText = "";
            if (context.Response?.ContentType == null)
                return null;

            if (context.Response.ContentLength != null && context.Response.ContentLength > 1024 * 100)
            {
                bodyAsText = "{'data':'response body length exceeded 100K limit'}";
            }
            else
            {
                if (context.Response.ContentType.Contains("json", StringComparison.InvariantCultureIgnoreCase)
                    || context.Response.ContentType.Contains("text", StringComparison.InvariantCultureIgnoreCase)
                    || context.Response.ContentType.Contains("html", StringComparison.InvariantCultureIgnoreCase)
                    || context.Response.ContentType.Contains("xml", StringComparison.InvariantCultureIgnoreCase)
                 )
                {
                    context.Response.Body.Seek(0, SeekOrigin.Begin);
                    bodyAsText = await new StreamReader(context.Response.Body).ReadToEndAsync();
                }
            }
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            await ms.CopyToAsync(originalResponseBodyStream);
            return bodyAsText;
        }

        /// <summary>
        /// Reponse objesinden header'daki key-value pair'ları dönüyoruz
        /// </summary>
        /// <param name="context">HttpContext</param>
        /// <returns>Reponse Header objesinden KeyValuePair dictionary dönüyor</returns>
        private Dictionary<string, string> GetResponseHeaders(HttpContext context)
        {
            var headers = new Dictionary<string, string>();
            foreach (string key in context.Response.Headers.Keys)
            {
                context.Request.Headers.TryGetValue(key, out StringValues values);
                if (!string.IsNullOrEmpty(values))
                    headers.Add(key, string.Join(",", values));
            }
            return headers;
        }
              
    }
}