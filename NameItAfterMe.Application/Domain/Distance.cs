using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NameItAfterMe.Application.Domain;

public class Distance
{
    public decimal Value { get; set; }
    public string Unit { get; set; }

    public Distance(string unit, decimal value)
    {
        Unit = unit;
        Value = value;
    }
}
