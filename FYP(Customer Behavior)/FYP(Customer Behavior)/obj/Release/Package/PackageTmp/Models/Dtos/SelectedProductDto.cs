using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FYP_Customer_Behavior_.Models.Dtos
{
    public class SelectedProductDto
    {
        public int proID { get; set; }
        public string image { get; set; }
        public string pName { get; set; }
        public int colorid { get; set; }
        public string color { get; set; }
        public int storageid { get; set; }
        public string storage { get; set; }
        public int sizeid { get; set; }
        public string size { get; set; }
        public float pPrice { get; set; }
        public int qty { get; set; }
        /// <summary>
        /// //////////////////////////////////////
        /// </summary>
        public List<string> selectedItems { get; set; }
        public string _guid { get; set; }
        public float totalPrice { get; set; }
        public float grandTotal { get; set; }
        /// 
        /// Guest Info
        /// 
        public string _gName { get; set; }
        public string _gPhone { get; set; }
        public string _gEmail { get; set; }


    }
}