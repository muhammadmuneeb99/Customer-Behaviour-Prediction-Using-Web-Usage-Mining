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
    
    public partial class EmartProfit
    {
        public int Id { get; set; }
        public Nullable<int> SalesOrderID { get; set; }
        public Nullable<double> Amout { get; set; }
    
        public virtual SalesOrder SalesOrder { get; set; }
    }
}
