using System.Collections.Generic;
using System.Linq;

namespace FYP_Customer_Behavior_.Models.DtosForMining
{
    public class HighestDiscountBasedRecommendation
    {
        public int productId { get; set; }
        public float Discount { get; set; }

        FYPEntities _db = new FYPEntities();

        public List<int> highestDiscountBasedRecommendations()
        {
            var discountRec = (from p in _db.Products
                               join pp in _db.ProductPrices on p.Product_ID equals pp.ProductID
                               where pp.EndDate == null
                               select new HighestDiscountBasedRecommendation
                               {
                                   productId = p.Product_ID,
                                   Discount = (float)p.DiscountInAmount + ((float)p.Discount / 100)
                               }).ToList();
            var lowestprice = (float)discountRec.Max(x => x.Discount);
            var ids = discountRec.Where(x => x.Discount == lowestprice).Select(x => x.productId).ToList();
            return ids;//discountBasedRecommendationDone
        }
    }
}