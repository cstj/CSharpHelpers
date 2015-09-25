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
            try
            {
                InitializeComponent();
                if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this)) return;
                LayoutRoot.DataContext = this;
            }
            catch
            {

            }
        }

        public string LineOne
        {
            get { return GetValue(LineOneProperty).ToString(); }
            set { SetValue(LineOneProperty, value); }
        }

        public static readonly DependencyProperty LineOneProperty =
            DependencyProperty.Register("LineOne", 
                typeof(string),
                typeof(LoadingBar), new PropertyMetadata("Line One"));


        public string LineTwo
        {
            get { return GetValue(LineTwoProperty).ToString(); }
            set { SetValue(LineTwoProperty, value); }
        }

        public static readonly DependencyProperty LineTwoProperty =
            DependencyProperty.Register("LineTwo", typeof(string),
            typeof(LoadingBar), new PropertyMetadata("Line Two"));


        public object PgsMax
        {
            get { return GetValue(PgsMaxProperty); }
            set { SetValue(PgsMaxProperty, value); }
        }

        public static readonly DependencyProperty PgsMaxProperty =
            DependencyProperty.Register("PgsMax",
            typeof(object),
            typeof(LoadingBar),
            new PropertyMetadata((object)100.0, 
            new PropertyChangedCallback((o, e) =>
            {
                LoadingBar bar = (LoadingBar)o;
                double value = ReturnValue<double>(e.NewValue, new Func<object,double>(val => Convert.ToDouble(val)), 0);
                bar.PgsBar.Maximum = value;
            })));


        public object PgsCurrent
        {
            get { return GetValue(PgsCurrentProperty); }
            set { SetValue(PgsCurrentProperty, value); }
        }

        public static readonly DependencyProperty PgsCurrentProperty =
            DependencyProperty.Register(
                "PgsCurrent",
                typeof(object),
                typeof(LoadingBar),
                new PropertyMetadata((object)1.0, 
                new PropertyChangedCallback((o, e) =>
                {
                    LoadingBar bar = (LoadingBar)o;
                    double value = ReturnValue<double>(e.NewValue, new Func<object,double>(val => Convert.ToDouble(val)), 0);
                    bar.PgsBar.Value = value;
                }
            )));

        public object LabelFontSize
        {
            get { return GetValue(LabelFontSizeProperty); }
            set { SetValue(LabelFontSizeProperty, value); }
        }

        public static readonly DependencyProperty LabelFontSizeProperty =
            DependencyProperty.Register(
                "LabelFontSize",
                typeof(object),
                typeof(LoadingBar),
                new PropertyMetadata((object)12.0, 
                new PropertyChangedCallback((o, e) =>
                {
                    LoadingBar bar = (LoadingBar)o;
                    double fontSize = ReturnValue<double>(e.NewValue, new Func<object,double>(val => Convert.ToDouble(val)), 12);

                    bar.LineOneBlock.FontSize = fontSize;
                    bar.LineTwoBlock.FontSize = fontSize;
                }
            )));

        private static T ReturnValue<T>(object value, Func<object, T> converter, T defaultValue)
        {
            if (value == null) return defaultValue;
            if (value is IConvertible) return converter(value);
            return defaultValue;
        }
    }
}
