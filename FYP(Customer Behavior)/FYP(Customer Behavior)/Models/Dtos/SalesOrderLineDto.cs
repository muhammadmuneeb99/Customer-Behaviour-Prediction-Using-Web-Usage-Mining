using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FYP_Customer_Behavior_.Models.Dtos
{
    public class SalesOrderLineDto
    {
        public int soID { get; set; }
        public int solID { get; set; }
        public int proID { get; set; }
        public string OrderDate { get; set; }
        public string pName { get; set; }
        public int qty { get; set; }
        public float OrderTot { get; set; }
        public string img { get; set; }
        public string SellerName { get; set; }
        public string trackID { get; set; }
        public int isConfirmed { get; set; }


        public float dt { get; set; }
    }
}