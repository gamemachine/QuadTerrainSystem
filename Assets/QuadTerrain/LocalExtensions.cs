using UnityEngine;

namespace QuadTerrain
{
    public static class LocalExtensions
    {
        public static bool Approximately(this Quaternion self, Quaternion other)
        {
            return Mathf.Approximately(self.x, other.x)
                && Mathf.Approximately(self.y, other.y)
                && Mathf.Approximately(self.z, other.z)
                && Mathf.Approximately(self.w, other.w);
        }

        public static bool Approximately(this Vector3 self, Vector3 other)
        {
            return Mathf.Approximately(self.x, other.x) && Mathf.Approximately(self.y, other.y) && Mathf.Approximately(self.z, other.z);
        }

    }
}
