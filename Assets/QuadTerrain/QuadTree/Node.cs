using System;
using Unity.Mathematics;

namespace QuadTerrain
{
    [Serializable]
    public struct Node
    {
        public NodeBounds Bounds;
        public int4 Data;
        public int Flags;

        public static Node Default
        {
            get
            {
                return new Node { Data = int4.zero };
            }
        }

        public bool IsLeaf { get => Flags != 0; set => Flags = value ? 1 : 0; }

        public bool IsChildValid(int index)
        {
            if (Data[index] == 0) return false;
            return true;
        }

        public int NumValidChildren()
        {
            int cnt = 0;
            for (int i = 0; i < 4; i++)
            {
                cnt += IsChildValid(i) ? 1 : 0;
            }

            return cnt;
        }

    }
}
