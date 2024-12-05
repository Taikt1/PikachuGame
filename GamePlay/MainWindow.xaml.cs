using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;


namespace GamePlay
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private GameModel gameModel;
        private Border[,] pokemonCells;
        private Border firstSelected = null;
        private int score = 0;
        private int timeLeft = DEFAULT_TIME_LEFT;
        private DispatcherTimer timer;
        private int shuffleCount = 0;  // Biến đếm số lần shuffle
        private int level = 1;
        

        private const int POKEMON_TYPES = 36;  // Số loại pokemon
        private int boardWidth = BOARD_WIDTH;    // Số cột
        private int boardHeight = BOARD_HEIGHT;   // Số hàng
        private const int CELL_SIZE = 40;      // Kích thước mỗi ô
        private const int MAX_SHUFFLE_COUNT = 5;  // Giới hạn số lần shuffle
        private const int DEFAULT_TIME_LEFT = 300;
        private const int BOARD_WIDTH = 4;
        private const int BOARD_HEIGHT = 4;

        public MainWindow()
        {
            InitializeComponent();

            GameCanvas.Width = boardWidth * CELL_SIZE;
            GameCanvas.Height = boardHeight * CELL_SIZE;
          
            ((Viewbox)GameCanvas.Parent).MinWidth = boardWidth * CELL_SIZE;
            ((Viewbox)GameCanvas.Parent).MinHeight = boardHeight * CELL_SIZE;
            ((Viewbox)GameCanvas.Parent).MaxHeight = boardHeight * CELL_SIZE + 100;
            ((Viewbox)GameCanvas.Parent).MaxWidth = boardWidth * CELL_SIZE + 100;

            this.MinWidth = boardWidth * CELL_SIZE + 106;
            this.MinHeight = boardHeight * CELL_SIZE + 106;

            StartNewGame();
        }

        private void StartNewGame()
        {
            // Reset game
            score = 0;
            ScoreText.Text = "0";
            TimeText.Text = $"{DEFAULT_TIME_LEFT}";
            shuffleCount = 0;
            ShuffleButton.IsEnabled = true;
            ShuffleButton.Content = $"Shuffle: {MAX_SHUFFLE_COUNT - shuffleCount}";
            GameCanvas.Children.Clear();
            firstSelected = null;
            boardWidth = BOARD_WIDTH;
            boardHeight = BOARD_HEIGHT;

            SetUpWindowSize();

            // Khởi tạo game mới
            gameModel = new GameModel(boardWidth, boardHeight, POKEMON_TYPES);
            pokemonCells = new Border[boardHeight, boardWidth];

            CreateGameBoard();
            StartTimer();
        }

        private void SetUpWindowSize()
        {
            GameCanvas.Width = boardWidth * CELL_SIZE;
            GameCanvas.Height = boardHeight * CELL_SIZE;

            ((Viewbox)GameCanvas.Parent).MinWidth = boardWidth * CELL_SIZE;
            ((Viewbox)GameCanvas.Parent).MinHeight = boardHeight * CELL_SIZE;
            ((Viewbox)GameCanvas.Parent).MaxHeight = boardHeight * CELL_SIZE + 100;
            ((Viewbox)GameCanvas.Parent).MaxWidth = boardWidth * CELL_SIZE + 100;

            this.MinWidth = boardWidth * CELL_SIZE + 106;
            this.MinHeight = boardHeight * CELL_SIZE + 106;
        }

        private void NextLevel()
        {
            timeLeft = DEFAULT_TIME_LEFT;
            TimeText.Text = $"{timeLeft}";
            GameCanvas.Children.Clear();
            firstSelected = null;
            level++;

            // Khởi tạo game mới
            IncreaseBoardSize();
            gameModel = new GameModel(boardWidth, boardHeight, POKEMON_TYPES);
            pokemonCells = new Border[boardHeight, boardWidth];
            
            SetUpWindowSize();

            CreateGameBoard();
            StartTimer();
        }

        public void IncreaseBoardSize()
        {
            // Tăng kích thước xen kẽ giữa chiều rộng và chiều cao
            if (level % 2 == 0)
            {
                boardWidth += 2; // Tăng chiều rộng
            }
            else
            {
                boardHeight += 2; // Tăng chiều cao
            }

            // Đảm bảo số ô là số chẵn
            if ((boardWidth * boardHeight) % 2 != 0)
            {
                boardWidth++; // Điều chỉnh để tổng số ô là số chẵn
            }

            // Giới hạn kích thước tối đa (tuỳ chọn)
            int maxWidth = 20;
            int maxHeight = 10;

            if (boardWidth > maxWidth) { 
                boardWidth = maxWidth;
                timeLeft = DEFAULT_TIME_LEFT - level;
            }

            if (boardHeight > maxHeight)
                boardHeight = maxHeight;

        }

        private void CreateGameBoard()
        {
            for (int i = 0; i < boardHeight; i++)
            {
                for (int j = 0; j < boardWidth; j++)
                {
                    // Tạo hình ảnh pokemon
                    Image pokemonImage = new Image
                    {
                        Width = CELL_SIZE - 2,
                        Height = CELL_SIZE - 2,
                        Stretch = Stretch.Uniform,
                        Source = new BitmapImage(new Uri($"pack://application:,,,/Images/pieces{gameModel.GetCell(i, j)}.png"))
                    };

                    // Tạo border chứa pokemon
                    Border cell = new Border
                    {
                        Width = CELL_SIZE,
                        Height = CELL_SIZE,
                        Background = Brushes.White,
                        BorderBrush = Brushes.Gray,
                        BorderThickness = new Thickness(1),
                        Child = pokemonImage
                    };

                    // Gán sự kiện và vị trí
                    cell.MouseDown += Cell_MouseDown;
                    cell.Tag = new Point(i, j);
                    Canvas.SetLeft(cell, j * CELL_SIZE);
                    Canvas.SetTop(cell, i * CELL_SIZE);

                    GameCanvas.Children.Add(cell);
                    pokemonCells[i, j] = cell;
                }
            }
        }

        private void Cell_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Border clickedCell = sender as Border;
            Point position = (Point)clickedCell.Tag;
            int row = (int)position.X;
            int col = (int)position.Y;

            if (gameModel.GetCell(row, col) == 0)
                return;

            if (firstSelected == null)
            {
                firstSelected = clickedCell;
                clickedCell.Background = Brushes.Yellow;
            }
            else
            {
                Point firstPos = (Point)firstSelected.Tag;
                int firstRow = (int)firstPos.X;
                int firstCol = (int)firstPos.Y;

                if (firstRow == row && firstCol == col)
                {
                    firstSelected.Background = Brushes.White;
                    firstSelected = null;
                    return;
                }

                if (gameModel.IsSameType(firstRow, firstCol, row, col))
                {
                    gameModel.RemovePair(firstRow, firstCol, row, col);
                    firstSelected.Visibility = Visibility.Hidden;
                    clickedCell.Visibility = Visibility.Hidden;
                    score += 10 * level;
                    ScoreText.Text = score.ToString();
                    if (gameModel.PossibleMatches.Count > 0)
                    {
                        // Kiểm tra nếu không còn cặp hợp lệ và shuffle
                        if (!gameModel.HasValidPairs())
                        {
                            System.Windows.MessageBox.Show("Hết đường đi, màn chơi sẽ được xáo trộn");
                            gameModel.ShuffleBoard();
                            // Cập nhật giao diện sau khi shuffle
                            UpdateGameBoard();
                        }
                    } 
                    else
                    {
                        NextLevel();
                    }
                }

                //firstSelected.Background = Brushes.Red;
                firstSelected = null;
            }
        }


        private void StartTimer()
        {
            if (timer != null)
                timer.Stop();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            timeLeft--;
            TimeText.Text = timeLeft.ToString();

            if (timeLeft <= 0)
            {
                timer.Stop();
                MessageBox.Show($"Hết giờ! Điểm của bạn: {score}", "Game Over");
                StartNewGame();
            }
        }

        private void btnNewGame_Click(object sender, RoutedEventArgs e)
        {
            StartNewGame();
        }

        private void ShuffleButton_Click(object sender, RoutedEventArgs e)
        {
            if (shuffleCount >= MAX_SHUFFLE_COUNT)
                return;
            // Gọi phương thức Shuffle từ GameModel
            gameModel.ShuffleBoard();

            // Cập nhật giao diện sau khi shuffle
            UpdateGameBoard();

            shuffleCount++;
            if(shuffleCount > (MAX_SHUFFLE_COUNT - 1))
            {
                ShuffleButton.IsEnabled = false;
            }
            ShuffleButton.Content = $"Shuffle: {MAX_SHUFFLE_COUNT - shuffleCount}";
        }

        private void UpdateGameBoard()
        {
            for (int i = 0; i < boardHeight; i++)
            {
                for (int j = 0; j < boardWidth; j++)
                {
                    if (pokemonCells[i, j] != null)
                    {
                        int pokemonType = gameModel.GetCell(i, j);

                        if (pokemonType == 0)
                        {
                            pokemonCells[i, j].Visibility = Visibility.Hidden;
                        }
                        else
                        {
                            Image pokemonImage = (Image)pokemonCells[i, j].Child;
                            pokemonImage.Source = new BitmapImage(new Uri($"pack://application:,,,/Images/pieces{pokemonType}.png"));
                            pokemonCells[i, j].Visibility = Visibility.Visible;
                        }
                    }
                }
            }
        }


    }
}