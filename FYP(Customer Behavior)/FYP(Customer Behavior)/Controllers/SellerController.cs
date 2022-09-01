using FYP_Customer_Behavior_.Common;
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
    [OutputCache(NoStore = true, Duration = 15, VaryByParam = "None")]
    public class SellerController : Controller
    {
        FYPEntities _db = new FYPEntities();
        [TrackExecution]
        public JsonResult GetNotification()
        {
            int s_id = ((SellerDto)Session["sellerSession"]).Id;
            return Json(Models.NotificationServices.GetNotification(s_id), JsonRequestBehavior.AllowGet);
        }
        [TrackExecution]
        [HttpGet]
        public ActionResult Seller_Panel()
        {
            try
            {
                if (Session["sellerSession"] != null)
                {
                    return View();
                }
                else
                {
                    return Redirect("~/Seller/Seller_Login");
                }
            }
            catch (Exception e)
            {
                return Redirect("~/Seller/ErrorPage");
            }
        }
        [TrackExecution]
        [HttpGet]
        public ActionResult Seller_Product()
        {
            try
            {
                if (Session["sellerSession"] != null)
                {
                    int s_id = ((SellerDto)Session["sellerSession"]).Id;
                    var s = (from ss in _db.Sellers
                             join sp in _db.SellerProducts on ss.sellerId equals sp.sellerId
                             join p in _db.Products on sp.productId equals p.Product_ID
                             join scb in _db.SubCatBrands on p.brandCategoryId equals scb.Id
                             join b in _db.Brands on scb.BrandID equals b.BrandId
                             join ssc in _db.SubSubCategories on scb.subSubCatId equals ssc.SubSubCategoryID
                             join sc in _db.Sub_Category on ssc.Sub_Category_ID equals sc.Sub_Category_ID
                             join c in _db.Categories on sc.Cat_ID equals c.Cat_ID
                             join pi in _db.Product_Image on p.Product_ID equals pi.Product_ID
                             join pp in _db.ProductPrices on p.Product_ID equals pp.ProductID
                             where ss.sellerId == s_id && pp.EndDate == null
                             select new SellerProductsDto
                             {
                                 id = p.Product_ID,
                                 pName = p.Product_Name,
                                 qty = p.quantity,
                                 image = pi.Image_1,
                                 subSubCatName = ssc.SubSubCategoryName,
                                 subCatName = sc.Sub_Name,
                                 catName = c.Cat_Name,
                                 price = (float)pp.Price,
                                 BrandName = b.Brand_Name
                             }).OrderByDescending(x => x.id).ToList();
                    ViewBag.spv = s;
                    return View();
                }
                else
                {
                    return Redirect("~/Seller/Seller_Login");
                }
            }
            catch (Exception e)
            {
                return Redirect("~/Seller/ErrorPage");
            }
        }
        [TrackExecution]
        public ActionResult RemoveProduct(int productID)
        {
            try
            {
                var product = new Product { Product_ID = productID };
                _db.Entry(product).State = System.Data.Entity.EntityState.Deleted;
                _db.SaveChanges();
                return Redirect("~/Seller/Seller_Product");
            }
            catch (Exception e)
            {
                return Redirect("~/Seller/ErrorPage");
            }
        }
        [TrackExecution]
        [HttpGet]
        public ActionResult Seller_Order()
        {
            try
            {
                if (Session["sellerSession"] != null)
                {
                    int s_id = ((SellerDto)Session["sellerSession"]).Id;
                    List<SellerSalesOrderDto> sellerSalesOrderDtos = (from so in _db.SalesOrders
                                                                      join sol in _db.SalesOrderLines on so.order_ID equals sol.order_id
                                                                      join p in _db.Products on sol.product_id equals p.Product_ID
                                                                      join pi in _db.Product_Image on p.Product_ID equals pi.Product_ID
                                                                      join sp in _db.SellerProducts on p.Product_ID equals sp.productId
                                                                      join s in _db.Sellers on sp.sellerId equals s.sellerId
                                                                      where s.sellerId == s_id
                                                                      select new SellerSalesOrderDto
                                                                      {
                                                                          id = sol.id,
                                                                          GuestID = so.GuestId,
                                                                          GuestName = so.GuestName,
                                                                          custID = (int?)so.cust_Id,
                                                                          custName = _db.Customers.FirstOrDefault(x => x.Cust_ID == so.cust_Id).Cust_Name,
                                                                          OrderId = so.order_ID,
                                                                          productId = sol.product_id,
                                                                          trackingID = so.trackingID,
                                                                          proName = p.Product_Name,
                                                                          pImage = pi.Image_1,
                                                                          shipment = _db.Shipments.FirstOrDefault(x => x.id == so.shipmentID).Name,
                                                                          paymentType = _db.payment_methods.FirstOrDefault(x => x.payment_id == so.payment_id).payment_type,
                                                                          SellerId = s.sellerId,
                                                                          netPrice = (float)sol.netAmount,
                                                                          qty = sol.quantity,
                                                                          unitPrice = (float)sol.unitPrice,
                                                                          date = so.order_date,
                                                                          shipaddr = _db.Addresses.FirstOrDefault(x => x.Id == sol.shippingAddessId).Address1,
                                                                          billaddr = _db.Addresses.FirstOrDefault(x => x.Id == sol.billingAddressId).Address1,
                                                                          isConfirmed = (int)sol.isConfirmed
                                                                      }).ToList();
                    return View(sellerSalesOrderDtos.OrderByDescending(x => x.isConfirmed));
                }
                else
                {
                    return Redirect("~/Seller/Seller_Login");
                }
            }
            catch (Exception e)
            {
                return Redirect("~/Seller/ErrorPage");
            }
        }
        static ProductModelDto productModel;
        [TrackExecution]
        [HttpGet]
        public ActionResult Seller_AddProduct()
        {
            try
            {
                if (Session["sellerSession"] != null)
                {
                    ViewBag.cat = from Category in _db.Categories select Category;
                    ViewBag.wt = from WarrentyDurationType in _db.WarrentyDurationTypes select WarrentyDurationType;
                    ViewBag.Color = from Color in _db.Colors select Color;
                    ViewBag.Size = from Size in _db.Sizes select Size;
                    ViewBag.Storage = from Storage in _db.Storages select Storage;
                    return View();
                }
                else
                {
                    return Redirect("~/Seller/Seller_Login");
                }
            }
            catch (Exception e)
            {
                return Redirect("~/Seller/ErrorPage");
            }

        }
        public List<string> imgname = new List<string>();
        [TrackExecution]
        [HttpPost]
        public ActionResult Seller_AddProduct(int selected_cat, int selected_subCat, int selected_subBrand, int selected_brand, float pPrice,
            string pname, string pDes, float pDiscount, string date, int qty, string proWarrenty, int warrentyDurationID,
            string proWarrentyDes, HttpPostedFileBase[] image1, string[] featureName, string[] featureDes, string discountType
            , int[] size, int[] color, int[] storage)
        {
            try
            {
                ViewBag.cat = from Category in _db.Categories select Category;
                ViewBag.wt = from WarrentyDurationType in _db.WarrentyDurationTypes select WarrentyDurationType;
                ViewBag.Color = from Color in _db.Colors select Color;
                ViewBag.Size = from Size in _db.Sizes select Size;
                ViewBag.Storage = from Storage in _db.Storages select Storage;
                productModel = new ProductModelDto();
                productModel.date = date;
                productModel.featureDes = featureDes.ToList();
                productModel.featureName = featureName.ToList();
                productModel.pPrice = pPrice;
                productModel.pQty = qty;
                productModel.selectedBrand = selected_brand;
                productModel.selectedBrandName = _db.Brands.FirstOrDefault(x => x.BrandId == selected_brand).Brand_Name;
                productModel.selectedSubSubCatBrand = selected_subBrand;
                productModel.selectedSubSubCatBrandName = _db.SubSubCategories.FirstOrDefault(x => x.SubSubCategoryID == selected_subBrand).SubSubCategoryName;
                productModel.proWarrentyDes = proWarrentyDes;
                productModel.proName = pname;
                productModel.proDesc = pDes;
                productModel.selectedCat = selected_cat;
                productModel.selectedCatName = _db.Categories.FirstOrDefault(x => x.Cat_ID == selected_cat).Cat_Name;
                productModel.selectedSubCatName = _db.SubSubCategories.FirstOrDefault(x => x.SubSubCategoryID == selected_subCat).SubSubCategoryName;
                productModel.selectedSubCat = selected_subCat;
                if (Session["sellerSession"] != null)
                {
                    int imgCount = 0;
                    foreach (HttpPostedFileBase item in image1)
                    {
                        imgCount += 1;
                        if (item != null)
                        {
                            var InputFileName = Path.GetFileName(item.FileName);
                            InputFileName = Guid.NewGuid().ToString();
                            imgname.Add(InputFileName);
                            var ServerSavePath = Path.Combine(Server.MapPath("../Product_Images"), InputFileName + ".png");
                            item.SaveAs(ServerSavePath);
                        }
                    }
                    if (imgCount > 5)
                    {
                        ViewBag.imagenullError = "You can upload maximum 5 Images of your Product!";
                        //return Json("You can upload maximum 5 Images of your Product!");
                        return View(productModel);
                    }
                    else if (imgCount == 0)
                    {
                        ViewBag.imagenullError = "Atleast One image is Required!";
                        return View(productModel);
                        //return Json("Atleast One image is Required!");
                    }
                    float discountAmount = 0;
                    float discountPercent = 0;
                    if (discountType == "1")
                    {
                        discountAmount = pDiscount;
                        discountPercent = 0;
                    }
                    else if (discountType == "2")
                    {
                        discountAmount = 0;
                        discountPercent = pDiscount;
                    }
                    else if (discountType == "3")
                    {
                        discountAmount = 0;
                        discountPercent = 0;
                    }
                    int s_id = ((SellerDto)Session["sellerSession"]).Id;/////seller id/////
                    SubCatBrand subCatBrand = _db.SubCatBrands.FirstOrDefault(p => p.BrandID == selected_brand && p.subSubCatId == selected_subBrand);
                    Product product = new Product()
                    {
                        Product_Name = pname,
                        Product_Description = pDes,
                        brandCategoryId = subCatBrand.Id,
                        Discount = discountPercent,
                        Date = DateTime.Parse(date),
                        quantity = qty,
                        DiscountInAmount = discountAmount
                    };
                    _db.Products.Add(product);
                    _db.SaveChanges();
                    int p_id = product.Product_ID;/////product id/////
                    SellerProduct sellerP = new SellerProduct()
                    { sellerId = s_id, productId = p_id };
                    _db.SellerProducts.Add(sellerP);
                    _db.SaveChanges();
                    //TODO: pick warrenty duration type by select from UI
                    if (warrentyDurationID == 3)
                    { proWarrentyDes = "Not Mentioned"; proWarrenty = "0"; }
                    ProductWarrenty productWarrenty = new ProductWarrenty()
                    { productId = p_id, warrentyCount = int.Parse(proWarrenty), warrentyDescription = proWarrentyDes, warrentyDuraionId = warrentyDurationID };
                    _db.ProductWarrenties.Add(productWarrenty);
                    _db.SaveChanges();
                    Product_Image product_Image = null;
                    if (imgname.Count == 1)
                    {
                        product_Image = new Product_Image()
                        {
                            Product_ID = p_id,
                            Image_1 = "../Product_Images/" + imgname[0] + ".png"
                        };
                    }
                    else if (imgname.Count == 2)
                    {
                        product_Image = new Product_Image()
                        {
                            Product_ID = p_id,
                            Image_1 = "../Product_Images/" + imgname[0] + ".png",
                            Image_2 = "../Product_Images/" + imgname[1] + ".png"
                        };
                    }
                    else if (imgname.Count == 3)
                    {
                        product_Image = new Product_Image()
                        {
                            Product_ID = p_id,
                            Image_1 = "../Product_Images/" + imgname[0] + ".png",
                            Image_2 = "../Product_Images/" + imgname[1] + ".png",
                            Image_3 = "../Product_Images/" + imgname[2] + ".png"
                        };
                    }
                    else if (imgname.Count == 4)
                    {
                        product_Image = new Product_Image()
                        {
                            Product_ID = p_id,
                            Image_1 = "../Product_Images/" + imgname[0] + ".png",
                            Image_2 = "../Product_Images/" + imgname[1] + ".png",
                            Image_3 = "../Product_Images/" + imgname[2] + ".png",
                            Image_4 = "../Product_Images/" + imgname[3] + ".png"
                        };
                    }
                    else if (imgname.Count == 5)
                    {
                        product_Image = new Product_Image()
                        {
                            Product_ID = p_id,
                            Image_1 = "../Product_Images/" + imgname[0] + ".png",
                            Image_2 = "../Product_Images/" + imgname[1] + ".png",
                            Image_3 = "../Product_Images/" + imgname[2] + ".png",
                            Image_4 = "../Product_Images/" + imgname[3] + ".png",
                            Image_5 = "../Product_Images/" + imgname[4] + ".png"
                        };
                    }
                    _db.Product_Image.Add(product_Image);
                    _db.SaveChanges();
                    ProductPrice productPrice = new ProductPrice()
                    { ProductID = p_id, StartDate = DateTime.Parse(date), Price = (float)pPrice };
                    _db.ProductPrices.Add(productPrice);
                    _db.SaveChanges();
                    for (int i = 0; i < featureName.Length; i++)
                    {
                        Product_Feature product_Feature = new Product_Feature()
                        {
                            product_id = p_id,
                            featureName = featureName[i],
                            featureDescription = featureDes[i]
                        };
                        _db.Product_Feature.Add(product_Feature);
                        _db.SaveChanges();
                    }
                    if (storage != null)
                    {
                        for (int i = 0; i < storage.Length; i++)
                        {
                            ProductStorage productStorage = new ProductStorage()
                            { productid = p_id, storageid = storage[i] };
                            _db.ProductStorages.Add(productStorage);
                            _db.SaveChanges();
                        }
                    }
                    if (color != null)
                    {
                        for (int i = 0; i < color.Length; i++)
                        {
                            ProductColor productColor = new ProductColor()
                            { productId = p_id, colorid = color[i] };
                            _db.ProductColors.Add(productColor);
                            _db.SaveChanges();
                        }
                    }
                    if (size != null)
                    {
                        for (int i = 0; i < size.Length; i++)
                        {
                            ProductSize productSize = new ProductSize()
                            { productId = p_id, sizeid = size[i] };
                            _db.ProductSizes.Add(productSize);
                            _db.SaveChanges();
                        }
                    }
                    ViewBag.successProductAdded = "Your Product Succesfully Added";
                    return View();
                }
                else
                {
                    return Redirect("~/Seller/Seller_Login");
                }
            }
            catch (Exception e)
            {
                return Redirect("~/Seller/ErrorPage");
            }
        }
        [TrackExecution]
        [HttpGet]
        public ActionResult Seller_SignUp()
        {
            try
            {
                var c_country = from Country in _db.Countries select Country;
                ViewBag.cc = c_country;
                return View();
            }
            catch (Exception e)
            {
                return Redirect("~/Seller/ErrorPage");
            }
        }
        [TrackExecution]
        [HttpPost]
        public ActionResult Seller_SignUp(string CashOnDelivery, string CreditCard, HttpPostedFileBase httpPostedFileBase, int selected_Area, string cnic, string shop_Address, string mobile_number, string pass, string email, string shop_Name)
        {
            try
            {
                var emailCheck = _db.Sellers.FirstOrDefault(x => x.Email == email);
                if (emailCheck != null)
                {
                    var c_country = from Country in _db.Countries select Country;
                    ViewBag.cc = c_country;
                    ViewBag.errorEmail = "Sorry the email you entered already exist";
                    return View();
                }
                List<int> vs = new List<int>();
                Seller seller = new Seller()
                { sellerName = shop_Name, Email = email, CNIC = cnic, Pass = pass };
                _db.Sellers.Add(seller);
                _db.SaveChanges();
                int a = seller.sellerId;
                Seller_Contact seller_Contact = new Seller_Contact()
                { Seller_ID = a, PhoneNo = mobile_number };
                _db.Seller_Contact.Add(seller_Contact);
                _db.SaveChanges();
                Seller_address seller_Address = new Seller_address()
                { area_id = selected_Area, seller_id = a, address = shop_Address };
                _db.Seller_address.Add(seller_Address);
                _db.SaveChanges();
                var InputFileName = Path.GetFileName(httpPostedFileBase.FileName);
                InputFileName = Guid.NewGuid().ToString();
                string _fname = InputFileName.ToString();
                var ServerSavePath = Path.Combine(Server.MapPath("../Seller_Image"), _fname + ".png");
                httpPostedFileBase.SaveAs(ServerSavePath);
                SellerImage sellerImage = new SellerImage()
                { sellerID = a, image = "../Seller_Image/" + _fname + ".png" };
                _db.SellerImages.Add(sellerImage);
                _db.SaveChanges();
                if (CashOnDelivery != null)
                { vs.Add(2); }
                if (CreditCard != null)
                { vs.Add(1); }
                for (int i = 0; i < vs.Count; i++)
                {
                    SellerPaymentSubscription sellerPaymentSubscription = new SellerPaymentSubscription()
                    { sellerId = a, paymentTypeId = vs[i] };
                    _db.SellerPaymentSubscriptions.Add(sellerPaymentSubscription);
                    _db.SaveChanges();
                }
                TempData["SellerSignUpSuccess"] = "Your Seller Account has been Created Successfuly";
                return Redirect("~/Seller/Seller_Panel");
            }
            catch (Exception e)
            {
                return Redirect("~/Seller/ErrorPage");
            }
        }
        [TrackExecution]
        [HttpGet]
        public ActionResult Seller_Login()
        {
            try
            {
                return View();
            }
            catch (Exception)
            {
                return Redirect("~/Seller/ErrorPage");
            }
        }
        [TrackExecution]
        [HttpPost]
        public ActionResult Seller_Login(string email, string password)
        {
            try
            {
                var sellerDetails = (from a in _db.Sellers
                                     where a.Email == email && a.Pass == password
                                     select new SellerDto { Id = a.sellerId, Name = a.sellerName }).FirstOrDefault();
                if (sellerDetails != null)
                {
                    Session["sellerSession"] = sellerDetails;
                    return Redirect("~/Seller/Seller_Panel");
                }
                else
                {
                    ViewBag.errorLogin = "Invalid Email or Password";
                    return View();
                }
            }
            catch (Exception e)
            {
                return Redirect("~/Seller/ErrorPage");
            }
        }
        [TrackExecution]
        [HttpGet]
        public ActionResult Logout()
        {
            try
            {


                Session["sellerSession"] = null;
                Session.Abandon();
                Session.Remove("sellerSession");
                Session.Clear();

                return Redirect("~/Seller/Seller_Login");
            }
            catch (Exception)
            {
                return Redirect("~/Seller/ErrorPage");
            }
        }
        ///////////Country State City Area////////////
        [TrackExecution]
        public ActionResult loadstates(int countryId)
        {
            try
            {

            }
            catch (Exception)
            {
                return Redirect("~/Seller/ErrorPage");
            }
            return Json(_db.States.Where(s => s.Country_ID == countryId).Select(s => new
            {
                SID = s.State_ID,
                Name = s.State_Name
            }).ToList());
        }
        [TrackExecution]
        public ActionResult loadcity(int stateId)
        {
            try
            {
                return Json(_db.Cities.Where(s => s.State_ID == stateId).Select(s => new
                {
                    CID = s.City_ID,
                    cName = s.City_Name
                }).ToList());
            }
            catch (Exception)
            {
                return Redirect("~/Seller/ErrorPage");
            }
        }
        [TrackExecution]
        public ActionResult loadarea(int cityId)
        {
            try
            {
                return Json(_db.Areas.Where(s => s.City_ID == cityId).Select(s => new
                {
                    AID = s.City_ID,
                    aName = s.Area_Name
                }).ToList());
            }
            catch (Exception)
            {
                return Redirect("~/Seller/ErrorPage");
            }
        }
        ///////////Cat SubCat SubCatORBrand////////////
        [TrackExecution]
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

                return Redirect("~/Seller/ErrorPage");
            }
        }
        [TrackExecution]
        public ActionResult LoadSubCatBrand(int subcatId)
        {
            try
            {
                return Json(_db.SubSubCategories.Where(s => s.Sub_Category_ID == subcatId).Select(s => new
                {
                    CID = s.SubSubCategoryID,
                    cName = s.SubSubCategoryName
                }).ToList());
            }
            catch (Exception)
            {
                return Redirect("~/Seller/ErrorPage");
            }
        }
        [TrackExecution]
        public ActionResult LoadBrands(int subsubcatId)
        {
            try
            {
                return Json((from scb in _db.SubCatBrands
                             join b in _db.Brands on scb.BrandID equals b.BrandId
                             where scb.subSubCatId == subsubcatId
                             select new
                             {
                                 b_id = b.BrandId,
                                 b_name = b.Brand_Name
                             }).ToList());
            }
            catch (Exception)
            {
                return Redirect("~/Seller/ErrorPage");
            }
        }

        [TrackExecution]
        [HttpGet]
        public ActionResult SelfAdvertisement()
        {
            int s_id = ((SellerDto)Session["sellerSession"]).Id;
            var sellerProducts = (from s in _db.Sellers
                                  join sp in _db.SellerProducts
                                  on s.sellerId equals sp.sellerId
                                  join p in _db.Products
                                  on sp.productId equals p.Product_ID
                                  where s.sellerId == s_id
                                  select new SellerProductAdvertisementDto
                                  {
                                      ProId = p.Product_ID,
                                      PName = p.Product_Name
                                  }).ToList();
            ViewBag.sp = sellerProducts;
            return View();
        }
        [HttpPost]
        public ActionResult SelfAdvertisement(string self, string product, int[] pro)
        {
            //List<int> vs = new List<int>();
            //int selfAdvertisementID;
            //int productAvertisementID;
            //if (self != null)
            //{
            //    selfAdvertisementID = 911740;
            //    Advertisement advertisement = new Advertisement()
            //    {
            //        AdvertisementTypeID = selfAdvertisementID,
            //        AdvertisementRelationID = ((SellerDto)Session["sellerSession"]).Id
            //    };
            //    _db.Advertisements.Add(advertisement);
            //    _db.SaveChanges();
            //}
            //if (product != null)
            //{
            //    productAvertisementID = 911741;
            //    foreach (var item in pro)
            //    {
            //        Advertisement advertisement = new Advertisement()
            //        {
            //            AdvertisementTypeID = productAvertisementID,
            //            AdvertisementRelationID = item
            //        };
            //        _db.Advertisements.Add(advertisement);
            //        _db.SaveChanges();
            //    }
            //}

            return View();
        }

        [TrackExecution]
        [HttpGet]
        public ActionResult OrderConfirmation(int confirm)
        {
            try
            {
                var sol = _db.SalesOrderLines.Where(x => x.id == confirm).SingleOrDefault();
                sol.isConfirmed = 1;
                _db.Entry(sol).State = System.Data.Entity.EntityState.Modified;
                _db.SaveChanges();
                return Redirect("~/Seller/Seller_Order");
            }
            catch (Exception)
            {
                return Redirect("~/Seller/ErrorPage");
            }
        }
        public static int ProductID = 0;
        [TrackExecution]
        [HttpGet]
        public ActionResult EditProduct(int productID)
        {
            try
            {
                ProductID = productID;
                var ma = (from dd in _db.ProductPrices
                          where dd.ProductID == productID
                          select dd.StartDate).Max();
                var Max = (from d in _db.ProductPrices
                           where d.ProductID == productID && d.StartDate == ma
                           select new ProductPriceUpdateDto
                           {
                               StartDate = d.StartDate,
                               Price = (float)d.Price
                           }).FirstOrDefault();
                var res = (from p in _db.Products
                           join pp in _db.ProductPrices
                           on p.Product_ID equals pp.ProductID
                           where p.Product_ID == productID
                           select new ProductPriceUpdateDto
                           {
                               Product_ID = p.Product_ID,
                               Product_Name = p.Product_Name,
                               Product_Description = p.Product_Description,
                               Discount = (float)p.Discount,
                               quantity = p.quantity,
                               Price = Max.Price,
                               StartDate = Max.StartDate
                           }).FirstOrDefault();

                return View(res);
            }
            catch (Exception e)
            {
                return Redirect("~/Seller/ErrorPage");
            }
        }
        [TrackExecution]
        [HttpPost]
        public ActionResult EditProduct(string pDes, float pPrice, string pDiscount, string date, int qty)
        {
            try
            {
                var entity = _db.Products.FirstOrDefault(p => p.Product_ID == ProductID);
                entity.Product_Description = pDes;
                entity.Discount = float.Parse(pDiscount);
                entity.quantity = int.Parse(qty.ToString());
                _db.Entry(entity).State = System.Data.Entity.EntityState.Modified;
                _db.SaveChanges();
                var entity1 = _db.ProductPrices.FirstOrDefault(p => p.ProductID == ProductID && p.StartDate == (from dd in _db.ProductPrices
                                                                                                                where dd.ProductID == ProductID
                                                                                                                select dd.StartDate).Max());
                entity1.EndDate = DateTime.Parse(date).AddDays(-1);
                _db.Entry(entity1).State = System.Data.Entity.EntityState.Modified;
                _db.SaveChanges();
                ProductPrice pro = new ProductPrice
                {
                    Price = float.Parse(pPrice.ToString()),
                    StartDate = DateTime.Parse(date.ToString()),
                    ProductID = ProductID,
                };
                _db.ProductPrices.Add(pro);
                _db.SaveChanges();
                return RedirectToAction("EditProduct", new { productID = ProductID });
            }
            catch (Exception e)
            {
                return Redirect("~/Seller/ErrorPage");
            }
        }
        [TrackExecution]
        public void VisitedNotifucation(int S_id)
        {
            try
            {
                var userNotifications = _db.Notifications.Where(p => p.SellerId == S_id).ToList();

                foreach (var item in userNotifications)
                {
                    item.Status = "read";
                    _db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    _db.SaveChanges();
                }
            }
            catch (Exception)
            {
                Console.WriteLine();
            }
        }
        static List<DataPoint> dataPoints = null;
        [TrackExecution]
        [HttpGet]
        public ActionResult Analytics()
        {
            try
            {
                if (Session["sellerSession"] != null)
                {
                    int s_id = ((SellerDto)Session["sellerSession"]).Id;
                    AnalyticsAdminSeller mostViewProducts = new AnalyticsAdminSeller();
                    dataPoints = new List<DataPoint>();
                    var a = mostViewProducts.mostBoughtItem(s_id);
                    for (int i = 0; i < a.Count; i++)
                    {
                        dataPoints.Add(new DataPoint { X = a.ElementAt(i).Key, Y = a.ElementAt(i).Value });
                    }
                    JsonSerializerSettings _jsonSetting = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };
                    var json = JsonConvert.SerializeObject(dataPoints, _jsonSetting);
                    ViewBag.DataPoints = json;

                    dataPoints = new List<DataPoint>();
                    var b = mostViewProducts.mostViewedProductBasedTime(s_id);
                    for (int i = 0; i < b.Count; i++)
                    {
                        dataPoints.Add(new DataPoint { X1 = b.ElementAt(i).Key, Y1 = b.ElementAt(i).Value });
                    }
                    _jsonSetting = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };
                    json = JsonConvert.SerializeObject(dataPoints, _jsonSetting);
                    ViewBag.DataPoints1 = json;

                    dataPoints = new List<DataPoint>();
                    var c = mostViewProducts.mostViewedProductBasedClick(s_id);
                    for (int i = 0; i < c.Count; i++)
                    {
                        dataPoints.Add(new DataPoint { X2 = c.ElementAt(i).Key, Y2 = c.ElementAt(i).Value });
                    }
                    _jsonSetting = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };
                    json = JsonConvert.SerializeObject(dataPoints, _jsonSetting);
                    ViewBag.DataPoints2 = json;

                    dataPoints = new List<DataPoint>();
                    var d = mostViewProducts.mostRatedItem(s_id);
                    for (int i = 0; i < d.Count; i++)
                    {
                        dataPoints.Add(new DataPoint { X3 = d.ElementAt(i).Key, Y3 = d.ElementAt(i).Value });
                    }
                    _jsonSetting = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };
                    json = JsonConvert.SerializeObject(dataPoints, _jsonSetting);
                    ViewBag.DataPoints3 = json;

                    dataPoints = new List<DataPoint>();
                    var e = mostViewProducts.nOOrders(s_id);
                    for (int i = 0; i < e.Count; i++)
                    {
                        dataPoints.Add(new DataPoint { X7 = e.ElementAt(i).Key, Y7 = e.ElementAt(i).Value });
                    }
                    _jsonSetting = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };
                    json = JsonConvert.SerializeObject(dataPoints, _jsonSetting);
                    ViewBag.DataPoints4 = json;

                    dataPoints = new List<DataPoint>();
                    var f = mostViewProducts.ordersPerDay(s_id);
                    for (int i = 0; i < f.Count; i++)
                    {
                        dataPoints.Add(new DataPoint { X11 = f.ElementAt(i).Key, Y11 = f.ElementAt(i).Value });
                    }
                    _jsonSetting = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };
                    json = JsonConvert.SerializeObject(dataPoints, _jsonSetting);
                    ViewBag.DataPoints5 = json;

                    ///////////////
                    dataPoints = new List<DataPoint>();
                    var ij = mostViewProducts.earning(s_id);
                    for (int i = 0; i < ij.Count; i++)
                    {
                        dataPoints.Add(new DataPoint { X13 = ij.ElementAt(i).Key, Y13 = ij.ElementAt(i).Value });
                    }
                    _jsonSetting = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };
                    json = JsonConvert.SerializeObject(dataPoints, _jsonSetting);
                    ViewBag.ij = json;
                    ///////////////
                    ViewBag.co = mostViewProducts.cancelOrder(s_id);

                    return View();
                }
                else
                {
                    return Redirect("~/Seller/Seller_Login");
                }
            }
            catch (Exception)
            {
                return Redirect("~/Seller/ErrorPage");
            }
        }
        [TrackExecution]
        public ActionResult ErrorPage()
        {
            return View();
        }
    }
}