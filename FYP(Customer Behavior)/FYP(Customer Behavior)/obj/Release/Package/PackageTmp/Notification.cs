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
    
    public partial class Notification
    {
        public int NotificationId { get; set; }
        public int SellerId { get; set; }
        public Nullable<int> CustomerId { get; set; }
        public string Message { get; set; }
        public System.DateTime Date { get; set; }
        public string Status { get; set; }
        public string GuestId { get; set; }
    
        public virtual Customer Customer { get; set; }
        public virtual Seller Seller { get; set; }
    }
}
