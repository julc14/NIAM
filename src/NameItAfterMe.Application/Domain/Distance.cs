using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NameItAfterMe.Application.Domain;

public class Distance
{
    public required decimal Value { get; init; }
    public required string Unit { get; init; }
}
