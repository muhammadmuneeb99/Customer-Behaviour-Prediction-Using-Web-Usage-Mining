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
    
    public partial class subsubCatImage
    {
        public int Id { get; set; }
        public int subsubCatID { get; set; }
        public string Image { get; set; }
    
        public virtual SubSubCategory SubSubCategory { get; set; }
    }
}
