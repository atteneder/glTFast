#if NEWTONSOFT_JSON && (DEBUG || GLTFAST_USE_NEWTONSOFT_JSON)
using GLTFast.Schema;
using Newtonsoft.Json;

namespace GLTFast
{
    public class JsonNewtonsoftImplementation : IJsonImplementation
    {
        public Root ParseJson<T>(string json) where T : Root
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
#endif
