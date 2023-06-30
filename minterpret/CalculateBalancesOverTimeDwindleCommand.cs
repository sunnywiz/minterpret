public class CalculateBalancesOverTimeDwindleCommand
{
    public Dictionary<DateTime, Balances> Execute(List<MintRecord> mintRecords, DateTime dateTime,
        decimal initialBalance1)
    {
        Balances b = null;
        DateTime? lastDateTime = null;
        var dictionary = new Dictionary<DateTime, Balances>();

        foreach (var r in mintRecords.OrderBy(x => x.Date))
        {
            if (lastDateTime == null)
            {
                Console.WriteLine($"Starting at {r.Date.Date:d}");
                b = new Balances();
                b.AddTransaction("Initial Balance", -initialBalance1);
                lastDateTime = r.Date.Date;
                dictionary[lastDateTime.Value] = b;
            }

            if (r.Date.Date > lastDateTime.Value.Date)
            {
                Console.WriteLine($"New Date {r.Date.Date:d}");
                var newb = b.Clone();
                dictionary[r.Date.Date] = newb;
                lastDateTime = r.Date.Date;
                b = newb;
            }

            if (r.SignedAmount > 0)
            {
                if (r.Category == "Credit Card Payment")
                {
                    Console.WriteLine("CC Payment! Spread it!");
                    var totalToPay = r.SignedAmount;
                    var thingsToPayOff = b.Where(x=>x.Value<0m).ToList();
                    var totalToSpreadOver = thingsToPayOff.Sum(x => x.Value);
                    if (totalToSpreadOver == 0)
                    {
                        b.AddTransaction("Overpayment",totalToPay);
                    }
                    else
                    {
                        foreach (var kv in thingsToPayOff)
                        {
                            var thisAmount = (kv.Value * totalToPay / totalToSpreadOver);
                            b.AddTransaction(kv.Key, thisAmount);
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

        return dictionary;
    }
}