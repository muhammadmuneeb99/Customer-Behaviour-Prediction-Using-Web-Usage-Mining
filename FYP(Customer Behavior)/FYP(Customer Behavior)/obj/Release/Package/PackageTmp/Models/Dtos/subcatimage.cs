using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FYP_Customer_Behavior_.Models.Dtos
{
    public class subcatimage
    {
        public int id { get; set; }
        public string name { get; set; }
        public string image { get; set; }
        public int p_id { get; set; }
        public string p_name { get; set;}
        public double p_price { get; set; }
        public string p_image { get; set; }
        public int? colorid { get; set; }
        public int? storageid { get; set; }
        public int? sizeid { get; set; }
        public int? wishlistId { get; set; }
        public int? cartId { get; set; }
        public float newPrice { get; set; }
        public int qty { get; set; }
        public float dInAmount { get; set; }
        public float dInPercent { get; set; }
        public float rating { get; set; }
    }
}