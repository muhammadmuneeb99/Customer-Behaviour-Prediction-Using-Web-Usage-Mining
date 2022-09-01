using FYP_Customer_Behavior_.Models.DtosForMining;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace FYP_Customer_Behavior_.Models.Dtos
{
    public class NumberOfUsers
    {
        FYPEntities _db = new FYPEntities();
        Dictionary<string, int> nOU = new Dictionary<string, int>();
        Dictionary<string, int> cPP = new Dictionary<string, int>();
        Dictionary<string, int> noOfOrders = new Dictionary<string, int>();
        Dictionary<string, int> KeyValue = new Dictionary<string, int>();
        Dictionary<string, float> KeyValue1 = new Dictionary<string, float>();
        Dictionary<string, float> KeyValue5 = new Dictionary<string, float>();
        Dictionary<string, int> KeyValue2 = new Dictionary<string, int>();
        Dictionary<string, int> KeyValue3 = new Dictionary<string, int>();
        Dictionary<string, int> KeyValue4 = new Dictionary<string, int>();
        List<OrderHistoryBasedRecommendation> orderHistoryBasedRecommendation = null;
        public Dictionary<string, int> numberOfUser()
        {
            int noc = _db.Customers.Count();
            nOU.Add("Customers", noc);
            int nos = _db.Sellers.Count();
            nOU.Add("Sellers", nos);
            return nOU;
        }

        public Dictionary<string, int> ClicksPerPage()
        {
            int c = _db.Logs.Where(x => x.Url.Contains("CatSubCat?")).Count();
            cPP.Add("Categories", c);
            int sc = _db.Logs.Where(x => x.Url.Contains("SubSubCat?")).Count();
            cPP.Add("Sub Categories", sc);
            int ssc = _db.Logs.Where(x => x.Url.Contains("SubSubCatBrands?")).Count();
            cPP.Add("Sub Sub Categories", ssc);
            int brand = _db.Logs.Where(x => x.Url.Contains("Brands?")).Count();
            cPP.Add("Brands", brand);
            int pi = _db.Logs.Where(x => x.Url.Contains("ProductInformation?")).Count();
            cPP.Add("Product Information", pi);
            int spv = _db.Logs.Where(x => x.Url.Contains("SellersProductsView?")).Count();
            cPP.Add("Seller Product View", spv);
            return cPP;
        }

        public Dictionary<string, int> mostBoughtItem()
        {
            orderHistoryBasedRecommendation = (from so in _db.SalesOrders
                                               join sol in _db.SalesOrderLines
                                                      on so.order_ID equals sol.order_id
                                               where sol.isConfirmed == 2
                                               select new OrderHistoryBasedRecommendation
                                               {
                                                   Datetime = so.order_date,
                                                   ProductId = sol.product_id,
                                                   isConfirmed = (int)sol.isConfirmed,
                                                   qty = sol.quantity
                                               }).ToList();

            Dictionary<int, int> keyValuePairs = new Dictionary<int, int>();
            for (int i = 0; i < orderHistoryBasedRecommendation.Count; i++)
            {
                int count = 0;
                if (!keyValuePairs.ContainsKey(orderHistoryBasedRecommendation[i].ProductId))
                {
                    for (int j = 0; j < orderHistoryBasedRecommendation.FindAll(x => x.ProductId == orderHistoryBasedRecommendation[i].ProductId).Count; j++)
                    {
                        count = count + orderHistoryBasedRecommendation[j].qty;
                    }
                    keyValuePairs.Add(orderHistoryBasedRecommendation[i].ProductId, count);
                }
            }
            var pid = (from sp in _db.Products
                       select new { sp.Product_ID }).ToList();

            for (int i = 0; i < pid.Count; i++)
            {
                for (int j = 0; j < keyValuePairs.Count; j++)
                {
                    int key = keyValuePairs.ElementAt(j).Key;
                    if (pid[i].Product_ID == key)
                    {
                        int id = pid[i].Product_ID;
                        string name = _db.Products.FirstOrDefault(x => x.Product_ID == id).Product_Name;
                        if (!KeyValue.ContainsKey(name))
                        {
                            KeyValue.Add(name, keyValuePairs.ElementAt(j).Value);
                        }
                    }
                }
            }
            return KeyValue;
        }

        public Dictionary<string, int> nOOrders()
        {
            int onC = _db.SalesOrderLines.Where(x => x.isConfirmed == 0).Count();
            noOfOrders.Add("Not Confirmed", onC);
            int oC = _db.SalesOrderLines.Where(x => x.isConfirmed == 1).Count();
            noOfOrders.Add("Confirmed", oC);
            int oD = _db.SalesOrderLines.Where(x => x.isConfirmed == 2).Count();
            noOfOrders.Add("Delivered", oD);

            return noOfOrders;
        }

        public Dictionary<string, float> mostRatedItem()
        {
            var product = _db.RatingsAndReviews.Select(x => x.productId).Distinct().ToList();
            for (int i = 0; i < product.Count; i++)
            {
                int id = product[i];
                float rating = ((float)(_db.RatingsAndReviews.Where(x => x.rating == 1 && x.productId == id).Count()) + (float)(_db.RatingsAndReviews.Where(x => x.rating == 2 && x.productId == id).Count()) + (float)(_db.RatingsAndReviews.Where(x => x.rating == 3 && x.productId == id).Count()) + (float)(_db.RatingsAndReviews.Where(x => x.rating == 4 && x.productId == id).Count()) + (float)(_db.RatingsAndReviews.Where(x => x.rating == 5 && x.productId == id).Count())) / (float)5;
                string pName = _db.Products.FirstOrDefault(x => x.Product_ID == id).Product_Name;
                KeyValue1.Add(pName, rating);
            }
            return KeyValue1;
        }

        public Dictionary<string, float> mostViewedProductBasedTime()
        {
            var dayNightBasedRecommendations = _db.mostViewedItemClickAndTime();
            Dictionary<int, float> keyValuePairs = new Dictionary<int, float>();
            foreach (var item in dayNightBasedRecommendations)
            {
                string[] arr0 = item.Split('=');
                int key = int.Parse(arr0.Last());
                float time = float.Parse(arr0[1].Split('&').First());
                if (!keyValuePairs.ContainsKey(key))
                {
                    keyValuePairs.Add(key, time);
                }
                else
                {
                    keyValuePairs[key] += time;
                }
            }
           
            var pid = (from sp in _db.Products
                       select new { sp.Product_ID }).ToList();

            for (int i = 0; i < pid.Count; i++)
            {
                for (int j = 0; j < keyValuePairs.Count; j++)
                {
                    int key = keyValuePairs.ElementAt(j).Key;
                    if (pid[i].Product_ID == key)
                    {
                        int id = pid[i].Product_ID;
                        string name = _db.Products.FirstOrDefault(x => x.Product_ID == id).Product_Name;
                        if (!KeyValue5.ContainsKey(name))
                        {
                            KeyValue5.Add(name, keyValuePairs.ElementAt(j).Value);
                        }
                    }
                }
            }
            return KeyValue5;
        }

        public Dictionary<string, int> mostViewedProductBasedClick()
        {
            var dayNightBasedRecommendations = _db.mostViewedItemClickAndTime();
            Dictionary<int, int> keyValuePairs = new Dictionary<int, int>();
            int count = 1;
            foreach (var item in dayNightBasedRecommendations)
            {
                string[] arr0 = item.Split('=');
                int key = int.Parse(arr0.Last());
                //float time = float.Parse(arr0[1].Split('&').First());
                if (!keyValuePairs.ContainsKey(key))
                {
                    keyValuePairs.Add(key, count);
                }
                else
                {
                    keyValuePairs[key] += count;
                }
            }
            var pid = (from sp in _db.Products
                       select new { sp.Product_ID }).ToList();

            for (int i = 0; i < pid.Count; i++)
            {
                for (int j = 0; j < keyValuePairs.Count; j++)
                {
                    int key = keyValuePairs.ElementAt(j).Key;
                    if (pid[i].Product_ID == key)
                    {
                        int id = pid[i].Product_ID;
                        string name = _db.Products.FirstOrDefault(x => x.Product_ID == id).Product_Name;
                        if (!KeyValue2.ContainsKey(name))
                        {
                            KeyValue2.Add(name, keyValuePairs.ElementAt(j).Value);
                        }
                    }
                }
            }

            return KeyValue2;
        }
        public Dictionary<string, int> pageViews()
        {
            var dates = _db.Logs.Where(x => x.DateTime != null).Select(x => x.DateTime).ToList();
            List<string> _dates = new List<string>();
            foreach (var item in dates)
            {
                string dt = item.Split(' ').First();
                if (!_dates.Contains(dt))
                {
                    _dates.Add(dt);
                }
            }
            foreach (var item in _dates)
            {
                int a = _db.Logs.Where(x => x.DateTime.Contains(item) && x.Url.Contains("Brands")).Count();
                int b = _db.Logs.Where(x => x.DateTime.Contains(item) && x.Url.Contains("SubSubCatBrands")).Count();
                int c = _db.Logs.Where(x => x.DateTime.Contains(item) && x.Url.Contains("CustomerWishList")).Count();
                int d = _db.Logs.Where(x => x.DateTime.Contains(item) && x.Url.Contains("CustomerCart")).Count();
                int e = _db.Logs.Where(x => x.DateTime.Contains(item) && x.Url.Contains("BuyAndCheckOut")).Count();
                int k = _db.Logs.Where(x => x.DateTime.Contains(item) && x.Url.Contains("SummaryStatus")).Count();
                int j = _db.Logs.Where(x => x.DateTime.Contains(item) && x.Url.Contains("Products")).Count();
                int f = _db.Logs.Where(x => x.DateTime.Contains(item) && x.Url.Contains("ProductInformation")).Count();
                int g = _db.Logs.Where(x => x.DateTime.Contains(item) && x.Url.Contains("SellersProductsView")).Count();
                int h = _db.Logs.Where(x => x.DateTime.Contains(item) && x.Url.Contains("SaveAddress")).Count();
                int i = _db.Logs.Where(x => x.DateTime.Contains(item) && x.Url.Contains("SubSubCat")).Count();
                int l = _db.Logs.Where(x => x.DateTime.Contains(item) && x.Url.Contains("CatSubCat")).Count();
                int m = _db.Logs.Where(x => x.DateTime.Contains(item) && x.Url.Contains("Index")).Count();

                int totCountPG = a + b + c + d + e + f + g + h + i + l + m;

                KeyValue3.Add(item, totCountPG);
            }

            return KeyValue3;
        }

        public Dictionary<string, int> ordersPerDay()
        {
            var dates = _db.SalesOrders.Select(x => x.order_date).ToList();
            List<string> _dates = new List<string>();
            foreach (var item in dates)
            {
                string dt = item.Split(' ').First();
                if (!_dates.Contains(dt))
                {
                    _dates.Add(dt);
                }
            }

            foreach (var item in _dates)
            {
                int count = 0;
                var oid = _db.SalesOrders.Where(x=>x.order_date.Contains(item)).Select(x => x.order_ID).ToList();
                foreach (var item1 in oid)
                {
                    count = count + (_db.SalesOrderLines.Where(x => x.order_id == item1).Count());
                }

                KeyValue4.Add(item, count);
            }

            return KeyValue4;
        }
    }
}