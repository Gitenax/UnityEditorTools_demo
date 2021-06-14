using System;
using UnityEditor;
using UnityEngine;

namespace Project.Editors.MeshStats
{
    [Serializable]
    public struct MeshInfo
    {
        private MeshFilter _filter;
        private Mesh _mesh;
        private ModelImporter _modelImporter;

        public MeshInfo(MeshFilter meshFilter, ModelImporter modelImporter)
        {
            _filter = meshFilter;
            _mesh = _filter.sharedMesh;
            _modelImporter = modelImporter;
            Count = 1;
        }
        
        public MeshInfo(MeshFilter meshFilter, ModelImporter modelImporter, int count) 
            : this(meshFilter, modelImporter)
        {
            Count = count + 1;
        }

        public Mesh Mesh => _mesh;

        public ModelImporter Importer => _modelImporter;
        
        public string Name => _mesh.name;
        
        public int Vertices => _mesh.vertexCount;
        
        public int Polygons => _mesh.triangles.Length / 3;

        public int Count { get; }
        
        public int TotalVertices => Vertices * Count;

        public bool IsReadable
        {
            get => _modelImporter.isReadable;
            set
            { 
                EditorUtility.SetDirty(_modelImporter);
                _modelImporter.isReadable = value;
            }
        }

        public bool GenerateLightmap
        {
            get => _modelImporter.generateSecondaryUV;
            set
            {
                EditorUtility.SetDirty(_modelImporter);
                _modelImporter.generateSecondaryUV = value;
            }
        }
    }
}