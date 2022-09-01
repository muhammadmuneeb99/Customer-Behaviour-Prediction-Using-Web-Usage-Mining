using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FYP_Customer_Behavior_.Models.Dtos
{
    public class SellerProductsDto
    {
        public int id { get; set; }
        public string pName { get; set; }
        public string image { get; set; }
        public string catName { get; set; }
        public string subCatName { get; set; }
        public string subSubCatName { get; set; }
        public string BrandName { get; set; }
        public float price { get; set; }
        public int qty { get; set; }
    }
}