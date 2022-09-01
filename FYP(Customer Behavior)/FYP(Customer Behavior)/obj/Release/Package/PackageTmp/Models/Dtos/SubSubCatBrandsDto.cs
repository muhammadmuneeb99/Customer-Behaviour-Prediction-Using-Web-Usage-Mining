using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FYP_Customer_Behavior_.Models.Dtos
{
    public class SubSubCatBrandsDto
    {
        public int sscbId { get; set; }
        public int sscid { get; set; }
        public string sscImage { get; set; }
        public string sscName { get; set; }
        public int BrandId { get; set; }
        public string BrandName { get; set; }
        public string bImage { get; set; }
        public int pId { get; set; }
        public string pName { get; set; }
        public string pImage { get; set; }
        public float amountinDiscount { get; set; }
        public float amountinPercent { get; set; }
        public float price { get; set; }
        public int qty { get; set; }
        public float newPrice { get; set; }
        public float rating { get; set; }
        public int? colorid { get; set; }
        public int? storageid { get; set; }
        public int? sizeid { get; set; }
        public int? wishlistId { get; set; }
        public int? cartId { get; set; }
    }
}