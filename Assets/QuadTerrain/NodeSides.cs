namespace QuadTerrain
{
    public struct NodeSides
    {
        public bool Left;
        public bool Right;
        public bool Forward;
        public bool Back;

        public MeshType GetMeshType()
        {
            if (Left)
            {
                if (Forward)
                {
                    return MeshType.LeftForward;
                }
                else if (Back)
                {
                    return MeshType.LeftBack;
                }
                else
                {
                    return MeshType.Left;
                }
            }

            if (Right)
            {
                if (Forward)
                {
                    return MeshType.RightForward;
                }
                else if (Back)
                {
                    return MeshType.RightBack;
                }
                else
                {
                    return MeshType.Right;
                }
            }

            if (Forward)
            {
                return MeshType.Forward;
            }
            else if (Back)
            {
                return MeshType.Back;
            }

            return MeshType.Default;
        }
    }
}
