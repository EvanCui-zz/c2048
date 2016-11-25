using Microsoft.Expression.Media.Effects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace C2048
{
    /// <summary>
    /// Interaction logic for NumberBlock.xaml
    /// </summary>
    public partial class NumberBlock : UserControl
    {
        public NumberBlock()
        {
            InitializeComponent();
        }

        public static DependencyProperty NumberProperty = DependencyProperty.Register("Number", typeof(int), typeof(NumberBlock));
        public static DependencyProperty BlockBackgroundProperty = DependencyProperty.Register("BlockBackground", typeof(Color), typeof(NumberBlock));

        public int Number
        {
            get { return (int)this.GetValue(NumberProperty); }
            set
            {
                this.SetValue(NumberProperty, value);
                int t = 1;
                while ((value >>= 1) > 0) t++;
                Color c = Color.FromRgb((byte)(150 + 10 * t), (byte)(250 - 20 * t), (byte)(10 + 30 * t));
                this.BlockBackground = c;
                Brush b = new SolidColorBrush(c);
                this.border.Background = b;
            }
        }

        public Color BlockBackground
        {
            get
            {
                return (Color)this.GetValue(BlockBackgroundProperty);
            }

            set
            {
                this.SetValue(BlockBackgroundProperty, value);
            }
        }

        private bool isHot;
        public bool IsHot
        {
            get { return this.isHot; }
            set
            {
                this.isHot = value;
                if (value)
                {
                    this.border.Effect = new MagnifyEffect() { Amount = 0.5 };
                }
                else
                {
                    this.border.Effect = null;
                }
            }
        }
    }
}
