using Newtonsoft.Json.Linq;

namespace GCTPlugin.Serialization
{
    public interface IJsonSerializable
    {
        void SetFromJson(JObject node);
        JObject GetJson();
    }
}
