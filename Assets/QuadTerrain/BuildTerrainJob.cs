using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace QuadTerrain
{
    [BurstCompile]
    public struct BuildTerrainJob : IJob
    {
        public float2 CameraPosition;
        public NativeList<RenderNode> RenderNodes;
        public NativeArray<float4> Planes;
        public NodeBounds Bounds;

        public static BuildTerrainJob Create(float extents, float3 cameraPosition, NativeArray<float4> planes, NativeList<RenderNode> renderNodes)
        {
            float2 camPosition = new float2(cameraPosition.x, cameraPosition.z);

            float2 center = default;
            center.x = FloorToMultiple(camPosition.x, 32f);
            center.y = FloorToMultiple(camPosition.y, 32f);

            NodeBounds bounds = new NodeBounds
            {
                min = new float2(center.x - extents, center.y - extents),
                max = new float2(center.x + extents, center.y + extents)
            };

            BuildTerrainJob job = new BuildTerrainJob
            {
                CameraPosition = camPosition,
                Bounds = bounds,
                Planes = planes,
                RenderNodes = renderNodes
            };
            return job;
        }

        private static float FloorToMultiple(float value, float factor)
        {
            return math.floor(value / factor) * factor;
        }

        public void Execute()
        {
            TerrainTreeBuilder builder = new TerrainTreeBuilder { CameraPosition = CameraPosition };
            QuadTree tree = QuadTree.Create(2048, QuadTreeType.Terrain, Allocator.Temp);
            tree.TerrainTreeBuilder = builder;
            tree.Construct(Bounds);
            builder.GetRenderNodes(tree, RenderNodes, Planes);
        }
    }
}
