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

        public int Number
        {
            get { return (int)this.GetValue(NumberProperty); }
            set { this.SetValue(NumberProperty, value); }
        }

        public bool IsHot { get; set; }
    }
}
