using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FYP_Customer_Behavior_.Models.Dtos
{
    public class CustTokenDetailsDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Token { get; set; }
        public int TokenTypeId { get; set; }
    }
}