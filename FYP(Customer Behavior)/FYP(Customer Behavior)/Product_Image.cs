//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace FYP_Customer_Behavior_
{
    using System;
    using System.Collections.Generic;
    
    public partial class Product_Image
    {
        public int Product_ID { get; set; }
        public string Image_1 { get; set; }
        public string Image_2 { get; set; }
        public string Image_3 { get; set; }
        public string Image_4 { get; set; }
        public string Image_5 { get; set; }
        public int Id { get; set; }
    
        public virtual Product Product { get; set; }
    }
}
