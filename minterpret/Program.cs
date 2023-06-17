// See https://aka.ms/new-console-template for more information
using CsvHelper.Configuration;
using CsvHelper;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using ClosedXML.Excel;

var pathToInputFile = @"C:\Users\sgulati\OneDrive\2023P\mint_transactions.csv";
var pathToOutputFile = @"C:\Users\sgulati\OneDrive\2023P\credit_card_over_time.xlsx";

var config = new CsvConfiguration(CultureInfo.InvariantCulture)
{
    PrepareHeaderForMatch = args => args.Header.Replace(" ",""),
};
List<MintRecord> records;
using (var reader = new StreamReader(pathToInputFile))
{
    ;
    using var csv = new CsvReader(reader, config);
    records = csv.GetRecords<MintRecord>().ToList();
}

// second copy for dealing with FIFO etc without modifying first set
List<MintRecord> records2;
using (var reader2 = new StreamReader(pathToInputFile))
{
    using var csv2 = new CsvReader(reader2, config);
    records2 = csv2.GetRecords<MintRecord>().ToList();
}


Console.WriteLine($"Loaded {records.Count} records");

var startDate = new DateTime(2020, 1, 1); 
var creditCardRecords = records.Where(x=>x.AccountName=="CREDIT CARD" && x.Date>=startDate).ToList(); 
Console.WriteLine($"{creditCardRecords.Count} CC records");

Balances b = null;
DateTime? lastDateTime = null;
var balancesOverTime = new Dictionary<DateTime, Balances>();

var listToPayOff = records2.Where(x => x.AccountName == "CREDIT CARD" && x.Date >= startDate
                                                                      && x.SignedAmount < 0)
    .OrderBy(x=>x.Date).ToList();

foreach (var r in creditCardRecords.OrderBy(x=>x.Date))
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

    if (r.SignedAmount > 0)
    {
        if (r.Category == "Credit Card Payment")
        {
            Console.WriteLine("CC Payment!");
            var amountToPay = r.SignedAmount;
            while (amountToPay > 0)
            {
                if (listToPayOff.Any())
                {
                    // remember this is negative
                    var first = listToPayOff.First();
                    if (first.SignedAmount < -amountToPay)
                    {
                        // first can absorb payment
                        first.Amount -= amountToPay;
                        b.AddTransaction(first.Category, amountToPay);
                        amountToPay = 0;
                    }
                    else
                    {
                        // partial handle, discard the first
                        amountToPay += first.SignedAmount;
                        b.AddTransaction(first.Category,-first.SignedAmount);
                        listToPayOff.RemoveAt(0); 
                    }
                }
                else
                {
                    // nothing remaining to pay off
                    b.AddTransaction("Overpayment", amountToPay);
                    amountToPay = 0; 
                }
            }
        }
        else
        {
            Console.WriteLine($"Add Return Transaction {r.Date:d} {r.Category} {r.SignedAmount}");
            b.AddTransaction(r.Category, r.SignedAmount);
        }
    }
    else
    {
        Console.WriteLine($"Add Transaction {r.Date:d} {r.Category} {r.SignedAmount}");
        b.AddTransaction(r.Category, r.SignedAmount);
    }
}

using var wb = new XLWorkbook();
var ws = wb.Worksheets.Add("Balances");

// the last b has all the categories
var categories = b.Keys.OrderBy(x => x).ToList();
for (var c = 0; c < categories.Count; c++)
{
    var category = categories[c];
    ws.Cell(1, c+2).SetValue(category);
}

ws.Cell(1, 1).SetValue("Date");

int row = 2;
var days = balancesOverTime.Keys.OrderBy(x => x).ToList();
var nextDate = startDate; 
foreach (var day in days)
{
    if (day < nextDate) continue;
    nextDate = nextDate.AddDays(7); 

    ws.Cell(row, 1).SetValue(day);

    var bal = balancesOverTime[day];
    for (var c = 0; c < categories.Count; c++)
    {
        var category = categories[c];
        if (bal.TryGetValue(category, out var amount))
        {
            ws.Cell(row, c + 2).SetValue(-amount); 
        }
    }

    row++;
}

var table = ws.Range(1, 1, row - 1, categories.Count + 1).CreateTable(); 

wb.SaveAs(pathToOutputFile);

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
