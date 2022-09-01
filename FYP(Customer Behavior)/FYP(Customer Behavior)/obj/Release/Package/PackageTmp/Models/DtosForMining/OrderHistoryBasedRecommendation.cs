using System.Collections.Generic;
using System.Linq;
namespace FYP_Customer_Behavior_.Models.DtosForMining
{
    public class OrderHistoryBasedRecommendation
    {
        public int? UserId { get; set; }
        public string GuestId { get; set; }
        public string Datetime { get; set; }
        public int ProductId { get; set; }
        public int isConfirmed { get; set; }
        public int qty { get; set; }

        FYPEntities _db = new FYPEntities();

        List<OrderHistoryBasedRecommendation> orderHistoryBasedRecommendation = null;

        public List<int> orderHistoryBased(int userId)
        {
            orderHistoryBasedRecommendation = (from so in _db.SalesOrders
                                               join sol in _db.SalesOrderLines
                                                      on so.order_ID equals sol.order_id where so.cust_Id == userId && sol.isConfirmed == 2
                                               select new OrderHistoryBasedRecommendation
                                               {
                                                   UserId = (int?)so.cust_Id,
                                                   Datetime = so.order_date,
                                                   ProductId = sol.product_id,
                                                   isConfirmed = (int)sol.isConfirmed,
                                                   qty = sol.quantity
                                               }).ToList();
            if (orderHistoryBasedRecommendation == null)
            {
                return null;
            }
            //// Which item user bought most
            
           
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
                    keyValuePairs.Add(orderHistoryBasedRecommendation[i].ProductId,count);
                }
            }
            if (keyValuePairs.Count == 0)
            {
                return null;
            }
            int c = keyValuePairs.Values.Max();
            List<int> id = keyValuePairs.Where(x => x.Value == c).Select(x=>x.Key).ToList();
            if (id.Count != 0)
            {
                return id;
            }
            else
            {
                return null;
            }
        }

        public List<int> mostItemBought(int userId)
        {
            orderHistoryBasedRecommendation = (from so in _db.SalesOrders
                                               join sol in _db.SalesOrderLines
                                                      on so.order_ID equals sol.order_id where so.cust_Id != userId && sol.isConfirmed == 2
                                               select new OrderHistoryBasedRecommendation
                                               {
                                                   UserId = (int?)so.cust_Id,
                                                   Datetime = so.order_date,
                                                   ProductId = sol.product_id,
                                                   isConfirmed = (int)sol.isConfirmed
                                               }).ToList();
            if (orderHistoryBasedRecommendation == null)
            {
                return null;
            }
            //// Which item bought most by the users
            Dictionary<int, int> keyValuePairs = new Dictionary<int, int>();
            for (int i = 0; i < orderHistoryBasedRecommendation.Count; i++)
            {
                int count = 0;
                if (!keyValuePairs.ContainsKey(orderHistoryBasedRecommendation[i].ProductId))
                {
                    var a = orderHistoryBasedRecommendation.FindAll(x => x.ProductId == orderHistoryBasedRecommendation[i].ProductId).Count;
                    for (int j = 0; j < a; j++)
                    {
                        count++;
                    }
                    keyValuePairs.Add(orderHistoryBasedRecommendation[i].ProductId, count);
                }
            }
            if (keyValuePairs.Count == 0)
            {
                return null;
            }
            int c = keyValuePairs.Values.Max();
            List<int> id = keyValuePairs.Where(x => x.Value == c).Select(x=>x.Key).ToList();
            return id;
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////
        public List<int> orderHistoryBased(string userId)
        {
            orderHistoryBasedRecommendation = (from so in _db.SalesOrders
                                               join sol in _db.SalesOrderLines
                                                      on so.order_ID equals sol.order_id
                                               where so.GuestId == userId && sol.isConfirmed == 2
                                               select new OrderHistoryBasedRecommendation
                                               {
                                                   GuestId = so.GuestId,
                                                   Datetime = so.order_date,
                                                   ProductId = sol.product_id,
                                                   isConfirmed = (int)sol.isConfirmed,
                                                   qty = sol.quantity
                                               }).ToList();
            if (orderHistoryBasedRecommendation == null)
            {
                return null;
            }
            //// Which item user bought most

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
            if (keyValuePairs.Count == 0)
            {
                return null;
            }
            int c = keyValuePairs.Values.Max();
            List<int> id = keyValuePairs.Where(x => x.Value == c).Select(x=>x.Key).ToList();
            if (id.Count != 0)
            {
                return id;
            }
            else
            {
                return null;
            }
        }

        public List<int> mostItemBought(string userId)
        {
            orderHistoryBasedRecommendation = (from so in _db.SalesOrders
                                               join sol in _db.SalesOrderLines
                                                      on so.order_ID equals sol.order_id
                                               where so.GuestId != userId && sol.isConfirmed == 2
                                               select new OrderHistoryBasedRecommendation
                                               {
                                                   GuestId = so.GuestId,
                                                   Datetime = so.order_date,
                                                   ProductId = sol.product_id,
                                                   isConfirmed = (int)sol.isConfirmed
                                               }).ToList();
            if (orderHistoryBasedRecommendation == null)
            {
                return null;
            }
            //// Which item bought most by the users
            Dictionary<int, int> keyValuePairs = new Dictionary<int, int>();
            for (int i = 0; i < orderHistoryBasedRecommendation.Count; i++)
            {
                int count = 0;
                if (!keyValuePairs.ContainsKey(orderHistoryBasedRecommendation[i].ProductId))
                {
                    var a = orderHistoryBasedRecommendation.FindAll(x => x.ProductId == orderHistoryBasedRecommendation[i].ProductId).Count;
                    for (int j = 0; j < a; j++)
                    {
                        count++;
                    }
                    keyValuePairs.Add(orderHistoryBasedRecommendation[i].ProductId, count);
                }
            }
            if (keyValuePairs.Count == 0)
            {
                return null;
            }
            int c = keyValuePairs.Values.Max();
            List<int> id = keyValuePairs.Where(x => x.Value == c).Select(x=>x.Key).ToList();
            return id;
        }
    }
}