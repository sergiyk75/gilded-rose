using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GildedRoseWebApplication.Models
{
    public class InventoryItem
    {
        /// <summary>
        /// Product associated with inventory item
        /// </summary>
        public Product Product { get; set; }
        /// <summary>
        /// Associated product stock count
        /// </summary>
        public int StockCount { get; set; }
    }
}
