using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Project.Editors.MeshStats
{
    public class ModelImporterUtility
    {
        public static List<ModelImporter> LoadImportersFromObjects(GameObject[] gameObjects)
        {
            // Доп. фильтрация объектов содержащих <MeshFilter>
            var filteredObjects = gameObjects.Where(x => x.TryGetComponent(out MeshFilter _));
            var meshFilters = new List<MeshFilter>(filteredObjects.Select(x => x.GetComponent<MeshFilter>()));
            return LoadImportersFromMeshFilters(meshFilters.ToArray());
        }

        public static ModelImporter LoadImporterForMeshFilter(MeshFilter filter)
        {
            return LoadImportersFromMeshFilters(new[] {filter}).FirstOrDefault();
        }
        
        public static List<ModelImporter> LoadImportersFromMeshFilters(MeshFilter[] filters)
        {
            var modelImporters = new HashSet<ModelImporter>();
            
            foreach (var filter in filters)
            {
                var mesh = filter.sharedMesh;
                var meshPath= AssetDatabase.GetAssetPath(mesh);

                if (string.IsNullOrEmpty(meshPath))
                    meshPath = GetFbxModelPath(mesh.name);
                
                var assetImporter = AssetImporter.GetAtPath(meshPath);
                
                if(assetImporter is ModelImporter importer)
                    modelImporters.Add(importer);
            }

            return modelImporters.ToList();
        }

        private static string GetFbxModelPath(string meshName)
        {
            if (meshName.Contains(" Instance"))
                meshName = meshName.Replace(" Instance", "");
            
            var paths = new List<string>();
            var guids = AssetDatabase.FindAssets(meshName);

            for (int i = 0; i < guids.Length; i++)
                paths.Add(AssetDatabase.GUIDToAssetPath(guids[i]));
            
            return paths.FirstOrDefault(x => x.Contains(".fbx"));
        }
    }
}