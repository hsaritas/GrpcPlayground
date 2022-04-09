using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace GrpcService1
{
    public static class ObjectSerializerHelper
    {
        public static JObject JsonToJObject(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return new JObject();
            }

            var obj = new ObjectNotSerialized { ObjString = json };
            try
            {
                if (json.StartsWith("{"))
                    return JObject.Parse(json);
                else if (json.StartsWith("["))
                    return new JObject(new JProperty("Array", JsonToJArray(json)));
            }
            catch (Exception ex)
            {
                obj.ObjError += ex.Message;
                return JObject.Parse(JsonConvert.SerializeObject(obj));
            }
            obj.ObjError += "EndOfFunction";
            return JObject.Parse(JsonConvert.SerializeObject(obj));
        }

        public static JArray JsonToJArray(string json)
        {
            if (!string.IsNullOrEmpty(json))
                return JArray.Parse(json);

            return new JArray();
        }
    }
    public class ObjectNotSerialized
    {
        public string ObjString { get; set; }
        public string ObjError { get; set; }
    }
    public static class ObjectSerializerHelper<T>
    {
        public static string Serialize(T obj)
        {
            try
            {
                return JsonConvert.SerializeObject(obj,
                    new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Error,
                        ContractResolver = new DynamicContractResolver()
                    });
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static JObject ToJObject(T t)
        {
            var json = Serialize(t);
            if (!string.IsNullOrEmpty(json))
                return JObject.Parse(json);

            return new JObject();
        }

        public static T DeSerialize(string objStr)
        {
            if (string.IsNullOrEmpty(objStr))
                return default(T);
            return JsonConvert.DeserializeObject<T>(objStr);
        }
    }
}
