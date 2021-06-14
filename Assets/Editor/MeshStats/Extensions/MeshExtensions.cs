using UnityEngine;

namespace Project.Editors.MeshStats.Controls
{
    public static class MeshExtensions
    {
        public static bool CompareMesh(this Mesh original, Mesh other)
        {
            /*
            bool a1 = original.isReadable == other.isReadable;
            bool a2 = original.uv.CompateArrays(other.uv);
            bool a3 = original.uv2.CompateArrays(other.uv2);
            bool a4 = original.uv3.CompateArrays(other.uv3);
            bool a5 = original.uv4.CompateArrays(other.uv4);
            bool a6 = original.uv5.CompateArrays(other.uv5);
            bool a7 = original.uv6.CompateArrays(other.uv6);
            bool a8 = original.uv7.CompateArrays(other.uv7);
            bool a9 = original.uv8.CompateArrays(other.uv8);
            bool b1 = original.bounds == other.bounds;
            bool b2 = original.colors.CompateArrays(other.colors);
            bool b3 = original.normals.CompateArrays(other.normals);
            bool b4 = original.vertices.CompateArrays(other.vertices);
            bool b5 = original.tangents.CompateArrays(other.tangents);
            bool b6 = original.colors32.CompateArrays(other.colors32);
            bool b7 = original.hideFlags == other.hideFlags;
            bool b8 = original.triangles.CompateArrays(other.triangles);
            bool b9 = original.indexFormat == other.indexFormat;
            bool c1 = original.vertexCount == other.vertexCount;
            bool c2 = original.subMeshCount == other.subMeshCount;
            bool c3 = original.blendShapeCount == other.blendShapeCount;
            bool c4 = original.vertexBufferCount == other.vertexBufferCount;
            bool c5 = original.vertexAttributeCount == other.vertexAttributeCount;;
            */

            return original.isReadable == other.isReadable
                   && original.uv.CompateArrays(other.uv)
                   && original.uv2.CompateArrays(other.uv2)
                   && original.uv3.CompateArrays(other.uv3)
                   && original.uv4.CompateArrays(other.uv4)
                   && original.uv5.CompateArrays(other.uv5)
                   && original.uv6.CompateArrays(other.uv6)
                   && original.uv7.CompateArrays(other.uv7)
                   && original.uv8.CompateArrays(other.uv8)
                   && original.bounds == other.bounds
                   && original.colors.CompateArrays(other.colors)
                   && original.normals.CompateArrays(other.normals)
                   && original.vertices.CompateArrays(other.vertices)
                   && original.tangents.CompateArrays(other.tangents)
                   && original.colors32.CompateArrays(other.colors32)
                   && original.hideFlags == other.hideFlags
                   && original.triangles.CompateArrays(other.triangles)
                   //&& original?.bindposes == other?.bindposes
                   //&& original?.boneWeights == other?.boneWeights
                   && original.indexFormat == other.indexFormat
                   && original.vertexCount == other.vertexCount
                   && original.subMeshCount == other.subMeshCount
                   && original.blendShapeCount == other.blendShapeCount
                   && original.vertexBufferCount == other.vertexBufferCount
                   && original.vertexAttributeCount == other.vertexAttributeCount;
        }

        internal static bool CompateArrays<T>(this T[] original, T[] other)
        {
            if (original == null && other == null)
                return true;
            
            if (original.Length != other.Length)
                return false;

            int length = original.Length;
            int index = 0;

            while (index < length)
            {
                if (original[index].Equals(other[index]) == false)
                    return false;

                index++;
            }
            return true;
        }
    }
}