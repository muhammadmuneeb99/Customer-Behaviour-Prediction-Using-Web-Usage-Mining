using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FYP_Customer_Behavior_.Models.Dtos
{
    public class BrandsDto
    {
        public int brandID { get; set; }
        public string brandName { get; set; }
        public string brandImage { get; set; }
        public int proID { get; set; }
        public string proName { get; set; }
        public string proImg{ get; set; }
        public string proImg1{ get; set; }
        public float newPrice { get; set; }
        public float price { get; set; }
        public float dInAmount { get; set; }
        public float dInPercent { get; set; }
        public int qty { get; set; }
        public float rating { get; set; }
        public int? colorid { get; set; }
        public int? storageid { get; set; }
        public int? sizeid { get; set; }
        public int? wishlistId { get; set; }
        public int? cartId { get; set; }
    }
}