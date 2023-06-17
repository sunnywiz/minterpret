// See https://aka.ms/new-console-template for more information
using CsvHelper.Configuration;
using CsvHelper;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using System.Text;

var pathToInputFile = @"C:\Users\sgulati\OneDrive\2023P\mint_transactions.csv";

var config = new CsvConfiguration(CultureInfo.InvariantCulture)
{
    PrepareHeaderForMatch = args => args.Header.Replace(" ",""),
};
using var reader = new StreamReader(pathToInputFile);
using var csv = new CsvReader(reader, config);
var records = csv.GetRecords<MintRecord>().ToList();

Console.WriteLine($"Loaded {records.Count} records");

var startDate = new DateTime(2023, 1, 1); 
var creditCardRecords = records.Where(x=>x.AccountName=="CREDIT CARD" && x.Date>=startDate).ToList(); 
Console.WriteLine($"{creditCardRecords.Count} CC records");

var method = "FIFO";

Balances b = null;
DateTime? lastDateTime = null;
var balancesOverTime = new Dictionary<DateTime, Balances>(); 
foreach (var r in creditCardRecords.OrderBy(x=>x.Date).Take(1000))
{
    if (lastDateTime == null)
    {
        Console.WriteLine($"Starting at {r.Date.Date:d}");
        b = new Balances(); 
        lastDateTime = r.Date.Date;
        balancesOverTime[lastDateTime.Value] = b; 
    }

    if (r.Date.Date > lastDateTime.Value.Date)
    {
        Console.WriteLine($"New Date {r.Date.Date:d}");
        var newb = b.Clone(); 
        balancesOverTime[r.Date.Date] = newb;
        lastDateTime = r.Date.Date;
        b = newb;
    }

    Console.WriteLine($"Add Transaction {r.Date:d} {r.Category} {r.SignedAmount}");
    b.AddTransaction(r.Category, r.SignedAmount);
}



public class Balances : Dictionary<string, decimal>
{
    public void AddTransaction(string category, decimal amount)
    {
        if (!this.ContainsKey(category))
        {
            this[category] = 0.0m; 
        }
        this[category] += amount;
    }

    public Balances Clone()
    {
        var newb = new Balances();
        foreach (var kvp in this)
        {
            newb[kvp.Key] = kvp.Value;
        }

        return newb;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        foreach (var k in this.Keys.OrderBy(x => x))
        {
            if (sb.Length > 0) sb.Append(" ");
            sb.Append(k);
            sb.Append(":");
            sb.Append(this[k].ToString("C2"));
        }
        return sb.ToString();
    }
}
