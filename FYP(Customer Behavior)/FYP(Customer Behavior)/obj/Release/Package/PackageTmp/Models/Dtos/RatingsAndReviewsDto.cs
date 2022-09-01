using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FYP_Customer_Behavior_.Models.Dtos
{
    public class RatingsAndReviewsDto
    {
        public string custName { get; set; }
        public float Rating { get; set; }
        public string Review { get; set; }
        public int One { get; set; }
        public int Two { get; set; }
        public int Three { get; set; }
        public int Four { get; set; }
        public int Five { get; set; }
        public float AvgRating { get; set; }
        public int sellerId { get; set; }
        public int customerId { get; set; }
        public int productId { get; set; }
    }
}