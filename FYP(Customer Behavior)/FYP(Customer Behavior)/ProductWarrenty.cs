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
    
    public partial class ProductWarrenty
    {
        public Nullable<int> productId { get; set; }
        public int warrentyCount { get; set; }
        public int id { get; set; }
        public string warrentyDescription { get; set; }
        public Nullable<int> warrentyDuraionId { get; set; }
    
        public virtual Product Product { get; set; }
        public virtual WarrentyDurationType WarrentyDurationType { get; set; }
    }
}
