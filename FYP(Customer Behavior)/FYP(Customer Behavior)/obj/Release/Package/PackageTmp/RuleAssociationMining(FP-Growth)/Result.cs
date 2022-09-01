using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FYP_Customer_Behavior_.RuleAssociationMining_FP_Growth_
{
    public class Result
    {
        FYPEntities _db = new FYPEntities();
        List<int> final = null;

        public List<int> result(int custid)
        {
            List<List<int>> Transections = new List<List<int>>();
            var OrderIds = (from c in _db.SalesOrders
                            where c.cust_Id != custid //yahan pe sirff dusre customers ki order id uthani hai
                            select c.order_ID).ToList();

            foreach (var item in OrderIds)
            {
                var transection = (_db.SalesOrderLines.Where(x => x.order_id == item)
                                  .Select(c => c.product_id).Distinct()).ToList();
                if (transection != null)
                {
                    Transections.Add(transection);
                }
            }

            // frequency in desending Order
            IEnumerable<KeyValuePair<int, int>> FRequency = fre(Transections);
            List<List<int>> UpdatedItems = updatedData(ref Transections, FRequency);
            FPTree fPTree = new FPTree();
            for (int i = 0; i < UpdatedItems.Count; i++)
            {
                fPTree.BuildTree(UpdatedItems[i]);
            }
            // Frequency in reverse
            IEnumerable<KeyValuePair<int, int>> FRequency1 = FRequency.Reverse();
            //Iteration of frequent set
            foreach (var item in FRequency1)
            {
                fPTree.ConditionalPatternBase(fPTree.root, new Stack<int>(), item.Key, true);
                fPTree.ConditionalFpTree(fPTree.CPB);
                fPTree.FrequentPatternGenerated(item.Key);
            }
            //Now Calculating Rule Association mining
            Dictionary<List<int>, int> Patterns = fPTree.Pattern;

            List<int> Cart = _db.Carts.Where(x => x.customerId == custid).Select(y => y.productId).ToList();

            List<List<int>> CartCombinations = fPTree.GetCombination(Cart);

            List<int> RAmRecommendation = new List<int>();
            foreach (var CartCombination in CartCombinations)
            {
                foreach (var pattern in Patterns)
                {
                    bool check = false;
                    foreach (var item in CartCombination)
                    {
                        if (pattern.Key.Contains(item))
                        {
                            check = true;
                        }
                        else
                            check = false;
                    }
                    if (check)
                    {
                        int AccuranceOfCartCombinationes = AccuranceOfCartCombination(Transections, CartCombination);
                        if (AccuranceOfCartCombinationes > 0)
                        {
                            double confidence = ((double)pattern.Value / AccuranceOfCartCombinationes) * 100;
                            if (confidence >= 10) // yahan pe greater than equal to 50 tha mene 10 kyun k humne nechey 50 ki jaga 10 krdiya tha is liye
                            {
                                List<int> aa = pattern.Key;
                                foreach (var item in CartCombination)
                                {
                                    aa.Remove(item);
                                }
                                if (aa.Count > 0)
                                {
                                    foreach (var item in aa)
                                    {
                                        RAmRecommendation.Add(item);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            final = RAmRecommendation.Distinct().ToList();
            return final;

        }


        public List<int> resultForGuest(string guestId)
        {
            List<List<int>> Transections = new List<List<int>>();
            var OrderIds = (from c in _db.SalesOrders
                            where c.GuestId != guestId //yahan pe sirff dusre customers ki order id uthani hai
                            select c.order_ID).ToList();

            foreach (var item in OrderIds)
            {
                var transection = (_db.SalesOrderLines.Where(x => x.order_id == item)
                                  .Select(c => c.product_id).Distinct()).ToList();
                if (transection != null)
                {
                    Transections.Add(transection);
                }
            }

            // frequency in desending Order
            IEnumerable<KeyValuePair<int, int>> FRequency = fre(Transections);
            List<List<int>> UpdatedItems = updatedData(ref Transections, FRequency);
            FPTree fPTree = new FPTree();
            for (int i = 0; i < UpdatedItems.Count; i++)
            {
                fPTree.BuildTree(UpdatedItems[i]);
            }
            // Frequency in reverse
            IEnumerable<KeyValuePair<int, int>> FRequency1 = FRequency.Reverse();
            //Iteration of frequent set
            foreach (var item in FRequency1)
            {
                fPTree.ConditionalPatternBase(fPTree.root, new Stack<int>(), item.Key, true);
                fPTree.ConditionalFpTree(fPTree.CPB);
                fPTree.FrequentPatternGenerated(item.Key);
            }
            //Now Calculating Rule Association mining
            Dictionary<List<int>, int> Patterns = fPTree.Pattern;


            //List<int> Cart = new List<int>() { 10, 50, 60 };

            List<int> Cart = _db.Carts.Where(x => x.GuestId == guestId).Select(y => y.productId).ToList();

            List<List<int>> CartCombinations = fPTree.GetCombination(Cart);

            List<int> RAmRecommendation = new List<int>();
            foreach (var CartCombination in CartCombinations)
            {
                foreach (var pattern in Patterns)
                {
                    bool check = false;
                    foreach (var item in CartCombination)
                    {
                        if (pattern.Key.Contains(item))
                        {
                            check = true;
                        }
                        else
                            check = false;
                    }
                    if (check)
                    {
                        int AccuranceOfCartCombinationes = AccuranceOfCartCombination(Transections, CartCombination);
                        if (AccuranceOfCartCombinationes > 0)
                        {
                            double confidence = ((double)pattern.Value / AccuranceOfCartCombinationes) * 100;
                            if (confidence >= 10) // yahan pe greater than equal to 50 tha mene 10 kyun k humne nechey 50 ki jaga 10 krdiya tha is liye
                            {
                                List<int> aa = pattern.Key;
                                foreach (var item in CartCombination)
                                {
                                    aa.Remove(item);
                                }
                                if (aa.Count > 0)
                                {
                                    foreach (var item in aa)
                                    {
                                        RAmRecommendation.Add(item);
                                    }

                                }
                            }
                        }
                    }
                }
            }
            final = RAmRecommendation.Distinct().ToList();

            return final;

        }

        private int AccuranceOfCartCombination(List<List<int>> items, List<int> CartCombination)
        {
            int Count = 0;
            foreach (var item in items)
            {
                bool check = false;
                foreach (var Cart in CartCombination)
                {
                    if (item.Contains(Cart))
                    {
                        check = true;
                    }
                    else
                    {
                        check = false;
                        break;
                    }
                }
                if (check)
                {
                    Count++;
                }
            }
            return Count;
        }


        private List<List<int>> updatedData(ref List<List<int>> items, IEnumerable<KeyValuePair<int, int>> keyValuePair)
        {
            List<List<int>> updatedItems = new List<List<int>>();
            for (int i = 0; i < items.Count; i++)
            {
                List<int> peep = new List<int>();
                foreach (var item in keyValuePair)
                {
                    List<int> s = items[i];

                    bool check = s.Contains(item.Key);
                    if (check)
                    {
                        peep.Add(item.Key);
                    }
                }
                updatedItems.Add(peep);
            }
            items = updatedItems;
            return items;
        }


        public Dictionary<int, int> frequency = new Dictionary<int, int>();

        private IEnumerable<KeyValuePair<int, int>> fre(List<List<int>> items)
        {
            for (int i = 0; i < items.Count; i++)
            {
                List<int> s = items[i];
                for (int j = 0; j < s.Count; j++)
                {
                    if (frequency.ContainsKey(s[j]))
                    {
                        frequency[s[j]]++;
                    }
                    else
                    {
                        frequency[s[j]] = 1;
                    }
                }
            }
            int supportCount = (10 * items.Count) / 100;
            var res = frequency.Where(p => p.Value >= supportCount).OrderByDescending(c => c.Value);
            return res;
        }

    }
}