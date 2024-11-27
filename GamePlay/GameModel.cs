using System.Drawing;
using System.Media;
using System.Windows.Media;

public class GameModel
{
    private int[,] table;
    private int width, height;
    private MediaPlayer soundPlayer;
    private MediaPlayer backgroundMusicPlayer; // Thêm biến để lưu trữ MediaPlayer
    private Dictionary<int, List<Point>> possibleMatches;
    private static readonly int[] rowDirs = { -1, 1, 0, 0 };  // Các hướng: lên, xuống, trái, phải
    private static readonly int[] colDirs = { 0, 0, -1, 1 };  // Các hướng: lên, xuống, trái, phải

    public int Width { get => width; }
    public int Height { get => height; }

    public GameModel(int _width, int _height, int _numOfType)
    {
        width = _width;
        height = _height;
        table = new int[height, width];
        possibleMatches = new Dictionary<int, List<Point>>();
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

            if( !possibleMatches.ContainsKey(typeOfPokemon) )
            {
                possibleMatches[typeOfPokemon] = new List<Point>();
            }

            for (int j = 0; j < 2; j++)
            {
                // Sinh ô thứ 1
                int cell = random.Next(0, width * height);
                while (cellIndex.Contains(cell))
                    cell = random.Next(0, width * height);
                table[cell / width, cell % width] = typeOfPokemon;
                possibleMatches[typeOfPokemon].Add(new Point(cell / width, cell % width));
                cellIndex.Add(cell);
            }
        }
    }

    public int GetCell(int row, int col)
    {
        return table[row, col];
    }

    public bool IsSameType(int row1, int col1, int row2, int col2)
    {
        return table[row1, col1] == table[row2, col2];
    }

    public void RemovePair(int row1, int col1, int row2, int col2)
    {
        table[row1, col1] = 0;
        table[row2, col2] = 0;
    }

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
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                if (table[i, j] != 0)
                {
                    table[i, j] = remainingPokemon[index];
                    index++;
                }
            }
        }

        // Kiểm tra nếu không có cặp hợp lệ, thì xáo trộn lại
        if (!HasValidPairs())
        {
            ShuffleBoard(); // Gọi lại phương thức shuffle nếu không tìm thấy cặp hợp lệ
        }
    }

    public bool HasValidPairs()
    {
        for (int r1 = 0; r1 < height; r1++)
        {
            for (int c1 = 0; c1 < width; c1++)
            {
                if (table[r1, c1] == 0) continue;  // Bỏ qua các ô trống

                // Kiểm tra với tất cả các ô còn lại
                for (int r2 = 0; r2 < height; r2++)
                {
                    for (int c2 = 0; c2 < width; c2++)
                    {
                        if (table[r2, c2] == 0 || (r1 == r2 && c1 == c2)) continue;  // Bỏ qua các ô trống và chính nó

                        // Nếu có thể nối được, trả về true
                        if (CanConnect(r1, c1, r2, c2))
                        {
                            return true;
                        }
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