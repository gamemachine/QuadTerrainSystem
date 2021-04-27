using System;
using UnityEngine;

namespace QuadTerrain
{
    [Serializable]
    public struct RenderNode
    {

        public Node Node;
        public MeshType MeshType;

        public float MeshScale => Node.Bounds.Size.x / 32f;

        public Vector3 WorldScale
        {
            get
            {
                return new Vector3(MeshScale, 1f, MeshScale);
            }
        }

        public Vector3 WorldPosition
        {
            get
            {
                return new Vector3(Node.Bounds.min.x, 0f, Node.Bounds.min.y);
            }
        }

    }
}
