using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FYP_Customer_Behavior_.Models.Dtos
{
    public class AdminProductDetailDto
    {
        public int id { get; set; }
        public string pName { get; set; }
        public string BrandName { get; set; }
        public string subcat2 { get; set; }
        public string subcat1 { get; set; }
        public string cat { get; set; }
    }
}