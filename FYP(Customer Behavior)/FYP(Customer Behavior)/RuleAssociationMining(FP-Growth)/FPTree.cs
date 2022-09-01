using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FYP_Customer_Behavior_.RuleAssociationMining_FP_Growth_
{
    public class FPTree
    {
        public Node root;
        private int SupportCount;
        public FPTree()
        {
            root = new Node();
            SupportCount = 2;
            root.item = null;
        }
        public void BuildTree(List<int> elemsRecvd)
        {
            if (root.item == null)
            {
                Node crawler = root;
                foreach (int element in elemsRecvd)
                {
                    Node node = new Node();
                    node.Count = 1;
                    crawler.item = new Dictionary<int, Node>();
                    crawler.item[element] = node;
                    crawler = node;
                }
            }
            else
            {
                Node crawler = root;
                if (crawler.item != null)
                {
                    foreach (int element in elemsRecvd)
                    {
                        if (crawler.item.ContainsKey(element))
                        {
                            Node node = crawler.item[element];
                            node.Count = node.Count + 1;
                            crawler.item[element] = node;
                            crawler = node;
                        }
                        else
                        {
                            Node node = new Node();
                            node.Count = 1;
                            crawler.item.Add(element, node);
                            crawler = node;
                        }
                    }
                }
            }
        }
        public Dictionary<Stack<int>, int> CPB;
        public void ConditionalPatternBase(Node rootNode, Stack<int> frequentPattern, int c, bool b)
        {
            if (b)
            {
                CPB = new Dictionary<Stack<int>, int>();
            }
            foreach (KeyValuePair<int, Node> item in rootNode.item)
            {
                if (item.Key == c)
                {
                    int a = item.Value.Count;
                    CPB.Add(new Stack<int>(frequentPattern), a);
                    //break;
                }
                item.Value.Visited = true;
                frequentPattern.Push(item.Key);
                ConditionalPatternBase(item.Value, frequentPattern, c, false);
            }
            if (frequentPattern.Count != 0)
            {
                frequentPattern.Pop();
            }
        }
        private int countCheck;
        List<int> ConditionalFpTreeResult = null;
        public void ConditionalFpTree(IEnumerable<KeyValuePair<Stack<int>, int>> keyValuePair)
        {
            keyValuePair = keyValuePair.Where(x => x.Key.Count > 0);
            var keys = (from c in keyValuePair
                        select c.Key.ToList());
            if (keys.Count() != 0)
            {
                ConditionalFpTreeResult = keys.Aggregate<IEnumerable<int>>(
                                (previousList, nextList) => previousList.Intersect(nextList)
                                ).ToList();
                countCheck = -1;
                foreach (KeyValuePair<Stack<int>, int> item in keyValuePair)
                {
                    if (keyValuePair.Count() == 1)
                    {
                        if (item.Key.Count == 0)
                        {
                            return;
                        }
                        countCheck = item.Value;
                        return;
                    }
                    int temp = item.Value;
                    foreach (KeyValuePair<Stack<int>, int> item1 in keyValuePair)
                    {
                        if (item.Key != item1.Key)
                        {
                            var res = item.Key.Select(i => i).Intersect(item1.Key).ToList();
                            if (res != null)
                            {
                                temp += item1.Value;
                                if (temp > countCheck)
                                {
                                    countCheck = temp;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                //ConditionalFpTreeResult.Clear(); //yahan pe null error ka exception de raha tha
                ConditionalFpTreeResult = new List<int>(); //mene ye line add krdi
            }

        }
        //Finally
        public Dictionary<List<int>, int> Pattern = new Dictionary<List<int>, int>();
        public void FrequentPatternGenerated(int key)
        {
            ConditionalFpTreeResult.Add(key);
            if (ConditionalFpTreeResult.Count == 1) { return; }
            List<List<int>> res = GetCombination(ConditionalFpTreeResult);

            var res1 = (from c in res
                        where c.Count > 1 && c.Contains(key)
                        select c).ToList().Distinct();
            foreach (var item in res1)
            {
                if (!Pattern.ContainsKey(item))
                {
                    Pattern.Add(item, countCheck);
                }
            }
            countCheck = 0;
        }
        static List<List<int>> data;
        public List<List<int>> GetCombination(List<int> list)
        {
            data = new List<List<int>>();
            double count = Math.Pow(2, list.Count);
            
            for (int i = 1; i <= count - 1; i++)
            {
                string str = Convert.ToString(i, 2).PadLeft(list.Count, '0');
                List<int> ne = new List<int>();
                for (int j = 0; j < str.Length; j++)
                {
                    if (str[j] == '1')
                    {
                        ne.Add(list[j]);
                    }
                }
                data.Add(ne);
            }
            return data;
        }
    }
}