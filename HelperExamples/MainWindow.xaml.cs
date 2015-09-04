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

namespace HelperExamples
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Exception e1 = new InsufficientMemoryException("Test Memory Exception");
            e1.Source = this.Name;
            Exception b = new DivideByZeroException("Test Devide by Zero Exception", e1);
            b.Source = "BOB";
            Exception ex = new Exception("Test General Exception", b);
            ex.Data["Key 1"] = "Data 1";
            ex.Data["Key 2"] = "Data 2";
            ex.Data["Key 3"] = "Data 3";
            WPFHelpers.ExceptionsViewer ev = new WPFHelpers.ExceptionsViewer(ex, "Error");

        }
    }
}
