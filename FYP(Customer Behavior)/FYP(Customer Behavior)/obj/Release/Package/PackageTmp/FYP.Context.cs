﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class FYPEntities : DbContext
    {
        public FYPEntities()
            : base("name=FYPEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Address> Addresses { get; set; }
        public virtual DbSet<Admin> Admins { get; set; }
        public virtual DbSet<Area> Areas { get; set; }
        public virtual DbSet<Brand_Images> Brand_Images { get; set; }
        public virtual DbSet<Brand> Brands { get; set; }
        public virtual DbSet<CanceledOrder> CanceledOrders { get; set; }
        public virtual DbSet<Cart> Carts { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<CategoryImage> CategoryImages { get; set; }
        public virtual DbSet<City> Cities { get; set; }
        public virtual DbSet<Color> Colors { get; set; }
        public virtual DbSet<Country> Countries { get; set; }
        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<Customer_Contact> Customer_Contact { get; set; }
        public virtual DbSet<EmartProfit> EmartProfits { get; set; }
        public virtual DbSet<Gender> Genders { get; set; }
        public virtual DbSet<Log> Logs { get; set; }
        public virtual DbSet<Notification> Notifications { get; set; }
        public virtual DbSet<payment_methods> payment_methods { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<Product_Feature> Product_Feature { get; set; }
        public virtual DbSet<Product_Image> Product_Image { get; set; }
        public virtual DbSet<ProductColor> ProductColors { get; set; }
        public virtual DbSet<ProductPrice> ProductPrices { get; set; }
        public virtual DbSet<ProductSize> ProductSizes { get; set; }
        public virtual DbSet<ProductStorage> ProductStorages { get; set; }
        public virtual DbSet<ProductWarrenty> ProductWarrenties { get; set; }
        public virtual DbSet<RatingsAndReview> RatingsAndReviews { get; set; }
        public virtual DbSet<SalesOrder> SalesOrders { get; set; }
        public virtual DbSet<SalesOrderLine> SalesOrderLines { get; set; }
        public virtual DbSet<Seller> Sellers { get; set; }
        public virtual DbSet<Seller_address> Seller_address { get; set; }
        public virtual DbSet<Seller_Contact> Seller_Contact { get; set; }
        public virtual DbSet<SellerImage> SellerImages { get; set; }
        public virtual DbSet<SellerPaymentSubscription> SellerPaymentSubscriptions { get; set; }
        public virtual DbSet<SellerProduct> SellerProducts { get; set; }
        public virtual DbSet<SellerProfit> SellerProfits { get; set; }
        public virtual DbSet<Shipment> Shipments { get; set; }
        public virtual DbSet<Size> Sizes { get; set; }
        public virtual DbSet<State> States { get; set; }
        public virtual DbSet<Storage> Storages { get; set; }
        public virtual DbSet<Sub_Category> Sub_Category { get; set; }
        public virtual DbSet<SubCatBrand> SubCatBrands { get; set; }
        public virtual DbSet<SubCatImage> SubCatImages { get; set; }
        public virtual DbSet<SubSubCategory> SubSubCategories { get; set; }
        public virtual DbSet<subsubCatImage> subsubCatImages { get; set; }
        public virtual DbSet<sysdiagram> sysdiagrams { get; set; }
        public virtual DbSet<WarrentyDurationType> WarrentyDurationTypes { get; set; }
        public virtual DbSet<WishList> WishLists { get; set; }
    
        public virtual ObjectResult<AreaCityStateCountrySelect_Result> AreaCityStateCountrySelect()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<AreaCityStateCountrySelect_Result>("AreaCityStateCountrySelect");
        }
    
        public virtual ObjectResult<CityStateCountrySelect_Result> CityStateCountrySelect()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<CityStateCountrySelect_Result>("CityStateCountrySelect");
        }
    
        public virtual ObjectResult<customerLogin_Result> customerLogin(string email, string password)
        {
            var emailParameter = email != null ?
                new ObjectParameter("email", email) :
                new ObjectParameter("email", typeof(string));
    
            var passwordParameter = password != null ?
                new ObjectParameter("password", password) :
                new ObjectParameter("password", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<customerLogin_Result>("customerLogin", emailParameter, passwordParameter);
        }
    
        public virtual ObjectResult<string> dayAndNightBased(string format)
        {
            var formatParameter = format != null ?
                new ObjectParameter("format", format) :
                new ObjectParameter("format", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<string>("dayAndNightBased", formatParameter);
        }
    
        public virtual ObjectResult<string> dayBased(string user, string day)
        {
            var userParameter = user != null ?
                new ObjectParameter("user", user) :
                new ObjectParameter("user", typeof(string));
    
            var dayParameter = day != null ?
                new ObjectParameter("day", day) :
                new ObjectParameter("day", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<string>("dayBased", userParameter, dayParameter);
        }
    
        public virtual int MasterAdminLogin(string username, string password)
        {
            var usernameParameter = username != null ?
                new ObjectParameter("username", username) :
                new ObjectParameter("username", typeof(string));
    
            var passwordParameter = password != null ?
                new ObjectParameter("password", password) :
                new ObjectParameter("password", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("MasterAdminLogin", usernameParameter, passwordParameter);
        }
    
        public virtual ObjectResult<string> mostViewedItemClickAndTime()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<string>("mostViewedItemClickAndTime");
        }
    
        public virtual ObjectResult<Nullable<int>> PagesView(string date)
        {
            var dateParameter = date != null ?
                new ObjectParameter("date", date) :
                new ObjectParameter("date", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Nullable<int>>("PagesView", dateParameter);
        }
    
        public virtual int sellerProduct_View(Nullable<int> sellerID)
        {
            var sellerIDParameter = sellerID.HasValue ?
                new ObjectParameter("sellerID", sellerID) :
                new ObjectParameter("sellerID", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("sellerProduct_View", sellerIDParameter);
        }
    
        public virtual ObjectResult<StateCountrySelect_Result> StateCountrySelect()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<StateCountrySelect_Result>("StateCountrySelect");
        }
    
        public virtual ObjectResult<string> timeSpendBased(string userID)
        {
            var userIDParameter = userID != null ?
                new ObjectParameter("UserID", userID) :
                new ObjectParameter("UserID", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<string>("timeSpendBased", userIDParameter);
        }
    
        public virtual ObjectResult<string> usersIDForPage(string userid)
        {
            var useridParameter = userid != null ?
                new ObjectParameter("userid", userid) :
                new ObjectParameter("userid", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<string>("usersIDForPage", useridParameter);
        }
    
        public virtual ObjectResult<string> userVisitedPages(string userId)
        {
            var userIdParameter = userId != null ?
                new ObjectParameter("userId", userId) :
                new ObjectParameter("userId", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<string>("userVisitedPages", userIdParameter);
        }
    
        public virtual int wishlistGet(Nullable<int> customerID)
        {
            var customerIDParameter = customerID.HasValue ?
                new ObjectParameter("customerID", customerID) :
                new ObjectParameter("customerID", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("wishlistGet", customerIDParameter);
        }
    }
}