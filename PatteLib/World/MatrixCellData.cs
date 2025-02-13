namespace PatteLib.World;

public struct  MatrixCellData
{
    public static MatrixCellData Empty = new MatrixCellData
    {
        Z = int.MinValue,
        ChunkId = int.MinValue,
        HeaderId = int.MinValue,
    };
    
    public int Z;
    public int ChunkId;
    public int HeaderId;
}