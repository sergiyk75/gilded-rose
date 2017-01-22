using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GildedRoseWebApplication.Models
{
    public class Product
    {
        /// <summary>
        /// Product ID
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// Product Name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Product Description
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Product Price
        /// </summary>
        public int Price { get; set; }
    }
}
