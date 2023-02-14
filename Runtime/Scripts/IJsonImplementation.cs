using GLTFast.Schema;

namespace GLTFast
{
    public interface IJsonImplementation
    {
        Root ParseJson<T>(string json) where T : Root;
    }
}
