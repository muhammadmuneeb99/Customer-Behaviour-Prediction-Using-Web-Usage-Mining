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
    
    public partial class UserBehavior
    {
        public int id { get; set; }
        public int behaviorId { get; set; }
        public int userTokenId { get; set; }
        public System.DateTime dateTime { get; set; }
    
        public virtual Behavior Behavior { get; set; }
        public virtual UserToken UserToken { get; set; }
    }
}