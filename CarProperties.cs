using Microsoft.Bot.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EchoBotDemo
{
    public class CarProperties
    {
        public string Type { get; set; }
        public string FuelType { get; set; }
        public string ManufacturingCompany { get; set; }
        public string Color { get; set; }
        public bool IsSecondHand { get; set; } = false;
        public int MaximumAge { get; set; } = 0;
    }
}
