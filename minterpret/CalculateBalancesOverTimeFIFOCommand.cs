public class CalculateBalancesOverTimeFIFOCommand
{
    public Dictionary<DateTime, Balances> Execute(List<MintRecord> mintRecords, DateTime dateTime,
        decimal initialBalance1)
    {
        Balances b = null;
        DateTime? lastDateTime = null;
        var dictionary = new Dictionary<DateTime, Balances>();

// Make a duplicate so we can adjust balances as we go through it
        var listToPayOff = mintRecords.Where(x => x.AccountName == "CREDIT CARD" && x.Date >= dateTime
                && x.SignedAmount < 0)
            .Select(x =>
                new MintRecord()
                {
                    Date = x.Date,
                    Category = x.Category,
                    Amount = x.Amount,
                    TransactionType = x.TransactionType
                })
            .OrderBy(x => x.Date).ToList();

        listToPayOff.Insert(0, new MintRecord()
        {
            AccountName = "CREDIT CARD",
            Amount = initialBalance1,
            Category = "Initial Balance",
            Date = dateTime.AddDays(-1),
            Description = "Initial Balance",
            TransactionType = "debit"
        });

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
                                b.AddTransaction(first.Category, -first.SignedAmount);
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

        return dictionary;
    }
}