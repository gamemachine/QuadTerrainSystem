using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace QuadTerrain
{
    public struct FrustumPlanes2
    {
        public enum IntersectResult
        {
            Out,
            In,
            Partial
        };


        static public void FromCamera(Camera camera, Plane[] sourcePlanes, NativeArray<Plane> planes)
        {
            GeometryUtility.CalculateFrustumPlanes(camera, sourcePlanes);
            for(int i=0;i<planes.Length;i++)
            {
                planes[i] = sourcePlanes[i];
            }
        }

        static public void FromCamera(Camera camera, Plane[] sourcePlanes, NativeArray<float4> planes)
        {
            if (planes == null)
                throw new ArgumentNullException("The argument planes cannot be null.");
            if (planes.Length != 6)
                throw new ArgumentException("The argument planes does not have the expected length 6.");

            GeometryUtility.CalculateFrustumPlanes(camera, sourcePlanes);

            var cameraToWorld = camera.cameraToWorldMatrix;
            var eyePos = cameraToWorld.MultiplyPoint(Vector3.zero);
            var viewDir = new float3(cameraToWorld.m02, cameraToWorld.m12, cameraToWorld.m22);
            viewDir = -math.normalizesafe(viewDir);

            // Near Plane
            sourcePlanes[4].SetNormalAndPosition(viewDir, eyePos);
            sourcePlanes[4].distance -= camera.nearClipPlane;

            // Far plane
            sourcePlanes[5].SetNormalAndPosition(-viewDir, eyePos);
            sourcePlanes[5].distance += camera.farClipPlane;

            for (int i = 0; i < 6; ++i)
            {
                planes[i] = new float4(sourcePlanes[i].normal.x, sourcePlanes[i].normal.y, sourcePlanes[i].normal.z,
                    sourcePlanes[i].distance);
            }
        }

        static public IntersectResult Intersect(NativeArray<float4> cullingPlanes, Bounds a)
        {
            float3 m = a.center;
            float3 extent = a.extents;

            var inCount = 0;
            for (int i = 0; i < cullingPlanes.Length; i++)
            {
                float3 normal = cullingPlanes[i].xyz;
                float dist = math.dot(normal, m) + cullingPlanes[i].w;
                float radius = math.dot(extent, math.abs(normal));
                if (dist + radius <= 0)
                    return IntersectResult.Out;

                if (dist > radius)
                    inCount++;
            }

            return (inCount == cullingPlanes.Length) ? IntersectResult.In : IntersectResult.Partial;
        }

    }
}
