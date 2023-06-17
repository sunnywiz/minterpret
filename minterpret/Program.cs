// See https://aka.ms/new-console-template for more information
using CsvHelper.Configuration;
using CsvHelper;
using System.Globalization;

var pathToInputFile = @"C:\Users\sgulati\OneDrive\2023P\mint_transactions.csv";

var config = new CsvConfiguration(CultureInfo.InvariantCulture)
{
    PrepareHeaderForMatch = args => args.Header.Replace(" ",""),
};
using var reader = new StreamReader(pathToInputFile);
using var csv = new CsvReader(reader, config);
var records = csv.GetRecords<MintRecord>().ToList();

Console.WriteLine($"Loaded {records.Count} records");