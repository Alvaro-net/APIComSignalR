using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace signalR2.Models
{
    public class Item
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public bool IsComplete { get; set; }

        public override string ToString() => $"{Id} - {Name} - {IsComplete}";
    }
}
