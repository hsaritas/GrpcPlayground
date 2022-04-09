using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GrpcService1
{
    public class DynamicContractResolver : DefaultContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            IList<JsonProperty> properties = base.CreateProperties(type, memberSerialization);

            // yalnızca basit türlerin serialize edilmesi, collection vb. türlerin omit edilmesi için
            properties =
                properties.Where(p => p.DeclaringType.GetInterfaces().Length == 0).ToList();

            return properties;
        }
    }
}
