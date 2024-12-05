using System.Drawing;
using System.Media;
using System.Windows.Media;

public class GameModel
{
    private int[,] table;
    private int width, height;
    private MediaPlayer soundPlayer;
    private MediaPlayer backgroundMusicPlayer; // Thêm biến để lưu trữ MediaPlayer
    private List<Point> path;
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
        //System.Windows.MessageBox.Show($"[{row}, {col}]: {table[row, col]}");
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

    //public bool CanConnect(int r1, int c1, int r2, int c2)
    //{
    //    // Kiểm tra nếu các ô có cùng loại Pokémon
    //    if (table[r1, c1] != table[r2, c2]) return false;

    //    // Khởi tạo hàng đợi BFS
    //    Queue<(int, int, int, int)> queue = new Queue<(int, int, int, int)>();
    //    bool[,] visited = new bool[height, width];
    //    queue.Enqueue((r1, c1, -1, 0));  // Vị trí ban đầu, không có quẹo và chưa di chuyển

    //    visited[r1, c1] = true;

    //    while (queue.Count > 0)
    //    {
    //        var (currentR, currentC, lastDir, bends) = queue.Dequeue();

    //        // Nếu đã đến ô đích
    //        if (currentR == r2 && currentC == c2)
    //            return true;

    //        // Duyệt qua các hướng di chuyển
    //        for (int i = 0; i < 4; i++)
    //        {
    //            int newR = currentR + rowDirs[i];
    //            int newC = currentC + colDirs[i];

    //            if (IsValidMove(newR, newC) && !visited[newR, newC])
    //            {
    //                // Nếu đi theo hướng mới và khác hướng trước, thì sẽ là một lần quẹo
    //                int newBends = (lastDir == -1 || lastDir == i) ? bends : bends + 1;

    //                // Nếu số lần quẹo vượt quá 2 thì bỏ qua
    //                if (newBends > 2)
    //                    continue;

    //                visited[newR, newC] = true;
    //                queue.Enqueue((newR, newC, i, newBends));
    //            }
    //        }
    //    }

    //    return false;  // Không tìm thấy đường nối hợp lệ
    //}

    //private bool IsValidMove(int r, int c)
    //{
    //    return r >= 0 && r < height && c >= 0 && c < width && table[r, c] != 0;  // Không phải là ô trống
    //}

    bool BFSCheckPath(int[,] board, Point start, Point end)
    {
        path = new List<Point>();
        int rows = height;
        int cols = width;
        var directions = new Point[] { new Point(0, 1), new Point(0, -1), new Point(1, 0), new Point(-1, 0) };
        // System.Windows.MessageBox.Show("DirCOunt : " + directions.Count());

        // BFS Queue: Mỗi phần tử lưu (tọa độ hiện tại, số lần rẽ, hướng di chuyển)
        var queue = new Queue<(Point, int, Point)>();
        var visited = new bool[rows, cols];
        //System.Windows.MessageBox.Show("DirCOunt lan 1 : " + directions.Count());

        foreach (var dir in directions)
        {
            queue.Enqueue((start, 0, dir));
        }

        int finalTurn = -1;
        Point finalPoint = new Point(1000, 1000);
        while (queue.Count > 0)
        {
            var (current, turns, direction) = queue.Dequeue();

            finalPoint = current;
            finalTurn = turns;

            if (turns > 2) continue;

            if (turns == 1)
            {
                // Vector hướng từ current đến end
                var targetDirection = new Point(end.X - current.X, end.Y - current.Y);

                // Kiểm tra nếu hướng di chuyển ngược với targetDirection
                if (IsOppositeDirection(direction, targetDirection))
                {
                    //System.Windows.MessageBox.Show("Diem bi nguoc : " + current + " || direction : " + direction + "|| turns : " + turns);
                    continue; // Bỏ qua nếu hướng ngược chiều
                }
            }

            else if (turns == 2)
            {
                if (current.X != end.X && current.Y != end.Y)
                    continue;
            }

            if (!visited[current.X, current.Y] && turns <= 2)
                path.Add(current);

            visited[current.X, current.Y] = true;

            foreach (var dir in directions)
            {
                var next = new Point(current.X + dir.X, current.Y + dir.Y);

                if (IsValidMove(board, visited, next, rows, cols))
                {
                    int newTurns = (dir != direction) ? turns + 1 : turns;
                    queue.Enqueue((next, newTurns, dir));
                }

                if (next == end && turns <= 2)
                {
                    path.Add(end);
                    string s = "|| ";
                    foreach (Point p in path)
                    {
                        s += p + " || ";
                    }
                    // System.Windows.MessageBox.Show("So luong : " + path.Count());
                    // System.Windows.MessageBox.Show("Mem of path : " + s);
                    return true;
                }
            }
        }

        string s2 = "";
        foreach (Point p in path)
        {
            s2 += p + " || ";
        }
        // System.Windows.MessageBox.Show("Duong dan sai : \n " + s2 + "|| final turn : " + finalTurn + "|| final point : " + finalPoint);
        // ResetLinePaths();
        return false;
    }


    private bool IsOppositeDirection(Point dir1, Point dir2)
    {
        // Chuẩn hóa hướng để so sánh (chỉ giữ -1, 0, 1)
        dir1 = NormalizeDirection(dir1);
        dir2 = NormalizeDirection(dir2);

        // Hai hướng ngược chiều nếu tích vô hướng của chúng là -1
        return dir1.X * dir2.X + dir1.Y * dir2.Y == -1;
    }


    bool IsValidMove(int[,] board, bool[,] visited, Point next, int rows, int cols)
    {
        return next.X >= 0 && next.X < rows &&
               next.Y >= 0 && next.Y < cols &&
               !visited[next.X, next.Y] &&
               board[next.X, next.Y] == 0; // Ô trống
    }
    public bool CanConnect(int x1, int y1, int x2, int y2)
    {
        Point p1 = new Point(x1, y1);
        Point p2 = new Point(x2, y2);
        if (table[p1.X, p1.Y] != table[p2.X, p2.Y] || table[p1.X, p1.Y] == 0)
            return false;

        return BFSCheckPath(table, p1, p2) == true ? true : BFSCheckPath(table, p2, p1);
    }
    private Point NormalizeDirection(Point dir)
    {
        // Chuẩn hóa vector để có giá trị -1, 0, 1
        int x = dir.X == 0 ? 0 : dir.X / Math.Abs(dir.X);
        int y = dir.Y == 0 ? 0 : dir.Y / Math.Abs(dir.Y);
        return new Point(x, y);
    }
}