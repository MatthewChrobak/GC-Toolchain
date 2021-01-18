using Core;
using GCTPlugin.Serialization;
using System.IO;

namespace GCT.Caching
{
    public class ComponentCache
    {
        private const string CachePath = "cache";
        private readonly string _cachePath;

        public ComponentCache(ExecutionParameters parameters) {
            this._cachePath = parameters.GetFolderPath(CachePath, "temp");
        }

        public bool IsCached(string id, out string cachedFilePath) {
            cachedFilePath = Path.Combine(this._cachePath, id);
            return File.Exists(cachedFilePath);
        }

        internal void Cache(string id, IJsonSerializable serializable) {

        }
    }
}
