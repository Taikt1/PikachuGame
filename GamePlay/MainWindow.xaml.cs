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
        

        private const int POKEMON_TYPES = 36;  // Số loại pokemon
        private const int BOARD_WIDTH = 20;    // Số cột
        private const int BOARD_HEIGHT = 10;   // Số hàng
        private const int CELL_SIZE = 40;      // Kích thước mỗi ô
        private const int MAX_SHUFFLE_COUNT = 3;  // Giới hạn số lần shuffle
        private const int DEFAULT_TIME_LEFT = 240;
        public MainWindow()
        {
            InitializeComponent();
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

            // Khởi tạo game mới
            gameModel = new GameModel(BOARD_WIDTH, BOARD_HEIGHT, POKEMON_TYPES);
            pokemonCells = new Border[BOARD_HEIGHT, BOARD_WIDTH];
            CreateGameBoard();
            StartTimer();
        }

        private void CreateGameBoard()
        {
            for (int i = 0; i < BOARD_HEIGHT; i++)
            {
                for (int j = 0; j < BOARD_WIDTH; j++)
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
                    score += 10;
                    ScoreText.Text = score.ToString();
                    // Kiểm tra nếu không còn cặp hợp lệ và shuffle
                    if (!gameModel.HasValidPairs())
                    {
                        gameModel.ShuffleBoard();
                        // Cập nhật giao diện sau khi shuffle
                        UpdateGameBoard();
                    }
                }

                firstSelected.Background = Brushes.White;
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
            if(shuffleCount == 2)
            {
                ShuffleButton.IsEnabled = false;
            }
            ShuffleButton.Content = $"Shuffle: {MAX_SHUFFLE_COUNT - shuffleCount}";
        }

        private void UpdateGameBoard()
        {
            for (int i = 0; i < BOARD_HEIGHT; i++)
            {
                for (int j = 0; j < BOARD_WIDTH; j++)
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