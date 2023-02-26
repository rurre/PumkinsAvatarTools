using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Pumkin.AvatarTools
{
    internal static class VRChatHelpers
    {
        static PropertyInfo _legacyBlendShapeNormalsPropertyInfo;
        static PropertyInfo LegacyBlendShapeNormalsPropertyInfo
        {
            get
            {
                if(_legacyBlendShapeNormalsPropertyInfo != null)
                {
                    return _legacyBlendShapeNormalsPropertyInfo;
                }

                Type modelImporterType = typeof(ModelImporter);
                _legacyBlendShapeNormalsPropertyInfo = modelImporterType.GetProperty(
                    "legacyComputeAllNormalsFromSmoothingGroupsWhenMeshHasBlendShapes",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
                );

                return _legacyBlendShapeNormalsPropertyInfo;
            }
        }

        internal static void SetImportSettings(GameObject selectedAvatar)
        {
            var skinnedMeshes = selectedAvatar.GetComponentsInChildren<SkinnedMeshRenderer>(true).Select(x => x.sharedMesh);
            var incorrectMeshes = GetMeshesWithIncorrectBlendShapeNormalsSetting(skinnedMeshes);

            if(incorrectMeshes.Count > 0)
                EnableLegacyBlendShapeNormals(incorrectMeshes);
        }

        static void EnableLegacyBlendShapeNormals(IEnumerable<Mesh> meshesToFix)
        {
            HashSet<string> meshAssetPaths = new HashSet<string>();
            foreach(Mesh meshToFix in meshesToFix)
            {
                if(!AssetDatabase.Contains(meshToFix))
                    continue;

                string meshAssetPath = AssetDatabase.GetAssetPath(meshToFix);
                if(string.IsNullOrEmpty(meshAssetPath))
                    continue;

                if(meshAssetPaths.Contains(meshAssetPath))
                    continue;

                meshAssetPaths.Add(meshAssetPath);
            }

            foreach(string meshAssetPath in meshAssetPaths)
            {
                ModelImporter avatarImporter = AssetImporter.GetAtPath(meshAssetPath) as ModelImporter;
                if(avatarImporter == null)
                    continue;

                if(avatarImporter.importBlendShapeNormals != ModelImporterNormals.Calculate)
                    continue;

                LegacyBlendShapeNormalsPropertyInfo.SetValue(avatarImporter, true);
                avatarImporter.SaveAndReimport();
            }
        }

        static HashSet<Mesh> GetMeshesWithIncorrectBlendShapeNormalsSetting(IEnumerable<Mesh> avatarMeshes)
        {
            HashSet<Mesh> incorrectlyConfiguredMeshes = new HashSet<Mesh>();
            foreach(Mesh avatarMesh in avatarMeshes)
            {
                if(!AssetDatabase.Contains(avatarMesh))
                    continue;

                string meshAssetPath = AssetDatabase.GetAssetPath(avatarMesh);
                if(string.IsNullOrEmpty(meshAssetPath))
                    continue;

                ModelImporter avatarImporter = AssetImporter.GetAtPath(meshAssetPath) as ModelImporter;
                if(avatarImporter == null)
                    continue;

                if(avatarImporter.importBlendShapeNormals != ModelImporterNormals.Calculate)
                    continue;

                bool useLegacyBlendShapeNormals = (bool)LegacyBlendShapeNormalsPropertyInfo.GetValue(avatarImporter);
                if(useLegacyBlendShapeNormals)
                    continue;

                if(!incorrectlyConfiguredMeshes.Contains(avatarMesh))
                    incorrectlyConfiguredMeshes.Add(avatarMesh);
            }

            return incorrectlyConfiguredMeshes;
        }
    }
}
