using System.Media;
using System.Windows.Media;

public class GameModel
{
    private int[,] table;
    private int width, height;
    private MediaPlayer soundPlayer;
    private MediaPlayer backgroundMusicPlayer; // Thêm biến để lưu trữ MediaPlayer


    public int Width { get => width; }
    public int Height { get => height; }

    public GameModel(int _width, int _height, int _numOfType)
    {
        width = _width;
        height = _height;
        table = new int[height, width];
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

            // Sinh ô thứ 1
            int cell1 = random.Next(0, width * height);
            while (cellIndex.Contains(cell1))
                cell1 = random.Next(0, width * height);
            table[cell1 / width, cell1 % width] = typeOfPokemon;
            cellIndex.Add(cell1);

            // Sinh ô thứ 2
            int cell2 = random.Next(0, width * height);
            while (cellIndex.Contains(cell2))
                cell2 = random.Next(0, width * height);
            table[cell2 / width, cell2 % width] = typeOfPokemon;
            cellIndex.Add(cell2);
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

   
}