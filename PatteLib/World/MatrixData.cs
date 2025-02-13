using Newtonsoft.Json.Linq;

namespace PatteLib.World;

public class MatrixData
{
    public int Width;
    public int Height;
    public List<MatrixCellData> Cells = [];

    public static MatrixData LoadFromFile(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("The file could not be found.", path);
        }

        var fileContent = File.ReadAllText(path);
        var jArray = JArray.Parse(fileContent);

        // Determine Width and Height based on the data
        int maxX = 0;
        int maxY = 0;
        foreach (var jCombination in jArray)
        {
            int x = jCombination["x"].Value<int>();
            int y = jCombination["y"].Value<int>();
            maxX = Math.Max(maxX, x);
            maxY = Math.Max(maxY, y);
        }
        var matrixData = new MatrixData
        {
            Width = maxX + 1,
            Height = maxY + 1
        };

        // Initialize Cells list with empty cells
        matrixData.Cells = Enumerable.Repeat(MatrixCellData.Empty, matrixData.Width * matrixData.Height).ToList();
        
        foreach (var jCombination in jArray)
        {
            int x = jCombination["x"].Value<int>();
            int y = jCombination["y"].Value<int>();
            var cellData = new MatrixCellData
            {
                Z = jCombination["height"].Value<int>(),
                ChunkId = jCombination["mapId"].Value<int>(),
                HeaderId = jCombination["headerId"].Value<int>()
            };
            matrixData.Cells[y * matrixData.Width + x] = cellData;
        }

        return matrixData;
    }

    public MatrixCellData Get(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
        {
            return MatrixCellData.Empty; // Or throw an exception for out-of-bounds access
        }
        return Cells[y * Width + x];
    }
}