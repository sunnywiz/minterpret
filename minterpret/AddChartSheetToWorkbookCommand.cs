using ClosedXML.Excel;
using minterpret;

public class AddChartSheetToWorkbookCommand
{
    public void Execute(XLWorkbook xlWorkbook, string sheetName, Dictionary<DateTime, Balances> balancesOverTime,
        int totalCategories)
    {
// the last balance has all the categories
        var lastBalance = balancesOverTime.MaxBy(x => x.Key).Value;
        var categories = lastBalance.Keys.OrderBy(x => x).ToList();

// we want to collapse the categories. 
// find the most active categories
        var categorySize = categories.ToDictionary(x => x, x => 0m);
        foreach (var bot in balancesOverTime)
        {
            foreach (var c in bot.Value)
            {
                categorySize[c.Key] += Math.Abs(c.Value);
            }
        }

        var categoriesToSave = categorySize
            .OrderByDescending(x => x.Value)
            .Select(x => x.Key)
            .Take(totalCategories - 1)
            .ToList();

// ...    
//var model = new PlotModel { Title = "Plot", Background = OxyColor.FromRgb(255, 255, 255) };
//model.Series.Add(new FunctionSeries(Math.Sin, 0d, 10d, 0.1, "Sin(x)"));
//PngExporter.Export(model, "plot.png", 1280, 720);

        var ws = xlWorkbook.Worksheets.Add(sheetName);

        for (var c = 0; c < categoriesToSave.Count; c++)
        {
            var category = categoriesToSave[c];
            ws.Cell(1, c + 2).SetValue(category);
        }

        ws.Cell(1, totalCategories + 1).SetValue("OTHER");

        ws.Cell(1, 1).SetValue("Date");

        int row = 2;
        var days = balancesOverTime.Keys.OrderBy(x => x).ToList();
        var nextDate = balancesOverTime.MinBy(x=>x.Key).Key;
        foreach (var day in days)
        {
            if (day < nextDate) continue;
            nextDate = nextDate.AddDays(7);

            ws.Cell(row, 1).SetValue(day);

            var bal = balancesOverTime[day];
            for (var c = 0; c < categoriesToSave.Count; c++)
            {
                var category = categoriesToSave[c];
                if (bal.TryGetValue(category, out var amount))
                {
                    ws.Cell(row, c + 2).SetValue(-amount);
                }
            }

            var otherTotal = 0m;
            foreach (var category in bal.Keys)
            {
                if (categoriesToSave.Contains(category)) continue;
                otherTotal += bal[category];
            }

            ws.Cell(row, totalCategories + 1).SetValue(-otherTotal);

            row++;
        }

        var table = ws.Range(1, 1, row - 1, totalCategories + 1).CreateTable();
    }
}