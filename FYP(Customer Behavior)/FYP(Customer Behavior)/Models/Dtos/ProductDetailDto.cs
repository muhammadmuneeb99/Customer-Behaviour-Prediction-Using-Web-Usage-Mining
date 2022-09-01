using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FYP_Customer_Behavior_.Models.Dtos
{
    public class ProductDetailDto
    {
        public int id { get; set; }
        public string pName { get; set; }
        public int brandId { get; set; }
        public string bName { get; set; }
        public float newPrice { get; set; }
        public float pPrice { get; set; }
        public int qty { get; set; }
        public string img1 { get; set; }
        public string img2 { get; set; }
        public string img3 { get; set; }
        public string img4 { get; set; }
        public string img5 { get; set; }
        public string pDetail { get; set; }
        public string pWDetail { get; set; }
        public string f_name { get; set; }
        public string f_des { get; set; }
        public float pDiscountPercent { get; set; }
        public float pDiscountAmount { get; set; }
        public float rating { get; set; }
        public int catid { get; set; }
        public string catname { get; set; }
        public int subcatid { get; set; }
        public string subcatname { get; set; }
        public int subsubcatid { get; set; }
        public string subsubcatname { get; set; }
        public int? colorid { get; set; }
        public int? storageid { get; set; }
        public int? sizeid { get; set; }
        public int? wishlistId { get; set; }
        public int? cartId { get; set; }
        public Nullable<int> _colorid { get; set; }
        public Nullable<int> _storageid { get; set; }
        public Nullable<int> _sizeid { get; set; }
    }
}