using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FYP_Customer_Behavior_.Models.Dtos
{
    public class WishListDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string SellerName { get; set; }
        public float Price { get; set; }
        public string Image { get; set; }
        public int? colorid { get; set; }
        public int? storageid { get; set; }
        public int? sizeid { get; set; }
        public string colorName { get; set; }
        public string sizeName { get; set; }
        public string storageName { get; set; }
    }
}