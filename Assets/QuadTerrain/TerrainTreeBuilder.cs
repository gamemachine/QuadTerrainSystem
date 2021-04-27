using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace QuadTerrain
{
    public struct TerrainTreeBuilder
    {
        public float2 CameraPosition;

        [BurstCompile]
        public void GetRenderNodes(QuadTree tree, NativeList<RenderNode> renderNodes, NativeArray<float4> planes)
        {
            
            NativeList<int> results = new NativeList<int>(Allocator.Temp);

            var nodes = tree.Nodes;
            for(int i=0;i<nodes.Length;i++)
            {
                Node node = nodes[i];
                if (!node.IsLeaf)
                {
                    continue;
                }

                NodeBounds bounds = node.Bounds;
                bounds.Expand(2f);

                float2 center = bounds.Center;
                float2 extents = bounds.Extents;
                Bounds frustumBounds = new Bounds();
                frustumBounds.center = new Vector3(center.x, 0f, center.y);
                frustumBounds.extents = new Vector3(extents.x, 1000f, extents.y);
               
                var intersectResult = FrustumPlanes2.Intersect(planes, frustumBounds);
                if (intersectResult == FrustumPlanes2.IntersectResult.Out)
                {
                    continue;
                }
               
              
                tree.FindOverlapping(bounds, results);
                NodeSides sides = default;

                for(int j=0;j<results.Length;j++)
                {
                    int otherId = results[j];
                    if (otherId == i)
                    {
                        continue;
                    }
                    Node other = nodes[otherId];

                    // only smaller neighbors set different edges
                    if (!(node.Bounds.Size.x + 1 < other.Bounds.Size.x))
                    {
                        continue;
                    }

                    
                    if (IsLeft(node.Bounds,other.Bounds))
                    {
                        sides.Left = true;
                    } else if (IsRight(node.Bounds,other.Bounds))
                    {
                        sides.Right = true;
                    } else if (IsForward(node.Bounds,other.Bounds))
                    {
                        sides.Forward = true;
                    }
                    else if (IsBack(node.Bounds,other.Bounds))
                    {
                        sides.Back = true;
                    }

                }
                results.Clear();

                RenderNode renderNode = new RenderNode
                {
                    Node = node,
                    MeshType = sides.GetMeshType()
                };
                renderNodes.Add(renderNode);
            }
        }

        public bool IsLeft(NodeBounds bounds, NodeBounds other)
        {
            float2 point = new float2(bounds.min.x - 1f, bounds.Center.y);
            return other.Contains(point);
        }

        public bool IsRight(NodeBounds bounds, NodeBounds other)
        {
            float2 point = new float2(bounds.max.x + 1f, bounds.Center.y);
            return other.Contains(point);
        }

        public bool IsBack(NodeBounds bounds, NodeBounds other)
        {
            float2 point = new float2(bounds.Center.x, bounds.min.y - 1f);
            return other.Contains(point);
        }

        public bool IsForward(NodeBounds bounds, NodeBounds other)
        {
            float2 point = new float2(bounds.Center.x, bounds.max.y + 1f);
            return other.Contains(point);
        }

        

        public bool IsLeaf(Node node)
        {
            float distance = math.distance(node.Bounds.Center, CameraPosition);
            float lod = GetLod(distance);
            if (node.Bounds.Size.x <= lod + 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public float GetLod(float distance)
        {
            if (distance <= 128f)
            {
                return 32f;
            }
            if (distance <= 256f)
            {
                return 64f;
            }
            if (distance <= 1024f)
            {
                return 128f;
            }
            if (distance <= 2048)
            {
                return 256f;
            }
            
            return 512f;
        }
    }

    

    

}
