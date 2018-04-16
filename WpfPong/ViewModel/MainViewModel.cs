using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Linq;
using System.ComponentModel;
using System.Windows.Input;
using System.Runtime.CompilerServices;

namespace WpfPong.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        #region Constants
        const int PADDLE_VELOCITY = 15;
        const int BALL_VELOCITY = 20;
        #endregion

        #region Member Variables
        readonly DispatcherTimer _timer = new DispatcherTimer();
        int _reboundAngle = 0;
        #endregion

        #region Constructor(s)
        public MainViewModel() { }
        #endregion

        #region Properties
        public double BallTop { get; set; } = 200;
        public double BallLeft { get; set; } = 300;
        public double BallWidth { get; set; } = 15;
        public double BallHeight { get; set; } = 15;
        public double CanvasHeight { get; set; } = 400;
        public double CanvasWidth { get; set; } = 600;
        public double Paddle1Width { get; set; } = 8;
        public double Paddle2Width { get; set; } = 8;
        public double Paddle1Height { get; set; } = 80;
        public double Paddle2Height { get; set; } = 80;
        public double Paddle1Top { get; set; } = 0;
        public double Paddle1Left { get; set; } = 5;
        public double Paddle2Top { get; set; } = 0;
        public double Paddle2Left { get; set; } = 587;
        public string Player1Score { get; set; } = "0";
        public string Player2Score { get; set; } = "0";
        public Ellipse Ball { get; set; }
        public Rectangle Paddle1 { get; set; }
        public Rectangle Paddle2 { get; set; }
        #endregion

        #region Commands
        public RelayCommand<Canvas> _loadedCommand { get; private set; }
        public RelayCommand<Canvas> LoadedCommand {
            get {
                return _loadedCommand ?? (_loadedCommand = new RelayCommand<Canvas>((canvas) => {
                    InitializeBallAndPaddles(canvas);
                    InitializeTimer();
                    Reset();
                }));
            }
        }

        public RelayCommand<EventArgs> _keyDownCommand { get; private set; }
        public RelayCommand<EventArgs> KeyDownCommand
        {
            get
            {
                return _keyDownCommand ?? (_keyDownCommand = new RelayCommand<EventArgs>((e) => {
                    var pressedKey = (e != null) ? (KeyEventArgs)e : null;

                    if (pressedKey.Key == Key.Up)
                    {
                        MovePaddle(Paddle2, -PADDLE_VELOCITY);
                    }
                    else if (pressedKey.Key == Key.Down)
                    {
                        MovePaddle(Paddle2, PADDLE_VELOCITY);
                    }
                    else if (pressedKey.Key == Key.W)
                    {
                        MovePaddle(Paddle1, -PADDLE_VELOCITY);
                    }
                    else if (pressedKey.Key == Key.S)
                    {
                        MovePaddle(Paddle1, PADDLE_VELOCITY);
                    }
                }));
            }
        }
        #endregion

        #region Event Handlers
        private void timer_Tick(object sender, EventArgs e)
        {
            var x = BallLeft;
            var y = BallTop;

            var ballDeltaX = BALL_VELOCITY * Math.Cos(Math.PI / 180 * _reboundAngle);
            var ballDeltaY = BALL_VELOCITY * Math.Sin(Math.PI / 180 * _reboundAngle);

            var ballNewXPos = x + ballDeltaX;
            var ballNewYPos = y + ballDeltaY;

            BallLeft = ballNewXPos;
            BallTop = ballNewYPos;
            Canvas.SetLeft(Ball, BallLeft);
            Canvas.SetTop(Ball, BallTop);

            if (_reboundAngle > 360)
            {
                _reboundAngle -= 360;
            }
            else if (_reboundAngle < 0)
            {
                _reboundAngle += 360;
            }

            if (BallCollidedWithPaddle((int)ballNewXPos, (int)ballNewYPos))
            {
                if (_reboundAngle >= 0 && _reboundAngle <= 90)
                {
                    _reboundAngle += 180 - (2 * _reboundAngle);
                }
                else if (_reboundAngle >= 91 && _reboundAngle <= 360)
                {
                    _reboundAngle -= 180 + (2 * _reboundAngle);
                }
            }
            else if (BallCollidedWithCanvas((int)ballNewXPos, (int)ballNewYPos))
            {
                _reboundAngle = 360 - _reboundAngle;
            }
            else if (BallIsOutOfBounds((int)ballNewXPos, (int)ballNewYPos))
            {
                if (ballNewXPos < 0)
                {
                    var score = Convert.ToInt32(Player2Score);
                    Player2Score = (score + 1).ToString();

                    RaisePropertyChanged(nameof(Player2Score));
                }
                else
                {
                    var score = Convert.ToInt32(Player1Score);
                    Player1Score = (score + 1).ToString();

                    RaisePropertyChanged(nameof(Player1Score));
                }

                Reset();
            }
        }
        #endregion

        #region Helper Methods
        private void InitializeBallAndPaddles(Canvas canvas)
        {
            Ball = canvas.Children.OfType<Ellipse>().FirstOrDefault();

            var paddles = canvas.Children.OfType<Rectangle>();

            Paddle1 = paddles.FirstOrDefault(x => x.Name.Equals("paddle1"));
            Paddle2 = paddles.FirstOrDefault(x => x.Name.Equals("paddle2"));
        }

        private void InitializeTimer()
        {
            _timer.Tick += timer_Tick;
            _timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
        }

        private void Reset()
        {
            var random = new Random();

            _timer.Stop();

            Canvas.SetLeft(Ball, CanvasWidth / 2);
            Canvas.SetTop(Ball, CanvasHeight / 2);
            BallLeft = CanvasWidth / 2;
            BallTop = CanvasHeight / 2;

            if (random.Next(1, 3) == 1)
            {
                if (random.Next(1, 3) == 1)
                {
                    _reboundAngle = random.Next(1, 60);
                }
                else
                {
                    _reboundAngle = random.Next(300, 359);
                }
            }
            else
            {
                _reboundAngle = random.Next(120, 240);
            }

            _timer.Start();
        }

        private void MovePaddle(Rectangle paddle, int delta)
        {
            var y = Canvas.GetTop(paddle);

            if (!IsPaddleOutOfBounds(paddle, y, delta))
            {
                Canvas.SetTop(paddle, y + delta);

                if (paddle.Name.Equals(Paddle1.Name))
                {
                    Paddle1Top = y + delta;
                }
                else
                {
                    Paddle2Top = y + delta;
                }
            }
        }

        private bool IsPaddleOutOfBounds(Rectangle paddle, double y, int delta)
        {
            return y + delta < 0 || y + delta + paddle.Height > CanvasHeight;
        }

        private bool BallCollidedWithPaddle(int x, int y)
        {
            return x <= Paddle1Left + Paddle1Width + 3 && y >= Paddle1Top && y <= Paddle1Top + Paddle1Height
                || x + BallWidth >= Paddle2Left - 3 && y >= Paddle2Top && y <= Paddle2Top + Paddle2Height;
        }

        private bool BallCollidedWithCanvas(int x, int y)
        {
            return y + BallHeight >= CanvasHeight || y <= 0;
        }

        private bool BallIsOutOfBounds(int x, int y)
        {
            return x < 0 - BallWidth || x > CanvasWidth + BallWidth;
        }
        #endregion
    }
}