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
    
    public partial class ProductStorage
    {
        public int id { get; set; }
        public Nullable<int> productid { get; set; }
        public Nullable<int> storageid { get; set; }
    
        public virtual Product Product { get; set; }
        public virtual Storage Storage { get; set; }
    }
}