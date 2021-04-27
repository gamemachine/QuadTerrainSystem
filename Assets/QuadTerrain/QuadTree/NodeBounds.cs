using System;
using Unity.Mathematics;

namespace QuadTerrain
{
    [Serializable]
    public struct NodeBounds
    {
        public float2 min;
        public float2 max;

        public float2 Center => min + Extents;
        public float2 Extents => Size * 0.5f;
        public float2 Size => (max - min);

        public bool Contains(float2 point)
        {
            return min.x <= point.x && max.x >= point.x && min.y <= point.y && max.y >= point.y;
        }

        public bool Intersects(NodeBounds other)
        {
            if (min.x > other.max.x || other.min.x > max.x)
            {
                return false;
            }

            if (min.y > other.max.y || other.min.y > max.y)
            {
                return false;
            }

            return true;
        }

        public void Expand(float amount)
        {
            min = new float2(min.x - amount, min.y - amount);
            max = new float2(max.x + amount, max.y + amount);
        }

        public FourNodeBounds Subdivide()
        {
            FourNodeBounds bounds = new FourNodeBounds();

            var extents = Extents;
            bounds.Bl = new NodeBounds
            {
                min = min,
                max = min + extents
            };

            bounds.Tl = new NodeBounds();
            bounds.Tl.min = new float2(min.x, min.y + extents.y);
            bounds.Tl.max = bounds.Tl.min + extents;


            bounds.Br = new NodeBounds();
            bounds.Br.min = new float2(min.x + extents.x, min.y);
            bounds.Br.max = bounds.Br.min + extents;

            bounds.Tr = new NodeBounds();
            bounds.Tr.min = new float2(min.x + extents.x, min.y + extents.y);
            bounds.Tr.max = max;

            return bounds;
        }

        public override string ToString()
        {
            return string.Format("min:{0} max:{1}", min.ToString(), max.ToString());
        }

    }
}
