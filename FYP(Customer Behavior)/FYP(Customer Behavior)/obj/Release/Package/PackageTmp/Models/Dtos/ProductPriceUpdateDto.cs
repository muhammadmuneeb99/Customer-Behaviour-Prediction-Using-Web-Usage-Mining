using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FYP_Customer_Behavior_.Models.Dtos
{
    public class ProductPriceUpdateDto
    {
        public int Product_ID { get; set; }
        public string Product_Name { get; set; }
        public string Product_Description { get; set; }
        public float Discount { get; set; }
        public int quantity { get; set; }
        public float Price { get; set; }
        public DateTime StartDate { get; set; }
    }
}