using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FYP_Customer_Behavior_.Models.Dtos
{

    public class RelatedProductsDto
    {
        FYPEntities _db = new FYPEntities();
        public ConcurrentBag<int> ids = new ConcurrentBag<int>();
        RecommendationClass recommendationClass = new RecommendationClass();
        //List<RecommentedProducts> RecommentedProducts = new List<RecommentedProducts>();
        List<RecommentedProducts> RecommentedProducts = new List<RecommentedProducts>();
        public List<RecommentedProducts> relatedProducts(int productId)
        {
            var a = (from Product in _db.Products where (Product.Product_ID == productId) select Product.SubCatBrand.subSubCatId).FirstOrDefault();
            int scid = int.Parse(a.ToString());
            var products = (from Product in _db.Products where (Product.SubCatBrand.subSubCatId == scid) select Product);
            foreach (var itemToBeChecked in products)
            {
                if (itemToBeChecked.Product_ID != productId)
                {
                    var p1FeaturesCollection = _db.Product_Feature.Where(p => p.product_id == productId);
                    string p1Features = string.Empty;
                    string p2Features = string.Empty;
                    foreach (var feature in p1FeaturesCollection)
                        p1Features = p1Features + "," + feature.featureDescription;
                    p1Features = p1Features.Remove(0, 1);
                    foreach (var feature in itemToBeChecked.Product_Feature)
                        p2Features = p2Features + "," + feature.featureDescription;
                    p2Features = p2Features.Remove(0, 1);
                    if (recommendationClass.CalculateSimilarity(p1Features, p2Features) > 0.2)
                    {
                        ids.Add(itemToBeChecked.Product_ID);
                    }
                }
            }
            var id = ids.Distinct();
            //Parallel.ForEach(id,new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount },itemm => {
            //    int one = (from o in _db.RatingsAndReviews where o.rating == 1 && o.productId == itemm select o).Count();
            //    int two = (from o in _db.RatingsAndReviews where o.rating == 2 && o.productId == itemm select o).Count();
            //    int Three = (from o in _db.RatingsAndReviews where o.rating == 3 && o.productId == itemm select o).Count();
            //    int Four = (from o in _db.RatingsAndReviews where o.rating == 4 && o.productId == itemm select o).Count();
            //    int Five = (from o in _db.RatingsAndReviews where o.rating == 5 && o.productId == itemm select o).Count();
            //    Reviews r = new Reviews()
            //    { One = one, Two = two, Three = Three, Four = Four, Five = Five };
            //    float aaa = (one + two + Three + Four + Five) / 5;
            //    var rItem = (from p1 in _db.Products
            //                 join p2 in _db.Product_Image
            //                 on p1.Product_ID equals p2.Product_ID
            //                 join pp in _db.ProductPrices on p1.Product_ID equals pp.ProductID
            //                 where p2.Product_ID == itemm && pp.EndDate == null
            //                 select new RecommentedProducts
            //                 {
            //                     id = p1.Product_ID,
            //                     name = p1.Product_Name,
            //                     newPrice = (float)(((pp.Price * 1) - (float)p1.DiscountInAmount) - (((float)p1.Discount / 100) * (pp.Price * 1))),
            //                     Image1 = p2.Image_1,
            //                     Image2 = p2.Image_2,
            //                     Image3 = p2.Image_3,
            //                     Image4 = p2.Image_4,
            //                     Image5 = p2.Image_5,
            //                     dAmount = (float)p1.Discount,
            //                     dPercent = (float)p1.DiscountInAmount,
            //                     price = (float)pp.Price,
            //                     rating = aaa
            //                 }).FirstOrDefault();
            //    if (!RecommentedProducts.Contains(rItem))
            //    {
            //        RecommentedProducts.Add(rItem);
            //    }

            //});
            foreach (var itemm in id)
            {
                var rItem = (from p1 in _db.Products
                             join p2 in _db.Product_Image
                             on p1.Product_ID equals p2.Product_ID
                             join pp in _db.ProductPrices on p1.Product_ID equals pp.ProductID
                             where p2.Product_ID == itemm && pp.EndDate == null
                             select new RecommentedProducts
                             {
                                 id = p1.Product_ID,
                                 name = p1.Product_Name,
                                 newPrice = (float)(((pp.Price * 1) - (float)p1.DiscountInAmount) - (((float)p1.Discount / 100) * (pp.Price * 1))),
                                 Image1 = p2.Image_1,
                                 Image2 = p2.Image_2,
                                 Image3 = p2.Image_3,
                                 Image4 = p2.Image_4,
                                 Image5 = p2.Image_5,
                                 dAmount = (float)p1.Discount,
                                 dPercent = (float)p1.DiscountInAmount,
                                 price = (float)pp.Price,
                                 rating = ((float)_db.RatingsAndReviews.Where(y => y.rating == 1).Count(x => x.productId == itemm) + (float)_db.RatingsAndReviews.Where(y => y.rating == 2).Count(x => x.productId == itemm) + (float)_db.RatingsAndReviews.Where(y => y.rating == 3).Count(x => x.productId == itemm) + (float)_db.RatingsAndReviews.Where(y => y.rating == 4).Count(x => x.productId == itemm) + (float)_db.RatingsAndReviews.Where(y => y.rating == 5).Count(x => x.productId == itemm)) / (float)5
                             }).FirstOrDefault();
                if (!RecommentedProducts.Contains(rItem))
                {
                    RecommentedProducts.Add(rItem);
                }
            }
            return RecommentedProducts;
        }
    }
}