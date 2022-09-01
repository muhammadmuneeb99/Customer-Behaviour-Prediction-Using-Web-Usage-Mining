using System.Collections.Generic;
using System.Linq;

namespace FYP_Customer_Behavior_.Models.DtosForMining
{
    public class LowestPriceBasedRecommendation
    {
        public int productId { get; set; }
        public float productPrice { get; set; }

        FYPEntities _db = new FYPEntities();

        public List<int> lowestPriceBasedRecommendations()
        {
            var lowestPriceRec = (from p in _db.Products
                                  join pp in _db.ProductPrices on p.Product_ID equals pp.ProductID
                                  where pp.EndDate == null
                                  select new LowestPriceBasedRecommendation
                                  {
                                      productId = p.Product_ID,
                                      productPrice = (float)((pp.Price * 1) - (float)p.DiscountInAmount - (((float)p.Discount) / 100) * (pp.Price * 1))
                                  }).ToList();

            var lowestprice = (float)lowestPriceRec.Min(x => x.productPrice);
            var ids = lowestPriceRec.Where(x => x.productPrice == lowestprice).Select(x => x.productId).ToList();
            return ids;
        }
    }
}