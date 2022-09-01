using System.Collections.Generic;

namespace FYP_Customer_Behavior_.Models.Dtos
{
    public class CartDto
    {
        public string cartProductPriceId { get; set; }
        public int cartID { get; set; }
        public int qty { get; set; }
        public string pname { get; set; }
        public string _color { get; set; }
        public string _storage { get; set; }
        public string _size { get; set; }
        public float price { get; set; }
        public string img1 { get; set; }
        public string img2 { get; set; }
        public string img3 { get; set; }
        public string img4 { get; set; }
        public string img5 { get; set; }
        public float dCharge { get; set; }
        public int __productId { get; set; }
        public int? colorid { get; set; }
        public int? storageid { get; set; }
        public int? sizeid { get; set; }
    }
}