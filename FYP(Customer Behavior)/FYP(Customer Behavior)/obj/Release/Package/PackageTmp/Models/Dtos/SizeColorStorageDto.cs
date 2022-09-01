using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FYP_Customer_Behavior_.Models.Dtos
{
    public class SizeColorStorageDto
    {
        public int sizeid { get; set; }
        public int storageid { get; set; }
        public int colorid { get; set; }
        public string sizeName { get; set; }
        public string storageName { get; set; }
        public string colorName { get; set; }
        public int productId { get; set; }
    }
}