// See https://aka.ms/new-console-template for more information
using CsvHelper.Configuration;
using CsvHelper;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using ClosedXML.Excel;
using OxyPlot.Series;
using OxyPlot;
using OxyPlot.ImageSharp;
using OxyPlot.Series;

var pathToInputFile = @"C:\Users\sgulati\OneDrive\2023P\mint_transactions_2020-now.csv";
var pathToOutputFile = @"C:\Users\sgulati\OneDrive\2023P\credit_card_over_time.xlsx";


var config = new CsvConfiguration(CultureInfo.InvariantCulture)
{
    PrepareHeaderForMatch = args => args.Header.Replace(" ", ""),
};
List<MintRecord> records;
using (var reader = new StreamReader(pathToInputFile))
{
    ;
    using var csv = new CsvReader(reader, config);
    records = csv.GetRecords<MintRecord>().ToList();
}

Console.WriteLine($"Loaded {records.Count} records");

var startDate = new DateTime(2022, 7, 18);
var initialBalance = 7836.17m;
var creditCardRecords = records.Where(x => x.AccountName == "CREDIT CARD" && x.Date >= startDate).ToList();
Console.WriteLine($"{creditCardRecords.Count} CC records");


var fifoBoT = new CalculateBalancesOverTimeFIFOCommand()
    .Execute(creditCardRecords, startDate, initialBalance);

var spreadBOT = new CalculateBalancesOverTimeDwindleCommand()
    .Execute(creditCardRecords, startDate, initialBalance);

using var wb = new XLWorkbook();

new AddChartSheetToWorkbookCommand().Execute
    (wb, "FIFO25", fifoBoT, totalCategories: 25);
new AddChartSheetToWorkbookCommand().Execute
    (wb, "Spread25", spreadBOT, totalCategories: 25);

wb.SaveAs(pathToOutputFile);