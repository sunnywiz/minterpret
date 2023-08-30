using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Win32;

namespace mintwpf;

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
        try
        {
            var di = new DirectoryInfo(TextBoxMintCsvDirectory.Text);
            if (!di.Exists) return;

            var files = di.GetFiles("*.csv").OrderByDescending(x => x.Name).ToList();

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                PrepareHeaderForMatch = args => args.Header.Replace(" ", ""),
            };
            List<MintRecord> allRecords = new List<MintRecord>();
            foreach (var file in files)
            {
                List<MintRecord> records;
                using (var reader = new StreamReader(file.FullName))
                {
                    using var csv = new CsvReader(reader, config);
                    records = csv.GetRecords<MintRecord>().ToList();
                    
                    var byGroup = records.GroupBy(x => new
                        { x.Date, x.OriginalDescription, x.Amount, x.TransactionType, x.AccountName });
                    foreach (var g in byGroup)
                    {
                        var gl = g.ToList();
                        if (gl.Count == 1) continue; 
                        for (int i = 1; i < gl.Count; i++)
                        {
                            gl[i].OriginalDescription += $" (Minterpret:{i + 1}/{gl.Count})";
                        }
                    }

                    // now that we have de-duped within a file, lets combine all them
                    foreach (var r in records)
                    {
                        var match = allRecords.FirstOrDefault(q =>
                            q.Date == r.Date &&
                            q.Amount == r.Amount &&
                            String.Compare(q.OriginalDescription, r.OriginalDescription,
                                StringComparison.InvariantCulture) == 0 &&
                            String.Compare(q.AccountName, r.AccountName, StringComparison.InvariantCulture) == 0 &&
                            String.Compare(q.TransactionType, r.TransactionType) == 0);
                        if (match == null) allRecords.Add(r); 
                    }

                }
                TextBoxLoadResult1.Text = $"Loaded {records.Count} records from {file.Name}"; 
            }

            TextBoxLoadResult1.Text = $"Loaded total {allRecords.Count} from {files.Count} files";
        }
        catch (Exception ex)
        {
            HandleException(ex); 
        }
    }

    public void HandleException(Exception ex)
    {
        TextBoxErrors.Text = ex.ToString();
        TextBoxErrors.Foreground = new SolidColorBrush(Colors.Red); 
    }

}