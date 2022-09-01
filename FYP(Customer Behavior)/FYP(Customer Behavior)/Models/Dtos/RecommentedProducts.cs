using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FYP_Customer_Behavior_.Models.Dtos
{
    public class RecommentedProducts
    {
        public int id { get; set; }
        public string name { get; set; }
        public float price { get; set; }
        public string Image1 { get; set; }
        public string Image2 { get; set; }
        public string Image3 { get; set; }
        public string Image4 { get; set; }
        public string Image5 { get; set; }
        public float dAmount { get; set; }
        public float dPercent { get; set; }
        public float newPrice { get; set; }
        public float rating { get; set; }
    }
}