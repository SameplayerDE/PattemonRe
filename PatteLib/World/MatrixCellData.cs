namespace PatteLib.World;

public struct  MatrixCellData : IEquatable<MatrixCellData>
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
    
    public override bool Equals(object? obj)
    {
        if (obj is MatrixCellData other)
        {
            return this == other;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Z, ChunkId, HeaderId);
    }

    public static bool operator ==(MatrixCellData left, MatrixCellData right)
    {
        return left.Z == right.Z && left.ChunkId == right.ChunkId && left.HeaderId == right.HeaderId;
    }

    public static bool operator !=(MatrixCellData left, MatrixCellData right)
    {
        return !(left == right);
    }

    public bool Equals(MatrixCellData other)
    {
        return Z == other.Z && ChunkId == other.ChunkId && HeaderId == other.HeaderId;
    }
}