using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace C2048
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int MaxY = 10;
        private const int MaxX = 7;
        private const double BlockSize = 50;

        private Random r = new Random(DateTime.Now.Millisecond);

        private readonly Duration CombineDuration = new Duration(TimeSpan.FromMilliseconds(100));
        private readonly Duration FallDuration = new Duration(TimeSpan.FromMilliseconds(500));

        private NumberBlock[,] blocks = new NumberBlock[7, 10];

        private class Coordinate
        {
            public Coordinate(int x, int y) { this.X = x; this.Y = y; }
            public int X;
            public int Y;

            public bool IsValid { get { return this.X >= 0 && this.X < MaxX && this.Y >= 0 && this.Y < MaxY; } }

            public IEnumerable<Coordinate> GetAdjacent()
            {
                var c = this.Left; if (c.IsValid) yield return c;
                c = this.Right; if (c.IsValid) yield return c;
                c = this.Up; if (c.IsValid) yield return c;
                c = this.Down; if (c.IsValid) yield return c;
            }

            public Coordinate Left { get { return new Coordinate(this.X - 1, this.Y); } }
            public Coordinate Right { get { return new Coordinate(this.X + 1, this.Y); } }
            public Coordinate Up { get { return new Coordinate(this.X, this.Y - 1); } }
            public Coordinate Down { get { return new Coordinate(this.X, this.Y + 1); } }
        }

        private bool[] HotColumns = new bool[MaxX];

        public MainWindow()
        {
            InitializeComponent();
        }

        private Coordinate Current { get; set; }

        public void PutNewBlock(int number)
        {
            NumberBlock nb = new NumberBlock() { Number = number };
            nb.Width = nb.Height = BlockSize;
            this.canvas.Children.Add(nb);
            Canvas.SetTop(nb, 0.0);
            Canvas.SetLeft(nb, 0.0);

            int x = 0;
            this.HotColumns[x] = true;
            this.blocks[x, 0] = nb;
            nb.IsHot = true;
            this.Current = new Coordinate(0, 0);
        }

        public void MoveTo(int c)
        {
            var b = this.blocks[c, this.Current.Y] = Interlocked.Exchange(ref this.blocks[this.Current.X, this.Current.Y], null);
            this.Current.X = c;
            Canvas.SetLeft(b, this.Current.X * BlockSize);
        }

        public Task<bool> Combine()
        {
            Storyboard combineStory = new Storyboard();
            bool[] newHotColumns = new bool[MaxX];

            bool combined = false;

            foreach (int c in Enumerable.Range(0, MaxX).Where(i => this.HotColumns[i]))
            {
                for (int y = MaxY - 1; y >= 0; y--)
                {
                    var b = this.blocks[c, y];
                    if (b != null && b.IsHot)
                    {
                        bool hoter = false;
                        int number = b.Number;
                        foreach (var ad in new Coordinate(c, y).GetAdjacent().Where(co => this.blocks[co.X, co.Y] != null && this.blocks[co.X, co.Y].Number == b.Number))
                        {
                            number *= 2;
                            var a = Interlocked.Exchange(ref this.blocks[ad.X, ad.Y], null);
                            newHotColumns[ad.X] = true;
                            newHotColumns[c] = true;
                            hoter = true;
                            combined = true;

                            DoubleAnimation opacity = new DoubleAnimation(0.0, CombineDuration, FillBehavior.HoldEnd);
                            Storyboard.SetTarget(opacity, a);
                            Storyboard.SetTargetProperty(opacity, new PropertyPath(NumberBlock.OpacityProperty));
                            combineStory.Children.Add(opacity);

                            DoubleAnimation xAni = new DoubleAnimation(c * BlockSize, CombineDuration);
                            Storyboard.SetTarget(xAni, a);
                            Storyboard.SetTargetProperty(xAni, new PropertyPath(Canvas.LeftProperty));
                            combineStory.Children.Add(xAni);

                            DoubleAnimation yAni = new DoubleAnimation(y * BlockSize, CombineDuration);
                            Storyboard.SetTarget(yAni, a);
                            Storyboard.SetTargetProperty(yAni, new PropertyPath(Canvas.TopProperty));
                            combineStory.Children.Add(yAni);
                        }

                        b.Number = number;
                        b.IsHot = hoter;
                    }
                }
            }

            combineStory.Begin();

            this.HotColumns = newHotColumns;

            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            combineStory.Completed += (s, e) => { tcs.SetResult(combined); };
            combineStory.Begin();
            return tcs.Task;
        }

        public Task<bool> Fall()
        {
            Storyboard fallStory = new Storyboard();
            bool fallen = false;

            foreach (int c in Enumerable.Range(0, MaxX))
            {
                int y1 = MaxY - 1;
                int y2 = y1;

                while (y2 >= 0)
                {
                    if (this.blocks[c, y2] == null || y2 == y1)
                    {
                        y2--;
                        continue;
                    }

                    if (this.blocks[c, y1] != null)
                    {
                        y1--;
                        continue;
                    }

                    var b = this.blocks[c, y1] = Interlocked.Exchange(ref this.blocks[c, y2], null);
                    b.IsHot = true;
                    fallen = true;
                    this.HotColumns[c] = true;

                    DoubleAnimation fall = new DoubleAnimation(y1 * BlockSize, FallDuration, FillBehavior.HoldEnd);
                    Storyboard.SetTarget(fall, b);
                    Storyboard.SetTargetProperty(fall, new PropertyPath(Canvas.TopProperty));

                    fallStory.Children.Add(fall);
                }
            }

            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            fallStory.Completed += (s,e) => { tcs.SetResult(fallen); };
            fallStory.Begin();
            return tcs.Task;
        }

        private int GetRandom()
        {
            int p = r.Next(6);
            int re = 2;
            while(p-- > 0)
            {
                re *= 2;
            }

            return re;
        }

        private async void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.N:
                    this.PutNewBlock(2);
                    break;

                case Key.F:
                case Key.Space:
                    bool loop = true;

                    while (loop)
                    {
                        await this.Fall();
                        loop = await this.Combine();
                    }

                    this.PutNewBlock(this.GetRandom());
                    break;

                case Key.C:
                    this.status.Text = this.Combine().ToString();
                    break;

                default:
                    if (e.Key >= Key.D1 && e.Key <= Key.D7)
                    {
                        int offset = e.Key - Key.D1;
                        this.MoveTo(offset);
                    }

                    break;
            }
        }
    }
}
