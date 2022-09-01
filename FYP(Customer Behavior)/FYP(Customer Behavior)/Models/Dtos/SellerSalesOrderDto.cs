using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FYP_Customer_Behavior_.Models.Dtos
{
    public class SellerSalesOrderDto
    {
        public int id { get; set; }
        public int OrderId { get; set; }
        public int? custID { get; set; }
        public string GuestID { get; set; }
        public string GuestName { get; set; }
        public int productId { get; set; }
        public int SellerId { get; set; }
        public string proName { get; set; }
        public string pImage { get; set; }
        public string custName { get; set; }
        public string trackingID { get; set; }
        public int qty { get; set; }
        public float unitPrice { get; set; }
        public float netPrice { get; set; }
        public string date { get; set; }
        public string shipment { get; set; }
        public string paymentType { get; set; }
        public string shipaddr { get; set; }
        public string billaddr { get; set; }
        public int isConfirmed { get; set; }
    }
}