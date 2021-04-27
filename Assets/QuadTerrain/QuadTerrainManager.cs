using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace QuadTerrain
{
    [ExecuteAlways]
    public class QuadTerrainManager : MonoBehaviour
    {
        [SerializeField]
        private float Extents = 1024f;
        [SerializeField]
        private bool EnableRendering;
        [SerializeField]
        private bool DebugMode;
        [SerializeField]
        private bool BuildEveryFrame;
        [SerializeField]
        private Plane[] SourcePlanes = new Plane[6];

        [SerializeField]
        private GameObject TestPrefab;

        [SerializeField]
        private int RenderNodeCount;
        [SerializeField]
        private Quaternion LastCameraRotation;
        private Vector3 LastCameraPosition;
        [SerializeField]
        private int BuildCount;
       
        private NativeArray<float4> Planes;
        private NativeList<RenderNode> RenderNodes;
        private JobHandle JobHandle;
        private QuadTerrainRenderMesh[] RenderMeshes = new QuadTerrainRenderMesh[9];

        private void OnEnable()
        {
            RenderMeshes = new QuadTerrainRenderMesh[9];
            Planes = new NativeArray<float4>(6, Allocator.Persistent);
            RenderNodes = new NativeList<RenderNode>(Allocator.Persistent);
        }

        private void OnDisable()
        {
            Planes.Dispose();
            RenderNodes.Dispose();
        }

        private void Update()
        {
            if (BuildEveryFrame)
            {
                Camera cam = SceneCamera.Camera;
                if (cam.transform.rotation.Approximately(LastCameraRotation) && cam.transform.position.Approximately(LastCameraPosition))
                {
                    return;
                }
                LastCameraRotation = cam.transform.rotation;
                LastCameraPosition = cam.transform.position;

                FrustumPlanes2.FromCamera(cam, SourcePlanes, Planes);

                RenderNodes.Clear();
                BuildTerrainJob job = BuildTerrainJob.Create(Extents, cam.transform.position, Planes, RenderNodes);
                JobHandle = job.Schedule();
                BuildCount++;
            }
        }

        private void LateUpdate()
        {
            if (BuildEveryFrame)
            {
                JobHandle.Complete();
                RenderNodeCount = RenderNodes.Length;
            }

            if (EnableRendering)
            {
                Draw(QuadTerrainConfig.Instance.TestMaterial, RenderNodes);
            }
        }

        private QuadTerrainRenderMesh GetRenderMesh(MeshType meshType)
        {
            var renderMesh = RenderMeshes[(int)meshType];
            if (renderMesh != null)
            {
                return renderMesh;
            }

            renderMesh = new QuadTerrainRenderMesh();
            renderMesh.Mesh = QuadTerrainConfig.Instance.GetMesh(meshType);
            renderMesh.Props = new MaterialPropertyBlock();
            renderMesh.Matrices = new Matrix4x4[1023];
            renderMesh.RenderingLayers = new Vector4[1023];

            int flags = 0xff; // enable all light layers
            float flagsFloat = BitConverter.ToSingle(BitConverter.GetBytes(flags), 0);

            for (int i = 0; i < renderMesh.RenderingLayers.Length; i++)
            {
                renderMesh.RenderingLayers[i] = new Vector4(flagsFloat, 0, 0, 0);
            }
            renderMesh.Props.SetVectorArray("unity_RenderingLayer", renderMesh.RenderingLayers);

            RenderMeshes[(int)meshType] = renderMesh;

            return renderMesh;
        }

        private void Draw(Material material, NativeList<RenderNode> renderNodes)
        {
            for (int i = 0; i < RenderMeshes.Length; i++)
            {
                var renderMesh = RenderMeshes[i];
                if (renderMesh != null)
                {
                    renderMesh.Count = 0;
                }
            }

            for (int i = 0; i < renderNodes.Length; i++)
            {
                var renderNode = renderNodes[i];
                var renderMesh = GetRenderMesh(renderNode.MeshType);
                Matrix4x4 matrix = Matrix4x4.TRS(renderNode.WorldPosition, Quaternion.identity, renderNode.WorldScale);
                renderMesh.Matrices[renderMesh.Count] = matrix;
                renderMesh.Count++;
            }

            for (int i = 0; i < RenderMeshes.Length; i++)
            {
                var renderMesh = RenderMeshes[i];
                Draw(material, renderMesh);
            }
        }

        private void Draw(Material material, QuadTerrainRenderMesh item)
        {
            Graphics.DrawMeshInstanced(
                        mesh: item.Mesh,
                        submeshIndex: 0,
                        material: material,
                        matrices: item.Matrices,
                        count: item.Count,
                        properties: item.Props,
                        castShadows: ShadowCastingMode.Off,
                        receiveShadows: true,
                        layer: 0,
                        null,
                        LightProbeUsage.Off
                    );
        }




#if UNITY_EDITOR

        public void Build()
        {
            Camera camera = SceneCamera.Camera;
            FrustumPlanes2.FromCamera(camera, SourcePlanes, Planes);

            RenderNodes.Clear();
            BuildTerrainJob job = BuildTerrainJob.Create(Extents, camera.transform.position, Planes, RenderNodes);


            var watch = System.Diagnostics.Stopwatch.StartNew();
            job.Run();

            watch.Stop();
            Debug.LogFormat("Construct: {0}", watch.ElapsedTicks);

           
            Debug.LogFormat("RenderNode count {0}", RenderNodes.Length);
        }

        public void TestDisplayRenderNodes()
        {
            GameObject parent = new GameObject();
            parent.transform.position = default;

            for (int i = 0; i < RenderNodes.Length; i++)
            {
                RenderNode node = RenderNodes[i];
                Mesh mesh = QuadTerrainConfig.Instance.GetMesh(node.MeshType);
                GameObject go = Instantiate(TestPrefab);
                go.transform.SetParent(parent.transform);
                MeshFilter filter = go.GetComponent<MeshFilter>();
                filter.sharedMesh = mesh;
                go.transform.localPosition = node.WorldPosition;
                go.transform.localScale = node.WorldScale;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!DebugMode)
            {
                return;
            }

            var cam = SceneCamera.Camera;
            float2 camPosition = new float2(cam.transform.position.x, cam.transform.position.z);

            GUIStyle style = new GUIStyle();
            style.fontSize = 24;

            Gizmos.color = Color.green;
            for(int i=0;i<RenderNodes.Length;i++)
            {
                
                var node = RenderNodes[i].Node;

                if (!node.IsLeaf)
                {
                    continue;
                }

                Vector3 center = new Vector3(node.Bounds.Center.x, 0f, node.Bounds.Center.y);
                Vector3 size = new Vector3(node.Bounds.Size.x, 1f, node.Bounds.Size.y);
                Gizmos.DrawWireCube(center, size);

                float distance = math.distance(camPosition, node.Bounds.Center);
                if (distance < 1024)
                {
                    UnityEditor.Handles.Label(center, node.Bounds.Size.x.ToString(), style);
                }
                
            }
        }
#endif

    }


}
