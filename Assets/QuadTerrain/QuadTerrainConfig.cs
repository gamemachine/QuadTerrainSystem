using System.Collections.Generic;
using UnityEngine;

namespace QuadTerrain
{
    [CreateAssetMenu(menuName = @"Custom/QuadTerrainConfig")]
    public class QuadTerrainConfig : ScriptableObject
    {
        private static QuadTerrainConfig m_Instance;

        public Material TestMaterial;
        public List<QuadTerrainMesh> Meshes = new List<QuadTerrainMesh>();

        private Dictionary<MeshType, Mesh> MeshMap = new Dictionary<MeshType, Mesh>();

        public Mesh GetMesh(MeshType type)
        {
            if(MeshMap.TryGetValue(type, out var mesh))
            {
                return mesh;
            }

            foreach(var quadMesh in Meshes)
            {
                if (type == quadMesh.MeshType)
                {
                    MeshMap[type] = quadMesh.Mesh;
                    return quadMesh.Mesh;
                }
            }
            return null;
        }

        public static QuadTerrainConfig Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = Resources.Load(typeof(QuadTerrainConfig).Name, typeof(QuadTerrainConfig)) as QuadTerrainConfig;
                }
                return m_Instance;
            }
        }
    }
}
