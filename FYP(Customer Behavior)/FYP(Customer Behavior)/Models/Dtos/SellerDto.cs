using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FYP_Customer_Behavior_.Models.Dtos
{
    public class SellerDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string CNIC { get; set; }
        public string Contact { get; set; }
        public string Address { get; set; }
    }
}