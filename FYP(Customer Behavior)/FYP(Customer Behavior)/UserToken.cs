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
    
    public partial class UserToken
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public UserToken()
        {
            this.UserBehaviors = new HashSet<UserBehavior>();
        }
    
        public int id { get; set; }
        public Nullable<int> userId { get; set; }
        public string token { get; set; }
        public int tokenTypeId { get; set; }
    
        public virtual Customer Customer { get; set; }
        public virtual TokenType TokenType { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UserBehavior> UserBehaviors { get; set; }
    }
}