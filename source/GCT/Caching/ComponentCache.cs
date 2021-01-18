using Core;
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

        public bool IsCached(ICachable cachable, out string cachedFilePath) {
            cachedFilePath = Path.Combine(this._cachePath, cachable.GetCacheID());
            return File.Exists(cachedFilePath);
        }
    }
}
