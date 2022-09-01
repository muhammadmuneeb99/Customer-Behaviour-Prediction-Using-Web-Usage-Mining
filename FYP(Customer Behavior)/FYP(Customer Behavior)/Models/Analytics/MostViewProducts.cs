using FYP_Customer_Behavior_.Models.DtosForMining;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FYP_Customer_Behavior_.Models.Analytics
{
    public class MostViewProducts
    {
        public int proId { get; set; }
        public string proName { get; set; }
        public int count { get; set; }
        FYPEntities _db = new FYPEntities();
        Dictionary<string, int> KeyValue = new Dictionary<string, int>();
        Dictionary<string, int> noOfOrders = new Dictionary<string, int>();
        Dictionary<string, float> KeyValue1 = new Dictionary<string, float>();
        Dictionary<string, float> KeyValue3 = new Dictionary<string, float>();
        Dictionary<string, int> KeyValue2 = new Dictionary<string, int>();
        List<OrderHistoryBasedRecommendation> orderHistoryBasedRecommendation = null;
        //List<OrderHistoryBasedRecommendation> itemToRemove = null;
        public Dictionary<string, int> nOOrders( int sellerId)
        {
            int onC = _db.SalesOrderLines.Where(x => x.isConfirmed == 0).Count();
            noOfOrders.Add("Not Confirmed", onC);
            int oC = _db.SalesOrderLines.Where(x => x.isConfirmed == 1).Count();
            noOfOrders.Add("Confirmed", oC);
            int oD = _db.SalesOrderLines.Where(x => x.isConfirmed == 2).Count();
            noOfOrders.Add("Delivered", oD);

            return noOfOrders;
        }
        public Dictionary<string, int> mostBoughtItem(int sellerId)
        {
            orderHistoryBasedRecommendation = (from so in _db.SalesOrders
                                               join sol in _db.SalesOrderLines
                                                      on so.order_ID equals sol.order_id where sol.isConfirmed == 2
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
            //List<DayNightAndTimeBasedRecommendation> dayNightBasedRecommendations = (from lg in _db.Logs
            //                                                                         select new DayNightAndTimeBasedRecommendation
            //                                                                         {
            //                                                                             Id = lg.Id,
            //                                                                             DateTime = lg.DateTime,
            //                                                                             Users = lg.Users,
            //                                                                             Url = lg.Url
            //                                                                         }).ToList();

            //List<int> idToRemove = new List<int>();
            //if (dayNightBasedRecommendations != null)
            //{
            //    for (int i = 0; i < dayNightBasedRecommendations.Count; i++)
            //    {
            //        if (dayNightBasedRecommendations[i].Users == "")
            //        {
            //            idToRemove.Add(dayNightBasedRecommendations[i].Id);
            //        }
            //    }
            //}
            //if (idToRemove != null)
            //{
            //    for (int i = 0; i < idToRemove.Count; i++)
            //    {
            //        for (int j = 0; j < dayNightBasedRecommendations.Count; j++)
            //        {
            //            if (idToRemove[i] == dayNightBasedRecommendations[j].Id)
            //            {
            //                dayNightBasedRecommendations.RemoveAt(j);
            //            }
            //        }
            //    }
            //}
            //idToRemove = new List<int>();
            //if (dayNightBasedRecommendations != null)
            //{
            //    for (int i = 0; i < dayNightBasedRecommendations.Count; i++)
            //    {
            //        if (!dayNightBasedRecommendations[i].Url.Contains("UserBehaviour?"))
            //        {
            //            idToRemove.Add(dayNightBasedRecommendations[i].Id);
            //        }
            //    }
            //}
            //if (idToRemove != null)
            //{
            //    for (int i = 0; i < idToRemove.Count; i++)
            //    {
            //        for (int j = 0; j < dayNightBasedRecommendations.Count; j++)
            //        {
            //            if (idToRemove[i] == dayNightBasedRecommendations[j].Id)
            //            {
            //                dayNightBasedRecommendations.RemoveAt(j);
            //            }
            //        }
            //    }
            //}
            var dayNightBasedRecommendations = _db.mostViewedItemClickAndTime();
            ConcurrentDictionary<int, float> keyValuePairs = new ConcurrentDictionary<int, float>();
            Parallel.ForEach(dayNightBasedRecommendations, new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount }, i =>
            {
                string[] arr0 = i.Split('=');
                int key = int.Parse(arr0.Last());
                float time = float.Parse(arr0[1].Split('&').First());
                if (!keyValuePairs.ContainsKey(key))
                {
                    keyValuePairs.TryAdd(key, time);
                }
                else
                {
                    keyValuePairs[key] += time;
                }
                //string[] arr0 = i.Split('/');
                //for (int a = 0; a < arr0.Length; a++)
                //{
                //    if (arr0[a].Contains("UserBehaviour?"))
                //    {
                //        string[] arr1 = arr0[a].Split('&');
                //        for (int b = 0; b < arr1.Length; b++)
                //        {
                //            if (arr1[b].Contains("relationId"))
                //            {
                //                string[] arr2 = arr1[b].Split('=');
                //                for (int c = 0; c < arr2.Length; c++)
                //                {
                //                    if (!arr2[c].Contains("relationId"))
                //                    {
                //                        for (int e = 0; e < arr1.Length; e++)
                //                        {
                //                            if (arr1[e].Contains("UserBehaviour?"))
                //                            {
                //                                string[] arr3 = arr1[e].Split('=');
                //                                for (int d = 0; d < arr3.Length; d++)
                //                                {
                //                                    if (!arr3[d].Contains("UserBehaviour?"))
                //                                    {
                //                                        if (!keyValuePairs.ContainsKey(int.Parse(arr2[c])))
                //                                        {
                //                                            keyValuePairs.TryAdd(int.Parse(arr2[c]), float.Parse(arr3[d]));
                //                                        }
                //                                        else
                //                                        {
                //                                            int key = int.Parse(arr2[c]);
                //                                            keyValuePairs[key] += float.Parse(arr3[d]);
                //                                        }
                //                                    }
                //                                }
                //                            }
                //                        }
                //                    }
                //                }
                //            }
                //        }
                //    }
                //}
            });
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
            //List<DayNightAndTimeBasedRecommendation> dayNightBasedRecommendations = (from lg in _db.Logs
            //                                                                         select new DayNightAndTimeBasedRecommendation
            //                                                                         {
            //                                                                             Id = lg.Id,
            //                                                                             DateTime = lg.DateTime,
            //                                                                             Users = lg.Users,
            //                                                                             Url = lg.Url
            //                                                                         }).ToList();

            //List<int> idToRemove = new List<int>();
            //if (dayNightBasedRecommendations != null)
            //{
            //    for (int i = 0; i < dayNightBasedRecommendations.Count; i++)
            //    {
            //        if (dayNightBasedRecommendations[i].Users == "")
            //        {
            //            idToRemove.Add(dayNightBasedRecommendations[i].Id);
            //        }
            //    }
            //}
            //if (idToRemove != null)
            //{
            //    for (int i = 0; i < idToRemove.Count; i++)
            //    {
            //        for (int j = 0; j < dayNightBasedRecommendations.Count; j++)
            //        {
            //            if (idToRemove[i] == dayNightBasedRecommendations[j].Id)
            //            {
            //                dayNightBasedRecommendations.RemoveAt(j);
            //            }
            //        }
            //    }
            //}
            //idToRemove = new List<int>();
            //if (dayNightBasedRecommendations != null)
            //{
            //    for (int i = 0; i < dayNightBasedRecommendations.Count; i++)
            //    {
            //        if (!dayNightBasedRecommendations[i].Url.Contains("UserBehaviour?"))
            //        {
            //            idToRemove.Add(dayNightBasedRecommendations[i].Id);
            //        }
            //    }
            //}
            //if (idToRemove != null)
            //{
            //    for (int i = 0; i < idToRemove.Count; i++)
            //    {
            //        for (int j = 0; j < dayNightBasedRecommendations.Count; j++)
            //        {
            //            if (idToRemove[i] == dayNightBasedRecommendations[j].Id)
            //            {
            //                dayNightBasedRecommendations.RemoveAt(j);
            //            }
            //        }
            //    }
            //}
            var dayNightBasedRecommendations = _db.mostViewedItemClickAndTime();
            ConcurrentDictionary<int, int> keyValuePairs = new ConcurrentDictionary<int, int>();
            int count = 1;
            Parallel.ForEach(dayNightBasedRecommendations, new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount }, i =>
            {

                string[] arr0 = i.Split('=');
                int key = int.Parse(arr0.Last());
                //float time = float.Parse(arr0[1].Split('&').First());
                if (!keyValuePairs.ContainsKey(key))
                {
                    keyValuePairs.TryAdd(key, count);
                }
                else
                {
                    keyValuePairs[key] += count;
                }
                //string[] arr0 = i.Split('/');
                //for (int a = 0; a < arr0.Length; a++)
                //{
                //    if (arr0[a].Contains("UserBehaviour?"))
                //    {
                //        string[] arr1 = arr0[a].Split('&');
                //        for (int b = 0; b < arr1.Length; b++)
                //        {
                //            if (arr1[b].Contains("relationId"))
                //            {
                //                string[] arr2 = arr1[b].Split('=');
                //                for (int c = 0; c < arr2.Length; c++)
                //                {
                //                    if (!arr2[c].Contains("relationId"))
                //                    {
                //                        for (int e = 0; e < arr1.Length; e++)
                //                        {
                //                            if (arr1[e].Contains("UserBehaviour?"))
                //                            {
                //                                string[] arr3 = arr1[e].Split('=');
                //                                for (int d = 0; d < arr3.Length; d++)
                //                                {
                //                                    if (!arr3[d].Contains("UserBehaviour?"))
                //                                    {
                //                                        if (!keyValuePairs.ContainsKey(int.Parse(arr2[c])))
                //                                        {
                //                                            keyValuePairs.TryAdd(int.Parse(arr2[c]), count);
                //                                        }
                //                                        else
                //                                        {
                //                                            int key = int.Parse(arr2[c]);
                //                                            keyValuePairs[key] += count;
                //                                        }
                //                                    }
                //                                }
                //                            }
                //                        }
                //                    }
                //                }
                //            }
                //        }
                //    }
                //}
            });
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
        public Dictionary<string,float> mostRatedItem(int sellerid)
        {
            var product = _db.RatingsAndReviews.Where(x => x.sellerId == sellerid).Select(x=>x.productId).Distinct().ToList();
            for (int i = 0; i < product.Count; i++)
            {
                int id = product[i];
                float rating = ((float)(_db.RatingsAndReviews.Where(x => x.rating == 1 && x.productId == id).Count()) + (float)(_db.RatingsAndReviews.Where(x => x.rating == 2 && x.productId == id).Count()) + (float)(_db.RatingsAndReviews.Where(x => x.rating == 3 && x.productId == id).Count()) + (float)(_db.RatingsAndReviews.Where(x => x.rating == 4 && x.productId == id).Count()) + (float)(_db.RatingsAndReviews.Where(x => x.rating == 5 && x.productId == id).Count())) / (float)5;
                string pName = _db.Products.FirstOrDefault(x => x.Product_ID == id).Product_Name;
                KeyValue3.Add(pName,rating);
            }
            return KeyValue3;
        }
    }
}