using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FYP_Customer_Behavior_.RuleAssociationMining_FP_Growth_
{
    public class Node
    {
        public int Count { get; set; }
        public bool Visited { get; set; }
        public Dictionary<int, Node> item = new Dictionary<int, Node>();
    }
}