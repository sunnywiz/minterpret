using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
using Microsoft.Win32;

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

            CheckGuards(); 

            var c = new Chart();

            var sas = new StackedAreaSeries();
            sas.SeriesDefinitions.Add(new SeriesDefinition()
            {
                Name="A2", 
                IndependentValuePath = "Key",
                DependentValuePath = "Value",
                ItemsSource = new[]
                {
                    new KeyValuePair<DateTime, int>(DateTime.Now.Date.AddDays(-30), 5),
                    new KeyValuePair<DateTime, int>(DateTime.Now.Date.AddDays(-20), 6),
                    new KeyValuePair<DateTime, int>(DateTime.Now.Date.AddDays(-10), 8)
                },
            });
            sas.SeriesDefinitions.Add(new SeriesDefinition()
            {
                Name = "A1",
                IndependentValuePath = "Key",
                DependentValuePath = "Value",
                ItemsSource = new[]
            {
                new KeyValuePair<DateTime, int>(DateTime.Now.Date.AddDays(-30), 4),
                new KeyValuePair<DateTime, int>(DateTime.Now.Date.AddDays(-20), 3),
                new KeyValuePair<DateTime, int>(DateTime.Now.Date.AddDays(-10), 0)
            }
            });
            sas.Name = "Boogie";
            c.Series.Add(sas);
            GroupBoxGraphResult.Content=c;
        }

        public void CheckGuards()
        {
            ButtonLoadMintCsv.IsEnabled = !String.IsNullOrWhiteSpace(TextBoxMintCsvDirectory.Text) &&
                                          Directory.Exists(TextBoxMintCsvDirectory.Text);

        }

        private void ButtonChooseMintCsvDirectory_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new SaveFileDialog()
            {
                InitialDirectory = TextBoxMintCsvDirectory.Text,
                FileName = "Choose folder filename doesnt matter",
                CheckPathExists = true,
                DefaultExt = ".csv"
            };
            if (ofd.ShowDialog() == true)
            {
                TextBoxMintCsvDirectory.Text = System.IO.Path.GetDirectoryName(ofd?.FileName)??string.Empty;
            }

            CheckGuards(); 
        }

        private void ButtonLoadMintCsv_Click(object sender, RoutedEventArgs e)
        {

        }

    }
}
