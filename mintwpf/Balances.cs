using System.Collections.Generic;
using System.Linq;
using System.Text;

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