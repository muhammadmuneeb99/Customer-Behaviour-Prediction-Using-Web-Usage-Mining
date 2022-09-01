using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FYP_Customer_Behavior_.Models.Dtos
{
    public class ProductModelDto
    {
        public int selectedCat { get; set; }
        public string selectedCatName { get; set; }
        public int selectedSubCat { get; set; }
        public string selectedSubCatName { get; set; }
        public int selectedSubSubCatBrand { get; set; }
        public string selectedSubSubCatBrandName { get; set; }
        public int selectedBrand { get; set; }
        public string selectedBrandName { get; set; }
        public string proName { get; set; }
        public float pPrice{ get; set; }
        public string proDesc{ get; set; }
        public string date{ get; set; }
        public int pQty{ get; set; }
        public string proWarrentyDes { get; set; }
        public List<string> featureName { get; set; }
        public List<string> featureDes { get; set; }
    }
}