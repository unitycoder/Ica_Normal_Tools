﻿using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace IcaNormal
{
    [BurstCompile]
    public static class MergedMeshDataUtils
    {
        [BurstCompile]
        public static void GetMergedVertices([NoAlias]in Mesh.MeshDataArray mda,[NoAlias] ref NativeArray<float3> outMergedVertices,[NoAlias] ref NativeArray<int> map)
        {
            var vertexList = new UnsafeList<NativeArray<float3>>(mda.Length, Allocator.Temp);
            for (int i = 0; i < mda.Length; i++)
            {
                var v = new NativeArray<float3>(mda[i].vertexCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                mda[i].GetVertices(v.Reinterpret<Vector3>());
                vertexList.Add(v);
            }

            NativeContainerUtils.UnrollListOfArrayToArray(vertexList, ref map, ref outMergedVertices);
        }

        public static void GetTotalVertexCountFomMDA([NoAlias]in Mesh.MeshDataArray mda,[NoAlias] out int count)
        {
            count = 0;
            for (int i = 0; i < mda.Length; i++)
            {
                count += mda[i].vertexCount;
            }
        }

        [BurstCompile]
        public static void GetMergedUVs([NoAlias]in Mesh.MeshDataArray mda,[NoAlias] ref NativeArray<float2> outMergedUVs,[NoAlias] ref NativeArray<int> map)
        {
            var nestedData = new UnsafeList<NativeArray<float2>>(mda.Length, Allocator.Temp);
            for (int i = 0; i < mda.Length; i++)
            {
                var singleMeshData = new NativeArray<float2>(mda[i].vertexCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                mda[i].GetUVs(0, singleMeshData.Reinterpret<Vector2>());
                nestedData.Add(singleMeshData);
            }

            NativeContainerUtils.UnrollListOfArrayToArray(nestedData, ref map, ref outMergedUVs);
        }
        
        
        [BurstCompile]
        public static void GetMergedNormals([NoAlias]in Mesh.MeshDataArray mda,[NoAlias] ref NativeArray<float3> outMergedNormals,[NoAlias] ref NativeArray<int> map)
        {
            var nestedData = new UnsafeList<NativeArray<float3>>(mda.Length, Allocator.Temp);
            for (int i = 0; i < mda.Length; i++)
            {
                var singleMeshData = new NativeArray<float3>(mda[i].vertexCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                mda[i].GetNormals( singleMeshData.Reinterpret<Vector3>());
                nestedData.Add(singleMeshData);
            }

            NativeContainerUtils.UnrollListOfArrayToArray(nestedData, ref map, ref outMergedNormals);
        }
        [BurstCompile]
        public static void GetMergedTangents([NoAlias]in Mesh.MeshDataArray mda,[NoAlias] ref NativeArray<float4> outMergedNormals,[NoAlias] ref NativeArray<int> map)
        {
            var nestedData = new UnsafeList<NativeArray<float4>>(mda.Length, Allocator.Temp);
            for (int i = 0; i < mda.Length; i++)
            {
                var singleMeshData = new NativeArray<float4>(mda[i].vertexCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                mda[i].GetTangents( singleMeshData.Reinterpret<Vector4>());
                nestedData.Add(singleMeshData);
            }

            NativeContainerUtils.UnrollListOfArrayToArray(nestedData, ref map, ref outMergedNormals);
        }


        [BurstCompile]
        public static void CreateAndGetMergedIndices(in Mesh.MeshDataArray mda, out NativeList<int> outMergedIndices, out NativeArray<int> outMergedIndicesMap, Allocator allocator)
        {
            GetAllIndicesCountOfMultipleMeshes(mda, out int totalIndexCount);
            outMergedIndices = new NativeList<int>(totalIndexCount, allocator);
            outMergedIndicesMap = new NativeArray<int>(totalIndexCount + 1, allocator);

            GetMergedIndices(mda, ref outMergedIndices, ref outMergedIndicesMap);
        }

        [BurstCompile]
        public static void GetMergedIndices([NoAlias] in Mesh.MeshDataArray mda,[NoAlias] ref NativeList<int> mergedIndices,[NoAlias] ref NativeArray<int> mergedIndicesMap)
        {
            var indexList = new UnsafeList<NativeList<int>>(1, Allocator.Temp);
            var prevMeshesTotalVertexCount = 0;
            for (int i = 0; i < mda.Length; i++)
            {
                mda[i].GetAllIndicesData(out var indices, Allocator.Temp);
                for (int j = 0; j < indices.Length; j++)
                {
                    indices[j] += prevMeshesTotalVertexCount;
                }

                indexList.Add(indices);
                prevMeshesTotalVertexCount += mda[i].vertexCount;
            }

            NativeContainerUtils.UnrollListOfListToList(indexList, ref mergedIndices, ref mergedIndicesMap);
        }

        [BurstCompile]
        public static void GetAllIndicesCountOfMultipleMeshes([NoAlias] in Mesh.MeshDataArray data, [NoAlias]out int count)
        {
            count = 0;
            for (int i = 0; i < data.Length; i++)
            {
                NativeIndicesUtil.GetCountOfAllIndices(data[i], out var meshIndexCount);
                count += meshIndexCount;
            }
        }
    }
}