namespace QuadTerrain
{
    public struct DefaultTreeBuilder
    {
        public float CellSize;
        public bool IsLeaf(Node node)
        {
            return node.Bounds.Size.x <= CellSize + 1;
        }
    }
}
