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
    
    public partial class SubSubCategory
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SubSubCategory()
        {
            this.SubCatBrands = new HashSet<SubCatBrand>();
            this.subsubCatImages = new HashSet<subsubCatImage>();
        }
    
        public int SubSubCategoryID { get; set; }
        public string SubSubCategoryName { get; set; }
        public int Sub_Category_ID { get; set; }
    
        public virtual Sub_Category Sub_Category { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SubCatBrand> SubCatBrands { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<subsubCatImage> subsubCatImages { get; set; }
    }
}
