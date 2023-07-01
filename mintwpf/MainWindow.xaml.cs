using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.DataVisualization.Charting;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace mintwpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var c = new Chart();
            var series = new AreaSeries() { Title = "Area1", IndependentValuePath = "Key", DependentValuePath = "Value"};
            series.ItemsSource = new KeyValuePair<string, int>[]
            {
                new KeyValuePair<string, int>("A", 5),
                new KeyValuePair<string, int>("B", 6),
                new KeyValuePair<string, int>("C", 7)
            };
            c.Series.Add(series); 
            
            g.Children.Add(c);
        }
    }
}
