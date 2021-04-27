using System;
using Unity.Collections;
using Unity.Mathematics;

namespace QuadTerrain
{
    public struct QuadTree
    {
        public QuadTreeType TreeType;
        public TerrainTreeBuilder TerrainTreeBuilder;
        public DefaultTreeBuilder DefaultTreeBuilder;

        public NativeArray<Node> m_Nodes;
        public int CurrentNodeId;

        public static QuadTree Create(int maxNodes, QuadTreeType treeType, Allocator allocator)
        {
            QuadTree tree = new QuadTree();
            tree.TreeType = treeType;
            tree.m_Nodes = new NativeArray<Node>(maxNodes, allocator);
            return tree;
        }

        public NativeArray<Node> Nodes => m_Nodes.GetSubArray(0, CurrentNodeId);

        public void Dispose()
        {
            m_Nodes.Dispose();
        }

        private int NextNodeId()
        {
            int id = CurrentNodeId;
            CurrentNodeId++;
            return id;
        }

        public int AddNode(NodeBounds bounds)
        {
            Node node = Node.Default;
            node.Bounds = bounds;
            int id = NextNodeId();
            if (id > m_Nodes.Length - 1)
            {
                //Debug.LogFormat("AddNode {0} out of nodes", bounds);
                throw new Exception();
            }
            m_Nodes[id] = node;
            return id;
        }

        public void SetBounds(int nodeId, NodeBounds bounds)
        {
            Node node = m_Nodes[nodeId];
            node.Bounds = bounds;
            m_Nodes[nodeId] = node;
        }

        private bool IsLeaf(Node node)
        {
            switch(TreeType)
            {
                case QuadTreeType.Terrain:
                    return TerrainTreeBuilder.IsLeaf(node);
                default:
                    return DefaultTreeBuilder.IsLeaf(node);
            }
        }

        public void FindOverlapping(NodeBounds bounds, NativeList<int> results)
        {
            FindOverlapping(0, bounds, results);
        }

        private void FindOverlapping(int nodeId, NodeBounds bounds, NativeList<int> results)
        {
            Node node = m_Nodes[nodeId];
            if (node.Bounds.Intersects(bounds))
            {
                if (node.IsLeaf)
                {
                    results.Add(nodeId);
                } else
                {
                    for (int i = 0; i < 4; i++)
                    {
                        FindOverlapping(node.Data[i], bounds, results);
                    }
                }
            }
        }

        private bool BuildNode(int nodeId)
        {
            Node node = m_Nodes[nodeId];
            if (IsLeaf(node))
            {
                node.IsLeaf = true;
                m_Nodes[nodeId] = node;
                return false;
            }

            FourNodeBounds fourBounds = node.Bounds.Subdivide();
            if (node.NumValidChildren() != 4)
            {
                int4 data = new int4();
                data.x = AddNode(fourBounds.Bl);
                data.y = AddNode(fourBounds.Tl);
                data.z = AddNode(fourBounds.Br);
                data.w = AddNode(fourBounds.Tr);
                node.Data = data;
            } else
            {
                SetBounds(node.Data.x, fourBounds.Bl);
                SetBounds(node.Data.y, fourBounds.Tl);
                SetBounds(node.Data.z, fourBounds.Br);
                SetBounds(node.Data.w, fourBounds.Tr);
            }
            
            m_Nodes[nodeId] = node;

            for (int i = 0; i < 4; i++)
            {
                BuildNode(node.Data[i]);
            }

            return true;
        }

        public void Construct(NodeBounds bounds)
        {
            int rootId = AddNode(bounds);
            BuildNode(rootId);
        }
    }

}
