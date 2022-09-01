using FYP_Customer_Behavior_.Models.Dtos;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace FYP_Customer_Behavior_.Controllers
{
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
    public class ShippingClerkController : Controller
    {
        FYPEntities _db = new FYPEntities();

        // GET: ShippingClerk
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(string email, string password)
        {
            if (email == "tcs@tcs.com" && password == "tcs")
            {
                Session["tcs"] = "tcs";
                return Redirect("~/ShippingClerk/Index");
            }
            else
            {
                return View();
            }
        }
        [HttpGet]
        public ActionResult Index()
        {
            if (Session["tcs"] != null)
            {
                List<SellerSalesOrderDto> sellerSalesOrderDtos = (from so in _db.SalesOrders
                                                                  join sol in _db.SalesOrderLines on so.order_ID equals sol.order_id
                                                                  join p in _db.Products on sol.product_id equals p.Product_ID
                                                                  join pi in _db.Product_Image on p.Product_ID equals pi.Product_ID
                                                                  select new SellerSalesOrderDto
                                                                  {
                                                                      id = sol.id,
                                                                      custID = (int)so.cust_Id,
                                                                      custName = _db.Customers.FirstOrDefault(x=>x.Cust_ID == so.cust_Id).Cust_Name,
                                                                      OrderId = so.order_ID,
                                                                      GuestID = so.GuestId,
                                                                      GuestName = so.GuestName,
                                                                      productId = sol.product_id,
                                                                      proName = p.Product_Name,
                                                                      trackingID = so.trackingID,
                                                                      pImage = pi.Image_1,
                                                                      shipment = _db.Shipments.FirstOrDefault(x=>x.id == so.shipmentID).Name,
                                                                      paymentType = _db.payment_methods.FirstOrDefault(x=>x.payment_id == so.payment_id).payment_type,
                                                                      netPrice = (float)sol.netAmount,
                                                                      qty = sol.quantity,
                                                                      unitPrice = (float)sol.unitPrice,
                                                                      date = so.order_date,
                                                                      shipaddr = _db.Addresses.FirstOrDefault(x => x.Id == sol.shippingAddessId).Address1,
                                                                      billaddr = _db.Addresses.FirstOrDefault(x => x.Id == sol.billingAddressId).Address1,
                                                                      isConfirmed = (int)sol.isConfirmed
                                                                  }).OrderByDescending(x=>x.OrderId).ToList();
                return View(sellerSalesOrderDtos);
            }
            else
            {
                return Redirect("~/ShippingClerk/Login");
            }
        }

        public ActionResult DeliveryConfirmation(int confirm)
        {
            var sol = _db.SalesOrderLines.Where(x => x.id == confirm).SingleOrDefault();
            sol.isConfirmed = 2;
            _db.Entry(sol).State = System.Data.Entity.EntityState.Modified;
            _db.SaveChanges();
            return Redirect("~/ShippingClerk/Index");
        }

        public ActionResult LogOut()
        {
            Session["tcs"] = null;
            Session.Abandon();
            Session.Remove("tcs");
            Session.Clear();
            return Redirect("~/ShippingClerk/Login");
        }
    }
}