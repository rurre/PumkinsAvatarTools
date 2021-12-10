#if PUMKIN_DEV
using Pumkin.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Pumkin.AvatarTools.MaterialPreview
{
    /// <summary>
    /// Creates a material cache in the Assets folder that allows you to associate a material with a cached copy of it.
    /// The cached copy has the GUID of the original material as it's name, which allows us to find the original again.
    /// </summary>
    public class PumkinsMaterialCache
    {
        public const string DEFAULT_CACHE_PATH = "_MaterialCache";

        string CachePath
        {
            get
            {
                if(_cachePath == null)
                    _cachePath = DEFAULT_CACHE_PATH;
                return _cachePath;
            }

            set => _cachePath = value;
        }
        string _cachePath;

        Dictionary<Material, Material> cache = new Dictionary<Material, Material>();

        private PumkinsMaterialCache() { }

        /// <summary>
        /// Creates the cache
        /// </summary>
        /// <param name="cachePath">Path for the cache inside the assets folder. Shouldn not start with "Assets"</param>
        /// <param name="materials">Initial materials to cache</param>
        public PumkinsMaterialCache(string cachePath = null, params Material[] materials)
        {
            var invalid = Path.GetInvalidPathChars()
                .Union(new char[] { '.' })
                .ToArray();

            if(cachePath != null && cachePath.IndexOfAny(invalid) != -1)
            {
                PumkinsAvatarTools.Log($"Cache path <b>{cachePath}</b> contains invalid characters. Using default path", LogType.Warning);
                CachePath = DEFAULT_CACHE_PATH;
            }

            CachePath = string.IsNullOrWhiteSpace(cachePath) ? DEFAULT_CACHE_PATH : cachePath;
            UpdateCache(materials);
        }

        /// <summary>
        /// Gets or creates a material in the cache folder
        /// </summary>
        /// <param name="original"></param>
        /// <returns>Returns the new material from the cache</returns>
        public Material GetCachedCopy(Material original, out bool wasCreated)
        {
            CachePath = DEFAULT_CACHE_PATH;
            wasCreated = false;
            if(original == null)
                return null;

            //Try to get from cache dictionary
            if(cache.TryGetValue(original, out Material cached))
                return cached;

            //Try to get from cache folder
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(original, out string guid, out long _);
            if(string.IsNullOrWhiteSpace(guid) || guid == Guid.Empty.ToString())
                return null;

            var loadedMat = AssetDatabase.LoadAssetAtPath<Material>($"Assets/{CachePath}/{guid}.mat");

            if(loadedMat)
                return loadedMat;

            //If missing create a new copy in the cache with the guid of our original material as the name
            var fullOriginalPath = AssetDatabase.GetAssetPath(original);
            if(string.IsNullOrWhiteSpace(fullOriginalPath))
                return null;

            var newFolderPath = $"Assets/{DEFAULT_CACHE_PATH}";
            var fullNewPath = $"{newFolderPath}/{guid}.mat";

            //Create cache folder
            if(!AssetDatabase.IsValidFolder(newFolderPath))
                AssetDatabase.CreateFolder("Assets", DEFAULT_CACHE_PATH);

            PumkinsAvatarTools.LogVerbose($"Creating and caching fallback material: {fullOriginalPath} => {fullNewPath}", LogType.Warning);

            if(AssetDatabase.CopyAsset(fullOriginalPath, fullNewPath))
            {
                wasCreated = true;
                return AssetDatabase.LoadAssetAtPath<Material>(fullNewPath);
            }
            return null;
        }

        /// <summary>
        /// Caches all given materials into the cache folder
        /// </summary>
        /// <param name="materials"></param>
        public void UpdateCache(Material[] materials)
        {
            if(materials.IsNullOrEmpty())
                return;

            cache.Clear();
            var cachedMatsArray = AssetDatabase.LoadAllAssetsAtPath(CachePath);

            foreach(var mat in materials)
            {
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(mat, out string guid, out long _);
                if(string.IsNullOrWhiteSpace(guid))
                    continue;
                var cached = cachedMatsArray.FirstOrDefault(m => m.name == guid) as Material;
                if(cached)
                    cache.Add(mat, cached);
            }
        }

        /// <summary>
        /// Gets the original material from the cached material
        /// </summary>
        /// <param name="cachedMaterial">Material who's name is used to find the original material again</param>
        /// <returns>The original material</returns>
        public Material GetOriginalMaterialFromCached(Material cachedMaterial)
        {
            if(!cachedMaterial)
                return null;

            if(!GUID.TryParse(cachedMaterial.name, out GUID guid))
                return null;

            var path = AssetDatabase.GUIDToAssetPath(guid.ToString());
            if(string.IsNullOrEmpty(path))
                return null;

            return AssetDatabase.LoadAssetAtPath<Material>(path);
        }
    }
}
#endif