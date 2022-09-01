using FYP_Customer_Behavior_.Models.Analytics;
using FYP_Customer_Behavior_.Models.Dtos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FYP_Customer_Behavior_.Controllers
{
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
    public class AdminController : Controller
    {
        FYPEntities _db = new FYPEntities();
        // GET: Admin
        public ActionResult Index()
        {
            try
            {
                if (Session["Admin"] != null)
                {
                    return View();
                }
                else
                {
                    return Redirect("~/Admin/AdminLogin");
                }
            }
            catch (Exception)
            {
                return Redirect("~/Admin/ErrorPage");
            }
        }
        [HttpGet]
        public ActionResult Customer()
        {
            try
            {
                if (Session["Admin"] != null)
                {
                    var cus = (from c in _db.Customers
                               join g in _db.Genders on c.Gender_ID equals g.Gender_ID
                               join cc in _db.Customer_Contact on c.Cust_ID equals cc.Customer_ID
                               select new CustomerDto
                               {
                                   Id = c.Cust_ID,
                                   Name = c.Cust_Name,
                                   Email = c.Email,
                                   gender = g.Type,
                                   contact = cc.PhoneNo1
                               }).ToList();
                    ViewBag.cl = cus;
                    return View();
                }
                else
                {
                    return Redirect("~/Admin/AdminLogin");
                }
            }
            catch (Exception)
            {
                return Redirect("~/Admin/ErrorPage");
            }
        }
        [HttpGet]
        public ActionResult Seller_Info()
        {
            try
            {
                if (Session["Admin"] != null)
                {
                    var sInfo = (from s in _db.Sellers
                                 join sc in _db.Seller_Contact
                                 on s.sellerId equals sc.Seller_ID
                                 join sa in _db.Seller_address
                                 on s.sellerId equals sa.seller_id
                                 select new SellerDto
                                 {
                                     Id = s.sellerId,
                                     Name = s.sellerName,
                                     CNIC = s.CNIC,
                                     Email = s.Email,
                                     Address = sa.address,
                                     Contact = sc.PhoneNo
                                 }).ToList();
                    ViewBag.si = sInfo;
                    return View();
                }
                else
                {
                    return Redirect("~/Admin/AdminLogin");
                }
            }
            catch (Exception)
            {
                return Redirect("~/Admin/ErrorPage");
            }
        }
        [HttpGet]
        public ActionResult Category()
        {
            try
            {
                if (Session["Admin"] != null)
                {
                    var cat = (from c in _db.Categories
                               select new CatNameImage
                               {
                                   id = c.Cat_ID,
                                   name = c.Cat_Name,
                                   Image1 = _db.CategoryImages.Where(x => x.CatId == c.Cat_ID).FirstOrDefault().Images
                               }).ToList();
                    return View(cat);
                }
                else
                {
                    return Redirect("~/Admin/AdminLogin");
                }
            }
            catch (Exception)
            {
                return Redirect("~/Admin/ErrorPage");
            }
        }
        [HttpPost]
        public ActionResult Category(string cName, string cDes, HttpPostedFileBase httpPostedFileBase)
        {
            try
            {
                if (Session["Admin"] != null)
                {
                    Category category = new Category()
                    { Cat_Name = cName, Description = cDes };
                    _db.Categories.Add(category);
                    _db.SaveChanges();
                    var InputFileName = Path.GetFileName(httpPostedFileBase.FileName);
                    InputFileName = Guid.NewGuid().ToString();
                    string _fname = InputFileName.ToString();
                    var ServerSavePath = Path.Combine(Server.MapPath("../Cat_Images"), _fname + ".png");
                    httpPostedFileBase.SaveAs(ServerSavePath);
                    int newCatId = _db.Categories.Max(id => id.Cat_ID);
                    CategoryImage catImage = new CategoryImage()
                    { CatId = newCatId, Images = "../Cat_Images/" + _fname + ".png" };
                    _db.CategoryImages.Add(catImage);
                    _db.SaveChanges();
                    return Redirect("~/Admin/Category");
                }
                else
                {
                    return Redirect("~/Admin/AdminLogin");
                }
            }
            catch (Exception)
            {
                return Redirect("~/Admin/ErrorPage");
            }
        }
        [HttpGet]
        public ActionResult SubCategory()
        {
            try
            {
                if (Session["Admin"] != null)
                {
                    var cat = (from c in _db.Categories select new CatNameImage { id = c.Cat_ID, name = c.Cat_Name }).ToList();
                    ViewBag.c = cat;
                    var sccat = (from c in _db.Categories
                                 join sc in _db.Sub_Category on c.Cat_ID equals sc.Cat_ID
                                 select new subCatDto
                                 {
                                     cname = c.Cat_Name,
                                     scname = sc.Sub_Name,
                                     id = sc.Sub_Category_ID,
                                     image = _db.SubCatImages.Where(x => x.SubCatId == sc.Sub_Category_ID).FirstOrDefault().Images
                                 }).ToList();
                    return View(sccat);
                }
                else
                {
                    return Redirect("~/Admin/AdminLogin");
                }
            }
            catch (Exception)
            {
                return Redirect("~/Admin/ErrorPage");
            }
        }
        [HttpPost]
        public ActionResult SubCategory(int selectedCat, string sName, string scDes, HttpPostedFileBase httpPostedFileBase)
        {
            try
            {
                if (Session["Admin"] != null)
                {
                    Sub_Category subCategory = new Sub_Category()
                    { Cat_ID = selectedCat, Sub_Name = sName, Description = scDes };
                    _db.Sub_Category.Add(subCategory);
                    _db.SaveChanges();
                    var InputFileName = Path.GetFileName(httpPostedFileBase.FileName);
                    InputFileName = Guid.NewGuid().ToString();
                    string _fname = InputFileName.ToString();
                    var ServerSavePath = Path.Combine(Server.MapPath("../SubCatImages"), _fname + ".png");
                    httpPostedFileBase.SaveAs(ServerSavePath);
                    int newsubCatId = _db.Sub_Category.Max(id => id.Sub_Category_ID);
                    SubCatImage subcatImage = new SubCatImage()
                    { SubCatId = newsubCatId, Images = "../SubCatImages/" + _fname + ".png" };
                    _db.SubCatImages.Add(subcatImage);
                    _db.SaveChanges();
                    return Redirect("~/Admin/SubCategory");
                }
                else
                {
                    return Redirect("~/Admin/AdminLogin");
                }
            }
            catch (Exception)
            {
                return Redirect("~/Admin/ErrorPage");
            }
        }
        [HttpGet]
        public ActionResult SubSubCategory()
        {
            try
            {
                if (Session["Admin"] != null)
                {
                    var cat = (from c in _db.Categories select new CatNameImage { id = c.Cat_ID, name = c.Cat_Name }).ToList();
                    ViewBag.c = cat;
                    var sccat = (from c in _db.Categories
                                 join sc in _db.Sub_Category on c.Cat_ID equals sc.Cat_ID
                                 join ssc in _db.SubSubCategories on sc.Sub_Category_ID equals ssc.Sub_Category_ID
                                 select new subCatDto
                                 {
                                     cname = c.Cat_Name,
                                     scname = sc.Sub_Name,
                                     sscname = ssc.SubSubCategoryName,
                                     id = ssc.SubSubCategoryID,
                                     image = _db.subsubCatImages.Where(x => x.subsubCatID == ssc.SubSubCategoryID).FirstOrDefault().Image
                                 }).ToList();
                    return View(sccat);
                }
                else
                {
                    return Redirect("~/Admin/AdminLogin");
                }
            }
            catch (Exception)
            {
                return Redirect("~/Admin/ErrorPage");
            }
        }
        [HttpPost]
        public ActionResult SubSubCategory(int selectedSubCat, string ssName, HttpPostedFileBase httpPostedFileBase)
        {
            try
            {
                if (Session["Admin"] != null)
                {
                    SubSubCategory subSubCategory = new SubSubCategory()
                    {
                        Sub_Category_ID = selectedSubCat,
                        SubSubCategoryName = ssName
                    };
                    _db.SubSubCategories.Add(subSubCategory);
                    _db.SaveChanges();
                    var InputFileName = Path.GetFileName(httpPostedFileBase.FileName);
                    InputFileName = Guid.NewGuid().ToString();
                    string _fname = InputFileName.ToString();
                    var ServerSavePath = Path.Combine(Server.MapPath("../SubSubCatImages"), _fname + ".png");
                    httpPostedFileBase.SaveAs(ServerSavePath);
                    int newsubCatId = _db.SubSubCategories.Max(id => id.SubSubCategoryID);
                    subsubCatImage subSubcatImage = new subsubCatImage()
                    { subsubCatID = newsubCatId, Image = "../SubSubCatImages/" + _fname + ".png" };
                    _db.subsubCatImages.Add(subSubcatImage);
                    _db.SaveChanges();
                    return Redirect("~/Admin/SubSubCategory");
                }
                else
                {
                    return Redirect("~/Admin/AdminLogin");
                }
            }
            catch (Exception)
            {
                return Redirect("~/Admin/ErrorPage");
            }
        }
        [HttpGet]
        public ActionResult Product()
        {
            try
            {
                if (Session["Admin"] != null)
                {
                    List<AdminProductDetailDto> adminProductDetailDto = (from c in _db.Categories
                                                                         join sc in _db.Sub_Category on c.Cat_ID equals sc.Cat_ID
                                                                         join ssc in _db.SubSubCategories on sc.Sub_Category_ID equals ssc.Sub_Category_ID
                                                                         join sb in _db.SubCatBrands on ssc.SubSubCategoryID equals sb.subSubCatId
                                                                         join b in _db.Brands on sb.BrandID equals b.BrandId
                                                                         join p in _db.Products on sb.Id equals p.brandCategoryId
                                                                         select new AdminProductDetailDto
                                                                         {
                                                                             id = p.Product_ID,
                                                                             pName = p.Product_Name,
                                                                             BrandName = b.Brand_Name,
                                                                             subcat2 = ssc.SubSubCategoryName,
                                                                             subcat1 = sc.Sub_Name,
                                                                             cat = c.Cat_Name
                                                                         }).ToList();
                    return View(adminProductDetailDto);
                }
                else
                {
                    return Redirect("~/Admin/AdminLogin");
                }
            }
            catch (Exception)
            {
                return Redirect("~/Admin/ErrorPage");
            }
        }
        [HttpGet]
        public ActionResult Order()
        {
            try
            {
                if (Session["Admin"] != null)
                {
                    List<SellerSalesOrderDto> adminOrderDetailsDto = (from so in _db.SalesOrders
                                                                      join sol in _db.SalesOrderLines on so.order_ID equals sol.order_id
                                                                      join pay in _db.payment_methods on so.payment_id equals pay.payment_id
                                                                      join p in _db.Products on sol.product_id equals p.Product_ID
                                                                      join a in _db.Addresses on sol.billingAddressId equals a.Id
                                                                      join a1 in _db.Addresses on sol.shippingAddessId equals a1.Id
                                                                      join sh in _db.Shipments on so.shipmentID equals sh.id
                                                                      select new SellerSalesOrderDto
                                                                      {
                                                                          OrderId = so.order_ID,
                                                                          GuestID = so.GuestId,
                                                                          custName = _db.Customers.FirstOrDefault(x => x.Cust_ID == so.cust_Id).Cust_Name,
                                                                          GuestName = so.GuestName,
                                                                          paymentType = pay.payment_type,
                                                                          date = so.order_date,
                                                                          qty = sol.quantity,
                                                                          proName = p.Product_Name,
                                                                          billaddr = a.Address1,
                                                                          shipaddr = a1.Address1,
                                                                          shipment = sh.Name,
                                                                          trackingID = so.trackingID,
                                                                          netPrice = (float)sol.netAmount,
                                                                          unitPrice = (float)sol.unitPrice
                                                                      }).OrderByDescending(x => x.OrderId).ToList();

                    return View(adminOrderDetailsDto);
                }
                else
                {
                    return Redirect("~/Admin/AdminLogin");
                }
            }
            catch (Exception)
            {
                return Redirect("~/Admin/ErrorPage");
            }
        }
        [HttpGet]
        public ActionResult Brands()
        {
            try
            {
                if (Session["Admin"] != null)
                {
                    var brands = (from b in _db.Brands
                                  join bi in _db.Brand_Images on b.BrandId equals bi.BrandID
                                  select new subCatDto
                                  {
                                      id = b.BrandId,
                                      brandName = b.Brand_Name,
                                      image = bi.Image
                                  }).ToList();
                    return View(brands);
                }
                else
                {
                    return Redirect("~/Admin/AdminLogin");
                }
            }
            catch (Exception)
            {
                return Redirect("~/Admin/ErrorPage");
            }
        }
        [HttpPost]
        public ActionResult Brands(string bName, HttpPostedFileBase httpPostedFileBase)
        {
            try
            {
                if (Session["Admin"] != null)
                {
                    Brand brand = new Brand() { Brand_Name = bName };
                    _db.Brands.Add(brand);
                    _db.SaveChanges();
                    var InputFileName = Path.GetFileName(httpPostedFileBase.FileName);
                    InputFileName = Guid.NewGuid().ToString();
                    string _fname = InputFileName.ToString();
                    var ServerSavePath = Path.Combine(Server.MapPath("../Brand_Images"), _fname + ".png");
                    int brandID = _db.Brands.Max(id => id.BrandId);
                    httpPostedFileBase.SaveAs(ServerSavePath);
                    Brand_Images brand_Images = new Brand_Images()
                    { BrandID = brandID, Image = "../Brand_Images/" + _fname + ".png" };
                    _db.Brand_Images.Add(brand_Images);
                    _db.SaveChanges();

                    return Redirect("~/Admin/Brands");
                }
                else
                {
                    return Redirect("~/Admin/AdminLogin");
                }
            }
            catch (Exception)
            {
                return Redirect("~/Admin/ErrorPage");
            }
        }
        [HttpGet]
        public ActionResult AdminLogin()
        {
            try
            {
                return View();
            }
            catch (Exception)
            {
                return Redirect("~/Admin/ErrorPage");
            }
        }
        [HttpPost]
        public ActionResult AdminLogin(string email1, string pass1)
        {
            try
            {
                var a_Log = _db.Admins.FirstOrDefault(x => x.User_Name == email1 && x.Pass == pass1);
                if (a_Log != null)
                {
                    Session["Admin"] = a_Log;
                    return Redirect("~/Admin/Index");
                }
                else
                {
                    return View();
                }
            }
            catch (Exception)
            {
                return Redirect("~/Admin/ErrorPage");
            }
        }
        [HttpGet]
        public ActionResult Logout()
        {
            try
            {
                Session["Admin"] = null;
                Session.Abandon();
                Session.Remove("Admin");
                Session.Clear();

                return Redirect("~/Admin/AdminLogin");
            }
            catch (Exception)
            {
                return Redirect("~/Admin/ErrorPage");
            }
        }
        public ActionResult LoadSubCat(int catId)
        {
            try
            {
                return Json(_db.Sub_Category.Where(s => s.Cat_ID == catId).Select(s => new
                {
                    SID = s.Sub_Category_ID,
                    Name = s.Sub_Name
                }).ToList());
            }
            catch (Exception)
            {
                return Redirect("~/Admin/ErrorPage");
            }
        }

        public ActionResult LoadSubSubCat(int subcatId)
        {
            try
            {
                return Json(_db.SubSubCategories.Where(s => s.Sub_Category_ID == subcatId).Select(s => new
                {
                    SID = s.SubSubCategoryID,
                    Name = s.SubSubCategoryName
                }).ToList());
            }
            catch (Exception)
            {
                return Redirect("~/Admin/ErrorPage");
            }
        }
        [HttpGet]
        public ActionResult InsertBrandInCat()
        {
            try
            {
                if (Session["Admin"] != null)
                {
                    var subsubcats = (from ssc in _db.SubSubCategories select new SubSubCatBrandsDto { sscid = ssc.SubSubCategoryID, sscName = ssc.SubSubCategoryName }).ToList();
                    var brands = (from b in _db.Brands select new BrandDto { brandId = b.BrandId, brandName = b.Brand_Name }).ToList();
                    ViewBag.subsubcats = subsubcats;
                    ViewBag.brands = brands;
                    var subsubcatbrand = (from sscb in _db.SubCatBrands
                                          select new SubSubCatBrandsDto
                                          {
                                              sscbId = sscb.Id,
                                              sscName = _db.SubSubCategories.FirstOrDefault(x => x.SubSubCategoryID == sscb.subSubCatId).SubSubCategoryName,
                                              BrandName = _db.Brands.FirstOrDefault(x => x.BrandId == sscb.BrandID).Brand_Name
                                          }).ToList();
                    return View(subsubcatbrand);
                }
                else
                {
                    return Redirect("~/Admin/AdminLogin");
                }
            }
            catch (Exception)
            {
                return Redirect("~/Admin/ErrorPage");
            }
        }
        [HttpPost]
        public ActionResult InsertBrandInCat(int subsubcatId, int brandId)
        {
            try
            {
                SubCatBrand subCatBrand = new SubCatBrand()
                {
                    BrandID = brandId,
                    subSubCatId = subsubcatId
                };
                _db.SubCatBrands.Add(subCatBrand);
                _db.SaveChanges();
                return Redirect("~/Admin/InsertBrandInCat");
            }
            catch (Exception)
            {
                return Redirect("~/Admin/ErrorPage");
            }
        }
        List<DataPoint> dataPoints;
        [HttpGet]
        public ActionResult AnalyticsAdmin()
        {
            try
            {
                if (Session["Admin"] != null)
                {
                    AnalyticsAdminSeller numberOfUsers = new AnalyticsAdminSeller();
                    dataPoints = new List<DataPoint>();
                    var a = numberOfUsers.numberOfUser();
                    for (int i = 0; i < a.Count; i++)
                    {
                        dataPoints.Add(new DataPoint { X4 = a.ElementAt(i).Key, Y4 = a.ElementAt(i).Value });
                    }
                    JsonSerializerSettings _jsonSetting = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };
                    var json = JsonConvert.SerializeObject(dataPoints, _jsonSetting);
                    ViewBag.a = json;
                    /////////////////
                    dataPoints = new List<DataPoint>();
                    var c = numberOfUsers.mostBoughtItem();
                    for (int i = 0; i < c.Count; i++)
                    {
                        dataPoints.Add(new DataPoint { X6 = c.ElementAt(i).Key, Y6 = c.ElementAt(i).Value });
                    }
                    _jsonSetting = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };
                    json = JsonConvert.SerializeObject(dataPoints, _jsonSetting);
                    ViewBag.c = json;
                    //////////////
                    dataPoints = new List<DataPoint>();
                    var d = numberOfUsers.nOOrders();
                    for (int i = 0; i < d.Count; i++)
                    {
                        dataPoints.Add(new DataPoint { X7 = d.ElementAt(i).Key, Y7 = d.ElementAt(i).Value });
                    }
                    _jsonSetting = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };
                    json = JsonConvert.SerializeObject(dataPoints, _jsonSetting);
                    ViewBag.d = json;
                    ///////////////
                    dataPoints = new List<DataPoint>();
                    var e = numberOfUsers.mostRatedItem();
                    for (int i = 0; i < e.Count; i++)
                    {
                        dataPoints.Add(new DataPoint { X8 = e.ElementAt(i).Key, Y8 = e.ElementAt(i).Value });
                    }
                    _jsonSetting = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };
                    json = JsonConvert.SerializeObject(dataPoints, _jsonSetting);
                    ViewBag.e = json;
                    ///////////////
                    dataPoints = new List<DataPoint>();
                    var f = numberOfUsers.mostViewedProductBasedClick();
                    for (int i = 0; i < f.Count; i++)
                    {
                        dataPoints.Add(new DataPoint { X9 = f.ElementAt(i).Key, Y9 = f.ElementAt(i).Value });
                    }
                    _jsonSetting = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };
                    json = JsonConvert.SerializeObject(dataPoints, _jsonSetting);
                    ViewBag.f = json;
                    ///////////////
                    dataPoints = new List<DataPoint>();
                    var g = numberOfUsers.pageViews();
                    for (int i = 0; i < g.Count; i++)
                    {
                        dataPoints.Add(new DataPoint { X10 = g.ElementAt(i).Key, Y10 = g.ElementAt(i).Value });
                    }
                    _jsonSetting = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };
                    json = JsonConvert.SerializeObject(dataPoints, _jsonSetting);
                    ViewBag.g = json;
                    ///////////////
                    dataPoints = new List<DataPoint>();
                    var h = numberOfUsers.ordersPerDay();
                    for (int i = 0; i < h.Count; i++)
                    {
                        dataPoints.Add(new DataPoint { X11 = h.ElementAt(i).Key, Y11 = h.ElementAt(i).Value });
                    }
                    _jsonSetting = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };
                    json = JsonConvert.SerializeObject(dataPoints, _jsonSetting);
                    ViewBag.h = json;
                    ///////////////
                    dataPoints = new List<DataPoint>();
                    var ii = numberOfUsers.mostViewedProductBasedTime();
                    for (int i = 0; i < ii.Count; i++)
                    {
                        dataPoints.Add(new DataPoint { X12 = ii.ElementAt(i).Key, Y12 = ii.ElementAt(i).Value });
                    }
                    _jsonSetting = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };
                    json = JsonConvert.SerializeObject(dataPoints, _jsonSetting);
                    ViewBag.ii = json;
                    ///////////////
                    dataPoints = new List<DataPoint>();
                    var ij = numberOfUsers.earning();
                    for (int i = 0; i < ij.Count; i++)
                    {
                        dataPoints.Add(new DataPoint { X13 = ij.ElementAt(i).Key, Y13 = ij.ElementAt(i).Value });
                    }
                    _jsonSetting = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };
                    json = JsonConvert.SerializeObject(dataPoints, _jsonSetting);
                    ViewBag.ij = json;

                    return View();
                }
                else
                {
                    return Redirect("~/Admin/AdminLogin");
                }
            }
            catch (Exception ex)
            {
                return Redirect("~/Admin/ErrorPage");
            }
        }
        [HttpGet]
        public ActionResult ErrorPage()
        {
            return View();
        }
    }
}