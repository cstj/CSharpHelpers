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

namespace Helpers.WPFHelpers.Controls
{
    /// <summary>
    /// Interaction logic for LoadingBar.xaml
    /// </summary>
    public partial class LoadingBar : UserControl
    {
        public LoadingBar()
        {
            InitializeComponent();
            LayoutRoot.DataContext = this;
        }

        public string LineOne
        {
            get { return (string)GetValue(LineOneProperty); }
            set { SetValue(LineOneProperty, value); }
        }

        public static readonly DependencyProperty LineOneProperty =
            DependencyProperty.Register("LineOne", typeof(string),
            typeof(LoadingBar), new PropertyMetadata("Line One"));


        public string LineTwo
        {
            get { return (string)GetValue(LineTwoProperty); }
            set { SetValue(LineTwoProperty, value); }
        }

        public static readonly DependencyProperty LineTwoProperty =
            DependencyProperty.Register("LineTwo", typeof(string),
            typeof(LoadingBar), new PropertyMetadata("Line Two"));


        public double PgsMax
        {
            get { return Convert.ToDouble(GetValue(PgsMaxProperty)); }
            set { SetValue(PgsMaxProperty, value); }
        }

        public static readonly DependencyProperty PgsMaxProperty =
            DependencyProperty.Register("PgsMax", typeof(double),
            typeof(LoadingBar),
            new PropertyMetadata((double)100, new PropertyChangedCallback((o, e) =>
            {
                ((LoadingBar)o).PgsBar.Maximum = Convert.ToDouble(e.NewValue);
            })));


        public double PgsCurrent
        {
            get { return Convert.ToDouble(GetValue(PgsCurrentProperty)); }
            set { SetValue(PgsCurrentProperty, value); }
        }

        public static readonly DependencyProperty PgsCurrentProperty =
            DependencyProperty.Register(
                "PgsCurrent",
                typeof(double),
                typeof(LoadingBar),
                new PropertyMetadata((double)1, new PropertyChangedCallback((o, e) =>
                {
                    ((LoadingBar)o).PgsBar.Value = Convert.ToDouble(e.NewValue);
                }
            )));

        public double LabelFontSize
        {
            get { return Convert.ToDouble(GetValue(LabelFontSizeProperty)); }
            set { SetValue(LabelFontSizeProperty, value); }
        }

        public static readonly DependencyProperty LabelFontSizeProperty =
            DependencyProperty.Register(
                "LabelFontSize",
                typeof(double),
                typeof(LoadingBar),
                new PropertyMetadata((double)12, new PropertyChangedCallback((o, e) =>
                {
                    LoadingBar bar = (LoadingBar)o;
                    double fontSize = Convert.ToDouble(e.NewValue);
                    if (fontSize < 1) fontSize = 12;
                    bar.LineOneBlock.FontSize = fontSize;
                    bar.LineTwoBlock.FontSize = fontSize;
                }
            )));
    }
}
