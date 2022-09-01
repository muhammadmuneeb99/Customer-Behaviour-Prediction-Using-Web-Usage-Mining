using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FYP_Customer_Behavior_.Models.Dtos
{
    public class SellerProductViewToCustomerDto
    {

        public List<BrandDto> brandDtos { get; set; }
        public List<subsubcatimage> subSubCategories { get; set; }
        public List<ProductDetailDto> productDetailDtos { get; set; }

        public int sellerId { get; set; }
        public string sellerName { get; set; }
        public string sellerImage { get; set; }
        public int productId { get; set; }
        public string productName { get; set; }
        public float price { get; set; }
        public float newPrice { get; set; }
        public float rating { get; set; }
        public string ProductImage { get; set; }
        public float dInAmount { get; set; }
        public float dInPercent { get; set; }
        public int subsubcatId { get; set; }
        public string subsubcatName { get; set; }
        public int brandId { get; set; }
        public string brandName { get; set; }
    }
}