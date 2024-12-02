using System.Drawing;
using System.Media;
using System.Windows.Media;

public class GameModel
{
    private int[,] table;
    private int width, height;
    private MediaPlayer soundPlayer;
    private MediaPlayer backgroundMusicPlayer; // Thêm biến để lưu trữ MediaPlayer
    public Dictionary<int, List<Point>> PossibleMatches { get; private set; }
    private static readonly int[] rowDirs = { -1, 1, 0, 0 };  // Các hướng: lên, xuống, trái, phải
    private static readonly int[] colDirs = { 0, 0, -1, 1 };  // Các hướng: lên, xuống, trái, phải

    public int Width { get => width; }
    public int Height { get => height; }

    public GameModel(int _width, int _height, int _numOfType)
    {
        width = _width;
        height = _height;
        table = new int[height, width];
        PossibleMatches = new Dictionary<int, List<Point>>();
        InitializeBoard(_numOfType);
        backgroundMusicPlayer = new MediaPlayer(); // Khởi tạo MediaPlayer
        backgroundMusicPlayer.Open(new Uri($"pack://application:,,,/Sounds/NhacNenGamePikachu.wav")); // Đường dẫn đến tệp nhạc nền
        backgroundMusicPlayer.MediaEnded += (sender, e) =>
        {
            backgroundMusicPlayer.Position = TimeSpan.Zero; // Đặt lại vị trí khi nhạc kết thúc
            backgroundMusicPlayer.Play(); // Phát lại nhạc
        };
        backgroundMusicPlayer.Play();

    }

    private void InitializeBoard(int _numOfType)
    {
        // Sử dụng để lưu các ô đã sinh ra pokemon
        HashSet<int> cellIndex = new HashSet<int>();
        Random random = new Random();

        for (int i = 0; i < width * height / 2; i++)
        {
            // Sinh ngẫu nhiên 1 loại pokemon (1 -> _numOfType)
            int typeOfPokemon = random.Next(1, _numOfType + 1);

            if( !PossibleMatches.ContainsKey(typeOfPokemon) )
            {
                PossibleMatches[typeOfPokemon] = new List<Point>();
            }

            for (int j = 0; j < 2; j++)
            {
                // Sinh ô thứ 1
                int cell = random.Next(0, width * height);
                while (cellIndex.Contains(cell))
                    cell = random.Next(0, width * height);
                table[cell / width, cell % width] = typeOfPokemon;
                PossibleMatches[typeOfPokemon].Add(new Point(cell / width, cell % width));
                cellIndex.Add(cell);
            }
        }
    }

    public int GetCell(int row, int col)
    {
        System.Windows.MessageBox.Show($"[{row}, {col}]: {table[row, col]}");
        return table[row, col];
    }

    public bool IsSameType(int row1, int col1, int row2, int col2)
    {
        return table[row1, col1] == table[row2, col2];
    }

    public void RemovePair(int row1, int col1, int row2, int col2)
    {
        RemovePairPointFromPossibleMatches(row1, col1, row2, col2);
        table[row1, col1] = 0;
        table[row2, col2] = 0;
    }

    private void RemovePairPointFromPossibleMatches(int row1, int col1, int row2, int col2)
    {
        int type = table[row1, col1];
        if (PossibleMatches.ContainsKey(type))
        {
            PossibleMatches[type].RemoveAll(p => (p.X == row1 && p.Y == col1) || (p.X == row2 && p.Y == col2));
            if (!(PossibleMatches[type].Count > 0))
            {
                PossibleMatches.Remove(type);
            } 
        }
    }

    private int countShuffle = 0;
    public void ShuffleBoard()
    {
        // Thu thập tất cả các Pokémon còn lại
        List<int> remainingPokemon = new List<int>();
        for (int r = 0; r < height; r++)
        {
            for (int c = 0; c < width; c++)
            {
                if (table[r, c] != 0)
                    remainingPokemon.Add(table[r, c]);
            }
        }

        // Xáo trộn danh sách Pokémon
        Random random = new Random();
        for (int i = remainingPokemon.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (remainingPokemon[i], remainingPokemon[j]) = (remainingPokemon[j], remainingPokemon[i]);
        }

        // Đặt lại Pokémon vào bảng, giữ nguyên vị trí các ô trống
        int index = 0;
        PossibleMatches.Clear();

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                if (table[i, j] != 0)
                {
                    table[i, j] = remainingPokemon[index];

                    if (!PossibleMatches.ContainsKey(table[i, j]))
                    {
                        PossibleMatches[table[i, j]] = new List<Point>();
                    }
                    PossibleMatches[table[i, j]].Add(new Point(i, j));

                    index++;
                }
            }
        }

        // Kiểm tra nếu không có cặp hợp lệ, thì xáo trộn lại
        if (!HasValidPairs() && countShuffle < 10)
        {
            countShuffle++;
            ShuffleBoard(); // Gọi lại phương thức shuffle nếu không tìm thấy cặp hợp lệ
        }
        countShuffle = 0;
    }
    

    public bool HasValidPairs()
    {
        foreach (var entry in PossibleMatches)
        {
            var points = entry.Value; // Danh sách tọa độ của loại Pokémon hiện tại

            // Chỉ kiểm tra mỗi cặp (i, j) một lần với i < j
            for (int i = 0; i < points.Count; i++)
            {
                for (int j = i + 1; j < points.Count; j++)
                {
                    var p1 = points[i];
                    var p2 = points[j];

                    // Nếu hai tọa độ có thể kết nối được, trả về true
                    if (CanConnect(p1.X, p1.Y, p2.X, p2.Y))
                    {
                        return true;
                    }
                }
            }
        }

        return false;  // Không tìm thấy cặp hợp lệ
    }

    public bool CanConnect(int r1, int c1, int r2, int c2)
    {
        // Kiểm tra nếu các ô có cùng loại Pokémon
        if (table[r1, c1] != table[r2, c2]) return false;

        // Khởi tạo hàng đợi BFS
        Queue<(int, int, int, int)> queue = new Queue<(int, int, int, int)>();
        bool[,] visited = new bool[height, width];
        queue.Enqueue((r1, c1, -1, 0));  // Vị trí ban đầu, không có quẹo và chưa di chuyển

        visited[r1, c1] = true;

        while (queue.Count > 0)
        {
            var (currentR, currentC, lastDir, bends) = queue.Dequeue();

            // Nếu đã đến ô đích
            if (currentR == r2 && currentC == c2)
                return true;

            // Duyệt qua các hướng di chuyển
            for (int i = 0; i < 4; i++)
            {
                int newR = currentR + rowDirs[i];
                int newC = currentC + colDirs[i];

                if (IsValidMove(newR, newC) && !visited[newR, newC])
                {
                    // Nếu đi theo hướng mới và khác hướng trước, thì sẽ là một lần quẹo
                    int newBends = (lastDir == -1 || lastDir == i) ? bends : bends + 1;

                    // Nếu số lần quẹo vượt quá 2 thì bỏ qua
                    if (newBends > 2)
                        continue;

                    visited[newR, newC] = true;
                    queue.Enqueue((newR, newC, i, newBends));
                }
            }
        }

        return false;  // Không tìm thấy đường nối hợp lệ
    }

    private bool IsValidMove(int r, int c)
    {
        return r >= 0 && r < height && c >= 0 && c < width && table[r, c] != 0;  // Không phải là ô trống
    }

}