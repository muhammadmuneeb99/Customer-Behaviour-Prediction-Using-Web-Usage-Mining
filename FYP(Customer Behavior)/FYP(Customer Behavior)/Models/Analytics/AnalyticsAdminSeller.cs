using FYP_Customer_Behavior_.Models.DtosForMining;
using FYP_Customer_Behavior_.Models.Dtos;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace FYP_Customer_Behavior_.Models.Analytics
{
    public class AnalyticsAdminSeller
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
        Dictionary<string, float> KeyValue6 = new Dictionary<string, float>();
        Dictionary<string, float> _earnings = new Dictionary<string, float>();

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
            HashSet<string> vs = new HashSet<string>();
            foreach (var item in _db.Logs)
            {
                string dt = item.DateTime.Split(' ')[0];
                vs.Add(dt);
            }
            foreach (var item in vs)
            {
                int a = _db.Logs.Where(x => x.DateTime.StartsWith(item)).Count();
                KeyValue3.Add(item, a);
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
                var oid = _db.SalesOrders.Where(x => x.order_date.Contains(item)).Select(x => x.order_ID).ToList();
                foreach (var item1 in oid)
                {
                    count = count + (_db.SalesOrderLines.Where(x => x.order_id == item1).Count());
                }

                KeyValue4.Add(item, count);
            }

            return KeyValue4;
        }
        public Dictionary<string, float> earning()
        {
            HashSet<string> vs = new HashSet<string>();
            foreach (var item in _db.SalesOrders)
            {
                string dt = item.order_date.Split(' ')[0];
                vs.Add(dt);
            }
            foreach (var item in vs)
            {
                float earn = (float)_db.SalesOrders.Where(x => x.order_date.StartsWith(item)).Sum(x => x.GrandTotal) * (float)0.1;
                _earnings.Add(item,(int)earn);
            }
            return _earnings;
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public Dictionary<string, int> ordersPerDay(int sellerId)
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
                var oid = _db.SalesOrders.Where(x => x.order_date.Contains(item)).Select(x => x.order_ID).ToList();
                //var oid = (from sol in _db.SalesOrderLines join p in _db.Products on sol.product_id equals p.Product_ID join sp in _db.SellerProducts on p.Product_ID equals sp.productId where sp.sellerId == sellerId && sol. select new { sol.order_id }).ToList();
                foreach (var item1 in oid)
                {
                    //count = count + (_db.SalesOrderLines.Where(x => x.order_id == item1).Count());
                    count = count + (from sol in _db.SalesOrderLines join p in _db.Products on sol.product_id equals p.Product_ID join sp in _db.SellerProducts on p.Product_ID equals sp.productId where sp.sellerId == sellerId && sol.order_id == item1 select new { sol.order_id }).Count();
                }

                KeyValue4.Add(item, count);
            }

            return KeyValue4;
        }
        public Dictionary<string, int> nOOrders(int sellerId)
        {
            int onC = (from sol in _db.SalesOrderLines join p in _db.Products on sol.product_id equals p.Product_ID join sp in _db.SellerProducts on p.Product_ID equals sp.productId where sp.sellerId == sellerId && sol.isConfirmed == 0 select new { sol.isConfirmed }).Count();
            noOfOrders.Add("Not Confirmed", onC);
            int oC = (from sol in _db.SalesOrderLines join p in _db.Products on sol.product_id equals p.Product_ID join sp in _db.SellerProducts on p.Product_ID equals sp.productId where sp.sellerId == sellerId && sol.isConfirmed == 1 select new { sol.isConfirmed }).Count();
            noOfOrders.Add("Confirmed", oC);
            int oD = (from sol in _db.SalesOrderLines join p in _db.Products on sol.product_id equals p.Product_ID join sp in _db.SellerProducts on p.Product_ID equals sp.productId where sp.sellerId == sellerId && sol.isConfirmed == 2 select new { sol.isConfirmed }).Count();
            noOfOrders.Add("Delivered", oD);

            return noOfOrders;
        }
        public Dictionary<string, int> mostBoughtItem(int sellerId)
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
                    var a = orderHistoryBasedRecommendation.FindAll(x => x.ProductId == orderHistoryBasedRecommendation[i].ProductId).Count;
                    for (int j = 0; j < a; j++)
                    {
                        count = count + orderHistoryBasedRecommendation[j].qty;
                    }
                    keyValuePairs.Add(orderHistoryBasedRecommendation[i].ProductId, count);
                }
            }
            var pid = (from sp in _db.SellerProducts
                       where sp.sellerId == sellerId
                       select new { sp.productId }).ToList();

            for (int i = 0; i < pid.Count; i++)
            {
                for (int j = 0; j < keyValuePairs.Count; j++)
                {
                    int key = keyValuePairs.ElementAt(j).Key;
                    if (pid[i].productId == key)
                    {
                        int id = pid[i].productId;
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

        public Dictionary<string, float> mostViewedProductBasedTime(int sellerId)
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

            var pid = (from sp in _db.SellerProducts
                       where sp.sellerId == sellerId
                       select new { sp.productId }).ToList();

            for (int i = 0; i < pid.Count; i++)
            {
                for (int j = 0; j < keyValuePairs.Count; j++)
                {
                    int key = keyValuePairs.ElementAt(j).Key;
                    if (pid[i].productId == key)
                    {
                        int id = pid[i].productId;
                        string name = _db.Products.FirstOrDefault(x => x.Product_ID == id).Product_Name;
                        if (!KeyValue1.ContainsKey(name))
                        {
                            KeyValue1.Add(name, keyValuePairs.ElementAt(j).Value);
                        }
                    }
                }
            }
            return KeyValue1;
        }

        public Dictionary<string, int> mostViewedProductBasedClick(int sellerId)
        {
            var dayNightBasedRecommendations = _db.mostViewedItemClickAndTime();
            Dictionary<int, int> keyValuePairs = new Dictionary<int, int>();
            int count = 1;
            foreach (var item in dayNightBasedRecommendations)
            {
                string[] arr0 = item.Split('=');
                int key = int.Parse(arr0.Last());
                if (!keyValuePairs.ContainsKey(key))
                {
                    keyValuePairs.Add(key, count);
                }
                else
                {
                    keyValuePairs[key] += count;
                }
            }

            var pid = (from sp in _db.SellerProducts
                       where sp.sellerId == sellerId
                       select new { sp.productId }).ToList();

            for (int i = 0; i < pid.Count; i++)
            {
                for (int j = 0; j < keyValuePairs.Count; j++)
                {
                    int key = keyValuePairs.ElementAt(j).Key;
                    if (pid[i].productId == key)
                    {
                        int id = pid[i].productId;
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
        public Dictionary<string, float> mostRatedItem(int sellerid)
        {
            var product = _db.RatingsAndReviews.Where(x => x.sellerId == sellerid).Select(x => x.productId).Distinct().ToList();
            for (int i = 0; i < product.Count; i++)
            {
                int id = product[i];
                float rating = ((float)(_db.RatingsAndReviews.Where(x => x.rating == 1 && x.productId == id).Count()) + (float)(_db.RatingsAndReviews.Where(x => x.rating == 2 && x.productId == id).Count()) + (float)(_db.RatingsAndReviews.Where(x => x.rating == 3 && x.productId == id).Count()) + (float)(_db.RatingsAndReviews.Where(x => x.rating == 4 && x.productId == id).Count()) + (float)(_db.RatingsAndReviews.Where(x => x.rating == 5 && x.productId == id).Count())) / (float)5;
                string pName = _db.Products.FirstOrDefault(x => x.Product_ID == id).Product_Name;
                KeyValue6.Add(pName, rating);
            }
            return KeyValue6;
        }

        public Dictionary<string, float> earning(int sellerId)
        {
            HashSet<string> vs = new HashSet<string>();
            var sp = _db.SellerProfits.Where(x => x.sellerId == sellerId);
            foreach (var item in sp)
            {
                string dt = item.Datetime.Split(' ')[0];
                vs.Add(dt);
            }
            foreach (var item in vs)
            {
                float earn = (float)_db.SellerProfits.Where(x => x.Datetime.StartsWith(item)).Sum(x=>x.Amount);
                _earnings.Add(item, (int)earn);
            }
            return _earnings;
        }

        public List<ProductPriceImageDto> cancelOrder(int sellerId)
        {
            List<ProductPriceImageDto> cancelOrders = (from co in _db.CanceledOrders join P in _db.Products on co.productId equals P.Product_ID join SP in _db.SellerProducts on P.Product_ID equals SP.productId
                                where SP.sellerId == sellerId
                                select new ProductPriceImageDto
                                {
                                    id = co.productId,
                                    name = _db.Products.FirstOrDefault(x=>x.Product_ID == co.productId).Product_Name,
                                    Image1 = _db.Product_Image.FirstOrDefault(x=>x.Product_ID == co.productId).Image_1
                                }).ToList();
            return cancelOrders;
        }
    }
}