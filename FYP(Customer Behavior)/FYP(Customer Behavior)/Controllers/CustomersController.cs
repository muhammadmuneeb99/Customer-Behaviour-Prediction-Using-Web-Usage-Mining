using FYP_Customer_Behavior_.Common;
using FYP_Customer_Behavior_.Models;
using FYP_Customer_Behavior_.Models.Dtos;
using FYP_Customer_Behavior_.Models.DtosForMining;
using FYP_Customer_Behavior_.Models.ViewModels;
using FYP_Customer_Behavior_.RuleAssociationMining_FP_Growth_;
using MVCEncrypt;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace FYP_Customer_Behavior_.Controllers
{
    public class CustomersController : Controller
    {
        FYPEntities _db = new FYPEntities();
        DayNightAndTimeBasedRecommendation dayNightBasedRecommendation = new DayNightAndTimeBasedRecommendation();
        DayBasedRecommendation dayBasedRecommendation = new DayBasedRecommendation();
        SementicAnalysisBasedRecommendation sementicAnalysisBasedRecommendation = new SementicAnalysisBasedRecommendation();
        List<int> Product_Price_Image = new List<int>();
        HighestDiscountBasedRecommendation highestDiscountBased = new HighestDiscountBasedRecommendation();
        LowestPriceBasedRecommendation lowestPriceBasedRecommendation = new LowestPriceBasedRecommendation();
        OrderHistoryBasedRecommendation orderHistoryBasedRecommendation = new OrderHistoryBasedRecommendation();
        public List<int> timeSpendProductIds = new List<int>();
        public List<int> productPriceImageDtos = new List<int>();
        List<int> listOfRecommendedProductIds = new List<int>();
        public IndexViews indexViews = new IndexViews();

        [OutputCache(Duration = 50)]
        [TrackExecution]
        [ChildActionOnly]
        public void getFpGrowth(string token)
        {
            Result result1 = new Result();
            List<int> fpid = null;
            if (Session["customerSession"] != null)
            {
                int cid = int.Parse(token);
                fpid = result1.result(cid);
                productPriceImageDtos = fpid;
            }
            else
            {
                fpid = result1.resultForGuest(token);
                productPriceImageDtos = fpid;
            }
        }
        [OutputCache(NoStore = true, Duration = 10)]
        // GET: Customer
        [TrackExecution]
        [HttpGet]
        public ActionResult Index()
        {
            try
            {
                var sellerImg = _db.SellerImages;
                ViewBag.sImg = sellerImg;
                Product_Price_Image = _db.Products.Select(x => x.Product_ID).ToList();
                var CatName_Image = (from c in _db.Categories
                                     join ci in _db.CategoryImages on c.Cat_ID equals ci.CatId
                                     select new CatNameImage
                                     {
                                         id = c.Cat_ID,
                                         name = c.Cat_Name,
                                         Image1 = ci.Images
                                     }).ToList();
                indexViews.catNameImages = CatName_Image;
                //ViewBag.psci = CatName_Image;
                if (Session["customerSession"] == null)
                {
                    /////Without Login
                    HttpCookie cookie = Request.Cookies["SID"];
                    string token = string.Empty;
                    if (cookie == null)
                    {
                        Response.Cookies["SID"].Values["x"] = Session.SessionID;
                        Response.Cookies["SID"].Expires = DateTime.MaxValue;
                    }
                    else
                    {
                        Session.Add("cookieId", cookie.Values["x"].ToString());
                        token = cookie.Values["x"].ToString();
                    }

                    if (token == string.Empty)
                        token = Response.Cookies["SID"].Values["x"].ToString();
                    Thread thread = new Thread(() => getFpGrowth(token));
                    thread.Start();

                    TempData["ID"] = token;
                    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    var tsbr = dayNightBasedRecommendation.timeSpendBasedRecommendation(TempData["ID"].ToString()).ToList();
                    if (tsbr.Count != 0)
                    {
                        timeSpendProductIds = tsbr;
                        ViewBag.TimeSpendProduct = (from p in tsbr
                                                    join p2 in _db.Product_Image
                                                      on p equals p2.Product_ID
                                                    join pp in _db.ProductPrices on p equals pp.ProductID
                                                    where pp.EndDate == null
                                                    select new ProductPriceImageDto
                                                    {
                                                        id = p,
                                                        name = _db.Products.Find(p).Product_Name,
                                                        newPrice = (float)(((pp.Price * 1) - (float)_db.Products.Find(p).DiscountInAmount) - (((float)_db.Products.Find(p).Discount / 100) * (pp.Price * 1))),
                                                        Image1 = p2.Image_1,
                                                        Image2 = p2.Image_2,
                                                        Image3 = p2.Image_3,
                                                        Image4 = p2.Image_4,
                                                        Image5 = p2.Image_5,
                                                        dAmount = (float)_db.Products.Find(p).DiscountInAmount,
                                                        dPercent = (float)_db.Products.Find(p).Discount,
                                                        price = (float)pp.Price,
                                                        rating = ((float)(_db.RatingsAndReviews.Where(x => x.rating == 0 && x.productId == p).Count()) + (float)(_db.RatingsAndReviews.Where(x => x.rating == 1 && x.productId == p).Count()) + (float)(_db.RatingsAndReviews.Where(x => x.rating == 2 && x.productId == p).Count()) + (float)(_db.RatingsAndReviews.Where(x => x.rating == 3 && x.productId == p).Count()) + (float)(_db.RatingsAndReviews.Where(x => x.rating == 4 && x.productId == p).Count()) + (float)(_db.RatingsAndReviews.Where(x => x.rating == 5 && x.productId == p).Count())) / (float)5
                                                    }).ToList();

                    }//TimeSpend
                    var rftsp = dayNightBasedRecommendation.RecommendFromTimeSpendProducts();
                    if (rftsp.Count != 0)
                    {
                        for (int i = 0; i < rftsp.Count; i++)
                        {
                            listOfRecommendedProductIds.Add(rftsp[i]);
                        }
                    }//RecommendFromTimeSpendProducts
                    var danb = dayNightBasedRecommendation.dayAndnightBased(DateTime.Now.ToString()).ToList();
                    if (danb.Count != 0)
                    {
                        for (int i = 0; i < danb.Count; i++)
                        {
                            listOfRecommendedProductIds.Add(danb[i]);
                        }
                    }//DayNight
                    dayNightBasedRecommendation._BrandsFromTimeSpendProducts();
                    dayNightBasedRecommendation._BrandsFromDayAndNight();
                    dayNightBasedRecommendation._Brands();
                    var brands = dayNightBasedRecommendation.brandsResult();
                    if (brands.Count == 6)
                    {
                        indexViews.brandsDtos = brands;
                    }

                    ///fpGrowth
                    if (productPriceImageDtos.Count != 0)
                    {
                        for (int i = 0; i < productPriceImageDtos.Count; i++)
                        {
                            listOfRecommendedProductIds.Add(productPriceImageDtos[i]);
                        }
                    }
                    ///fpGrowth
                    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    //OrderHistoryBasedRecommendations => 2 Recommendation
                    var ohbr = orderHistoryBasedRecommendation.orderHistoryBased(token);
                    if (ohbr != null)
                    {
                        for (int i = 0; i < ohbr.Count; i++)
                        {
                            listOfRecommendedProductIds.Add(ohbr[i]);
                        }
                    }
                    var ohbr2 = orderHistoryBasedRecommendation.mostItemBought(token);
                    if (ohbr2 != null)
                    {
                        for (int i = 0; i < ohbr2.Count; i++)
                        {
                            listOfRecommendedProductIds.Add(ohbr2[i]);
                        }
                    }
                    ///////////////////////
                    var rbr = sementicAnalysisBasedRecommendation.reviewsBasedRecommendation().ToList();
                    if (rbr.Count != 0)
                    {
                        for (int i = 0; i < rbr.Count; i++)
                        {
                            listOfRecommendedProductIds.Add(rbr[i]);
                        }
                    }//SementicAnalysis
                     ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    var dbr = dayBasedRecommendation.dayBasedRecommendation(DateTime.Now.DayOfWeek.ToString(), token.ToString()).ToList();
                    if (dbr.Count != 0)
                    {
                        for (int i = 0; i < dbr.Count; i++)
                        {
                            listOfRecommendedProductIds.Add(dbr[i]);
                        }
                    }//DayBasedRecommendation
                     ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    var discountRec = highestDiscountBased.highestDiscountBasedRecommendations();
                    if (discountRec.Count != 0)
                    {
                        for (int i = 0; i < discountRec.Count; i++)
                        {
                            listOfRecommendedProductIds.Add(discountRec[i]);
                        }
                    }//Highest Discount Based Recommendation
                     ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    var lowestPriceids = lowestPriceBasedRecommendation.lowestPriceBasedRecommendations();
                    if (lowestPriceids.Count != 0)
                    {
                        for (int i = 0; i < lowestPriceids.Count; i++)
                        {
                            listOfRecommendedProductIds.Add(lowestPriceids[i]);
                        }
                    }//LowestPriceOfProductRecommendationDone
                     ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    listOfRecommendedProductIds = listOfRecommendedProductIds.Distinct().ToList();
                    Product_Price_Image = Product_Price_Image.Distinct().ToList();
                    for (int i = 0; i < listOfRecommendedProductIds.Count; i++)
                    {
                        //Product_Price_Image.RemoveAll(x => x.id == multiR[i].id);
                        for (int j = 0; j < Product_Price_Image.Count; j++)
                        {
                            if (listOfRecommendedProductIds[i] == Product_Price_Image[j])
                            {
                                Product_Price_Image.RemoveAt(j);
                            }
                        }
                    }
                    for (int i = 0; i < timeSpendProductIds.Count; i++)
                    {
                        //Product_Price_Image.RemoveAll(x => x.id == multiR[i].id);
                        for (int j = 0; j < Product_Price_Image.Count; j++)
                        {
                            if (timeSpendProductIds[i] == Product_Price_Image[j])
                            {
                                Product_Price_Image.RemoveAt(j);
                            }
                        }
                    }
                    for (int i = 0; i < timeSpendProductIds.Count; i++)
                    {
                        //Product_Price_Image.RemoveAll(x => x.id == multiR[i].id);
                        for (int j = 0; j < listOfRecommendedProductIds.Count; j++)
                        {
                            if (timeSpendProductIds[i] == listOfRecommendedProductIds[j])
                            {
                                listOfRecommendedProductIds.RemoveAt(j);
                            }
                        }
                    }

                    indexViews.productPriceImageDtoMultiR = (from p in listOfRecommendedProductIds
                                                             join p2 in _db.Product_Image
                                                               on p equals p2.Product_ID
                                                             join pp in _db.ProductPrices on p equals pp.ProductID
                                                             where pp.EndDate == null
                                                             select new ProductPriceImageDto
                                                             {
                                                                 id = p,
                                                                 name = _db.Products.Find(p).Product_Name,
                                                                 newPrice = (float)(((pp.Price * 1) - (float)_db.Products.Find(p).DiscountInAmount) - (((float)_db.Products.Find(p).Discount / 100) * (pp.Price * 1))),
                                                                 Image1 = p2.Image_1,
                                                                 Image2 = p2.Image_2,
                                                                 Image3 = p2.Image_3,
                                                                 Image4 = p2.Image_4,
                                                                 Image5 = p2.Image_5,
                                                                 dAmount = (float)_db.Products.Find(p).DiscountInAmount,
                                                                 dPercent = (float)_db.Products.Find(p).Discount,
                                                                 price = (float)pp.Price,
                                                                 rating = ((float)(_db.RatingsAndReviews.Where(x => x.rating == 0 && x.productId == p).Count()) + (float)(_db.RatingsAndReviews.Where(x => x.rating == 1 && x.productId == p).Count()) + (float)(_db.RatingsAndReviews.Where(x => x.rating == 2 && x.productId == p).Count()) + (float)(_db.RatingsAndReviews.Where(x => x.rating == 3 && x.productId == p).Count()) + (float)(_db.RatingsAndReviews.Where(x => x.rating == 4 && x.productId == p).Count()) + (float)(_db.RatingsAndReviews.Where(x => x.rating == 5 && x.productId == p).Count())) / (float)5
                                                             }).ToList();
                    ////////////////////////////////////////////////////////
                    indexViews.productPriceImageDto = (from p in Product_Price_Image
                                                       join p2 in _db.Product_Image
                                                         on p equals p2.Product_ID
                                                       join pp in _db.ProductPrices on p equals pp.ProductID
                                                       where pp.EndDate == null
                                                       select new ProductPriceImageDto
                                                       {
                                                           id = p,
                                                           name = _db.Products.Find(p).Product_Name,
                                                           newPrice = (float)(((pp.Price * 1) - (float)_db.Products.Find(p).DiscountInAmount) - (((float)_db.Products.Find(p).Discount / 100) * (pp.Price * 1))),
                                                           Image1 = p2.Image_1,
                                                           Image2 = p2.Image_2,
                                                           Image3 = p2.Image_3,
                                                           Image4 = p2.Image_4,
                                                           Image5 = p2.Image_5,
                                                           dAmount = (float)_db.Products.Find(p).DiscountInAmount,
                                                           dPercent = (float)_db.Products.Find(p).Discount,
                                                           price = (float)pp.Price,
                                                           rating = ((float)(_db.RatingsAndReviews.Where(x => x.rating == 0 && x.productId == p).Count()) + (float)(_db.RatingsAndReviews.Where(x => x.rating == 1 && x.productId == p).Count()) + (float)(_db.RatingsAndReviews.Where(x => x.rating == 2 && x.productId == p).Count()) + (float)(_db.RatingsAndReviews.Where(x => x.rating == 3 && x.productId == p).Count()) + (float)(_db.RatingsAndReviews.Where(x => x.rating == 4 && x.productId == p).Count()) + (float)(_db.RatingsAndReviews.Where(x => x.rating == 5 && x.productId == p).Count())) / (float)5
                                                       }).ToList();
                    ////////////////////////////////////////////////////////
                }
                else
                { //With Login
                    int userId = ((CustomerDto)Session["customerSession"]).Id;
                    TempData["ID"] = userId.ToString();
                    Thread thread = new Thread(() => getFpGrowth(userId.ToString()));
                    thread.Start();
                    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    var tsbr = dayNightBasedRecommendation.timeSpendBasedRecommendation(TempData["ID"].ToString()).ToList();
                    if (tsbr.Count != 0)
                    {
                        timeSpendProductIds = tsbr;
                        ViewBag.TimeSpendProduct = (from p in tsbr
                                                    join p2 in _db.Product_Image
                                                      on p equals p2.Product_ID
                                                    join pp in _db.ProductPrices on p equals pp.ProductID
                                                    where pp.EndDate == null
                                                    select new ProductPriceImageDto
                                                    {
                                                        id = p,
                                                        name = _db.Products.Find(p).Product_Name,
                                                        newPrice = (float)(((pp.Price * 1) - (float)_db.Products.Find(p).DiscountInAmount) - (((float)_db.Products.Find(p).Discount / 100) * (pp.Price * 1))),
                                                        Image1 = p2.Image_1,
                                                        Image2 = p2.Image_2,
                                                        Image3 = p2.Image_3,
                                                        Image4 = p2.Image_4,
                                                        Image5 = p2.Image_5,
                                                        dAmount = (float)_db.Products.Find(p).DiscountInAmount,
                                                        dPercent = (float)_db.Products.Find(p).Discount,
                                                        price = (float)pp.Price,
                                                        rating = ((float)(_db.RatingsAndReviews.Where(x => x.rating == 0 && x.productId == p).Count()) + (float)(_db.RatingsAndReviews.Where(x => x.rating == 1 && x.productId == p).Count()) + (float)(_db.RatingsAndReviews.Where(x => x.rating == 2 && x.productId == p).Count()) + (float)(_db.RatingsAndReviews.Where(x => x.rating == 3 && x.productId == p).Count()) + (float)(_db.RatingsAndReviews.Where(x => x.rating == 4 && x.productId == p).Count()) + (float)(_db.RatingsAndReviews.Where(x => x.rating == 5 && x.productId == p).Count())) / (float)5
                                                    }).ToList();
                    }//TimeSpend
                    var rftsp = dayNightBasedRecommendation.RecommendFromTimeSpendProducts();
                    if (rftsp.Count != 0)
                    {
                        for (int i = 0; i < rftsp.Count; i++)
                        {
                            listOfRecommendedProductIds.Add(rftsp[i]);
                        }
                    }//RecommendFromTimeSpendProducts
                    var danb = dayNightBasedRecommendation.dayAndnightBased(DateTime.Now.ToString()).ToList();
                    if (danb.Count != 0)
                    {
                        for (int i = 0; i < danb.Count; i++)
                        {
                            listOfRecommendedProductIds.Add(danb[i]);
                        }
                    }//DayNight
                    dayNightBasedRecommendation._BrandsFromTimeSpendProducts();
                    dayNightBasedRecommendation._BrandsFromDayAndNight();
                    dayNightBasedRecommendation._Brands();
                    var brands = dayNightBasedRecommendation.brandsResult();
                    if (brands.Count() == 6)
                    {
                        indexViews.brandsDtos = brands;
                    }
                    ///fpGrowth
                    if (productPriceImageDtos.Count != 0)
                    {
                        for (int i = 0; i < productPriceImageDtos.Count; i++)
                        {
                            listOfRecommendedProductIds.Add(productPriceImageDtos[i]);
                        }
                    }
                    ///fpGrowth
                    //OrderHistoryBasedRecommendations => 2 Recommendation
                    var ohbr = orderHistoryBasedRecommendation.orderHistoryBased(userId);
                    if (ohbr != null)
                    {
                        for (int i = 0; i < ohbr.Count; i++)
                        {
                            listOfRecommendedProductIds.Add(ohbr[i]);
                        }
                    }
                    var ohbr2 = orderHistoryBasedRecommendation.mostItemBought(userId);
                    if (ohbr2 != null)
                    {
                        for (int i = 0; i < ohbr2.Count; i++)
                        {
                            listOfRecommendedProductIds.Add(ohbr2[i]);
                        }
                    }
                    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    var rbr = sementicAnalysisBasedRecommendation.reviewsBasedRecommendation().ToList();
                    if (rbr.Count != 0)
                    {
                        for (int i = 0; i < rbr.Count; i++)
                        {
                            listOfRecommendedProductIds.Add(rbr[i]);
                        }
                    }//SementicAnalysis
                    var discountRec = highestDiscountBased.highestDiscountBasedRecommendations();
                    if (discountRec.Count != 0)
                    {
                        for (int i = 0; i < discountRec.Count; i++)
                        {
                            listOfRecommendedProductIds.Add(discountRec[i]);
                        }
                    }//Highest Discount Based Recommendation
                     ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    var lowestPriceids = lowestPriceBasedRecommendation.lowestPriceBasedRecommendations();
                    if (lowestPriceids.Count != 0)
                    {
                        for (int i = 0; i < lowestPriceids.Count; i++)
                        {
                            listOfRecommendedProductIds.Add(lowestPriceids[i]);
                        }
                    }//LowestPriceOfProductRecommendationDone
                     ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    var dbr = dayBasedRecommendation.dayBasedRecommendation(DateTime.Now.DayOfWeek.ToString(), userId.ToString()).ToList();
                    if (dbr.Count != 0)
                    {
                        for (int i = 0; i < dbr.Count; i++)
                        {
                            listOfRecommendedProductIds.Add(dbr[i]);
                        }
                    }//DayBasedRecommendation

                    //indexViews.productPriceImageDtoMultiR = multiR;
                    listOfRecommendedProductIds = listOfRecommendedProductIds.Distinct().ToList();
                    Product_Price_Image = Product_Price_Image.Distinct().ToList();

                    for (int i = 0; i < listOfRecommendedProductIds.Count; i++)
                    {
                        for (int j = 0; j < Product_Price_Image.Count; j++)
                        {
                            if (listOfRecommendedProductIds[i] == Product_Price_Image[j])
                            {
                                Product_Price_Image.RemoveAt(j);
                            }
                        }
                    }
                    for (int i = 0; i < timeSpendProductIds.Count; i++)
                    {
                        for (int j = 0; j < Product_Price_Image.Count; j++)
                        {
                            if (timeSpendProductIds[i] == Product_Price_Image[j])
                            {
                                Product_Price_Image.RemoveAt(j);
                            }
                        }
                    }
                    for (int i = 0; i < timeSpendProductIds.Count; i++)
                    {
                        for (int j = 0; j < listOfRecommendedProductIds.Count; j++)
                        {
                            if (timeSpendProductIds[i] == listOfRecommendedProductIds[j])
                            {
                                listOfRecommendedProductIds.RemoveAt(j);
                            }
                        }
                    }

                    indexViews.productPriceImageDtoMultiR = (from p in listOfRecommendedProductIds
                                                             join p2 in _db.Product_Image
                                                               on p equals p2.Product_ID
                                                             join pp in _db.ProductPrices on p equals pp.ProductID
                                                             where pp.EndDate == null
                                                             select new ProductPriceImageDto
                                                             {
                                                                 id = p,
                                                                 name = _db.Products.Find(p).Product_Name,
                                                                 newPrice = (float)(((pp.Price * 1) - (float)_db.Products.Find(p).DiscountInAmount) - (((float)_db.Products.Find(p).Discount / 100) * (pp.Price * 1))),
                                                                 Image1 = p2.Image_1,
                                                                 Image2 = p2.Image_2,
                                                                 Image3 = p2.Image_3,
                                                                 Image4 = p2.Image_4,
                                                                 Image5 = p2.Image_5,
                                                                 dAmount = (float)_db.Products.Find(p).DiscountInAmount,
                                                                 dPercent = (float)_db.Products.Find(p).Discount,
                                                                 price = (float)pp.Price,
                                                                 rating = ((float)(_db.RatingsAndReviews.Where(x => x.rating == 0 && x.productId == p).Count()) + (float)(_db.RatingsAndReviews.Where(x => x.rating == 1 && x.productId == p).Count()) + (float)(_db.RatingsAndReviews.Where(x => x.rating == 2 && x.productId == p).Count()) + (float)(_db.RatingsAndReviews.Where(x => x.rating == 3 && x.productId == p).Count()) + (float)(_db.RatingsAndReviews.Where(x => x.rating == 4 && x.productId == p).Count()) + (float)(_db.RatingsAndReviews.Where(x => x.rating == 5 && x.productId == p).Count())) / (float)5
                                                             }).ToList();
                    //indexViews.productPriceImageDto = Product_Price_Image; 
                    indexViews.productPriceImageDto = (from p in Product_Price_Image
                                                       join p2 in _db.Product_Image
                                                         on p equals p2.Product_ID
                                                       join pp in _db.ProductPrices on p equals pp.ProductID
                                                       where pp.EndDate == null
                                                       select new ProductPriceImageDto
                                                       {
                                                           id = p,
                                                           name = _db.Products.Find(p).Product_Name,
                                                           newPrice = (float)(((pp.Price * 1) - (float)_db.Products.Find(p).DiscountInAmount) - (((float)_db.Products.Find(p).Discount / 100) * (pp.Price * 1))),
                                                           Image1 = p2.Image_1,
                                                           Image2 = p2.Image_2,
                                                           Image3 = p2.Image_3,
                                                           Image4 = p2.Image_4,
                                                           Image5 = p2.Image_5,
                                                           dAmount = (float)_db.Products.Find(p).DiscountInAmount,
                                                           dPercent = (float)_db.Products.Find(p).Discount,
                                                           price = (float)pp.Price,
                                                           rating = ((float)(_db.RatingsAndReviews.Where(x => x.rating == 0 && x.productId == p).Count()) + (float)(_db.RatingsAndReviews.Where(x => x.rating == 1 && x.productId == p).Count()) + (float)(_db.RatingsAndReviews.Where(x => x.rating == 2 && x.productId == p).Count()) + (float)(_db.RatingsAndReviews.Where(x => x.rating == 3 && x.productId == p).Count()) + (float)(_db.RatingsAndReviews.Where(x => x.rating == 4 && x.productId == p).Count()) + (float)(_db.RatingsAndReviews.Where(x => x.rating == 5 && x.productId == p).Count())) / (float)5
                                                       }).ToList();

                    ////////////////////////////////////////////////////////
                }
                return View(indexViews);
            }
            catch (Exception e)
            {
                //throw e;
                return Redirect("~/Customers/ErrorPage");
            }
        }

        [MVCDecryptFilter(secret = "mySecret")]
        [TrackExecution]
        [HttpGet]
        public async Task<ActionResult> CatSubCat(int catid)
        {
            try
            {
                if (Session["customerSession"] == null)
                {
                    //Without Login
                    HttpCookie cookie = Request.Cookies["SID"];
                    string token = string.Empty;
                    //If Cookie is null then create new Cookie
                    if (cookie == null)
                    {
                        Response.Cookies["SID"].Values["x"] = Session.SessionID;
                        Response.Cookies["SID"].Expires = DateTime.MaxValue;
                    }
                    else
                    {
                        Session.Add("cookieId", cookie.Values["x"].ToString());
                        token = cookie.Values["x"].ToString();
                    }
                    if (token == string.Empty)
                        token = Response.Cookies["SID"].Values["x"].ToString();
                    TempData["ID"] = token.ToString();
                    var csc_i = (from sc in _db.Sub_Category
                                 join sci in _db.SubCatImages on sc.Sub_Category_ID equals sci.SubCatId
                                 where sc.Cat_ID == catid
                                 select new subcatimage
                                 {
                                     id = sc.Sub_Category_ID,
                                     name = sc.Sub_Name,
                                     image = sci.Images
                                 });
                    var aa = csc_i.Take(5);
                    ViewBag.subcat = aa;
                    ViewBag.csci = csc_i;
                    ViewBag.b = (from b in _db.Brands
                                 join scb in _db.SubCatBrands on b.BrandId equals scb.BrandID
                                 join ssc in _db.SubSubCategories on scb.subSubCatId equals ssc.SubSubCategoryID
                                 join sc in _db.Sub_Category on ssc.Sub_Category_ID equals sc.Sub_Category_ID
                                 join c in _db.Categories on sc.Cat_ID equals c.Cat_ID
                                 where c.Cat_ID == catid
                                 select new BrandDto { brandId = b.BrandId, brandName = b.Brand_Name }).Distinct().AsParallel().ToList();

                    ViewBag.CategoryID = catid;
                    ViewBag.catName = _db.Categories.Find(catid).Cat_Name;
                    ViewBag.catImage = _db.CategoryImages.Where(x => x.CatId == catid).FirstOrDefault().Images;
                    List<subcatimage> cscp = null;

                    cscp = (from sc in _db.Sub_Category
                            join ssc in _db.SubSubCategories
                            on sc.Sub_Category_ID equals ssc.Sub_Category_ID
                            join p in _db.Products
                            on ssc.SubSubCategoryID equals p.SubCatBrand.subSubCatId
                            join pi in _db.Product_Image
                            on p.Product_ID equals pi.Product_ID
                            join pp in _db.ProductPrices on p.Product_ID equals pp.ProductID
                            where sc.Cat_ID == catid && pp.EndDate == null
                            select new subcatimage
                            {
                                p_id = p.Product_ID,
                                p_name = p.Product_Name,
                                p_price = (float)pp.Price,
                                p_image = pi.Image_1,
                                qty = p.quantity,
                                colorid = (int?)_db.ProductColors.FirstOrDefault(x => x.productId == p.Product_ID).Color.colorId,
                                storageid = (int?)_db.ProductStorages.FirstOrDefault(x => x.productid == p.Product_ID).Storage.id,
                                sizeid = (int?)_db.ProductSizes.FirstOrDefault(x => x.productId == p.Product_ID).Size.sizeId,
                                wishlistId = (int?)_db.WishLists.FirstOrDefault(x => x.SellerProduct.productId == p.Product_ID && x.GuestId == token).id,
                                cartId = (int?)_db.Carts.FirstOrDefault(x => x.GuestId == token && x.productId == p.Product_ID).cartId,
                                newPrice = (float)(((pp.Price * 1) - (float)p.DiscountInAmount) - (((float)p.Discount / 100) * (pp.Price * 1))),
                                dInAmount = (float)p.DiscountInAmount,
                                dInPercent = (float)p.Discount,
                                rating = (((float)_db.RatingsAndReviews.Where(y => y.rating == 1).Count(x => x.productId == p.Product_ID) + (float)_db.RatingsAndReviews.Where(y => y.rating == 2).Count(x => x.productId == p.Product_ID) + (float)_db.RatingsAndReviews.Where(y => y.rating == 3).Count(x => x.productId == p.Product_ID) + (float)_db.RatingsAndReviews.Where(y => y.rating == 4).Count(x => x.productId == p.Product_ID) + (float)_db.RatingsAndReviews.Where(y => y.rating == 5).Count(x => x.productId == p.Product_ID)) / (float)5)
                            }).ToList();
                    if (cscp.Count == 0)
                    {
                        ViewBag.MaxPrice = 0;
                    }
                    else
                    {
                        ViewBag.MaxPrice = cscp.Max(x => x.newPrice);
                    }
                    return View(cscp);
                }
                else
                {
                    //WithLogin
                    int userId = ((CustomerDto)Session["customerSession"]).Id;
                    TempData["ID"] = ((CustomerDto)Session["customerSession"]).Id.ToString();
                    var csc_i = (from sc in _db.Sub_Category
                                 join sci in _db.SubCatImages on sc.Sub_Category_ID equals sci.SubCatId
                                 where sc.Cat_ID == catid
                                 select new subcatimage
                                 {
                                     id = sc.Sub_Category_ID,
                                     name = sc.Sub_Name,
                                     image = sci.Images
                                 });
                    var aa = csc_i.Take(5);
                    ViewBag.subcat = aa;
                    ViewBag.csci = csc_i;
                    ViewBag.b = (from b in _db.Brands
                                 join scb in _db.SubCatBrands on b.BrandId equals scb.BrandID
                                 join ssc in _db.SubSubCategories on scb.subSubCatId equals ssc.SubSubCategoryID
                                 join sc in _db.Sub_Category on ssc.Sub_Category_ID equals sc.Sub_Category_ID
                                 join c in _db.Categories on sc.Cat_ID equals c.Cat_ID
                                 where c.Cat_ID == catid
                                 select new BrandDto { brandId = b.BrandId, brandName = b.Brand_Name }).Distinct().AsParallel().ToList();
                    ViewBag.CategoryID = catid;
                    ViewBag.catName = _db.Categories.Find(catid).Cat_Name;
                    ViewBag.catImage = _db.CategoryImages.Where(x => x.CatId == catid).FirstOrDefault().Images;
                    List<subcatimage> cscp = null;


                    cscp = (from sc in _db.Sub_Category
                            join ssc in _db.SubSubCategories
                            on sc.Sub_Category_ID equals ssc.Sub_Category_ID
                            join p in _db.Products
                            on ssc.SubSubCategoryID equals p.SubCatBrand.subSubCatId
                            join pi in _db.Product_Image
                            on p.Product_ID equals pi.Product_ID
                            join pp in _db.ProductPrices on p.Product_ID equals pp.ProductID
                            where sc.Cat_ID == catid && pp.EndDate == null
                            select new subcatimage
                            {
                                p_id = p.Product_ID,
                                p_name = p.Product_Name,
                                p_price = (float)pp.Price,
                                p_image = pi.Image_1,
                                qty = p.quantity,
                                colorid = (int?)_db.ProductColors.FirstOrDefault(x => x.productId == p.Product_ID).Color.colorId,
                                storageid = (int?)_db.ProductStorages.FirstOrDefault(x => x.productid == p.Product_ID).Storage.id,
                                sizeid = (int?)_db.ProductSizes.FirstOrDefault(x => x.productId == p.Product_ID).Size.sizeId,
                                wishlistId = (int?)_db.WishLists.FirstOrDefault(x => x.SellerProduct.productId == p.Product_ID && x.customerId == userId).id,
                                cartId = (int?)_db.Carts.FirstOrDefault(x => x.customerId == userId && x.productId == p.Product_ID).cartId,
                                newPrice = (float)(((pp.Price * 1) - (float)p.DiscountInAmount) - (((float)p.Discount / 100) * (pp.Price * 1))),
                                dInAmount = (float)p.DiscountInAmount,
                                dInPercent = (float)p.Discount,
                                rating = (((float)_db.RatingsAndReviews.Where(y => y.rating == 1).Count(x => x.productId == p.Product_ID) + (float)_db.RatingsAndReviews.Where(y => y.rating == 2).Count(x => x.productId == p.Product_ID) + (float)_db.RatingsAndReviews.Where(y => y.rating == 3).Count(x => x.productId == p.Product_ID) + (float)_db.RatingsAndReviews.Where(y => y.rating == 4).Count(x => x.productId == p.Product_ID) + (float)_db.RatingsAndReviews.Where(y => y.rating == 5).Count(x => x.productId == p.Product_ID)) / (float)5)
                            }).ToList();
                    if (cscp.Count == 0)
                    {
                        ViewBag.MaxPrice = 0;
                    }
                    else
                    {
                        ViewBag.MaxPrice = cscp.Max(x => x.newPrice);
                    }
                    return View(cscp);
                }
            }
            catch (Exception)
            {
                return Redirect("~/Customers/ErrorPage");
            }
        }
        [MVCDecryptFilter(secret = "mySecret")]
        [TrackExecution]
        [HttpGet]
        public ActionResult SubSubCat(int _ssc)
        {
            try
            {
                if (Session["customerSession"] == null)
                {
                    //Without Login
                    HttpCookie cookie = Request.Cookies["SID"];
                    string token = string.Empty;
                    //If Cookie is null then create new Cookie
                    if (cookie == null)
                    {
                        Response.Cookies["SID"].Values["x"] = Session.SessionID;
                        Response.Cookies["SID"].Expires = DateTime.MaxValue;
                    }
                    else
                    {
                        Session.Add("cookieId", cookie.Values["x"].ToString());
                        token = cookie.Values["x"].ToString();
                    }
                    if (token == string.Empty)
                        token = Response.Cookies["SID"].Values["x"].ToString();
                    TempData["ID"] = token.ToString();
                    var ssc_i = (from ssc in _db.SubSubCategories
                                 join ssci in _db.subsubCatImages on ssc.SubSubCategoryID equals ssci.subsubCatID
                                 where ssc.Sub_Category_ID == _ssc
                                 select new subsubcatimage
                                 { id = ssc.SubSubCategoryID, name = ssc.SubSubCategoryName, image = ssci.Image });
                    ViewBag.subsubcat = ssc_i.Take(5);
                    ViewBag.subCatImage = _db.SubCatImages.Where(x => x.SubCatId == _ssc).FirstOrDefault().Images;
                    ViewBag.subCatId = _ssc;
                    ViewBag.subCatName = _db.Sub_Category.Find(_ssc).Sub_Name;
                    ViewBag.catName = _db.Sub_Category.Find(_ssc).Category.Cat_Name;
                    ViewBag.catId = _db.Sub_Category.Find(_ssc).Category.Cat_ID;
                    ViewBag.ssci = ssc_i;
                    ViewBag.b = (from b in _db.Brands
                                 join scb in _db.SubCatBrands on b.BrandId equals scb.BrandID
                                 join ssc in _db.SubSubCategories on scb.subSubCatId equals ssc.SubSubCategoryID
                                 join sc in _db.Sub_Category on ssc.Sub_Category_ID equals sc.Sub_Category_ID
                                 join c in _db.Categories on sc.Cat_ID equals c.Cat_ID
                                 where ssc.Sub_Category_ID == _ssc
                                 select new BrandDto { brandId = b.BrandId, brandName = b.Brand_Name }).Distinct().ToList();

                    List<subsubcatimage> sscp = (from sc in _db.SubSubCategories
                                                 join p in _db.Products
                                                 on sc.SubSubCategoryID equals p.SubCatBrand.subSubCatId
                                                 join pi in _db.Product_Image
                                                 on p.Product_ID equals pi.Product_ID
                                                 join pp in _db.ProductPrices on p.Product_ID equals pp.ProductID
                                                 where sc.Sub_Category_ID == _ssc && pp.EndDate == null
                                                 select new subsubcatimage
                                                 {
                                                     p_id = p.Product_ID,
                                                     p_name = p.Product_Name,
                                                     p_price = (float)pp.Price,
                                                     p_image = pi.Image_1,
                                                     qty = p.quantity,
                                                     colorid = (int?)_db.ProductColors.FirstOrDefault(x => x.productId == p.Product_ID).Color.colorId,
                                                     storageid = (int?)_db.ProductStorages.FirstOrDefault(x => x.productid == p.Product_ID).Storage.id,
                                                     sizeid = (int?)_db.ProductSizes.FirstOrDefault(x => x.productId == p.Product_ID).Size.sizeId,
                                                     wishlistId = (int?)_db.WishLists.FirstOrDefault(x => x.SellerProduct.productId == p.Product_ID && x.GuestId == token).id,
                                                     cartId = (int?)_db.Carts.FirstOrDefault(x => x.GuestId == token && x.productId == p.Product_ID).cartId,
                                                     newPrice = (float)(((pp.Price * 1) - (float)p.DiscountInAmount) - (((float)p.Discount / 100) * (pp.Price * 1))),
                                                     dInAmount = (float)p.DiscountInAmount,
                                                     dInPercent = (float)p.Discount,
                                                     rating = ((_db.RatingsAndReviews.Where(y => y.rating == 1).Count(x => x.productId == p.Product_ID) + _db.RatingsAndReviews.Where(y => y.rating == 2).Count(x => x.productId == p.Product_ID) + _db.RatingsAndReviews.Where(y => y.rating == 3).Count(x => x.productId == p.Product_ID) + _db.RatingsAndReviews.Where(y => y.rating == 4).Count(x => x.productId == p.Product_ID) + _db.RatingsAndReviews.Where(y => y.rating == 5).Count(x => x.productId == p.Product_ID)) / 5)
                                                 }).ToList();
                    if (sscp.Count == 0)
                    {
                        ViewBag.MaxPrice = 0;
                    }
                    else
                    {
                        ViewBag.MaxPrice = sscp.Max(x => x.newPrice);
                    }
                    return View(sscp);
                }
                else
                {
                    //WithLogin
                    int userId = ((CustomerDto)Session["customerSession"]).Id;
                    TempData["ID"] = ((CustomerDto)Session["customerSession"]).Id.ToString();
                    var ssc_i = (from ssc in _db.SubSubCategories
                                 join ssci in _db.subsubCatImages on ssc.SubSubCategoryID equals ssci.subsubCatID
                                 where ssc.Sub_Category_ID == _ssc
                                 select new subsubcatimage
                                 { id = ssc.SubSubCategoryID, name = ssc.SubSubCategoryName, image = ssci.Image });
                    ViewBag.subsubcat = ssc_i.Take(5);
                    ViewBag.subCatImage = _db.SubCatImages.Where(x => x.SubCatId == _ssc).FirstOrDefault().Images;
                    ViewBag.subCatId = _ssc;
                    ViewBag.subCatName = _db.Sub_Category.Find(_ssc).Sub_Name;
                    ViewBag.catName = _db.Sub_Category.Find(_ssc).Category.Cat_Name;
                    ViewBag.catId = _db.Sub_Category.Find(_ssc).Category.Cat_ID;
                    ViewBag.ssci = ssc_i;
                    ViewBag.b = (from b in _db.Brands
                                 join scb in _db.SubCatBrands on b.BrandId equals scb.BrandID
                                 join ssc in _db.SubSubCategories on scb.subSubCatId equals ssc.SubSubCategoryID
                                 join sc in _db.Sub_Category on ssc.Sub_Category_ID equals sc.Sub_Category_ID
                                 join c in _db.Categories on sc.Cat_ID equals c.Cat_ID
                                 where ssc.Sub_Category_ID == _ssc
                                 select new BrandDto { brandId = b.BrandId, brandName = b.Brand_Name }).Distinct().ToList();

                    List<subsubcatimage> sscp = (from sc in _db.SubSubCategories
                                                 join p in _db.Products
                                                 on sc.SubSubCategoryID equals p.SubCatBrand.subSubCatId
                                                 join pi in _db.Product_Image
                                                 on p.Product_ID equals pi.Product_ID
                                                 join pp in _db.ProductPrices on p.Product_ID equals pp.ProductID
                                                 where sc.Sub_Category_ID == _ssc && pp.EndDate == null
                                                 select new subsubcatimage
                                                 {
                                                     p_id = p.Product_ID,
                                                     p_name = p.Product_Name,
                                                     p_price = (float)pp.Price,
                                                     p_image = pi.Image_1,
                                                     qty = p.quantity,
                                                     colorid = (int?)_db.ProductColors.FirstOrDefault(x => x.productId == p.Product_ID).Color.colorId,
                                                     storageid = (int?)_db.ProductStorages.FirstOrDefault(x => x.productid == p.Product_ID).Storage.id,
                                                     sizeid = (int?)_db.ProductSizes.FirstOrDefault(x => x.productId == p.Product_ID).Size.sizeId,
                                                     wishlistId = (int?)_db.WishLists.FirstOrDefault(x => x.SellerProduct.productId == p.Product_ID && x.customerId == userId).id,
                                                     cartId = (int?)_db.Carts.FirstOrDefault(x => x.customerId == userId && x.productId == p.Product_ID).cartId,
                                                     newPrice = (float)(((pp.Price * 1) - (float)p.DiscountInAmount) - (((float)p.Discount / 100) * (pp.Price * 1))),
                                                     dInAmount = (float)p.DiscountInAmount,
                                                     dInPercent = (float)p.Discount,
                                                     rating = ((_db.RatingsAndReviews.Where(y => y.rating == 1).Count(x => x.productId == p.Product_ID) + _db.RatingsAndReviews.Where(y => y.rating == 2).Count(x => x.productId == p.Product_ID) + _db.RatingsAndReviews.Where(y => y.rating == 3).Count(x => x.productId == p.Product_ID) + _db.RatingsAndReviews.Where(y => y.rating == 4).Count(x => x.productId == p.Product_ID) + _db.RatingsAndReviews.Where(y => y.rating == 5).Count(x => x.productId == p.Product_ID)) / 5)
                                                 }).ToList();
                    if (sscp.Count == 0)
                    {
                        ViewBag.MaxPrice = 0;
                    }
                    else
                    {
                        ViewBag.MaxPrice = sscp.Max(x => x.newPrice);
                    }
                    return View(sscp);
                }
            }
            catch (Exception)
            {
                return Redirect("~/Customers/ErrorPage");
            }
        }
        [MVCDecryptFilter(secret = "mySecret")]
        [TrackExecution]
        [HttpGet]
        public ActionResult SubSubCatBrands(int subsubcatId)
        {
            try
            {
                if (Session["customerSession"] == null)
                {
                    //Without Login
                    HttpCookie cookie = Request.Cookies["SID"];
                    string token = string.Empty;
                    //If Cookie is null then create new Cookie
                    if (cookie == null)
                    {
                        Response.Cookies["SID"].Values["x"] = Session.SessionID;
                        Response.Cookies["SID"].Expires = DateTime.MaxValue;
                    }
                    else
                    {
                        Session.Add("cookieId", cookie.Values["x"].ToString());
                        token = cookie.Values["x"].ToString();
                    }
                    if (token == string.Empty)
                        token = Response.Cookies["SID"].Values["x"].ToString();
                    TempData["ID"] = token.ToString();
                    ViewBag.sscImage = _db.subsubCatImages.Where(x => x.subsubCatID == subsubcatId).FirstOrDefault().Image;
                    ViewBag.sscName = _db.SubSubCategories.Where(x => x.SubSubCategoryID == subsubcatId).FirstOrDefault().SubSubCategoryName;
                    ViewBag.scId = _db.SubSubCategories.FirstOrDefault(x => x.SubSubCategoryID == subsubcatId).Sub_Category.Sub_Category_ID;
                    ViewBag.scName = _db.SubSubCategories.FirstOrDefault(x => x.SubSubCategoryID == subsubcatId).Sub_Category.Sub_Name;
                    ViewBag.cId = _db.SubSubCategories.FirstOrDefault(x => x.SubSubCategoryID == subsubcatId).Sub_Category.Category.Cat_ID;
                    ViewBag.cName = _db.SubSubCategories.FirstOrDefault(x => x.SubSubCategoryID == subsubcatId).Sub_Category.Category.Cat_Name;
                    var subsubcatbrand = (from b in _db.Brands
                                          join bi in _db.Brand_Images on b.BrandId equals bi.BrandID
                                          join sscb in _db.SubCatBrands on b.BrandId equals sscb.BrandID
                                          join ssc in _db.SubSubCategories on sscb.subSubCatId equals ssc.SubSubCategoryID
                                          where ssc.SubSubCategoryID == subsubcatId
                                          select new SubSubCatBrandsDto
                                          { BrandId = b.BrandId, BrandName = b.Brand_Name, bImage = bi.Image }).Distinct().AsParallel().ToList();
                    ViewBag.subsubcatbrand = subsubcatbrand.Take(5);
                    ViewBag.sscb = subsubcatbrand;
                    ViewBag.sscId = subsubcatId;
                    List<SubSubCatBrandsDto> subSubCatBrandsDtos = (from sc in _db.SubSubCategories
                                                                    join p in _db.Products
                                                                    on sc.SubSubCategoryID equals p.SubCatBrand.subSubCatId
                                                                    join pi in _db.Product_Image
                                                                    on p.Product_ID equals pi.Product_ID
                                                                    join pp in _db.ProductPrices on p.Product_ID equals pp.ProductID
                                                                    where sc.SubSubCategoryID == subsubcatId && pp.EndDate == null
                                                                    select new SubSubCatBrandsDto
                                                                    {
                                                                        pId = p.Product_ID,
                                                                        pName = p.Product_Name,
                                                                        price = (float)pp.Price,
                                                                        pImage = pi.Image_1,
                                                                        qty = p.quantity,
                                                                        colorid = (int?)_db.ProductColors.FirstOrDefault(x => x.productId == p.Product_ID).Color.colorId,
                                                                        storageid = (int?)_db.ProductStorages.FirstOrDefault(x => x.productid == p.Product_ID).Storage.id,
                                                                        sizeid = (int?)_db.ProductSizes.FirstOrDefault(x => x.productId == p.Product_ID).Size.sizeId,
                                                                        wishlistId = (int?)_db.WishLists.FirstOrDefault(x => x.SellerProduct.productId == p.Product_ID && x.GuestId == token).id,
                                                                        cartId = (int?)_db.Carts.FirstOrDefault(x => x.GuestId == token && x.productId == p.Product_ID).cartId,
                                                                        newPrice = (float)(((pp.Price * 1) - (float)p.DiscountInAmount) - (((float)p.Discount / 100) * (pp.Price * 1))),
                                                                        amountinDiscount = (float)p.DiscountInAmount,
                                                                        amountinPercent = (float)p.Discount,
                                                                        rating = ((_db.RatingsAndReviews.Where(y => y.rating == 1).Count(x => x.productId == p.Product_ID) + _db.RatingsAndReviews.Where(y => y.rating == 2).Count(x => x.productId == p.Product_ID) + _db.RatingsAndReviews.Where(y => y.rating == 3).Count(x => x.productId == p.Product_ID) + _db.RatingsAndReviews.Where(y => y.rating == 4).Count(x => x.productId == p.Product_ID) + _db.RatingsAndReviews.Where(y => y.rating == 5).Count(x => x.productId == p.Product_ID)) / 5)
                                                                    }).ToList();
                    if (subSubCatBrandsDtos.Count == 0)
                    {
                        ViewBag.MaxPrice = 0;
                    }
                    else
                    {
                        ViewBag.MaxPrice = subSubCatBrandsDtos.Max(x => x.newPrice);
                    }
                    return View(subSubCatBrandsDtos);
                }
                else
                {
                    //WithLogin
                    int userId = ((CustomerDto)Session["customerSession"]).Id;
                    TempData["ID"] = ((CustomerDto)Session["customerSession"]).Id.ToString();
                    ViewBag.sscImage = _db.subsubCatImages.Where(x => x.subsubCatID == subsubcatId).FirstOrDefault().Image;
                    ViewBag.sscName = _db.SubSubCategories.Where(x => x.SubSubCategoryID == subsubcatId).FirstOrDefault().SubSubCategoryName;
                    ViewBag.scId = _db.SubSubCategories.FirstOrDefault(x => x.SubSubCategoryID == subsubcatId).Sub_Category.Sub_Category_ID;
                    ViewBag.scName = _db.SubSubCategories.FirstOrDefault(x => x.SubSubCategoryID == subsubcatId).Sub_Category.Sub_Name;
                    ViewBag.cId = _db.SubSubCategories.FirstOrDefault(x => x.SubSubCategoryID == subsubcatId).Sub_Category.Category.Cat_ID;
                    ViewBag.cName = _db.SubSubCategories.FirstOrDefault(x => x.SubSubCategoryID == subsubcatId).Sub_Category.Category.Cat_Name;
                    var subsubcatbrand = (from b in _db.Brands
                                          join bi in _db.Brand_Images on b.BrandId equals bi.BrandID
                                          join sscb in _db.SubCatBrands on b.BrandId equals sscb.BrandID
                                          join ssc in _db.SubSubCategories on sscb.subSubCatId equals ssc.SubSubCategoryID
                                          where ssc.SubSubCategoryID == subsubcatId
                                          select new SubSubCatBrandsDto
                                          { BrandId = b.BrandId, BrandName = b.Brand_Name, bImage = bi.Image }).Distinct().AsParallel().ToList();
                    ViewBag.subsubcatbrand = subsubcatbrand.Take(5);
                    ViewBag.sscb = subsubcatbrand;
                    ViewBag.sscId = subsubcatId;
                    List<SubSubCatBrandsDto> subSubCatBrandsDtos = (from sc in _db.SubSubCategories
                                                                    join p in _db.Products
                                                                    on sc.SubSubCategoryID equals p.SubCatBrand.subSubCatId
                                                                    join pi in _db.Product_Image
                                                                    on p.Product_ID equals pi.Product_ID
                                                                    join pp in _db.ProductPrices on p.Product_ID equals pp.ProductID
                                                                    where sc.SubSubCategoryID == subsubcatId && pp.EndDate == null
                                                                    select new SubSubCatBrandsDto
                                                                    {
                                                                        pId = p.Product_ID,
                                                                        pName = p.Product_Name,
                                                                        price = (float)pp.Price,
                                                                        pImage = pi.Image_1,
                                                                        qty = p.quantity,
                                                                        colorid = (int?)_db.ProductColors.FirstOrDefault(x => x.productId == p.Product_ID).Color.colorId,
                                                                        storageid = (int?)_db.ProductStorages.FirstOrDefault(x => x.productid == p.Product_ID).Storage.id,
                                                                        sizeid = (int?)_db.ProductSizes.FirstOrDefault(x => x.productId == p.Product_ID).Size.sizeId,
                                                                        wishlistId = (int?)_db.WishLists.FirstOrDefault(x => x.SellerProduct.productId == p.Product_ID && x.customerId == userId).id,
                                                                        cartId = (int?)_db.Carts.FirstOrDefault(x => x.customerId == userId && x.productId == p.Product_ID).cartId,
                                                                        newPrice = (float)(((pp.Price * 1) - (float)p.DiscountInAmount) - (((float)p.Discount / 100) * (pp.Price * 1))),
                                                                        amountinDiscount = (float)p.DiscountInAmount,
                                                                        amountinPercent = (float)p.Discount,
                                                                        rating = ((_db.RatingsAndReviews.Where(y => y.rating == 1).Count(x => x.productId == p.Product_ID) + _db.RatingsAndReviews.Where(y => y.rating == 2).Count(x => x.productId == p.Product_ID) + _db.RatingsAndReviews.Where(y => y.rating == 3).Count(x => x.productId == p.Product_ID) + _db.RatingsAndReviews.Where(y => y.rating == 4).Count(x => x.productId == p.Product_ID) + _db.RatingsAndReviews.Where(y => y.rating == 5).Count(x => x.productId == p.Product_ID)) / 5)
                                                                    }).ToList();
                    if (subSubCatBrandsDtos.Count == 0)
                    {
                        ViewBag.MaxPrice = 0;
                    }
                    else
                    {
                        ViewBag.MaxPrice = subSubCatBrandsDtos.Max(x => x.newPrice);
                    }
                    return View(subSubCatBrandsDtos);
                }
            }
            catch (Exception)
            {
                return Redirect("~/Customers/ErrorPage");
            }
        }
        [MVCDecryptFilter(secret = "mySecret")]
        [TrackExecution]
        [HttpGet]
        public ActionResult Brands(int brandID)
        {
            try
            {
                if (Session["customerSession"] == null)
                {
                    //Without Login
                    HttpCookie cookie = Request.Cookies["SID"];
                    string token = string.Empty;
                    //If Cookie is null then create new Cookie
                    if (cookie == null)
                    {
                        Response.Cookies["SID"].Values["x"] = Session.SessionID;
                        Response.Cookies["SID"].Expires = DateTime.MaxValue;
                    }
                    else
                    {
                        Session.Add("cookieId", cookie.Values["x"].ToString());
                        token = cookie.Values["x"].ToString();
                    }
                    if (token == string.Empty)
                        token = Response.Cookies["SID"].Values["x"].ToString();
                    TempData["ID"] = token.ToString();
                    ViewBag.brandImage = _db.Brand_Images.FirstOrDefault(x => x.BrandID == brandID).Image;
                    ViewBag.brandName = _db.Brands.FirstOrDefault(x => x.BrandId == brandID).Brand_Name;
                    ViewBag.brandID = _db.Brands.FirstOrDefault(x => x.BrandId == brandID).BrandId;

                    var brandProducts = (from p in _db.Products
                                         join pi in _db.Product_Image on p.Product_ID equals pi.Product_ID
                                         join pp in _db.ProductPrices on p.Product_ID equals pp.ProductID
                                         join scb in _db.SubCatBrands on p.brandCategoryId equals scb.Id
                                         where scb.Brand.BrandId == brandID && pp.EndDate == null
                                         select new BrandsDto
                                         {
                                             proID = p.Product_ID,
                                             proName = p.Product_Name,
                                             proImg = _db.Product_Image.FirstOrDefault(x => x.Product_ID == p.Product_ID).Image_1,
                                             price = (float)pp.Price,
                                             colorid = (int?)_db.ProductColors.FirstOrDefault(x => x.productId == p.Product_ID).Color.colorId,
                                             storageid = (int?)_db.ProductStorages.FirstOrDefault(x => x.productid == p.Product_ID).Storage.id,
                                             sizeid = (int?)_db.ProductSizes.FirstOrDefault(x => x.productId == p.Product_ID).Size.sizeId,
                                             wishlistId = (int?)_db.WishLists.FirstOrDefault(x => x.SellerProduct.productId == p.Product_ID && x.GuestId == token).id,
                                             cartId = (int?)_db.Carts.FirstOrDefault(x => x.GuestId == token && x.productId == p.Product_ID).cartId,
                                             newPrice = (float)(((pp.Price * 1) - (float)p.DiscountInAmount) - (((float)p.Discount / 100) * (pp.Price * 1))),
                                             dInAmount = (float)p.DiscountInAmount,
                                             dInPercent = (float)p.Discount,
                                             qty = p.quantity,
                                             rating = ((_db.RatingsAndReviews.Where(y => y.rating == 1).Count(x => x.productId == p.Product_ID) + _db.RatingsAndReviews.Where(y => y.rating == 2).Count(x => x.productId == p.Product_ID) + _db.RatingsAndReviews.Where(y => y.rating == 3).Count(x => x.productId == p.Product_ID) + _db.RatingsAndReviews.Where(y => y.rating == 4).Count(x => x.productId == p.Product_ID) + _db.RatingsAndReviews.Where(y => y.rating == 5).Count(x => x.productId == p.Product_ID)) / 5)
                                         }).ToList();
                    return View(brandProducts);
                }
                else
                {
                    //WithLogin
                    int userId = ((CustomerDto)Session["customerSession"]).Id;
                    TempData["ID"] = ((CustomerDto)Session["customerSession"]).Id.ToString();
                    ViewBag.brandImage = _db.Brand_Images.FirstOrDefault(x => x.BrandID == brandID).Image;
                    ViewBag.brandName = _db.Brands.FirstOrDefault(x => x.BrandId == brandID).Brand_Name;
                    ViewBag.brandID = _db.Brands.FirstOrDefault(x => x.BrandId == brandID).BrandId;

                    var brandProducts = (from p in _db.Products
                                         join pi in _db.Product_Image on p.Product_ID equals pi.Product_ID
                                         join pp in _db.ProductPrices on p.Product_ID equals pp.ProductID
                                         join scb in _db.SubCatBrands on p.brandCategoryId equals scb.Id
                                         where scb.Brand.BrandId == brandID && pp.EndDate == null
                                         select new BrandsDto
                                         {
                                             proID = p.Product_ID,
                                             proName = p.Product_Name,
                                             proImg = _db.Product_Image.FirstOrDefault(x => x.Product_ID == p.Product_ID).Image_1,
                                             price = (float)pp.Price,
                                             colorid = (int?)_db.ProductColors.FirstOrDefault(x => x.productId == p.Product_ID).Color.colorId,
                                             storageid = (int?)_db.ProductStorages.FirstOrDefault(x => x.productid == p.Product_ID).Storage.id,
                                             sizeid = (int?)_db.ProductSizes.FirstOrDefault(x => x.productId == p.Product_ID).Size.sizeId,
                                             wishlistId = (int?)_db.WishLists.FirstOrDefault(x => x.SellerProduct.productId == p.Product_ID && x.customerId == userId).id,
                                             cartId = (int?)_db.Carts.FirstOrDefault(x => x.customerId == userId && x.productId == p.Product_ID).cartId,
                                             newPrice = (float)(((pp.Price * 1) - (float)p.DiscountInAmount) - (((float)p.Discount / 100) * (pp.Price * 1))),
                                             dInAmount = (float)p.DiscountInAmount,
                                             dInPercent = (float)p.Discount,
                                             qty = p.quantity,
                                             rating = ((_db.RatingsAndReviews.Where(y => y.rating == 1).Count(x => x.productId == p.Product_ID) + _db.RatingsAndReviews.Where(y => y.rating == 2).Count(x => x.productId == p.Product_ID) + _db.RatingsAndReviews.Where(y => y.rating == 3).Count(x => x.productId == p.Product_ID) + _db.RatingsAndReviews.Where(y => y.rating == 4).Count(x => x.productId == p.Product_ID) + _db.RatingsAndReviews.Where(y => y.rating == 5).Count(x => x.productId == p.Product_ID)) / 5)
                                         }).ToList();
                    return View(brandProducts);
                }
            }
            catch (Exception)
            {
                return Redirect("~/Customers/ErrorPage");
            }
        }
        [TrackExecution]
        [HttpGet]
        public ActionResult AddAddress()
        {
            try
            {
                //ViewBag.Category = dataSet.Tables[0];
                var c_country = from Country in _db.Countries select Country;
                ViewBag.cc = c_country;
                return View();
            }
            catch (Exception e)
            {
                return Redirect("~/Customers/ErrorPage");
            }
        }
        [TrackExecution]
        public ActionResult SaveAddress(int selected_Area, string address, string addressName)
        {
            try
            {
                if (Session["customerSession"] != null)
                {
                    Address Addr = new Address()
                    {
                        Area_ID = selected_Area,
                        Cust_ID = (((CustomerDto)Session["customerSession"]).Id),
                        Address1 = address,
                        AddressName = addressName
                    };
                    _db.Addresses.Add(Addr);
                    _db.SaveChanges();
                    return Redirect("~/Customers/BuyAndCheckOut");
                }
                else
                {
                    HttpCookie cookie = Request.Cookies["SID"];
                    string token = string.Empty;
                    //If Cookie is null then create new Cookie
                    if (cookie == null)
                    {
                        Response.Cookies["SID"].Values["x"] = Session.SessionID;
                        Response.Cookies["SID"].Expires = DateTime.MaxValue;
                    }
                    else
                    {
                        Session.Add("cookieId", cookie.Values["x"].ToString());
                        token = cookie.Values["x"].ToString();
                    }
                    if (token == string.Empty)
                        token = Response.Cookies["SID"].Values["x"].ToString();
                    TempData["ID"] = token.ToString();
                    Address Addr = new Address()
                    {
                        Area_ID = selected_Area,
                        GuestId = token,
                        Address1 = address,
                        AddressName = addressName
                    };
                    _db.Addresses.Add(Addr);
                    _db.SaveChanges();
                    return Redirect("~/Customers/BuyAndCheckOut");
                }

            }
            catch (Exception e)
            {
                return Redirect("~/Customers/ErrorPage");
            }
        }

        [TrackExecution]
        [HttpPost]
        public JsonResult UpdateCustInfo(string name, string email, string phone, string pass)
        {
            try
            {
                Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
                Regex regexPass = new Regex(@"^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9]).{8,}$");
                //var emailCheck = _db.Customers.FirstOrDefault(x => x.Email == email);
                var match = regex.Match(email);
                var match1 = regexPass.Match(pass);
                if (name == string.Empty)
                {
                    return Json("Please Enter Name!", JsonRequestBehavior.AllowGet);
                }
                if (match == null)
                {
                    return Json("Your Email is empty!", JsonRequestBehavior.AllowGet);
                }
                if (!match.Success)
                {
                    return Json("Invalid Email!");
                }
                else if (phone == null)
                {
                    return Json("Your Phone Number field is empty!");
                }
                else if (phone.Length > 11 || phone.Length < 11 || phone.Length == 0)
                {
                    return Json("Invalid Phone Number!");
                }
                else
                {
                    if (pass != string.Empty)
                    {
                        if (pass.Length < 8)
                        {
                            return Json("Your Password doesn't fulfill our requirements!");
                        }
                        if (!match1.Success)
                        {
                            return Json("Invalid Password! You must use 1 Upper Case Letter, 1 Lower case Letter and 1 Digit!");
                        }
                        string hashPass = EncryptionClass.GetMD5Hash(pass);
                        int a = ((CustomerDto)Session["customerSession"]).Id;
                        var c = _db.Customers.FirstOrDefault(x => x.Cust_ID == a);
                        c.Email = email;
                        c.Cust_Name = name;
                        c.Password = hashPass;
                        _db.Entry(c).State = EntityState.Modified;
                        _db.SaveChanges();
                        var cc = _db.Customer_Contact.FirstOrDefault(x => x.Customer_ID == a);
                        cc.PhoneNo1 = phone;
                        _db.Entry(cc).State = EntityState.Modified;
                        _db.SaveChanges();
                    }
                    else
                    {
                        int a = ((CustomerDto)Session["customerSession"]).Id;
                        var c = _db.Customers.FirstOrDefault(x => x.Cust_ID == a);
                        c.Email = email;
                        c.Cust_Name = name;
                        _db.Entry(c).State = EntityState.Modified;
                        _db.SaveChanges();
                        var cc = _db.Customer_Contact.FirstOrDefault(x => x.Customer_ID == a);
                        cc.PhoneNo1 = phone;
                        _db.Entry(cc).State = EntityState.Modified;
                        _db.SaveChanges();
                    }
                    return Json("Success");
                }
            }
            catch (Exception e)
            {
                return Json("/Customers/ErrorPage");
                //throw e;
            }
        }
        [TrackExecution]
        [HttpPost]
        public JsonResult CustomerLogin(string email, string pass)
        {
            try
            {
                if (email != string.Empty && pass == string.Empty)
                {
                    return Json("Please Enter Password");
                }
                if (email == string.Empty && pass != string.Empty)
                {
                    return Json("Please Enter Email");
                }
                if (email == string.Empty && pass == string.Empty)
                {
                    return Json("Please Fill the Form");
                }
                if (email != string.Empty && pass != string.Empty)
                {
                    pass = EncryptionClass.GetMD5Hash(pass);
                    var cust_detail1 = (from a in _db.Customers
                                        where a.Email == email && a.Password == pass
                                        select new CustomerDto
                                        {
                                            Id = a.Cust_ID,
                                            Name = a.Cust_Name,
                                            contact = _db.Customer_Contact.FirstOrDefault(x => x.Customer_ID == a.Cust_ID).PhoneNo1,
                                            Email = a.Email
                                        }).FirstOrDefault();
                    if (cust_detail1 == null)
                    {
                        return Json("Invalid Email or Password");
                    }
                    else
                    {
                        CustomerDto cust_detail = new CustomerDto
                        {
                            Id = cust_detail1.Id,
                            Name = cust_detail1.Name,
                            Email = cust_detail1.Email,
                            contact = cust_detail1.contact
                        };
                        Session["customerSession"] = cust_detail;
                        TempData["ID"] = cust_detail.Id.ToString();
                        return Json("Success");
                    }
                }
                return Json("/Customers/ErrorPage");
            }
            catch (Exception e)
            {
                return Json("/Customers/ErrorPage");
            }
        }

        [TrackExecution]
        [HttpGet]
        public ActionResult ActivateEmail(string message)
        {
            try
            {
                ViewBag.msg = message;
                return View();
            }
            catch (Exception)
            {
                TempData["ActivationEmailError"] = "Something Went Wrong!";
                return Redirect("~/Customers/ActivateEmail");
            }
        }
        [TrackExecution]
        [HttpPost]
        public ActionResult ActivateEmail(float code)
        {
            try
            {
                string useremail = TempData["email"].ToString();
                var cus = _db.Customers.FirstOrDefault(p => p.Email == useremail);
                if (cus != null)
                {
                    if (cus.ActivationCode.Equals(code.ToString()))
                    {
                        //Now Change Status
                        cus.Status = "verified";
                        _db.Entry(cus).State = System.Data.Entity.EntityState.Modified;
                        _db.SaveChanges();
                        TempData["IsVerified"] = "Your Email Has Been Verified Successfully!";
                        CustomerDto cust_detail = new CustomerDto
                        {
                            Id = cus.Cust_ID,
                            Name = cus.Cust_Name,
                            Email = cus.Email,
                            contact = _db.Customer_Contact.FirstOrDefault(x=>x.Customer_ID == cus.Cust_ID).PhoneNo1
                        };
                        Session["customerSession"] = cust_detail;
                        return Redirect("~/Customers/Index");
                    }
                    else
                    {
                        TempData["NotVerified"] = "Your Have Entered Invalid Activation Code, kindly Check Your Mail Inbox";
                    }
                }
                return View();
            }
            catch (Exception)
            {
                TempData["ActivationEmailError"] = "Invalid Email.";
                return Redirect("~/Customers/Index");
            }
        }
        [TrackExecution]
        private void SendCode(string name, string activatiocode, string email)
        {
            try
            {
                SmtpClient sc = new SmtpClient();
                sc.Host = "smtp.gmail.com";
                sc.Port = 587;
                sc.Credentials = new NetworkCredential("PakEmart12345@gmail.com", "emart12345");
                sc.EnableSsl = true;
                MailMessage msg = new MailMessage();
                msg.Subject = "Activation Code to Verify Email Address";
                msg.Body = "Dear " + name + " Your Activation Code is " + activatiocode + "\n\n\nThanks & Regards";
                string toAddress = email;
                msg.To.Add(toAddress);
                string fromAddress = "Emart Shopping <PakEmart12345@gmail.com>";
                msg.From = new MailAddress(fromAddress);
                sc.Send(msg);
            }
            catch (Exception)
            {
                TempData["ActivationEmailError"] = "Something Went Wrong!";
            }
        }
        [TrackExecution]
        public string generatingToken(int id, string name, string pass)
        {
            string plaintext = id + name + pass + DateTime.Now;
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plaintext);
            return Convert.ToBase64String(plainTextBytes);
        }

        [TrackExecution]
        [HttpGet]
        public ActionResult CustomerWishList()
        {
            try
            {
                if ((Session["customerSession"]) != null)
                {
                    int custId = (((CustomerDto)Session["customerSession"]).Id);
                    TempData["ID"] = custId.ToString();
                    List<WishListDto> w = (from wl in _db.WishLists
                                           join sp in _db.SellerProducts
                                           on wl.sellerProductId equals sp.Id
                                           join p in _db.Products on sp.productId equals p.Product_ID
                                           join pp in _db.ProductPrices on p.Product_ID equals pp.ProductID
                                           where wl.customerId == custId && pp.EndDate == null
                                           select new WishListDto
                                           {
                                               Id = wl.id,
                                               ProductId = p.Product_ID,
                                               Price = (float)(((pp.Price * 1) - (float)p.DiscountInAmount) - (((float)p.Discount / 100) * (pp.Price * 1))),
                                               ProductName = p.Product_Name,
                                               SellerName = sp.Seller.sellerName,
                                               Image = p.Product_Image.FirstOrDefault(x => x.Product_ID == p.Product_ID).Image_1,
                                               colorName = _db.Colors.FirstOrDefault(x => x.colorId == wl.colorid).colorName,
                                               storageName = _db.Storages.FirstOrDefault(x => x.id == wl.storageid).Storage1,
                                               sizeName = _db.Sizes.FirstOrDefault(x => x.sizeId == wl.sizeid).sizeName,
                                               colorid = (int?)wl.colorid,
                                               storageid = (int?)wl.storageid,
                                               sizeid = (int?)wl.sizeid
                                           }).ToList();
                    return View(w);
                }
                else
                {
                    HttpCookie cookie = Request.Cookies["SID"];
                    string token = string.Empty;
                    //If Cookie is null then create new Cookie
                    if (cookie == null)
                    { Response.Cookies["SID"].Values["x"] = Session.SessionID; Response.Cookies["SID"].Expires = DateTime.MaxValue; }
                    else
                    { Session.Add("cookieId", cookie.Values["x"].ToString()); token = cookie.Values["x"].ToString(); }
                    if (token == string.Empty)
                        token = Response.Cookies["SID"].Values["x"].ToString();
                    TempData["ID"] = token.ToString();
                    //int custId = (((CustomerDto)Session["customerSession"]).Id);
                    //TempData["ID"] = custId.ToString();
                    List<WishListDto> w = (from wl in _db.WishLists
                                           join sp in _db.SellerProducts
                                           on wl.sellerProductId equals sp.Id
                                           join p in _db.Products on sp.productId equals p.Product_ID
                                           join pp in _db.ProductPrices on p.Product_ID equals pp.ProductID
                                           where wl.GuestId == token && pp.EndDate == null
                                           select new WishListDto
                                           {
                                               Id = wl.id,
                                               ProductId = p.Product_ID,
                                               Price = (float)(((pp.Price * 1) - (float)p.DiscountInAmount) - (((float)p.Discount / 100) * (pp.Price * 1))),
                                               ProductName = p.Product_Name,
                                               SellerName = sp.Seller.sellerName,
                                               Image = p.Product_Image.FirstOrDefault(x => x.Product_ID == p.Product_ID).Image_1,
                                               colorName = _db.Colors.FirstOrDefault(x => x.colorId == wl.colorid).colorName,
                                               storageName = _db.Storages.FirstOrDefault(x => x.id == wl.storageid).Storage1,
                                               sizeName = _db.Sizes.FirstOrDefault(x => x.sizeId == wl.sizeid).sizeName,
                                               colorid = (int?)wl.colorid,
                                               storageid = (int?)wl.storageid,
                                               sizeid = (int?)wl.sizeid
                                           }).ToList();
                    return View(w);
                }
            }
            catch (Exception e)
            {
                ViewBag.e = e;
                return Redirect("~/Customers/ErrorPage");
            }
        }

        [MVCDecryptFilter(secret = "mySecret")]
        [TrackExecution]
        public ActionResult RemoveFromWishList(int wishlistID)
        {
            try
            {
                var wishList = new WishList { id = wishlistID };
                _db.Entry(wishList).State = EntityState.Deleted;
                _db.SaveChanges();
                return Redirect("~/Customers/CustomerWishList");
            }
            catch (Exception e)
            {
                ViewBag.e = e;
                return Redirect("~/Customers/ErrorPage");
            }
        }
        public static float dCharges = 0;
        [TrackExecution]
        [HttpGet]
        public ActionResult CustomerCart()
        {
            try
            {
                if ((Session["customerSession"]) != null)
                {
                    TempData["ID"] = ((CustomerDto)Session["customerSession"]).Id.ToString();
                    int cid = (((CustomerDto)Session["customerSession"]).Id);
                    //int cid = 3035;
                    List<CartDto> c_cart = (from c1 in _db.Carts
                                            join p2 in _db.Products on c1.productId equals p2.Product_ID
                                            join pi in _db.Product_Image on p2.Product_ID equals pi.Product_ID
                                            join pp in _db.ProductPrices on p2.Product_ID equals pp.ProductID
                                            where c1.customerId == cid && pp.EndDate == null
                                            select new CartDto
                                            {
                                                cartProductPriceId = p2.Product_ID.ToString() + "|" + (float)(((pp.Price * 1) - (float)p2.DiscountInAmount) - (((float)p2.Discount / 100) * (pp.Price * 1))),
                                                cartID = c1.cartId,
                                                qty = c1.quantity,
                                                pname = p2.Product_Name,
                                                price = (float)(((pp.Price * 1) - (float)p2.DiscountInAmount) - (((float)p2.Discount / 100) * (pp.Price * 1))),
                                                _color = _db.Colors.FirstOrDefault(x => x.colorId == c1.colorid).colorName,
                                                _storage = _db.Storages.FirstOrDefault(x => x.id == c1.storageid).Storage1,
                                                _size = _db.Sizes.FirstOrDefault(x => x.sizeId == c1.sizeid).sizeName,
                                                colorid = (int?)c1.colorid,
                                                storageid = (int?)c1.storageid,
                                                sizeid = (int?)c1.sizeid,
                                                img1 = pi.Image_1,
                                                img2 = pi.Image_2,
                                                img3 = pi.Image_3,
                                                img4 = pi.Image_4,
                                                img5 = pi.Image_5,
                                                __productId = p2.Product_ID
                                            }).ToList();
                    dCharges = (float)_db.Shipments.FirstOrDefault().deliveryCharges;
                    ViewBag.dc = dCharges;
                    return View(c_cart);
                }
                else
                {
                    HttpCookie cookie = Request.Cookies["SID"];
                    string token = string.Empty;
                    if (cookie == null)
                    { Response.Cookies["SID"].Values["x"] = Session.SessionID; Response.Cookies["SID"].Expires = DateTime.MaxValue; }
                    else
                    { Session.Add("cookieId", cookie.Values["x"].ToString()); token = cookie.Values["x"].ToString(); }

                    if (token == string.Empty)
                        token = Response.Cookies["SID"].Values["x"].ToString();
                    TempData["ID"] = token;
                    //int cid = 3035;
                    List<CartDto> c_cart = (from c1 in _db.Carts
                                            join p2 in _db.Products on c1.productId equals p2.Product_ID
                                            join pi in _db.Product_Image on p2.Product_ID equals pi.Product_ID
                                            join pp in _db.ProductPrices on p2.Product_ID equals pp.ProductID
                                            where c1.GuestId == token && pp.EndDate == null
                                            select new CartDto
                                            {
                                                cartProductPriceId = p2.Product_ID.ToString() + "|" + (float)(((pp.Price * 1) - (float)p2.DiscountInAmount) - (((float)p2.Discount / 100) * (pp.Price * 1))),
                                                cartID = c1.cartId,
                                                qty = c1.quantity,
                                                pname = p2.Product_Name,
                                                price = (float)(((pp.Price * 1) - (float)p2.DiscountInAmount) - (((float)p2.Discount / 100) * (pp.Price * 1))),
                                                _color = _db.Colors.FirstOrDefault(x => x.colorId == c1.colorid).colorName,
                                                _storage = _db.Storages.FirstOrDefault(x => x.id == c1.storageid).Storage1,
                                                _size = _db.Sizes.FirstOrDefault(x => x.sizeId == c1.sizeid).sizeName,
                                                colorid = (int?)c1.colorid,
                                                storageid = (int?)c1.storageid,
                                                sizeid = (int?)c1.sizeid,
                                                img1 = pi.Image_1,
                                                img2 = pi.Image_2,
                                                img3 = pi.Image_3,
                                                img4 = pi.Image_4,
                                                img5 = pi.Image_5,
                                                __productId = p2.Product_ID
                                            }).ToList();
                    dCharges = (float)_db.Shipments.FirstOrDefault().deliveryCharges;
                    ViewBag.dc = dCharges;
                    return View(c_cart);
                }
                //return Redirect("~/Customers/Index");
                //return Redirect(Request.UrlReferrer.ToString());
            }
            catch (Exception e)
            {
                ViewBag.e = e;
                return Redirect("~/Customers/ErrorPage");
            }
        }

        [TrackExecution]
        [HttpPost]
        public JsonResult addtoCartByPiWi(List<string> itemToAdd)
        {
            if (Session["customerSession"] != null)
            {
                TempData["ID"] = ((CustomerDto)Session["customerSession"]).Id.ToString();
                foreach (var item in itemToAdd)
                {
                    int cartItemQty = 0;
                    int cqty = 0;
                    int custId = ((CustomerDto)Session["customerSession"]).Id;
                    string[] a = item.Split('|');
                    int pid = int.Parse(a[0].ToString());
                    int qty = int.Parse(a[1].ToString());
                    var pdCheck = _db.Carts.FirstOrDefault(x => x.productId == pid && x.customerId == custId);
                    if (pdCheck != null)
                    {
                        return Json("Item already exist in the Cart!" + "+" + cartItemQty.ToString(), JsonRequestBehavior.AllowGet);
                    }
                    else
                    {

                        if (a[4] == "undefined" && a[2] != "undefined" && a[3] != "undefined")
                        {
                            int colorid = int.Parse(a[2].ToString());
                            int storageid = int.Parse(a[3].ToString());
                            Cart cart = new Cart()
                            {
                                productId = pid,
                                customerId = (((CustomerDto)Session["customerSession"]).Id),
                                quantity = qty,
                                colorid = colorid,
                                storageid = storageid
                            };
                            _db.Carts.Add(cart);
                            _db.SaveChanges();
                            var _item = _db.SellerProducts.Where(x => x.productId == pid).FirstOrDefault().Id;
                            var wishlistItem = _db.WishLists.FirstOrDefault(x => x.sellerProductId == _item);
                            if (wishlistItem != null)
                            {
                                _db.WishLists.Remove(wishlistItem);
                                _db.SaveChanges();
                            }
                            var count = _db.Carts.Where(x => x.customerId == custId).ToList();
                            for (int i = 0; i < count.Count; i++)
                            {
                                int id = count[i].productId;
                                cqty = _db.Carts.FirstOrDefault(x => x.customerId == custId && x.productId == id).quantity;
                                cartItemQty = cartItemQty + cqty;
                            }
                            int newCid = _db.Carts.Max(x => x.cartId);
                            int wCount = _db.WishLists.Where(x => x.customerId == custId).Count();
                            return Json("Item Added to the Cart Succesfully!" + "+" + cartItemQty.ToString() + "+" + newCid.ToString() + "+" + wCount.ToString(), JsonRequestBehavior.AllowGet);
                        }
                        else if (a[2] == "undefined" && a[4] != "undefined" && a[3] != "undefined")
                        {
                            int sizeid = int.Parse(a[4].ToString());
                            int storageid = int.Parse(a[3].ToString());
                            Cart cart = new Cart()
                            {
                                productId = pid,
                                customerId = (((CustomerDto)Session["customerSession"]).Id),
                                quantity = qty,
                                sizeid = sizeid,
                                storageid = storageid
                            };
                            _db.Carts.Add(cart);
                            _db.SaveChanges();
                            var _item = _db.SellerProducts.Where(x => x.productId == pid).FirstOrDefault().Id;
                            var wishlistItem = _db.WishLists.FirstOrDefault(x => x.sellerProductId == _item);
                            if (wishlistItem != null)
                            {
                                _db.WishLists.Remove(wishlistItem);
                                _db.SaveChanges();
                            }
                            var count = _db.Carts.Where(x => x.customerId == custId).ToList();
                            for (int i = 0; i < count.Count; i++)
                            {
                                int id = count[i].productId;
                                cqty = _db.Carts.FirstOrDefault(x => x.customerId == custId && x.productId == id).quantity;
                                cartItemQty = cartItemQty + cqty;
                            }
                            int newCid = _db.Carts.Max(x => x.cartId);
                            int wCount = _db.WishLists.Where(x => x.customerId == custId).Count();
                            return Json("Item Added to the Cart Succesfully!" + "+" + cartItemQty.ToString() + "+" + newCid.ToString() + "+" + wCount.ToString(), JsonRequestBehavior.AllowGet);
                        }
                        else if (a[3] == "undefined" && a[2] != "undefined" && a[4] != "undefined")
                        {
                            int sizeid = int.Parse(a[4].ToString());
                            int colorid = int.Parse(a[2].ToString());
                            Cart cart = new Cart()
                            {
                                productId = pid,
                                customerId = (((CustomerDto)Session["customerSession"]).Id),
                                quantity = qty,
                                sizeid = sizeid,
                                colorid = colorid
                            };
                            _db.Carts.Add(cart);
                            _db.SaveChanges();
                            var _item = _db.SellerProducts.Where(x => x.productId == pid).FirstOrDefault().Id;
                            var wishlistItem = _db.WishLists.FirstOrDefault(x => x.sellerProductId == _item);
                            if (wishlistItem != null)
                            {
                                _db.WishLists.Remove(wishlistItem);
                                _db.SaveChanges();
                            }
                            var count = _db.Carts.Where(x => x.customerId == custId).ToList();
                            for (int i = 0; i < count.Count; i++)
                            {
                                int id = count[i].productId;
                                cqty = _db.Carts.FirstOrDefault(x => x.customerId == custId && x.productId == id).quantity;
                                cartItemQty = cartItemQty + cqty;
                            }
                            int newCid = _db.Carts.Max(x => x.cartId);
                            int wCount = _db.WishLists.Where(x => x.customerId == custId).Count();
                            return Json("Item Added to the Cart Succesfully!" + "+" + cartItemQty.ToString() + "+" + newCid.ToString() + "+" + wCount.ToString(), JsonRequestBehavior.AllowGet);
                        }
                        else if (a[3] == "undefined" && a[2] == "undefined" && a[4] != "undefined")
                        {
                            int sizeid = int.Parse(a[4].ToString());
                            Cart cart = new Cart()
                            {
                                productId = pid,
                                customerId = (((CustomerDto)Session["customerSession"]).Id),
                                quantity = qty,
                                sizeid = sizeid,
                            };
                            _db.Carts.Add(cart);
                            _db.SaveChanges();
                            var _item = _db.SellerProducts.Where(x => x.productId == pid).FirstOrDefault().Id;
                            var wishlistItem = _db.WishLists.FirstOrDefault(x => x.sellerProductId == _item);
                            if (wishlistItem != null)
                            {
                                _db.WishLists.Remove(wishlistItem);
                                _db.SaveChanges();
                            }
                            var count = _db.Carts.Where(x => x.customerId == custId).ToList();
                            for (int i = 0; i < count.Count; i++)
                            {
                                int id = count[i].productId;
                                cqty = _db.Carts.FirstOrDefault(x => x.customerId == custId && x.productId == id).quantity;
                                cartItemQty = cartItemQty + cqty;
                            }
                            int newCid = _db.Carts.Max(x => x.cartId);
                            int wCount = _db.WishLists.Where(x => x.customerId == custId).Count();
                            return Json("Item Added to the Cart Succesfully!" + "+" + cartItemQty.ToString() + "+" + newCid.ToString() + "+" + wCount.ToString(), JsonRequestBehavior.AllowGet);
                        }
                        else if (a[4] == "undefined" && a[3] == "undefined" && a[2] != "undefined")
                        {
                            int colorid = int.Parse(a[2].ToString());
                            Cart cart = new Cart()
                            {
                                productId = pid,
                                customerId = (((CustomerDto)Session["customerSession"]).Id),
                                quantity = qty,
                                colorid = colorid
                            };
                            _db.Carts.Add(cart);
                            _db.SaveChanges();
                            var _item = _db.SellerProducts.Where(x => x.productId == pid).FirstOrDefault().Id;
                            var wishlistItem = _db.WishLists.FirstOrDefault(x => x.sellerProductId == _item);
                            if (wishlistItem != null)
                            {
                                _db.WishLists.Remove(wishlistItem);
                                _db.SaveChanges();
                            }
                            var count = _db.Carts.Where(x => x.customerId == custId).ToList();
                            for (int i = 0; i < count.Count; i++)
                            {
                                int id = count[i].productId;
                                cqty = _db.Carts.FirstOrDefault(x => x.customerId == custId && x.productId == id).quantity;
                                cartItemQty = cartItemQty + cqty;
                            }
                            int newCid = _db.Carts.Max(x => x.cartId);
                            int wCount = _db.WishLists.Where(x => x.customerId == custId).Count();
                            return Json("Item Added to the Cart Succesfully!" + "+" + cartItemQty.ToString() + "+" + newCid.ToString() + "+" + wCount.ToString(), JsonRequestBehavior.AllowGet);
                        }
                        else if (a[4] == "undefined" && a[2] == "undefined" && a[3] != "undefined")
                        {
                            int storageid = int.Parse(a[3].ToString());
                            Cart cart = new Cart()
                            {
                                productId = pid,
                                customerId = (((CustomerDto)Session["customerSession"]).Id),
                                quantity = qty,
                                storageid = storageid,
                            };
                            _db.Carts.Add(cart);
                            _db.SaveChanges();
                            var _item = _db.SellerProducts.Where(x => x.productId == pid).FirstOrDefault().Id;
                            var wishlistItem = _db.WishLists.FirstOrDefault(x => x.sellerProductId == _item);
                            if (wishlistItem != null)
                            {
                                _db.WishLists.Remove(wishlistItem);
                                _db.SaveChanges();
                            }
                            var count = _db.Carts.Where(x => x.customerId == custId).ToList();
                            for (int i = 0; i < count.Count; i++)
                            {
                                int id = count[i].productId;
                                cqty = _db.Carts.FirstOrDefault(x => x.customerId == custId && x.productId == id).quantity;
                                cartItemQty = cartItemQty + cqty;
                            }
                            int newCid = _db.Carts.Max(x => x.cartId);
                            int wCount = _db.WishLists.Where(x => x.customerId == custId).Count();
                            return Json("Item Added to the Cart Succesfully!" + " " + cartItemQty.ToString() + "+" + newCid.ToString() + "+" + wCount.ToString(), JsonRequestBehavior.AllowGet);
                        }
                        else if (a[3] != "undefined" && a[2] != "undefined" && a[4] != "undefined")
                        {
                            int storageid = int.Parse(a[3].ToString());
                            int sizeid = int.Parse(a[4].ToString());
                            int colorid = int.Parse(a[2].ToString());
                            Cart cart = new Cart()
                            {
                                productId = pid,
                                customerId = (((CustomerDto)Session["customerSession"]).Id),
                                quantity = qty,
                                colorid = colorid,
                                storageid = storageid,
                                sizeid = sizeid
                            };
                            _db.Carts.Add(cart);
                            _db.SaveChanges();
                            var _item = _db.SellerProducts.Where(x => x.productId == pid).FirstOrDefault().Id;
                            var wishlistItem = _db.WishLists.FirstOrDefault(x => x.sellerProductId == _item);
                            if (wishlistItem != null)
                            {
                                _db.WishLists.Remove(wishlistItem);
                                _db.SaveChanges();
                            }
                            var count = _db.Carts.Where(x => x.customerId == custId).ToList();
                            for (int i = 0; i < count.Count; i++)
                            {
                                int id = count[i].productId;
                                cqty = _db.Carts.FirstOrDefault(x => x.customerId == custId && x.productId == id).quantity;
                                cartItemQty = cartItemQty + cqty;
                            }
                            int newCid = _db.Carts.Max(x => x.cartId);
                            int wCount = _db.WishLists.Where(x => x.customerId == custId).Count();
                            return Json("Item Added to the Cart Succesfully!" + "+" + cartItemQty.ToString() + "+" + newCid.ToString() + "+" + wCount.ToString(), JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            Cart cart = new Cart()
                            {
                                productId = pid,
                                customerId = (((CustomerDto)Session["customerSession"]).Id),
                                quantity = qty
                            };
                            _db.Carts.Add(cart);
                            _db.SaveChanges();
                            var _item = _db.SellerProducts.Where(x => x.productId == pid).FirstOrDefault().Id;
                            var wishlistItem = _db.WishLists.FirstOrDefault(x => x.sellerProductId == _item);
                            if (wishlistItem != null)
                            {
                                _db.WishLists.Remove(wishlistItem);
                                _db.SaveChanges();
                            }
                            var count = _db.Carts.Where(x => x.customerId == custId).ToList();
                            for (int i = 0; i < count.Count; i++)
                            {
                                int id = count[i].productId;
                                cqty = _db.Carts.FirstOrDefault(x => x.customerId == custId && x.productId == id).quantity;
                                cartItemQty = cartItemQty + cqty;
                            }
                            int newCid = _db.Carts.Max(x => x.cartId);
                            int wCount = _db.WishLists.Where(x => x.customerId == custId).Count();
                            return Json("Item Added to the Cart Succesfully!" + "+" + cartItemQty.ToString() + "+" + newCid.ToString() + "+" + wCount.ToString(), JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                return Json("Item Added" + "+" + "&nbsp", JsonRequestBehavior.AllowGet);
            }
            else //without Login add to cart
            {
                HttpCookie cookie = Request.Cookies["SID"];
                string token = string.Empty;
                if (cookie == null)
                { Response.Cookies["SID"].Values["x"] = Session.SessionID; Response.Cookies["SID"].Expires = DateTime.MaxValue; }
                else
                { Session.Add("cookieId", cookie.Values["x"].ToString()); token = cookie.Values["x"].ToString(); }

                if (token == string.Empty)
                    token = Response.Cookies["SID"].Values["x"].ToString();
                TempData["ID"] = token;
                foreach (var item in itemToAdd)
                {
                    int cartItemQty = 0;
                    int cqty = 0;
                    string guestId = token;
                    string[] a = item.Split('|');
                    int pid = int.Parse(a[0].ToString());
                    int qty = int.Parse(a[1].ToString());
                    var pdCheck = _db.Carts.FirstOrDefault(x => x.productId == pid && x.GuestId == guestId);
                    if (pdCheck != null)
                    {
                        return Json("Item already exist in the Cart!" + "+" + cartItemQty.ToString(), JsonRequestBehavior.AllowGet);
                    }
                    else
                    {

                        if (a[4] == "undefined" && a[2] != "undefined" && a[3] != "undefined")
                        {
                            int colorid = int.Parse(a[2].ToString());
                            int storageid = int.Parse(a[3].ToString());
                            Cart cart = new Cart()
                            {
                                productId = pid,
                                GuestId = token,
                                quantity = qty,
                                colorid = colorid,
                                storageid = storageid
                            };
                            _db.Carts.Add(cart);
                            _db.SaveChanges();
                            var _item = _db.SellerProducts.Where(x => x.productId == pid).FirstOrDefault().Id;
                            var wishlistItem = _db.WishLists.FirstOrDefault(x => x.sellerProductId == _item);
                            if (wishlistItem != null)
                            {
                                _db.WishLists.Remove(wishlistItem);
                                _db.SaveChanges();
                            }
                            var count = _db.Carts.Where(x => x.GuestId == guestId).ToList();
                            for (int i = 0; i < count.Count; i++)
                            {
                                int id = count[i].productId;
                                cqty = _db.Carts.FirstOrDefault(x => x.GuestId == guestId && x.productId == id).quantity;
                                cartItemQty = cartItemQty + cqty;
                            }
                            int newCid = _db.Carts.Max(x => x.cartId);
                            int wCount = _db.WishLists.Where(x => x.GuestId == token).Count();
                            return Json("Item Added to the Cart Succesfully!" + "+" + cartItemQty.ToString() + "+" + newCid.ToString() + "+" + wCount.ToString(), JsonRequestBehavior.AllowGet);
                        }
                        else if (a[2] == "undefined" && a[4] != "undefined" && a[3] != "undefined")
                        {
                            int sizeid = int.Parse(a[4].ToString());
                            int storageid = int.Parse(a[3].ToString());
                            Cart cart = new Cart()
                            {
                                productId = pid,
                                GuestId = token,
                                quantity = qty,
                                sizeid = sizeid,
                                storageid = storageid
                            };
                            _db.Carts.Add(cart);
                            _db.SaveChanges();
                            var _item = _db.SellerProducts.Where(x => x.productId == pid).FirstOrDefault().Id;
                            var wishlistItem = _db.WishLists.FirstOrDefault(x => x.sellerProductId == _item);
                            if (wishlistItem != null)
                            {
                                _db.WishLists.Remove(wishlistItem);
                                _db.SaveChanges();
                            }
                            var count = _db.Carts.Where(x => x.GuestId == guestId).ToList();
                            for (int i = 0; i < count.Count; i++)
                            {
                                int id = count[i].productId;
                                cqty = _db.Carts.FirstOrDefault(x => x.GuestId == guestId && x.productId == id).quantity;
                                cartItemQty = cartItemQty + cqty;
                            }
                            int newCid = _db.Carts.Max(x => x.cartId);
                            int wCount = _db.WishLists.Where(x => x.GuestId == token).Count();
                            return Json("Item Added to the Cart Succesfully!" + "+" + cartItemQty.ToString() + "+" + newCid.ToString() + "+" + wCount.ToString(), JsonRequestBehavior.AllowGet);
                        }
                        else if (a[3] == "undefined" && a[2] != "undefined" && a[4] != "undefined")
                        {
                            int sizeid = int.Parse(a[4].ToString());
                            int colorid = int.Parse(a[2].ToString());
                            Cart cart = new Cart()
                            {
                                productId = pid,
                                GuestId = token,
                                quantity = qty,
                                sizeid = sizeid,
                                colorid = colorid
                            };
                            _db.Carts.Add(cart);
                            _db.SaveChanges();
                            var _item = _db.SellerProducts.Where(x => x.productId == pid).FirstOrDefault().Id;
                            var wishlistItem = _db.WishLists.FirstOrDefault(x => x.sellerProductId == _item);
                            if (wishlistItem != null)
                            {
                                _db.WishLists.Remove(wishlistItem);
                                _db.SaveChanges();
                            }
                            var count = _db.Carts.Where(x => x.GuestId == guestId).ToList();
                            for (int i = 0; i < count.Count; i++)
                            {
                                int id = count[i].productId;
                                cqty = _db.Carts.FirstOrDefault(x => x.GuestId == guestId && x.productId == id).quantity;
                                cartItemQty = cartItemQty + cqty;
                            }
                            int newCid = _db.Carts.Max(x => x.cartId);
                            int wCount = _db.WishLists.Where(x => x.GuestId == token).Count();
                            return Json("Item Added to the Cart Succesfully!" + "+" + cartItemQty.ToString() + "+" + newCid.ToString() + "+" + wCount.ToString(), JsonRequestBehavior.AllowGet);
                        }
                        else if (a[3] == "undefined" && a[2] == "undefined" && a[4] != "undefined")
                        {
                            int sizeid = int.Parse(a[4].ToString());
                            Cart cart = new Cart()
                            {
                                productId = pid,
                                GuestId = token,
                                quantity = qty,
                                sizeid = sizeid,
                            };
                            _db.Carts.Add(cart);
                            _db.SaveChanges();
                            var _item = _db.SellerProducts.Where(x => x.productId == pid).FirstOrDefault().Id;
                            var wishlistItem = _db.WishLists.FirstOrDefault(x => x.sellerProductId == _item);
                            if (wishlistItem != null)
                            {
                                _db.WishLists.Remove(wishlistItem);
                                _db.SaveChanges();
                            }
                            var count = _db.Carts.Where(x => x.GuestId == guestId).ToList();
                            for (int i = 0; i < count.Count; i++)
                            {
                                int id = count[i].productId;
                                cqty = _db.Carts.FirstOrDefault(x => x.GuestId == guestId && x.productId == id).quantity;
                                cartItemQty = cartItemQty + cqty;
                            }
                            int newCid = _db.Carts.Max(x => x.cartId);
                            int wCount = _db.WishLists.Where(x => x.GuestId == token).Count();
                            return Json("Item Added to the Cart Succesfully!" + "+" + cartItemQty.ToString() + "+" + newCid.ToString() + "+" + wCount.ToString(), JsonRequestBehavior.AllowGet);
                        }
                        else if (a[4] == "undefined" && a[3] == "undefined" && a[2] != "undefined")
                        {
                            int colorid = int.Parse(a[2].ToString());
                            Cart cart = new Cart()
                            {
                                productId = pid,
                                GuestId = token,
                                quantity = qty,
                                colorid = colorid
                            };
                            _db.Carts.Add(cart);
                            _db.SaveChanges();
                            var _item = _db.SellerProducts.Where(x => x.productId == pid).FirstOrDefault().Id;
                            var wishlistItem = _db.WishLists.FirstOrDefault(x => x.sellerProductId == _item);
                            if (wishlistItem != null)
                            {
                                _db.WishLists.Remove(wishlistItem);
                                _db.SaveChanges();
                            }
                            var count = _db.Carts.Where(x => x.GuestId == guestId).ToList();
                            for (int i = 0; i < count.Count; i++)
                            {
                                int id = count[i].productId;
                                cqty = _db.Carts.FirstOrDefault(x => x.GuestId == guestId && x.productId == id).quantity;
                                cartItemQty = cartItemQty + cqty;
                            }
                            int newCid = _db.Carts.Max(x => x.cartId);
                            int wCount = _db.WishLists.Where(x => x.GuestId == token).Count();
                            return Json("Item Added to the Cart Succesfully!" + "+" + cartItemQty.ToString() + "+" + newCid.ToString() + "+" + wCount.ToString(), JsonRequestBehavior.AllowGet);
                        }
                        else if (a[4] == "undefined" && a[2] == "undefined" && a[3] != "undefined")
                        {
                            int storageid = int.Parse(a[3].ToString());
                            Cart cart = new Cart()
                            {
                                productId = pid,
                                GuestId = token,
                                quantity = qty,
                                storageid = storageid,
                            };
                            _db.Carts.Add(cart);
                            _db.SaveChanges();
                            var _item = _db.SellerProducts.Where(x => x.productId == pid).FirstOrDefault().Id;
                            var wishlistItem = _db.WishLists.FirstOrDefault(x => x.sellerProductId == _item);
                            if (wishlistItem != null)
                            {
                                _db.WishLists.Remove(wishlistItem);
                                _db.SaveChanges();
                            }
                            var count = _db.Carts.Where(x => x.GuestId == guestId).ToList();
                            for (int i = 0; i < count.Count; i++)
                            {
                                int id = count[i].productId;
                                cqty = _db.Carts.FirstOrDefault(x => x.GuestId == guestId && x.productId == id).quantity;
                                cartItemQty = cartItemQty + cqty;
                            }
                            int newCid = _db.Carts.Max(x => x.cartId);
                            int wCount = _db.WishLists.Where(x => x.GuestId == token).Count();
                            return Json("Item Added to the Cart Succesfully!" + " " + cartItemQty.ToString() + "+" + newCid.ToString() + "+" + wCount.ToString(), JsonRequestBehavior.AllowGet);
                        }
                        else if (a[3] != "undefined" && a[2] != "undefined" && a[4] != "undefined")
                        {
                            int storageid = int.Parse(a[3].ToString());
                            int sizeid = int.Parse(a[4].ToString());
                            int colorid = int.Parse(a[2].ToString());
                            Cart cart = new Cart()
                            {
                                productId = pid,
                                GuestId = token,
                                quantity = qty,
                                colorid = colorid,
                                storageid = storageid,
                                sizeid = sizeid
                            };
                            _db.Carts.Add(cart);
                            _db.SaveChanges();
                            var _item = _db.SellerProducts.Where(x => x.productId == pid).FirstOrDefault().Id;
                            var wishlistItem = _db.WishLists.FirstOrDefault(x => x.sellerProductId == _item);
                            if (wishlistItem != null)
                            {
                                _db.WishLists.Remove(wishlistItem);
                                _db.SaveChanges();
                            }
                            var count = _db.Carts.Where(x => x.GuestId == guestId).ToList();
                            for (int i = 0; i < count.Count; i++)
                            {
                                int id = count[i].productId;
                                cqty = _db.Carts.FirstOrDefault(x => x.GuestId == guestId && x.productId == id).quantity;
                                cartItemQty = cartItemQty + cqty;
                            }
                            int newCid = _db.Carts.Max(x => x.cartId);
                            int wCount = _db.WishLists.Where(x => x.GuestId == token).Count();
                            return Json("Item Added to the Cart Succesfully!" + "+" + cartItemQty.ToString() + "+" + newCid.ToString() + "+" + wCount.ToString(), JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            Cart cart = new Cart()
                            {
                                productId = pid,
                                GuestId = token,
                                quantity = qty
                            };
                            _db.Carts.Add(cart);
                            _db.SaveChanges();
                            var _item = _db.SellerProducts.Where(x => x.productId == pid).FirstOrDefault().Id;
                            var wishlistItem = _db.WishLists.FirstOrDefault(x => x.sellerProductId == _item);
                            if (wishlistItem != null)
                            {
                                _db.WishLists.Remove(wishlistItem);
                                _db.SaveChanges();
                            }
                            var count = _db.Carts.Where(x => x.GuestId == guestId).ToList();
                            for (int i = 0; i < count.Count; i++)
                            {
                                int id = count[i].productId;
                                cqty = _db.Carts.FirstOrDefault(x => x.GuestId == guestId && x.productId == id).quantity;
                                cartItemQty = cartItemQty + cqty;
                            }
                            int newCid = _db.Carts.Max(x => x.cartId);
                            int wCount = _db.WishLists.Where(x => x.GuestId == token).Count();
                            return Json("Item Added to the Cart Succesfully!" + "+" + cartItemQty.ToString() + "+" + newCid.ToString() + "+" + wCount.ToString(), JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                return Json("Item Added" + "+" + "&nbsp", JsonRequestBehavior.AllowGet);
            }
        }

        [MVCDecryptFilter(secret = "mySecret")]
        [TrackExecution]
        public ActionResult RemoveFromCart(int cartID)
        {
            try
            {
                var cart = new Cart { cartId = cartID };
                _db.Entry(cart).State = EntityState.Deleted;
                _db.SaveChanges();
                return Redirect("~/Customers/CustomerCart");
            }
            catch (Exception)
            {
                return Redirect("~/Customers/ErrorPage");
            }
        }
        public static int salesOrderid = 0;
        public static List<string> selectedProductQuantity;
        public static int TotProductPrice = 0;
        public static int Qty = 0;
        public static int grandTot = 0;
        public static int count;
        [NoDirectAccess]
        [TrackExecution]
        [HttpGet]
        public ActionResult BuyAndCheckOut()
        {
            try
            {
                if (Session["customerSession"] != null)
                {
                    if (Session[((CustomerDto)Session["customerSession"]).Id.ToString()] == null)
                    {
                        TempData["selectedProductQuantity"] = "You have not selected any Item to Buy!";
                        return Redirect("~/Customers/CustomerCart");
                    }
                    TempData["ID"] = ((CustomerDto)Session["customerSession"]).Id.ToString();
                    int cid = (((CustomerDto)Session["customerSession"]).Id);
                    var addr = (from a in _db.Addresses
                                where a.Cust_ID == cid
                                select new AddressNameDto
                                { Id = a.Id, Name = a.AddressName }).ToList();
                    ViewBag.addr = addr;
                    //ViewBag.ProTot = TotProductPrice;
                    ViewBag.ProTot = ((SelectedProductDto)Session[((CustomerDto)Session["customerSession"]).Id.ToString()]).totalPrice;
                    //grandTot = int.Parse(TotProductPrice.ToString()) + int.Parse(dCharges.ToString());
                    //grandTot = int.Parse(((SelectedProductDto)Session[((CustomerDto)Session["customerSession"]).Id]).totalPrice.ToString()) + int.Parse(dCharges.ToString());
                    ViewBag.gTotal = ((SelectedProductDto)Session[((CustomerDto)Session["customerSession"]).Id.ToString()]).grandTotal;
                    dCharges = (float)_db.Shipments.FirstOrDefault().deliveryCharges;
                    ViewBag.dc = dCharges;
                    List<SelectedProductDto> selectedProducts = new List<SelectedProductDto>();
                    foreach (var item in ((SelectedProductDto)Session[((CustomerDto)Session["customerSession"]).Id.ToString()]).selectedItems)
                    {
                        string[] p_q = item.Split('|');
                        int id = int.Parse(p_q[0].ToString());
                        int _quantity = int.Parse(p_q[1].ToString());
                        if (p_q[4] == "undefined" && p_q[2] != "undefined" && p_q[3] != "undefined")
                        {
                            int colorid = int.Parse(p_q[2]);
                            int storageid = int.Parse(p_q[3]);
                            var selectedProduct = (from p in _db.Products
                                                   join pi in _db.Product_Image on
                                                   p.Product_ID equals pi.Product_ID
                                                   join pp in _db.ProductPrices on p.Product_ID equals pp.ProductID
                                                   where p.Product_ID == id && pp.EndDate == null
                                                   select new SelectedProductDto
                                                   {
                                                       proID = p.Product_ID,
                                                       pName = p.Product_Name,
                                                       image = pi.Image_1,
                                                       color = _db.Colors.FirstOrDefault(x => x.colorId == colorid).colorName,
                                                       storage = _db.Storages.FirstOrDefault(x => x.id == storageid).Storage1,
                                                       pPrice = (float)(((pp.Price * 1) - (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().DiscountInAmount) - (((float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().Discount / 100) * (pp.Price * 1))),
                                                       qty = _quantity
                                                   }).FirstOrDefault();
                            selectedProducts.Add(selectedProduct);
                        }
                        else if (p_q[2] == "undefined" && p_q[4] != "undefined" && p_q[3] != "undefined")
                        {
                            int sizeid = int.Parse(p_q[4]);
                            int storageid = int.Parse(p_q[3]);
                            var selectedProduct = (from p in _db.Products
                                                   join pi in _db.Product_Image on
                                                   p.Product_ID equals pi.Product_ID
                                                   join pp in _db.ProductPrices on p.Product_ID equals pp.ProductID
                                                   where p.Product_ID == id && pp.EndDate == null
                                                   select new SelectedProductDto
                                                   {
                                                       proID = p.Product_ID,
                                                       pName = p.Product_Name,
                                                       image = pi.Image_1,
                                                       storage = _db.Storages.FirstOrDefault(x => x.id == storageid).Storage1,
                                                       size = _db.Sizes.FirstOrDefault(x => x.sizeId == sizeid).sizeName,
                                                       pPrice = (float)(((pp.Price * 1) - (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().DiscountInAmount) - (((float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().Discount / 100) * (pp.Price * 1))),
                                                       qty = _quantity
                                                   }).FirstOrDefault();
                            selectedProducts.Add(selectedProduct);
                        }
                        else if (p_q[3] == "undefined" && p_q[4] != "undefined" && p_q[2] != "undefined")
                        {
                            int sizeid = int.Parse(p_q[4]);
                            int colorid = int.Parse(p_q[2]);
                            var selectedProduct = (from p in _db.Products
                                                   join pi in _db.Product_Image on
                                                   p.Product_ID equals pi.Product_ID
                                                   join pp in _db.ProductPrices on p.Product_ID equals pp.ProductID
                                                   where p.Product_ID == id && pp.EndDate == null
                                                   select new SelectedProductDto
                                                   {
                                                       proID = p.Product_ID,
                                                       pName = p.Product_Name,
                                                       image = pi.Image_1,
                                                       color = _db.Colors.FirstOrDefault(x => x.colorId == colorid).colorName,
                                                       size = _db.Sizes.FirstOrDefault(x => x.sizeId == sizeid).sizeName,
                                                       pPrice = (float)(((pp.Price * 1) - (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().DiscountInAmount) - (((float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().Discount / 100) * (pp.Price * 1))),
                                                       qty = _quantity
                                                   }).FirstOrDefault();
                            selectedProducts.Add(selectedProduct);
                        }
                        else if (p_q[3] == "undefined" && p_q[2] == "undefined" && p_q[4] != "undefined")
                        {
                            int sizeid = int.Parse(p_q[4]);
                            var selectedProduct = (from p in _db.Products
                                                   join pi in _db.Product_Image on
                                                   p.Product_ID equals pi.Product_ID
                                                   join pp in _db.ProductPrices on p.Product_ID equals pp.ProductID
                                                   where p.Product_ID == id && pp.EndDate == null
                                                   select new SelectedProductDto
                                                   {
                                                       proID = p.Product_ID,
                                                       pName = p.Product_Name,
                                                       image = pi.Image_1,
                                                       size = _db.Sizes.FirstOrDefault(x => x.sizeId == sizeid).sizeName,
                                                       pPrice = (float)(((pp.Price * 1) - (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().DiscountInAmount) - (((float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().Discount / 100) * (pp.Price * 1))),
                                                       qty = _quantity
                                                   }).FirstOrDefault();
                            selectedProducts.Add(selectedProduct);
                        }
                        else if (p_q[4] == "undefined" && p_q[3] == "undefined" && p_q[2] != "undefined")
                        {
                            int colorid = int.Parse(p_q[2]);
                            var selectedProduct = (from p in _db.Products
                                                   join pi in _db.Product_Image on
                                                   p.Product_ID equals pi.Product_ID
                                                   join pp in _db.ProductPrices on p.Product_ID equals pp.ProductID
                                                   where p.Product_ID == id && pp.EndDate == null
                                                   select new SelectedProductDto
                                                   {
                                                       proID = p.Product_ID,
                                                       pName = p.Product_Name,
                                                       image = pi.Image_1,
                                                       color = _db.Colors.FirstOrDefault(x => x.colorId == colorid).colorName,
                                                       pPrice = (float)(((pp.Price * 1) - (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().DiscountInAmount) - (((float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().Discount / 100) * (pp.Price * 1))),
                                                       qty = _quantity
                                                   }).FirstOrDefault();
                            selectedProducts.Add(selectedProduct);
                        }
                        else if (p_q[3] != "undefined" && p_q[2] != "undefined" && p_q[4] != "undefined")
                        {
                            int storageid = int.Parse(p_q[3]);
                            int sizeid = int.Parse(p_q[4]);
                            int colorid = int.Parse(p_q[2]);
                            var selectedProduct = (from p in _db.Products
                                                   join pi in _db.Product_Image on
                                                   p.Product_ID equals pi.Product_ID
                                                   join pp in _db.ProductPrices on p.Product_ID equals pp.ProductID
                                                   where p.Product_ID == id && pp.EndDate == null
                                                   select new SelectedProductDto
                                                   {
                                                       proID = p.Product_ID,
                                                       pName = p.Product_Name,
                                                       image = pi.Image_1,
                                                       color = _db.Colors.FirstOrDefault(x => x.colorId == colorid).colorName,
                                                       size = _db.Sizes.FirstOrDefault(x => x.sizeId == sizeid).sizeName,
                                                       storage = _db.Storages.Find(storageid).Storage1,
                                                       pPrice = (float)(((pp.Price * 1) - (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().DiscountInAmount) - (((float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().Discount / 100) * (pp.Price * 1))),
                                                       qty = _quantity
                                                   }).FirstOrDefault();
                            selectedProducts.Add(selectedProduct);
                        }
                        else
                        {
                            var selectedProduct = (from p in _db.Products
                                                   join pi in _db.Product_Image on
                                                   p.Product_ID equals pi.Product_ID
                                                   join pp in _db.ProductPrices on p.Product_ID equals pp.ProductID
                                                   where p.Product_ID == id && pp.EndDate == null
                                                   select new SelectedProductDto
                                                   {
                                                       proID = p.Product_ID,
                                                       pName = p.Product_Name,
                                                       image = pi.Image_1,
                                                       pPrice = (float)(((pp.Price * 1) - (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().DiscountInAmount) - (((float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().Discount / 100) * (pp.Price * 1))),
                                                       qty = _quantity
                                                   }).FirstOrDefault();
                            selectedProducts.Add(selectedProduct);
                        }
                        count++;
                    }
                    return View(selectedProducts);

                }
                else
                {
                    HttpCookie cookie = Request.Cookies["SID"];
                    string token = string.Empty;
                    //If Cookie is null then create new Cookie
                    if (cookie == null)
                    {
                        Response.Cookies["SID"].Values["x"] = Session.SessionID;
                        Response.Cookies["SID"].Expires = DateTime.MaxValue;
                    }
                    else
                    {
                        Session.Add("cookieId", cookie.Values["x"].ToString());
                        token = cookie.Values["x"].ToString();
                    }
                    if (token == string.Empty)
                        token = Response.Cookies["SID"].Values["x"].ToString();
                    TempData["ID"] = token.ToString();
                    if (Session[token] == null)
                    {
                        TempData["selectedProductQuantity"] = "You have not selected any Item to Buy!";
                        return Redirect("~/Customers/CustomerCart");
                    }
                    string guestId = token;
                    TempData["token"] = token;
                    var addr = (from a in _db.Addresses
                                where a.GuestId == guestId
                                select new AddressNameDto
                                { Id = a.Id, Name = a.AddressName }).ToList();
                    ViewBag.addr = addr;
                    ViewBag.ProTot = ((SelectedProductDto)Session[token]).totalPrice;
                    //grandTot = int.Parse(((SelectedProductDto)Session[token]).totalPrice.ToString()) + int.Parse(dCharges.ToString());
                    ViewBag.gTotal = ((SelectedProductDto)Session[token]).grandTotal;
                    dCharges = (float)_db.Shipments.FirstOrDefault().deliveryCharges;
                    ViewBag.dc = dCharges;
                    List<SelectedProductDto> selectedProducts = new List<SelectedProductDto>();
                    foreach (var item in ((SelectedProductDto)Session[token]).selectedItems)
                    {
                        string[] p_q = item.Split('|');
                        int id = int.Parse(p_q[0].ToString());
                        int _quantity = int.Parse(p_q[1].ToString());
                        if (p_q[4] == "undefined" && p_q[2] != "undefined" && p_q[3] != "undefined")
                        {
                            int colorid = int.Parse(p_q[2]);
                            int storageid = int.Parse(p_q[3]);
                            var selectedProduct = (from p in _db.Products
                                                   join pi in _db.Product_Image on
                                                   p.Product_ID equals pi.Product_ID
                                                   join pp in _db.ProductPrices on p.Product_ID equals pp.ProductID
                                                   where p.Product_ID == id && pp.EndDate == null
                                                   select new SelectedProductDto
                                                   {
                                                       proID = p.Product_ID,
                                                       pName = p.Product_Name,
                                                       image = pi.Image_1,
                                                       color = _db.Colors.FirstOrDefault(x => x.colorId == colorid).colorName,
                                                       storage = _db.Storages.FirstOrDefault(x => x.id == storageid).Storage1,
                                                       pPrice = (float)(((pp.Price * 1) - (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().DiscountInAmount) - (((float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().Discount / 100) * (pp.Price * 1))),
                                                       qty = _quantity
                                                   }).FirstOrDefault();
                            selectedProducts.Add(selectedProduct);
                        }
                        else if (p_q[2] == "undefined" && p_q[4] != "undefined" && p_q[3] != "undefined")
                        {
                            int sizeid = int.Parse(p_q[4]);
                            int storageid = int.Parse(p_q[3]);
                            var selectedProduct = (from p in _db.Products
                                                   join pi in _db.Product_Image on
                                                   p.Product_ID equals pi.Product_ID
                                                   join pp in _db.ProductPrices on p.Product_ID equals pp.ProductID
                                                   where p.Product_ID == id && pp.EndDate == null
                                                   select new SelectedProductDto
                                                   {
                                                       proID = p.Product_ID,
                                                       pName = p.Product_Name,
                                                       image = pi.Image_1,
                                                       storage = _db.Storages.FirstOrDefault(x => x.id == storageid).Storage1,
                                                       size = _db.Sizes.FirstOrDefault(x => x.sizeId == sizeid).sizeName,
                                                       pPrice = (float)(((pp.Price * 1) - (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().DiscountInAmount) - (((float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().Discount / 100) * (pp.Price * 1))),
                                                       qty = _quantity
                                                   }).FirstOrDefault();
                            selectedProducts.Add(selectedProduct);
                        }
                        else if (p_q[3] == "undefined" && p_q[4] != "undefined" && p_q[2] != "undefined")
                        {
                            int sizeid = int.Parse(p_q[4]);
                            int colorid = int.Parse(p_q[2]);
                            var selectedProduct = (from p in _db.Products
                                                   join pi in _db.Product_Image on
                                                   p.Product_ID equals pi.Product_ID
                                                   join pp in _db.ProductPrices on p.Product_ID equals pp.ProductID
                                                   where p.Product_ID == id && pp.EndDate == null
                                                   select new SelectedProductDto
                                                   {
                                                       proID = p.Product_ID,
                                                       pName = p.Product_Name,
                                                       image = pi.Image_1,
                                                       color = _db.Colors.FirstOrDefault(x => x.colorId == colorid).colorName,
                                                       size = _db.Sizes.FirstOrDefault(x => x.sizeId == sizeid).sizeName,
                                                       pPrice = (float)(((pp.Price * 1) - (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().DiscountInAmount) - (((float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().Discount / 100) * (pp.Price * 1))),
                                                       qty = _quantity
                                                   }).FirstOrDefault();
                            selectedProducts.Add(selectedProduct);
                        }
                        else if (p_q[3] == "undefined" && p_q[2] == "undefined" && p_q[4] != "undefined")
                        {
                            int sizeid = int.Parse(p_q[4]);
                            var selectedProduct = (from p in _db.Products
                                                   join pi in _db.Product_Image on
                                                   p.Product_ID equals pi.Product_ID
                                                   join pp in _db.ProductPrices on p.Product_ID equals pp.ProductID
                                                   where p.Product_ID == id && pp.EndDate == null
                                                   select new SelectedProductDto
                                                   {
                                                       proID = p.Product_ID,
                                                       pName = p.Product_Name,
                                                       image = pi.Image_1,
                                                       size = _db.Sizes.FirstOrDefault(x => x.sizeId == sizeid).sizeName,
                                                       pPrice = (float)(((pp.Price * 1) - (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().DiscountInAmount) - (((float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().Discount / 100) * (pp.Price * 1))),
                                                       qty = _quantity
                                                   }).FirstOrDefault();
                            selectedProducts.Add(selectedProduct);
                        }
                        else if (p_q[4] == "undefined" && p_q[3] == "undefined" && p_q[2] != "undefined")
                        {
                            int colorid = int.Parse(p_q[2]);
                            var selectedProduct = (from p in _db.Products
                                                   join pi in _db.Product_Image on
                                                   p.Product_ID equals pi.Product_ID
                                                   join pp in _db.ProductPrices on p.Product_ID equals pp.ProductID
                                                   where p.Product_ID == id && pp.EndDate == null
                                                   select new SelectedProductDto
                                                   {
                                                       proID = p.Product_ID,
                                                       pName = p.Product_Name,
                                                       image = pi.Image_1,
                                                       color = _db.Colors.FirstOrDefault(x => x.colorId == colorid).colorName,
                                                       pPrice = (float)(((pp.Price * 1) - (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().DiscountInAmount) - (((float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().Discount / 100) * (pp.Price * 1))),
                                                       qty = _quantity
                                                   }).FirstOrDefault();
                            selectedProducts.Add(selectedProduct);
                        }
                        else if (p_q[3] != "undefined" && p_q[2] != "undefined" && p_q[4] != "undefined")
                        {
                            int storageid = int.Parse(p_q[3]);
                            int sizeid = int.Parse(p_q[4]);
                            int colorid = int.Parse(p_q[2]);
                            var selectedProduct = (from p in _db.Products
                                                   join pi in _db.Product_Image on
                                                   p.Product_ID equals pi.Product_ID
                                                   join pp in _db.ProductPrices on p.Product_ID equals pp.ProductID
                                                   where p.Product_ID == id && pp.EndDate == null
                                                   select new SelectedProductDto
                                                   {
                                                       proID = p.Product_ID,
                                                       pName = p.Product_Name,
                                                       image = pi.Image_1,
                                                       color = _db.Colors.FirstOrDefault(x => x.colorId == colorid).colorName,
                                                       size = _db.Sizes.FirstOrDefault(x => x.sizeId == sizeid).sizeName,
                                                       storage = _db.Storages.Find(storageid).Storage1,
                                                       pPrice = (float)(((pp.Price * 1) - (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().DiscountInAmount) - (((float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().Discount / 100) * (pp.Price * 1))),
                                                       qty = _quantity
                                                   }).FirstOrDefault();
                            selectedProducts.Add(selectedProduct);
                        }
                        else
                        {
                            var selectedProduct = (from p in _db.Products
                                                   join pi in _db.Product_Image on
                                                   p.Product_ID equals pi.Product_ID
                                                   join pp in _db.ProductPrices on p.Product_ID equals pp.ProductID
                                                   where p.Product_ID == id && pp.EndDate == null
                                                   select new SelectedProductDto
                                                   {
                                                       proID = p.Product_ID,
                                                       pName = p.Product_Name,
                                                       image = pi.Image_1,
                                                       pPrice = (float)(((pp.Price * 1) - (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().DiscountInAmount) - (((float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().Discount / 100) * (pp.Price * 1))),
                                                       qty = _quantity
                                                   }).FirstOrDefault();
                            selectedProducts.Add(selectedProduct);
                        }
                        count++;
                    }
                    return View(selectedProducts);
                }
                //return View();
            }
            catch (Exception e)
            {
                ViewBag.e = e;
                return Redirect("~/Customers/ErrorPage");
            }
        }
        [TrackExecution]
        [HttpPost]
        public JsonResult BuyAndCheckOut(string selectedShipAddr, string selectedBillAddr, string gEmail, string gPhone, string gName)
        {
            try
            {
                if (Session["customerSession"] != null)
                {
                    if (selectedBillAddr == "-1" && selectedShipAddr == "-1")
                    {
                        return Json("Please Select Shipping and Billing Address", JsonRequestBehavior.AllowGet);
                    }
                    if (selectedBillAddr == "-1" && selectedShipAddr != "-1")
                    {
                        return Json("Please Select Billing Address", JsonRequestBehavior.AllowGet);
                    }
                    if (selectedBillAddr != "-1" && selectedShipAddr == "-1")
                    {
                        return Json("Please Select Shipping Address", JsonRequestBehavior.AllowGet);
                    }

                    TempData["ID"] = ((CustomerDto)Session["customerSession"]).Id.ToString();

                    //string guid = Guid.NewGuid().ToString();
                    SalesOrder salesOrder = new SalesOrder()
                    {
                        cust_Id = ((CustomerDto)Session["customerSession"]).Id,
                        order_date = DateTime.Now.ToString(),
                        shipmentID = 1,
                        trackingID = ((SelectedProductDto)Session[((CustomerDto)Session["customerSession"]).Id.ToString()])._guid,
                        GrandTotal = ((SelectedProductDto)Session[((CustomerDto)Session["customerSession"]).Id.ToString()]).grandTotal
                    };
                    _db.SalesOrders.Add(salesOrder);
                    _db.SaveChanges();
                    salesOrderid = (from so in _db.SalesOrders select so.order_ID).Max();
                    foreach (var item in ((SelectedProductDto)Session[((CustomerDto)Session["customerSession"]).Id.ToString()]).selectedItems)
                    {
                        string[] p_q = item.Split('|');
                        int id = int.Parse(p_q[0].ToString());
                        int _quantity = int.Parse(p_q[1].ToString());
                        float price = (float)_db.ProductPrices.Where(x => x.EndDate == null).FirstOrDefault(y => y.ProductID == id).Price;
                        if (p_q[4] == "undefined" && p_q[2] != "undefined" && p_q[3] != "undefined")
                        {
                            int colorid = int.Parse(p_q[2].ToString());
                            int storageid = int.Parse(p_q[3].ToString());
                            SalesOrderLine salesOrderLine = new SalesOrderLine()
                            {
                                order_id = salesOrderid,
                                product_id = id,
                                quantity = _quantity,
                                unitPrice = (((price * 1) - (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().DiscountInAmount) - (((float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().Discount / 100) * (price * 1))),
                                netAmount = (((price * (float)_quantity) - (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().DiscountInAmount) - (((float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().Discount / 100) * (price * (float)_quantity))),
                                discountAmount = (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().DiscountInAmount,
                                discountPercent = (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().Discount,
                                billingAddressId = int.Parse(selectedBillAddr),
                                colorid = _db.ProductColors.FirstOrDefault(x => x.productId == id && x.colorid == colorid).Color.colorId,
                                storageid = _db.ProductStorages.FirstOrDefault(x => x.productid == id && x.storageid == storageid).Storage.id,
                                shippingAddessId = int.Parse(selectedShipAddr),
                                isConfirmed = 0
                            };
                            _db.SalesOrderLines.Add(salesOrderLine);
                            _db.SaveChanges();
                            var qtyProductMinus = _db.Products.FirstOrDefault(x => x.Product_ID == id);
                            qtyProductMinus.quantity = qtyProductMinus.quantity - _quantity;
                            _db.Entry(qtyProductMinus).State = EntityState.Modified;
                            _db.SaveChanges();
                            float eEarn = (float)((SelectedProductDto)Session[((CustomerDto)Session["customerSession"]).Id.ToString()]).grandTotal * (float)0.1;
                            EmartProfit emartProfit = new EmartProfit()
                            { SalesOrderID = salesOrderid, Amout = eEarn };
                            _db.EmartProfits.Add(emartProfit);
                            _db.SaveChanges();
                            int s_id = _db.SellerProducts.FirstOrDefault(x => x.productId == id).sellerId;
                            SellerProfit sellerProfit = new SellerProfit()
                            {
                                salesOrderLineId = salesOrderLine.id,
                                proId = id,
                                sellerId = s_id,
                                Amount = (price * _quantity) - ((price * _quantity) * 0.1),
                                Datetime = DateTime.Now.ToString()
                            };
                            _db.SellerProfits.Add(sellerProfit);
                            _db.SaveChanges();
                        }
                        else if (p_q[2] == "undefined" && p_q[4] != "undefined" && p_q[3] != "undefined")
                        {
                            int sizeid = int.Parse(p_q[4].ToString());
                            int storageid = int.Parse(p_q[3].ToString());
                            SalesOrderLine salesOrderLine = new SalesOrderLine()
                            {
                                order_id = salesOrderid,
                                product_id = id,
                                quantity = _quantity,
                                unitPrice = (((price * 1) - (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().DiscountInAmount) - (((float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().Discount / 100) * (price * 1))),
                                netAmount = (((price * (float)_quantity) - (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().DiscountInAmount) - (((float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().Discount / 100) * (price * (float)_quantity))),
                                discountAmount = (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().DiscountInAmount,
                                discountPercent = (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().Discount,
                                billingAddressId = int.Parse(selectedBillAddr),
                                sizeid = _db.ProductSizes.FirstOrDefault(x => x.productId == id && x.sizeid == sizeid).Size.sizeId,
                                storageid = _db.ProductStorages.FirstOrDefault(x => x.productid == id && x.storageid == storageid).Storage.id,
                                shippingAddessId = int.Parse(selectedShipAddr),
                                isConfirmed = 0
                            };
                            _db.SalesOrderLines.Add(salesOrderLine);
                            _db.SaveChanges();
                            var qtyProductMinus = _db.Products.FirstOrDefault(x => x.Product_ID == id);
                            qtyProductMinus.quantity = qtyProductMinus.quantity - _quantity;
                            _db.Entry(qtyProductMinus).State = EntityState.Modified;
                            _db.SaveChanges();
                            float eEarn = (float)((SelectedProductDto)Session[((CustomerDto)Session["customerSession"]).Id.ToString()]).grandTotal * (float)0.1;
                            EmartProfit emartProfit = new EmartProfit()
                            { SalesOrderID = salesOrderid, Amout = eEarn };
                            _db.EmartProfits.Add(emartProfit);
                            _db.SaveChanges();
                            int s_id = _db.SellerProducts.FirstOrDefault(x => x.productId == id).sellerId;
                            SellerProfit sellerProfit = new SellerProfit()
                            {
                                salesOrderLineId = salesOrderLine.id,
                                proId = id,
                                sellerId = s_id,
                                Amount = (price * _quantity) - ((price * _quantity) * 0.1),
                                Datetime = DateTime.Now.ToString()
                            };
                            _db.SellerProfits.Add(sellerProfit);
                            _db.SaveChanges();
                        }
                        else if (p_q[3] == "undefined" && p_q[2] != "undefined" && p_q[4] != "undefined")
                        {
                            int sizeid = int.Parse(p_q[4].ToString());
                            int colorid = int.Parse(p_q[2].ToString());
                            SalesOrderLine salesOrderLine = new SalesOrderLine()
                            {
                                order_id = salesOrderid,
                                product_id = id,
                                quantity = _quantity,
                                unitPrice = (((price * 1) - (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().DiscountInAmount) - (((float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().Discount / 100) * (price * 1))),
                                netAmount = (((price * (float)_quantity) - (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().DiscountInAmount) - (((float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().Discount / 100) * (price * (float)_quantity))),
                                discountAmount = (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().DiscountInAmount,
                                discountPercent = (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().Discount,
                                billingAddressId = int.Parse(selectedBillAddr),
                                sizeid = _db.ProductSizes.FirstOrDefault(x => x.productId == id && x.sizeid == sizeid).Size.sizeId,
                                colorid = _db.ProductColors.FirstOrDefault(x => x.productId == id && x.colorid == colorid).Color.colorId,
                                shippingAddessId = int.Parse(selectedShipAddr),
                                isConfirmed = 0
                            };
                            _db.SalesOrderLines.Add(salesOrderLine);
                            _db.SaveChanges();
                            var qtyProductMinus = _db.Products.FirstOrDefault(x => x.Product_ID == id);
                            qtyProductMinus.quantity = qtyProductMinus.quantity - _quantity;
                            _db.Entry(qtyProductMinus).State = EntityState.Modified;
                            _db.SaveChanges();
                            float eEarn = (float)((SelectedProductDto)Session[((CustomerDto)Session["customerSession"]).Id.ToString()]).grandTotal * (float)0.1;
                            EmartProfit emartProfit = new EmartProfit()
                            { SalesOrderID = salesOrderid, Amout = eEarn };
                            _db.EmartProfits.Add(emartProfit);
                            _db.SaveChanges();
                            int s_id = _db.SellerProducts.FirstOrDefault(x => x.productId == id).sellerId;
                            SellerProfit sellerProfit = new SellerProfit()
                            {
                                salesOrderLineId = salesOrderLine.id,
                                proId = id,
                                sellerId = s_id,
                                Amount = (price * _quantity) - ((price * _quantity) * 0.1),
                                Datetime = DateTime.Now.ToString()
                            };
                            _db.SellerProfits.Add(sellerProfit);
                            _db.SaveChanges();
                        }
                        else if (p_q[3] == "undefined" && p_q[2] == "undefined" && p_q[4] != "undefined")
                        {
                            int sizeid = int.Parse(p_q[4].ToString());
                            SalesOrderLine salesOrderLine = new SalesOrderLine()
                            {
                                order_id = salesOrderid,
                                product_id = id,
                                quantity = _quantity,
                                unitPrice = (((price * 1) - (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().DiscountInAmount) - (((float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().Discount / 100) * (price * 1))),
                                netAmount = (((price * (float)_quantity) - (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().DiscountInAmount) - (((float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().Discount / 100) * (price * (float)_quantity))),
                                discountAmount = (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().DiscountInAmount,
                                discountPercent = (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().Discount,
                                billingAddressId = int.Parse(selectedBillAddr),
                                sizeid = _db.ProductSizes.FirstOrDefault(x => x.productId == id && x.sizeid == sizeid).Size.sizeId,
                                shippingAddessId = int.Parse(selectedShipAddr),
                                isConfirmed = 0
                            };
                            _db.SalesOrderLines.Add(salesOrderLine);
                            _db.SaveChanges();
                            var qtyProductMinus = _db.Products.FirstOrDefault(x => x.Product_ID == id);
                            qtyProductMinus.quantity = qtyProductMinus.quantity - _quantity;
                            _db.Entry(qtyProductMinus).State = EntityState.Modified;
                            _db.SaveChanges();
                            float eEarn = (float)((SelectedProductDto)Session[((CustomerDto)Session["customerSession"]).Id.ToString()]).grandTotal * (float)0.1;
                            EmartProfit emartProfit = new EmartProfit()
                            { SalesOrderID = salesOrderid, Amout = eEarn };
                            _db.EmartProfits.Add(emartProfit);
                            _db.SaveChanges();
                            int s_id = _db.SellerProducts.FirstOrDefault(x => x.productId == id).sellerId;
                            SellerProfit sellerProfit = new SellerProfit()
                            {
                                salesOrderLineId = salesOrderLine.id,
                                proId = id,
                                sellerId = s_id,
                                Amount = (price * _quantity) - ((price * _quantity) * 0.1),
                                Datetime = DateTime.Now.ToString()
                            };
                            _db.SellerProfits.Add(sellerProfit);
                            _db.SaveChanges();
                        }
                        else if (p_q[4] == "undefined" && p_q[3] == "undefined" && p_q[2] != "undefined")
                        {
                            int colorid = int.Parse(p_q[2].ToString());
                            SalesOrderLine salesOrderLine = new SalesOrderLine()
                            {
                                order_id = salesOrderid,
                                product_id = id,
                                quantity = _quantity,
                                unitPrice = (((price * 1) - (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().DiscountInAmount) - (((float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().Discount / 100) * (price * 1))),
                                netAmount = (((price * (float)_quantity) - (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().DiscountInAmount) - (((float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().Discount / 100) * (price * (float)_quantity))),
                                discountAmount = (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().DiscountInAmount,
                                discountPercent = (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().Discount,
                                billingAddressId = int.Parse(selectedBillAddr),
                                colorid = _db.ProductColors.FirstOrDefault(x => x.productId == id && x.colorid == colorid).Color.colorId,
                                shippingAddessId = int.Parse(selectedShipAddr),
                                isConfirmed = 0
                            };
                            _db.SalesOrderLines.Add(salesOrderLine);
                            _db.SaveChanges();
                            var qtyProductMinus = _db.Products.FirstOrDefault(x => x.Product_ID == id);
                            qtyProductMinus.quantity = qtyProductMinus.quantity - _quantity;
                            _db.Entry(qtyProductMinus).State = EntityState.Modified;
                            _db.SaveChanges();
                            float eEarn = (float)((SelectedProductDto)Session[((CustomerDto)Session["customerSession"]).Id.ToString()]).grandTotal * (float)0.1;
                            EmartProfit emartProfit = new EmartProfit()
                            { SalesOrderID = salesOrderid, Amout = eEarn };
                            _db.EmartProfits.Add(emartProfit);
                            _db.SaveChanges();
                            int s_id = _db.SellerProducts.FirstOrDefault(x => x.productId == id).sellerId;
                            SellerProfit sellerProfit = new SellerProfit()
                            {
                                salesOrderLineId = salesOrderLine.id,
                                proId = id,
                                sellerId = s_id,
                                Amount = (price * _quantity) - ((price * _quantity) * 0.1),
                                Datetime = DateTime.Now.ToString()
                            };
                            _db.SellerProfits.Add(sellerProfit);
                            _db.SaveChanges();
                        }
                        else if (p_q[4] == "undefined" && p_q[2] == "undefined" && p_q[3] != "undefined")
                        {
                            int storageid = int.Parse(p_q[3].ToString());
                            SalesOrderLine salesOrderLine = new SalesOrderLine()
                            {
                                order_id = salesOrderid,
                                product_id = id,
                                quantity = _quantity,
                                unitPrice = (((price * 1) - (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().DiscountInAmount) - (((float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().Discount / 100) * (price * 1))),
                                netAmount = (((price * (float)_quantity) - (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().DiscountInAmount) - (((float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().Discount / 100) * (price * (float)_quantity))),
                                discountAmount = (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().DiscountInAmount,
                                discountPercent = (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().Discount,
                                billingAddressId = int.Parse(selectedBillAddr),
                                storageid = _db.ProductStorages.FirstOrDefault(x => x.productid == id && x.storageid == storageid).Storage.id,
                                shippingAddessId = int.Parse(selectedShipAddr),
                                isConfirmed = 0
                            };
                            _db.SalesOrderLines.Add(salesOrderLine);
                            _db.SaveChanges();
                            var qtyProductMinus = _db.Products.FirstOrDefault(x => x.Product_ID == id);
                            qtyProductMinus.quantity = qtyProductMinus.quantity - _quantity;
                            _db.Entry(qtyProductMinus).State = EntityState.Modified;
                            _db.SaveChanges();
                            float eEarn = (float)((SelectedProductDto)Session[((CustomerDto)Session["customerSession"]).Id.ToString()]).grandTotal * (float)0.1;
                            EmartProfit emartProfit = new EmartProfit()
                            { SalesOrderID = salesOrderid, Amout = eEarn };
                            _db.EmartProfits.Add(emartProfit);
                            _db.SaveChanges();
                            int s_id = _db.SellerProducts.FirstOrDefault(x => x.productId == id).sellerId;
                            SellerProfit sellerProfit = new SellerProfit()
                            {
                                salesOrderLineId = salesOrderLine.id,
                                proId = id,
                                sellerId = s_id,
                                Amount = (price * _quantity) - ((price * _quantity) * 0.1),
                                Datetime = DateTime.Now.ToString()
                            };
                            _db.SellerProfits.Add(sellerProfit);
                            _db.SaveChanges();
                        }
                        else if (p_q[3] != "undefined" && p_q[2] != "undefined" && p_q[4] != "undefined")
                        {
                            int storageid = int.Parse(p_q[3].ToString());
                            int sizeid = int.Parse(p_q[4].ToString());
                            int colorid = int.Parse(p_q[2].ToString());
                            SalesOrderLine salesOrderLine = new SalesOrderLine()
                            {
                                order_id = salesOrderid,
                                product_id = id,
                                quantity = _quantity,
                                unitPrice = (((price * 1) - (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().DiscountInAmount) - (((float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().Discount / 100) * (price * 1))),
                                netAmount = (((price * (float)_quantity) - (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().DiscountInAmount) - (((float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().Discount / 100) * (price * (float)_quantity))),
                                discountAmount = (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().DiscountInAmount,
                                discountPercent = (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().Discount,
                                billingAddressId = int.Parse(selectedBillAddr),
                                storageid = _db.ProductStorages.FirstOrDefault(x => x.productid == id && x.storageid == storageid).Storage.id,
                                colorid = _db.ProductColors.FirstOrDefault(x => x.productId == id && x.colorid == colorid).Color.colorId,
                                sizeid = _db.ProductSizes.FirstOrDefault(x => x.productId == id && x.sizeid == sizeid).Size.sizeId,
                                shippingAddessId = int.Parse(selectedShipAddr),
                                isConfirmed = 0
                            };
                            _db.SalesOrderLines.Add(salesOrderLine);
                            _db.SaveChanges();
                            var qtyProductMinus = _db.Products.FirstOrDefault(x => x.Product_ID == id);
                            qtyProductMinus.quantity = qtyProductMinus.quantity - _quantity;
                            _db.Entry(qtyProductMinus).State = EntityState.Modified;
                            _db.SaveChanges();
                            float eEarn = (float)((SelectedProductDto)Session[((CustomerDto)Session["customerSession"]).Id.ToString()]).grandTotal * (float)0.1;
                            EmartProfit emartProfit = new EmartProfit()
                            { SalesOrderID = salesOrderid, Amout = eEarn };
                            _db.EmartProfits.Add(emartProfit);
                            _db.SaveChanges();
                            int s_id = _db.SellerProducts.FirstOrDefault(x => x.productId == id).sellerId;
                            SellerProfit sellerProfit = new SellerProfit()
                            {
                                salesOrderLineId = salesOrderLine.id,
                                proId = id,
                                sellerId = s_id,
                                Amount = (price * _quantity) - ((price * _quantity) * 0.1),
                                Datetime = DateTime.Now.ToString()
                            };
                            _db.SellerProfits.Add(sellerProfit);
                            _db.SaveChanges();
                        }
                        else
                        {
                            SalesOrderLine salesOrderLine = new SalesOrderLine()
                            {
                                order_id = salesOrderid,
                                product_id = id,
                                quantity = _quantity,
                                unitPrice = (((price * 1) - (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().DiscountInAmount) - (((float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().Discount / 100) * (price * 1))),
                                netAmount = (((price * (float)_quantity) - (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().DiscountInAmount) - (((float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().Discount / 100) * (price * (float)_quantity))),
                                discountAmount = (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().DiscountInAmount,
                                discountPercent = (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().Discount,
                                billingAddressId = int.Parse(selectedBillAddr),
                                shippingAddessId = int.Parse(selectedShipAddr),
                                isConfirmed = 0
                            };
                            _db.SalesOrderLines.Add(salesOrderLine);
                            _db.SaveChanges();
                            var qtyProductMinus = _db.Products.FirstOrDefault(x => x.Product_ID == id);
                            qtyProductMinus.quantity = qtyProductMinus.quantity - _quantity;
                            _db.Entry(qtyProductMinus).State = EntityState.Modified;
                            _db.SaveChanges();
                            float eEarn = (float)((SelectedProductDto)Session[((CustomerDto)Session["customerSession"]).Id.ToString()]).grandTotal * (float)0.1;
                            EmartProfit emartProfit = new EmartProfit()
                            { SalesOrderID = salesOrderid, Amout = eEarn };
                            _db.EmartProfits.Add(emartProfit);
                            _db.SaveChanges();
                            int s_id = _db.SellerProducts.FirstOrDefault(x => x.productId == id).sellerId;
                            SellerProfit sellerProfit = new SellerProfit()
                            {
                                salesOrderLineId = salesOrderLine.id,
                                proId = id,
                                sellerId = s_id,
                                Amount = (price * _quantity) - ((price * _quantity) * 0.1),
                                Datetime = DateTime.Now.ToString()
                            };
                            _db.SellerProfits.Add(sellerProfit);
                            _db.SaveChanges();
                        }
                        int _cust_Id = ((CustomerDto)Session["customerSession"]).Id;
                        var DeleteItemfromCart = _db.Carts.FirstOrDefault(x => x.productId == id && x.customerId == _cust_Id);
                        if (DeleteItemfromCart != null)
                        {
                            _db.Entry(DeleteItemfromCart).State = EntityState.Deleted;
                            _db.SaveChanges();
                        }
                    }
                    var SellerIds = (from s in _db.Sellers
                                     join sp in _db.SellerProducts on s.sellerId equals sp.sellerId
                                     join p in _db.Products on sp.productId equals p.Product_ID
                                     join sol in _db.SalesOrderLines on p.Product_ID equals sol.product_id
                                     where sol.order_id == salesOrder.order_ID
                                     group s by s.sellerId into g
                                     select new
                                     { g }).ToList();
                    foreach (var item in SellerIds)
                    {
                        Notification notify = new Notification()
                        {
                            SellerId = item.g.First().sellerId,
                            CustomerId = (int)salesOrder.cust_Id,
                            Status = "unread",
                            Message = "You have new Message",
                            Date = DateTime.Now
                        };
                        _db.Notifications.Add(notify);
                        _db.SaveChanges();
                    }
                    return Json("/Customers/CustOrder", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
                    var match = regex.Match(gEmail);
                    if (gEmail == string.Empty && gPhone == string.Empty && gName == string.Empty)
                    {
                        return Json("Please Enter Name, Phone Number and Email Address", JsonRequestBehavior.AllowGet);
                    }
                    if (gEmail != string.Empty && gPhone != string.Empty && gName == string.Empty)
                    {
                        return Json("Please Enter Name", JsonRequestBehavior.AllowGet);
                    }
                    if (gEmail != string.Empty && gPhone == string.Empty && gName != string.Empty)
                    {
                        return Json("Please Enter Phone Number", JsonRequestBehavior.AllowGet);
                    }
                    if (gEmail == string.Empty && gPhone != string.Empty && gName != string.Empty)
                    {
                        return Json("Please Enter Your Email Address", JsonRequestBehavior.AllowGet);
                    }
                    if (gEmail == string.Empty)
                    {
                        return Json("Please Enter Your Email Address", JsonRequestBehavior.AllowGet);
                    }
                    if (!match.Success)
                    {
                        return Json("Invalid Email", JsonRequestBehavior.AllowGet);
                    }
                    if (gPhone == string.Empty)
                    {
                        return Json("Please Enter Phone Number", JsonRequestBehavior.AllowGet);
                    }
                    if (gName == string.Empty)
                    {
                        return Json("Please Enter Name", JsonRequestBehavior.AllowGet);
                    }
                    if (selectedBillAddr == "-1" && selectedShipAddr == "-1")
                    {
                        return Json("Please Select Shipping and Billing Address", JsonRequestBehavior.AllowGet);
                    }
                    if (selectedBillAddr == "-1" && selectedShipAddr != "-1")
                    {
                        return Json("Please Select Billing Address", JsonRequestBehavior.AllowGet);
                    }
                    if (selectedBillAddr != "-1" && selectedShipAddr == "-1")
                    {
                        return Json("Please Select Shipping Address", JsonRequestBehavior.AllowGet);
                    }

                    HttpCookie cookie = Request.Cookies["SID"];
                    string token = string.Empty;
                    //If Cookie is null then create new Cookie
                    if (cookie == null)
                    {
                        Response.Cookies["SID"].Values["x"] = Session.SessionID;
                        Response.Cookies["SID"].Expires = DateTime.MaxValue;
                    }
                    else
                    {
                        Session.Add("cookieId", cookie.Values["x"].ToString());
                        token = cookie.Values["x"].ToString();
                    }
                    if (token == string.Empty)
                        token = Response.Cookies["SID"].Values["x"].ToString();
                    TempData["ID"] = token.ToString();
                    string guestId = token;

                    //string guid = Guid.NewGuid().ToString();
                    SalesOrder salesOrder = new SalesOrder()
                    {
                        GuestId = token,
                        order_date = DateTime.Now.ToString(),
                        shipmentID = 1,
                        trackingID = ((SelectedProductDto)Session[token])._guid,
                        GrandTotal = ((SelectedProductDto)Session[token]).grandTotal,
                        GuestEmail = gEmail,
                        GuestPhoneNumber = gPhone,
                        GuestName = gName
                    };
                    _db.SalesOrders.Add(salesOrder);
                    _db.SaveChanges();
                    salesOrderid = (from so in _db.SalesOrders select so.order_ID).Max();
                    foreach (var item in ((SelectedProductDto)Session[token]).selectedItems)
                    {
                        string[] p_q = item.Split('|');
                        int id = int.Parse(p_q[0].ToString());
                        int _quantity = int.Parse(p_q[1].ToString());
                        float price = (float)_db.ProductPrices.Where(x => x.EndDate == null).FirstOrDefault(y => y.ProductID == id).Price;
                        if (p_q[4] == "undefined" && p_q[2] != "undefined" && p_q[3] != "undefined")
                        {
                            int colorid = int.Parse(p_q[2].ToString());
                            int storageid = int.Parse(p_q[3].ToString());
                            SalesOrderLine salesOrderLine = new SalesOrderLine()
                            {
                                order_id = salesOrderid,
                                product_id = id,
                                quantity = _quantity,
                                unitPrice = (((price * 1) - (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().DiscountInAmount) - (((float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().Discount / 100) * (price * 1))),
                                netAmount = (((price * (float)_quantity) - (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().DiscountInAmount) - (((float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().Discount / 100) * (price * (float)_quantity))),
                                discountAmount = (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().DiscountInAmount,
                                discountPercent = (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().Discount,
                                billingAddressId = int.Parse(selectedBillAddr),
                                colorid = _db.ProductColors.FirstOrDefault(x => x.productId == id && x.colorid == colorid).Color.colorId,
                                storageid = _db.ProductStorages.FirstOrDefault(x => x.productid == id && x.storageid == storageid).Storage.id,
                                shippingAddessId = int.Parse(selectedShipAddr),
                                isConfirmed = 0
                            };
                            _db.SalesOrderLines.Add(salesOrderLine);
                            _db.SaveChanges();
                            var qtyProductMinus = _db.Products.FirstOrDefault(x => x.Product_ID == id);
                            qtyProductMinus.quantity = qtyProductMinus.quantity - _quantity;
                            _db.Entry(qtyProductMinus).State = EntityState.Modified;
                            _db.SaveChanges();
                            float eEarn = (float)((SelectedProductDto)Session[token]).grandTotal * (float)0.1;
                            EmartProfit emartProfit = new EmartProfit()
                            { SalesOrderID = salesOrderid, Amout = eEarn };
                            _db.EmartProfits.Add(emartProfit);
                            _db.SaveChanges();
                            int s_id = _db.SellerProducts.FirstOrDefault(x => x.productId == id).sellerId;
                            SellerProfit sellerProfit = new SellerProfit()
                            {
                                salesOrderLineId = salesOrderLine.id,
                                proId = id,
                                sellerId = s_id,
                                Amount = (price * _quantity) - ((price * _quantity) * 0.1),
                                Datetime = DateTime.Now.ToString()
                            };
                            _db.SellerProfits.Add(sellerProfit);
                            _db.SaveChanges();
                        }
                        else if (p_q[2] == "undefined" && p_q[4] != "undefined" && p_q[3] != "undefined")
                        {
                            int sizeid = int.Parse(p_q[4].ToString());
                            int storageid = int.Parse(p_q[3].ToString());
                            SalesOrderLine salesOrderLine = new SalesOrderLine()
                            {
                                order_id = salesOrderid,
                                product_id = id,
                                quantity = _quantity,
                                unitPrice = (((price * 1) - (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().DiscountInAmount) - (((float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().Discount / 100) * (price * 1))),
                                netAmount = (((price * (float)_quantity) - (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().DiscountInAmount) - (((float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().Discount / 100) * (price * (float)_quantity))),
                                discountAmount = (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().DiscountInAmount,
                                discountPercent = (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().Discount,
                                billingAddressId = int.Parse(selectedBillAddr),
                                sizeid = _db.ProductSizes.FirstOrDefault(x => x.productId == id && x.sizeid == sizeid).Size.sizeId,
                                storageid = _db.ProductStorages.FirstOrDefault(x => x.productid == id && x.storageid == storageid).Storage.id,
                                shippingAddessId = int.Parse(selectedShipAddr),
                                isConfirmed = 0
                            };
                            _db.SalesOrderLines.Add(salesOrderLine);
                            _db.SaveChanges();
                            var qtyProductMinus = _db.Products.FirstOrDefault(x => x.Product_ID == id);
                            qtyProductMinus.quantity = qtyProductMinus.quantity - _quantity;
                            _db.Entry(qtyProductMinus).State = EntityState.Modified;
                            _db.SaveChanges();
                            float eEarn = (float)((SelectedProductDto)Session[token]).grandTotal * (float)0.1;
                            EmartProfit emartProfit = new EmartProfit()
                            { SalesOrderID = salesOrderid, Amout = eEarn };
                            _db.EmartProfits.Add(emartProfit);
                            _db.SaveChanges();
                            int s_id = _db.SellerProducts.FirstOrDefault(x => x.productId == id).sellerId;
                            SellerProfit sellerProfit = new SellerProfit()
                            {
                                salesOrderLineId = salesOrderLine.id,
                                proId = id,
                                sellerId = s_id,
                                Amount = (price * _quantity) - ((price * _quantity) * 0.1),
                                Datetime = DateTime.Now.ToString()
                            };
                            _db.SellerProfits.Add(sellerProfit);
                            _db.SaveChanges();
                        }
                        else if (p_q[3] == "undefined" && p_q[2] != "undefined" && p_q[4] != "undefined")
                        {
                            int sizeid = int.Parse(p_q[4].ToString());
                            int colorid = int.Parse(p_q[2].ToString());
                            SalesOrderLine salesOrderLine = new SalesOrderLine()
                            {
                                order_id = salesOrderid,
                                product_id = id,
                                quantity = _quantity,
                                unitPrice = (((price * 1) - (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().DiscountInAmount) - (((float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().Discount / 100) * (price * 1))),
                                netAmount = (((price * (float)_quantity) - (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().DiscountInAmount) - (((float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().Discount / 100) * (price * (float)_quantity))),
                                discountAmount = (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().DiscountInAmount,
                                discountPercent = (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().Discount,
                                billingAddressId = int.Parse(selectedBillAddr),
                                sizeid = _db.ProductSizes.FirstOrDefault(x => x.productId == id && x.sizeid == sizeid).Size.sizeId,
                                colorid = _db.ProductColors.FirstOrDefault(x => x.productId == id && x.colorid == colorid).Color.colorId,
                                shippingAddessId = int.Parse(selectedShipAddr),
                                isConfirmed = 0
                            };
                            _db.SalesOrderLines.Add(salesOrderLine);
                            _db.SaveChanges();
                            var qtyProductMinus = _db.Products.FirstOrDefault(x => x.Product_ID == id);
                            qtyProductMinus.quantity = qtyProductMinus.quantity - _quantity;
                            _db.Entry(qtyProductMinus).State = EntityState.Modified;
                            _db.SaveChanges();
                            float eEarn = (float)((SelectedProductDto)Session[token]).grandTotal * (float)0.1;
                            EmartProfit emartProfit = new EmartProfit()
                            { SalesOrderID = salesOrderid, Amout = eEarn };
                            _db.EmartProfits.Add(emartProfit);
                            _db.SaveChanges();
                            int s_id = _db.SellerProducts.FirstOrDefault(x => x.productId == id).sellerId;
                            SellerProfit sellerProfit = new SellerProfit()
                            {
                                salesOrderLineId = salesOrderLine.id,
                                proId = id,
                                sellerId = s_id,
                                Amount = (price * _quantity) - ((price * _quantity) * 0.1),
                                Datetime = DateTime.Now.ToString()
                            };
                            _db.SellerProfits.Add(sellerProfit);
                            _db.SaveChanges();
                        }
                        else if (p_q[3] == "undefined" && p_q[2] == "undefined" && p_q[4] != "undefined")
                        {
                            int sizeid = int.Parse(p_q[4].ToString());
                            SalesOrderLine salesOrderLine = new SalesOrderLine()
                            {
                                order_id = salesOrderid,
                                product_id = id,
                                quantity = _quantity,
                                unitPrice = (((price * 1) - (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().DiscountInAmount) - (((float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().Discount / 100) * (price * 1))),
                                netAmount = (((price * (float)_quantity) - (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().DiscountInAmount) - (((float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().Discount / 100) * (price * (float)_quantity))),
                                discountAmount = (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().DiscountInAmount,
                                discountPercent = (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().Discount,
                                billingAddressId = int.Parse(selectedBillAddr),
                                sizeid = _db.ProductSizes.FirstOrDefault(x => x.productId == id && x.sizeid == sizeid).Size.sizeId,
                                shippingAddessId = int.Parse(selectedShipAddr),
                                isConfirmed = 0
                            };
                            _db.SalesOrderLines.Add(salesOrderLine);
                            _db.SaveChanges();
                            var qtyProductMinus = _db.Products.FirstOrDefault(x => x.Product_ID == id);
                            qtyProductMinus.quantity = qtyProductMinus.quantity - _quantity;
                            _db.Entry(qtyProductMinus).State = EntityState.Modified;
                            _db.SaveChanges();
                            float eEarn = (float)((SelectedProductDto)Session[token]).grandTotal * (float)0.1;
                            EmartProfit emartProfit = new EmartProfit()
                            { SalesOrderID = salesOrderid, Amout = eEarn };
                            _db.EmartProfits.Add(emartProfit);
                            _db.SaveChanges();
                            int s_id = _db.SellerProducts.FirstOrDefault(x => x.productId == id).sellerId;
                            SellerProfit sellerProfit = new SellerProfit()
                            {
                                salesOrderLineId = salesOrderLine.id,
                                proId = id,
                                sellerId = s_id,
                                Amount = (price * _quantity) - ((price * _quantity) * 0.1),
                                Datetime = DateTime.Now.ToString()
                            };
                            _db.SellerProfits.Add(sellerProfit);
                            _db.SaveChanges();
                        }
                        else if (p_q[4] == "undefined" && p_q[3] == "undefined" && p_q[2] != "undefined")
                        {
                            int colorid = int.Parse(p_q[2].ToString());
                            SalesOrderLine salesOrderLine = new SalesOrderLine()
                            {
                                order_id = salesOrderid,
                                product_id = id,
                                quantity = _quantity,
                                unitPrice = (((price * 1) - (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().DiscountInAmount) - (((float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().Discount / 100) * (price * 1))),
                                netAmount = (((price * (float)_quantity) - (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().DiscountInAmount) - (((float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().Discount / 100) * (price * (float)_quantity))),
                                discountAmount = (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().DiscountInAmount,
                                discountPercent = (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().Discount,
                                billingAddressId = int.Parse(selectedBillAddr),
                                colorid = _db.ProductColors.FirstOrDefault(x => x.productId == id && x.colorid == colorid).Color.colorId,
                                shippingAddessId = int.Parse(selectedShipAddr),
                                isConfirmed = 0
                            };
                            _db.SalesOrderLines.Add(salesOrderLine);
                            _db.SaveChanges();
                            var qtyProductMinus = _db.Products.FirstOrDefault(x => x.Product_ID == id);
                            qtyProductMinus.quantity = qtyProductMinus.quantity - _quantity;
                            _db.Entry(qtyProductMinus).State = EntityState.Modified;
                            _db.SaveChanges();
                            float eEarn = (float)((SelectedProductDto)Session[token]).grandTotal * (float)0.1;
                            EmartProfit emartProfit = new EmartProfit()
                            { SalesOrderID = salesOrderid, Amout = eEarn };
                            _db.EmartProfits.Add(emartProfit);
                            _db.SaveChanges();
                            int s_id = _db.SellerProducts.FirstOrDefault(x => x.productId == id).sellerId;
                            SellerProfit sellerProfit = new SellerProfit()
                            {
                                salesOrderLineId = salesOrderLine.id,
                                proId = id,
                                sellerId = s_id,
                                Amount = (price * _quantity) - ((price * _quantity) * 0.1),
                                Datetime = DateTime.Now.ToString()
                            };
                            _db.SellerProfits.Add(sellerProfit);
                            _db.SaveChanges();
                        }
                        else if (p_q[4] == "undefined" && p_q[2] == "undefined" && p_q[3] != "undefined")
                        {
                            int storageid = int.Parse(p_q[3].ToString());
                            SalesOrderLine salesOrderLine = new SalesOrderLine()
                            {
                                order_id = salesOrderid,
                                product_id = id,
                                quantity = _quantity,
                                unitPrice = (((price * 1) - (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().DiscountInAmount) - (((float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().Discount / 100) * (price * 1))),
                                netAmount = (((price * (float)_quantity) - (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().DiscountInAmount) - (((float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().Discount / 100) * (price * (float)_quantity))),
                                discountAmount = (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().DiscountInAmount,
                                discountPercent = (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().Discount,
                                billingAddressId = int.Parse(selectedBillAddr),
                                storageid = _db.ProductStorages.FirstOrDefault(x => x.productid == id && x.storageid == storageid).Storage.id,
                                shippingAddessId = int.Parse(selectedShipAddr),
                                isConfirmed = 0
                            };
                            _db.SalesOrderLines.Add(salesOrderLine);
                            _db.SaveChanges();
                            var qtyProductMinus = _db.Products.FirstOrDefault(x => x.Product_ID == id);
                            qtyProductMinus.quantity = qtyProductMinus.quantity - _quantity;
                            _db.Entry(qtyProductMinus).State = EntityState.Modified;
                            _db.SaveChanges();
                            float eEarn = (float)((SelectedProductDto)Session[token]).grandTotal * (float)0.1;
                            EmartProfit emartProfit = new EmartProfit()
                            { SalesOrderID = salesOrderid, Amout = eEarn };
                            _db.EmartProfits.Add(emartProfit);
                            _db.SaveChanges();
                            int s_id = _db.SellerProducts.FirstOrDefault(x => x.productId == id).sellerId;
                            SellerProfit sellerProfit = new SellerProfit()
                            {
                                salesOrderLineId = salesOrderLine.id,
                                proId = id,
                                sellerId = s_id,
                                Amount = (price * _quantity) - ((price * _quantity) * 0.1),
                                Datetime = DateTime.Now.ToString()
                            };
                            _db.SellerProfits.Add(sellerProfit);
                            _db.SaveChanges();
                        }
                        else if (p_q[3] != "undefined" && p_q[2] != "undefined" && p_q[4] != "undefined")
                        {
                            int storageid = int.Parse(p_q[3].ToString());
                            int sizeid = int.Parse(p_q[4].ToString());
                            int colorid = int.Parse(p_q[2].ToString());
                            SalesOrderLine salesOrderLine = new SalesOrderLine()
                            {
                                order_id = salesOrderid,
                                product_id = id,
                                quantity = _quantity,
                                unitPrice = (((price * 1) - (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().DiscountInAmount) - (((float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().Discount / 100) * (price * 1))),
                                netAmount = (((price * (float)_quantity) - (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().DiscountInAmount) - (((float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().Discount / 100) * (price * (float)_quantity))),
                                discountAmount = (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().DiscountInAmount,
                                discountPercent = (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().Discount,
                                billingAddressId = int.Parse(selectedBillAddr),
                                storageid = _db.ProductStorages.FirstOrDefault(x => x.productid == id && x.storageid == storageid).Storage.id,
                                colorid = _db.ProductColors.FirstOrDefault(x => x.productId == id && x.colorid == colorid).Color.colorId,
                                sizeid = _db.ProductSizes.FirstOrDefault(x => x.productId == id && x.sizeid == sizeid).Size.sizeId,
                                shippingAddessId = int.Parse(selectedShipAddr),
                                isConfirmed = 0
                            };
                            _db.SalesOrderLines.Add(salesOrderLine);
                            _db.SaveChanges();
                            var qtyProductMinus = _db.Products.FirstOrDefault(x => x.Product_ID == id);
                            qtyProductMinus.quantity = qtyProductMinus.quantity - _quantity;
                            _db.Entry(qtyProductMinus).State = EntityState.Modified;
                            _db.SaveChanges();
                            float eEarn = (float)((SelectedProductDto)Session[token]).grandTotal * (float)0.1;
                            EmartProfit emartProfit = new EmartProfit()
                            { SalesOrderID = salesOrderid, Amout = eEarn };
                            _db.EmartProfits.Add(emartProfit);
                            _db.SaveChanges();
                            int s_id = _db.SellerProducts.FirstOrDefault(x => x.productId == id).sellerId;
                            SellerProfit sellerProfit = new SellerProfit()
                            {
                                salesOrderLineId = salesOrderLine.id,
                                proId = id,
                                sellerId = s_id,
                                Amount = (price * _quantity) - ((price * _quantity) * 0.1),
                                Datetime = DateTime.Now.ToString()
                            };
                            _db.SellerProfits.Add(sellerProfit);
                            _db.SaveChanges();
                        }
                        else
                        {
                            SalesOrderLine salesOrderLine = new SalesOrderLine()
                            {
                                order_id = salesOrderid,
                                product_id = id,
                                quantity = _quantity,
                                unitPrice = (((price * 1) - (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().DiscountInAmount) - (((float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().Discount / 100) * (price * 1))),
                                netAmount = (((price * (float)_quantity) - (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().DiscountInAmount) - (((float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().Discount / 100) * (price * (float)_quantity))),
                                discountAmount = (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().DiscountInAmount,
                                discountPercent = (float)_db.Products.Where(x => x.Product_ID == id).FirstOrDefault().Discount,
                                billingAddressId = int.Parse(selectedBillAddr),
                                shippingAddessId = int.Parse(selectedShipAddr),
                                isConfirmed = 0
                            };
                            _db.SalesOrderLines.Add(salesOrderLine);
                            _db.SaveChanges();
                            var qtyProductMinus = _db.Products.FirstOrDefault(x => x.Product_ID == id);
                            qtyProductMinus.quantity = qtyProductMinus.quantity - _quantity;
                            _db.Entry(qtyProductMinus).State = EntityState.Modified;
                            _db.SaveChanges();
                            float eEarn = (float)((SelectedProductDto)Session[token]).grandTotal * (float)0.1;
                            EmartProfit emartProfit = new EmartProfit()
                            { SalesOrderID = salesOrderid, Amout = eEarn };
                            _db.EmartProfits.Add(emartProfit);
                            _db.SaveChanges();
                            int s_id = _db.SellerProducts.FirstOrDefault(x => x.productId == id).sellerId;
                            SellerProfit sellerProfit = new SellerProfit()
                            {
                                salesOrderLineId = salesOrderLine.id,
                                proId = id,
                                sellerId = s_id,
                                Amount = (price * _quantity) - ((price * _quantity) * 0.1),
                                Datetime = DateTime.Now.ToString()
                            };
                            _db.SellerProfits.Add(sellerProfit);
                            _db.SaveChanges();
                        }
                        var DeleteItemfromCart = _db.Carts.FirstOrDefault(x => x.productId == id && x.GuestId == token);
                        if (DeleteItemfromCart != null)
                        {
                            _db.Entry(DeleteItemfromCart).State = EntityState.Deleted;
                            _db.SaveChanges();
                        }
                    }
                    var SellerIds = (from s in _db.Sellers
                                     join sp in _db.SellerProducts on s.sellerId equals sp.sellerId
                                     join p in _db.Products on sp.productId equals p.Product_ID
                                     join sol in _db.SalesOrderLines on p.Product_ID equals sol.product_id
                                     where sol.order_id == salesOrder.order_ID
                                     group s by s.sellerId into g
                                     select new
                                     { g }).ToList();
                    foreach (var item in SellerIds)
                    {
                        Notification notify = new Notification()
                        {
                            SellerId = item.g.First().sellerId,
                            GuestId = salesOrder.GuestId,
                            Status = "unread",
                            Message = "You have new Message",
                            Date = DateTime.Now
                        };
                        _db.Notifications.Add(notify);
                        _db.SaveChanges();
                    }
                    return Json("/Customers/CustOrder", JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                ViewBag.e = e;
                return Json("/Customers/ErrorPage", JsonRequestBehavior.AllowGet);
            }
        }

        [TrackExecution]
        [HttpPost]
        public JsonResult SelectedProduct(List<string> pq, int ProductPriceTotal)
        {
            try
            {
                if (Session["customerSession"] != null)
                {
                    TempData["ID"] = ((CustomerDto)Session["customerSession"]).Id.ToString();
                    if (pq == null)
                    {
                        return Json("pq is null");
                    }
                    else
                    {
                        //TotProductPrice = 0;
                        //selectedProductQuantity = new List<string>();
                        //TotProductPrice = ProductPriceTotal;
                        string guid = Guid.NewGuid().ToString();
                        var temp = pq.Distinct();
                        SelectedProductDto selectedProductDto = new SelectedProductDto()
                        {
                            selectedItems = temp.ToList(),
                            _guid = guid,
                            totalPrice = ProductPriceTotal,
                            grandTotal = (float)_db.Shipments.FirstOrDefault().deliveryCharges + ProductPriceTotal
                        };
                        Session[((CustomerDto)Session["customerSession"]).Id.ToString()] = selectedProductDto;
                        //foreach (var item in temp)
                        //{
                        //    selectedProductQuantity.Add(item);
                        //}
                        return Json("/Customers/BuyAndCheckOut");
                    }
                }
                else
                {
                    HttpCookie cookie = Request.Cookies["SID"];
                    string token = string.Empty;
                    if (cookie == null)
                    {
                        Response.Cookies["SID"].Values["x"] = Session.SessionID;
                        Response.Cookies["SID"].Expires = DateTime.MaxValue;
                    }
                    else
                    {
                        Session.Add("cookieId", cookie.Values["x"].ToString());
                        token = cookie.Values["x"].ToString();
                    }
                    if (token == string.Empty)
                        token = Response.Cookies["SID"].Values["x"].ToString();
                    TempData["ID"] = token.ToString();
                    if (pq == null)
                    {
                        return Json("pq is null");
                    }
                    else
                    {
                        //TotProductPrice = 0;
                        //selectedProductQuantity = new List<string>();
                        //TotProductPrice = ProductPriceTotal;
                        string guid = Guid.NewGuid().ToString();
                        var temp = pq.Distinct();
                        SelectedProductDto selectedProductDto = new SelectedProductDto()
                        {
                            selectedItems = temp.ToList(),
                            _guid = guid,
                            totalPrice = ProductPriceTotal,
                            grandTotal = (float)_db.Shipments.FirstOrDefault().deliveryCharges + ProductPriceTotal
                        };
                        Session[token] = selectedProductDto;
                        //foreach (var item in temp)
                        //{
                        //    selectedProductQuantity.Add(item);
                        //}
                        return Json("/Customers/BuyAndCheckOut");
                    }

                }
            }
            catch (Exception e)
            {
                ViewBag.e = e;
                //return Redirect("~/Customers/ErrorPage");
                return Json("/Customers/ErrorPage");
            }
        }
        [TrackExecution]
        public ActionResult loadAddress(int a_id)
        {
            try
            {
                var name = _db.Addresses.FirstOrDefault(s => s.Id == a_id).Address1;
                var area = (from a in _db.Areas
                            join ad in _db.Addresses on a.Area_ID equals ad.Area_ID
                            select a.Area_Name).FirstOrDefault();
                var city = (from c in _db.Cities
                            join a in _db.Areas on c.City_ID equals a.City_ID
                            select c.City_Name).FirstOrDefault();
                var state = (from s in _db.States
                             join c in _db.Cities on s.State_ID equals c.State_ID
                             select s.State_Name).FirstOrDefault();
                string _addr = state + " ," + city + " ," + area + " ," + name;
                return Json(_addr);
            }
            catch (Exception e)
            {
                ViewBag.e = e;
                return Redirect("~/Customers/ErrorPage");
            }
        }
        [NoDirectAccess]
        [TrackExecution]
        [HttpGet]
        public ActionResult CustOrder()
        {
            try
            {
                if (Session["customerSession"] != null)
                {
                    TempData["ID"] = ((CustomerDto)Session["customerSession"]).Id.ToString();
                    ViewBag.Tot = ((SelectedProductDto)Session[((CustomerDto)Session["customerSession"]).Id.ToString()]).grandTotal;
                }
                else
                {
                    HttpCookie cookie = Request.Cookies["SID"];
                    string token = string.Empty;
                    //If Cookie is null then create new Cookie
                    if (cookie == null)
                    {
                        Response.Cookies["SID"].Values["x"] = Session.SessionID;
                        Response.Cookies["SID"].Expires = DateTime.MaxValue;
                    }
                    else
                    {
                        Session.Add("cookieId", cookie.Values["x"].ToString());
                        token = cookie.Values["x"].ToString();
                    }
                    if (token == string.Empty)
                        token = Response.Cookies["SID"].Values["x"].ToString();
                    TempData["ID"] = token.ToString();
                    ViewBag.Tot = ((SelectedProductDto)Session[token]).grandTotal;
                }
                return View();
            }
            catch (Exception e)
            {
                ViewBag.e = e;
                return Redirect("~/Customers/ErrorPage");
            }
        }
        [TrackExecution]
        [HttpPost]
        public ActionResult CustOrder(string cp)
        {
            try
            {
                if (Session["customerSession"] != null)
                {
                    TempData["ID"] = ((CustomerDto)Session["customerSession"]).Id.ToString();
                    var updatePayment = _db.SalesOrders.SingleOrDefault(x => x.order_ID == salesOrderid);
                    updatePayment.payment_id = int.Parse(cp);
                    _db.Entry(updatePayment).State = EntityState.Modified;
                    _db.SaveChanges();
                    TempData["OrderSuccess"] = "Your Order has been placed Successfully!";
                    return Redirect("~/Customers/SummaryStatus");
                }
                else
                {
                    HttpCookie cookie = Request.Cookies["SID"];
                    string token = string.Empty;
                    //If Cookie is null then create new Cookie
                    if (cookie == null)
                    {
                        Response.Cookies["SID"].Values["x"] = Session.SessionID;
                        Response.Cookies["SID"].Expires = DateTime.MaxValue;
                    }
                    else
                    {
                        Session.Add("cookieId", cookie.Values["x"].ToString());
                        token = cookie.Values["x"].ToString();
                    }
                    if (token == string.Empty)
                        token = Response.Cookies["SID"].Values["x"].ToString();
                    TempData["ID"] = token.ToString();
                    var updatePayment = _db.SalesOrders.SingleOrDefault(x => x.order_ID == salesOrderid);
                    updatePayment.payment_id = int.Parse(cp);
                    _db.Entry(updatePayment).State = EntityState.Modified;
                    _db.SaveChanges();
                    TempData["OrderSuccess"] = "Your Order has been placed Successfully!";
                    return Redirect("~/Customers/SummaryStatus");
                }
            }
            catch (Exception e)
            {
                ViewBag.e = e;
                return Redirect("~/Customers/ErrorPage");
            }
        }
        [NoDirectAccess]
        [TrackExecution]
        [HttpGet]
        public ActionResult SummaryStatus()
        {
            try
            {
                if (Session["customerSession"] != null)
                {
                    TempData["ID"] = ((CustomerDto)Session["customerSession"]).Id.ToString();
                    ViewBag.tot = ((SelectedProductDto)Session[((CustomerDto)Session["customerSession"]).Id.ToString()]).grandTotal;
                    ViewBag.OrderId = ((SelectedProductDto)Session[((CustomerDto)Session["customerSession"]).Id.ToString()])._guid;
                }
                else
                {
                    HttpCookie cookie = Request.Cookies["SID"];
                    string token = string.Empty;
                    //If Cookie is null then create new Cookie
                    if (cookie == null)
                    {
                        Response.Cookies["SID"].Values["x"] = Session.SessionID;
                        Response.Cookies["SID"].Expires = DateTime.MaxValue;
                    }
                    else
                    {
                        Session.Add("cookieId", cookie.Values["x"].ToString());
                        token = cookie.Values["x"].ToString();
                    }
                    if (token == string.Empty)
                        token = Response.Cookies["SID"].Values["x"].ToString();
                    TempData["ID"] = token.ToString();
                    ViewBag.OrderId = ((SelectedProductDto)Session[token])._guid;
                    ViewBag.tot = ((SelectedProductDto)Session[token]).grandTotal;
                }
                return View();
            }
            catch (Exception e)
            {
                ViewBag.e = e;
                return Redirect("~/Customers/ErrorPage");
            }
        }
        [TrackExecution]
        [HttpGet]
        public ActionResult Products()//Products after Searched //Stil some work remaining is addtocart and addtowishlist
        {
            try
            {
                if (Session["customerSession"] != null)
                {
                    int cid = ((CustomerDto)Session["customerSession"]).Id;
                    TempData["ID"] = cid.ToString();
                    var abc = Session["id"] as List<int>;
                    var def = Session["catid"] as List<int>;
                    var ghi = Session["subcatid"] as List<int>;
                    var jkl = Session["subsubcatid"] as List<int>;
                    var mno = Session["brandid"] as List<int>;
                    List<ProductDetailDto> datal = (from id in abc
                                                    join p in _db.Products on id equals p.Product_ID
                                                    join pp in _db.ProductPrices on p.Product_ID equals pp.ProductID
                                                    where pp.EndDate == null && p.Product_ID == id
                                                    select new ProductDetailDto
                                                    {
                                                        id = p.Product_ID,
                                                        pName = p.Product_Name,
                                                        pPrice = (float)pp.Price,
                                                        img1 = p.Product_Image.First().Image_1,
                                                        qty = p.quantity,
                                                        colorid = (_db.ProductColors.FirstOrDefault(x => x.productId == p.Product_ID))?.Color.colorId,
                                                        storageid = (_db.ProductStorages.FirstOrDefault(x => x.productid == p.Product_ID))?.Storage.id,
                                                        sizeid = (_db.ProductSizes.FirstOrDefault(x => x.productId == p.Product_ID))?.Size.sizeId,
                                                        wishlistId = (_db.WishLists.FirstOrDefault(x => x.SellerProduct.productId == p.Product_ID && x.customerId == cid))?.id,
                                                        cartId = (_db.Carts.FirstOrDefault(x => x.customerId == cid && x.productId == p.Product_ID))?.cartId,
                                                        newPrice = (float)(((pp.Price * 1) - (float)p.DiscountInAmount) - (((float)p.Discount / 100) * (pp.Price * 1))),
                                                        pDiscountAmount = (float)p.DiscountInAmount,
                                                        pDiscountPercent = (float)p.Discount,
                                                        rating = ((float)_db.RatingsAndReviews.Where(y => y.rating == 1).Count(x => x.productId == p.Product_ID) + (float)_db.RatingsAndReviews.Where(y => y.rating == 2).Count(x => x.productId == p.Product_ID) + (float)_db.RatingsAndReviews.Where(y => y.rating == 3).Count(x => x.productId == p.Product_ID) + (float)_db.RatingsAndReviews.Where(y => y.rating == 4).Count(x => x.productId == p.Product_ID) + (float)_db.RatingsAndReviews.Where(y => y.rating == 5).Count(x => x.productId == p.Product_ID)) / (float)5
                                                    }).ToList();
                    ViewBag.SearchCount = datal.Count;
                    if (datal.Count == 0)
                    {
                        ViewBag.MaxPrice = 0;
                    }
                    else
                    {
                        ViewBag.MaxPrice = datal.Max(x => x.newPrice);
                    }
                    var cat = (from id in def join c in _db.Categories on id equals c.Cat_ID select new ProductDetailDto { catid = c.Cat_ID, catname = c.Cat_Name }).ToList();
                    var subcat = (from id in ghi join c in _db.Sub_Category on id equals c.Sub_Category_ID select new ProductDetailDto { subcatid = c.Sub_Category_ID, subcatname = c.Sub_Name }).ToList();
                    var subsubcat = (from id in jkl join c in _db.SubSubCategories on id equals c.SubSubCategoryID select new ProductDetailDto { subsubcatid = c.SubSubCategoryID, subsubcatname = c.SubSubCategoryName }).ToList();
                    var brand = (from id in mno join c in _db.Brands on id equals c.BrandId select new ProductDetailDto { brandId = c.BrandId, bName = c.Brand_Name }).ToList();
                    ViewBag.cat = cat;
                    ViewBag.subcat = subcat;
                    ViewBag.subsubcat = subsubcat;
                    ViewBag.brand = brand;
                    return View(datal);
                }
                else
                {
                    HttpCookie cookie = Request.Cookies["SID"];
                    string token = string.Empty;
                    if (cookie == null)
                    {
                        Response.Cookies["SID"].Values["x"] = Session.SessionID;
                        Response.Cookies["SID"].Expires = DateTime.MaxValue;
                    }
                    else
                    {
                        Session.Add("cookieId", cookie.Values["x"].ToString());
                        token = cookie.Values["x"].ToString();
                    }
                    if (token == string.Empty)
                        token = Response.Cookies["SID"].Values["x"].ToString();
                    TempData["ID"] = token.ToString();
                    var abc = Session["id"] as List<int>;
                    var def = Session["catid"] as List<int>;
                    var ghi = Session["subcatid"] as List<int>;
                    var jkl = Session["subsubcatid"] as List<int>;
                    var mno = Session["brandid"] as List<int>;
                    List<ProductDetailDto> datal = (from id in abc
                                                    join p in _db.Products on id equals p.Product_ID
                                                    join pp in _db.ProductPrices on p.Product_ID equals pp.ProductID
                                                    where pp.EndDate == null && p.Product_ID == id
                                                    select new ProductDetailDto
                                                    {
                                                        id = p.Product_ID,
                                                        pName = p.Product_Name,
                                                        pPrice = (float)pp.Price,
                                                        img1 = p.Product_Image.First().Image_1,
                                                        qty = p.quantity,
                                                        colorid = (_db.ProductColors.FirstOrDefault(x => x.productId == p.Product_ID))?.Color.colorId,
                                                        storageid = (_db.ProductStorages.FirstOrDefault(x => x.productid == p.Product_ID))?.Storage.id,
                                                        sizeid = (_db.ProductSizes.FirstOrDefault(x => x.productId == p.Product_ID))?.Size.sizeId,
                                                        wishlistId = (_db.WishLists.FirstOrDefault(x => x.SellerProduct.productId == p.Product_ID && x.GuestId == token))?.id,
                                                        cartId = (_db.Carts.FirstOrDefault(x => x.GuestId == token && x.productId == p.Product_ID))?.cartId,
                                                        newPrice = (float)(((pp.Price * 1) - (float)p.DiscountInAmount) - (((float)p.Discount / 100) * (pp.Price * 1))),
                                                        pDiscountAmount = (float)p.DiscountInAmount,
                                                        pDiscountPercent = (float)p.Discount,
                                                        rating = ((float)_db.RatingsAndReviews.Where(y => y.rating == 1).Count(x => x.productId == p.Product_ID) + (float)_db.RatingsAndReviews.Where(y => y.rating == 2).Count(x => x.productId == p.Product_ID) + (float)_db.RatingsAndReviews.Where(y => y.rating == 3).Count(x => x.productId == p.Product_ID) + (float)_db.RatingsAndReviews.Where(y => y.rating == 4).Count(x => x.productId == p.Product_ID) + (float)_db.RatingsAndReviews.Where(y => y.rating == 5).Count(x => x.productId == p.Product_ID)) / (float)5
                                                    }).ToList();
                    ViewBag.SearchCount = datal.Count;
                    if (datal.Count == 0)
                    {
                        ViewBag.MaxPrice = 0;
                    }
                    else
                    {
                        ViewBag.MaxPrice = datal.Max(x => x.newPrice);
                    }
                    var cat = (from id in def join c in _db.Categories on id equals c.Cat_ID select new ProductDetailDto { catid = c.Cat_ID, catname = c.Cat_Name }).ToList();
                    var subcat = (from id in ghi join c in _db.Sub_Category on id equals c.Sub_Category_ID select new ProductDetailDto { subcatid = c.Sub_Category_ID, subcatname = c.Sub_Name }).ToList();
                    var subsubcat = (from id in jkl join c in _db.SubSubCategories on id equals c.SubSubCategoryID select new ProductDetailDto { subsubcatid = c.SubSubCategoryID, subsubcatname = c.SubSubCategoryName }).ToList();
                    var brand = (from id in mno join c in _db.Brands on id equals c.BrandId select new ProductDetailDto { brandId = c.BrandId, bName = c.Brand_Name }).ToList();
                    ViewBag.cat = cat;
                    ViewBag.subcat = subcat;
                    ViewBag.subsubcat = subsubcat;
                    ViewBag.brand = brand;
                    return View(datal);
                }
            }
            catch (Exception e)
            {
                ViewBag.e = e;
                return Redirect("~/Customers/Index");
            }
        }
        [TrackExecution]
        [HttpPost]
        public JsonResult KeywordForSearch(string KeyForSearch)//On enter => search
        {
            try
            {
                KeyForSearch = KeyForSearch.Trim();
                List<int> _ids = new List<int>();
                List<int> _catId = new List<int>();
                List<int> _subcatId = new List<int>();
                List<int> _subsubcatId = new List<int>();
                List<int> _brandId = new List<int>();
                var products = _db.Products.Where(p => p.Product_Name.Contains(KeyForSearch)).Select(y => y.Product_ID);
                var category = _db.Categories.Where(p => p.Cat_Name.Contains(KeyForSearch)).Select(y => y.Cat_ID);
                var subcategory = _db.Sub_Category.Where(p => p.Sub_Name.Contains(KeyForSearch)).Select(y => y.Sub_Category_ID);
                var subsubcategory = _db.SubSubCategories.Where(p => p.SubSubCategoryName.Contains(KeyForSearch)).Select(y => y.SubSubCategoryID);
                var brand = _db.Brands.Where(p => p.Brand_Name.Contains(KeyForSearch)).Select(y => y.BrandId);
                foreach (var i in products)
                { int id = int.Parse(i.ToString()); _ids.Add(id); }
                foreach (var i in category)
                { int id = int.Parse(i.ToString()); _catId.Add(id); }
                foreach (var i in subcategory)
                { int id = int.Parse(i.ToString()); _subcatId.Add(id); }
                foreach (var i in subsubcategory)
                { int id = int.Parse(i.ToString()); _subsubcatId.Add(id); }
                foreach (var i in brand)
                { int id = int.Parse(i.ToString()); _brandId.Add(id); }
                Session["id"] = _ids.ToList();
                Session["catid"] = _catId.ToList();
                Session["subcatid"] = _subcatId.ToList();
                Session["subsubcatid"] = _subsubcatId.ToList();
                Session["brandid"] = _brandId.ToList();
                var response = new { uri = "Search/Products", data = "Nothing" };
                return Json(response);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        [TrackExecution]
        [HttpPost]
        public JsonResult productSearchEnter(string KeyForSearch)// On keyup => search
        {
            try
            {
                KeyForSearch = KeyForSearch.Trim();
                var products = _db.Products.Where(p => p.Product_Name.Contains(KeyForSearch)).Select(y => y.Product_Name);
                var pr = (from p in _db.Products
                          join pi in _db.Product_Image on p.Product_ID equals pi.Product_ID
                          where (p.Product_Name.Contains(KeyForSearch))
                          select new autocomplete { name = p.Product_Name.TrimEnd(), img = pi.Image_1 });
                return Json(pr.ToList(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public static string orderNumber = "";

        [TrackExecution]
        [HttpGet]
        public async Task<ActionResult> Tracking()
        {
            try
            {
                if (Session["customerSession"] != null)
                {
                    int cid = ((CustomerDto)Session["customerSession"]).Id;
                    TempData["ID"] = cid.ToString();
                    if (orderNumber != "" && cid != 0)
                    {
                        List<SalesOrderLineDto> orderDetailTrack = (from so in _db.SalesOrders
                                                                    join sol in _db.SalesOrderLines on so.order_ID equals sol.order_id
                                                                    join p in _db.Products on sol.product_id equals p.Product_ID
                                                                    join pi in _db.Product_Image on p.Product_ID equals pi.Product_ID
                                                                    join sp in _db.SellerProducts on p.Product_ID equals sp.productId
                                                                    join s in _db.Sellers on sp.sellerId equals s.sellerId
                                                                    where so.trackingID == orderNumber && so.cust_Id == cid
                                                                    select new SalesOrderLineDto
                                                                    {
                                                                        soID = so.order_ID,
                                                                        OrderDate = so.order_date,
                                                                        solID = sol.id,
                                                                        OrderTot = (float)so.GrandTotal,
                                                                        qty = sol.quantity,
                                                                        pName = p.Product_Name,
                                                                        img = pi.Image_1,
                                                                        SellerName = s.sellerName,
                                                                        trackID = so.trackingID,
                                                                        isConfirmed = (int)sol.isConfirmed
                                                                    }).ToList();
                        ViewBag.o = orderNumber;
                        return View(orderDetailTrack);
                    }
                    else
                    {
                        return View();
                    }
                }
                else if (Session["customerSession"] == null)
                {
                    HttpCookie cookie = Request.Cookies["SID"];
                    string token = string.Empty;
                    if (cookie == null)
                    {
                        Response.Cookies["SID"].Values["x"] = Session.SessionID;
                        Response.Cookies["SID"].Expires = DateTime.MaxValue;
                    }
                    else
                    {
                        Session.Add("cookieId", cookie.Values["x"].ToString());
                        token = cookie.Values["x"].ToString();
                    }
                    if (token == string.Empty)
                        token = Response.Cookies["SID"].Values["x"].ToString();
                    TempData["ID"] = token.ToString();
                    if (orderNumber != "")
                    {
                        var orderDetailTrack = (from so in _db.SalesOrders
                                                join sol in _db.SalesOrderLines on so.order_ID equals sol.order_id
                                                join p in _db.Products on sol.product_id equals p.Product_ID
                                                join pi in _db.Product_Image on p.Product_ID equals pi.Product_ID
                                                join sp in _db.SellerProducts on p.Product_ID equals sp.productId
                                                join s in _db.Sellers on sp.sellerId equals s.sellerId
                                                where so.trackingID == orderNumber
                                                select new SalesOrderLineDto
                                                {
                                                    soID = so.order_ID,
                                                    OrderDate = so.order_date,
                                                    solID = sol.id,
                                                    OrderTot = (float)so.GrandTotal,
                                                    qty = sol.quantity,
                                                    pName = p.Product_Name,
                                                    img = pi.Image_1,
                                                    SellerName = s.sellerName,
                                                    trackID = so.trackingID,
                                                    isConfirmed = (int)sol.isConfirmed
                                                }).ToList();
                        ViewBag.o = orderNumber;
                        return View(orderDetailTrack);
                    }
                    else
                    {
                        return View();
                    }
                }
                return Redirect("~/Customers/Index");
            }
            catch (Exception e)
            {
                ViewBag.e = e;
                return Redirect("~/Customers/ErrorPage");
            }
        }
        [TrackExecution]
        public ActionResult OrderNumber(string OrderNumber)
        {
            try
            {
                orderNumber = "";
                orderNumber = OrderNumber;
                if (orderNumber != null)
                {
                    orderNumber = orderNumber.Trim();
                }
                return Redirect("~/Customers/Tracking");
            }
            catch (Exception e)
            {
                ViewBag.e = e;
                return Redirect("~/Customers/ErrorPage");
            }
        }
        RelatedProductsDto relatedProductsDto = new RelatedProductsDto();
        [MVCDecryptFilter(secret = "mySecret")]
        [TrackExecution]
        [HttpGet]
        public async Task<ActionResult> ProductInformation(int __proID)
        {
            try
            {
                if (Session["customerSession"] == null)
                {
                    Session["WithoutLogin"] = "WithoutLogin";
                    //Without Login
                    HttpCookie cookie = Request.Cookies["SID"];
                    string token = string.Empty;
                    //If Cookie is null then create new Cookie
                    if (cookie == null)
                    {
                        Response.Cookies["SID"].Values["x"] = Session.SessionID;
                        Response.Cookies["SID"].Expires = DateTime.MaxValue;
                    }
                    else
                    {
                        Session.Add("cookieId", cookie.Values["x"].ToString());
                        token = cookie.Values["x"].ToString();
                    }
                    if (token == string.Empty)
                        token = Response.Cookies["SID"].Values["x"].ToString();
                    TempData["ID"] = token.ToString();
                    ViewBag.relatedProductsDto = relatedProductsDto.relatedProducts(__proID).ToList();
                    int productID = __proID;
                    float price = (float)_db.ProductPrices.Where(x => x.ProductID == productID && x.EndDate == null).FirstOrDefault().Price;
                    var productDetails = from p1 in _db.Products
                                         join scb in _db.SubCatBrands on p1.brandCategoryId equals scb.Id
                                         join b in _db.Brands on scb.BrandID equals b.BrandId
                                         join pim in _db.Product_Image on p1.Product_ID equals pim.Product_ID
                                         join pwd in _db.ProductWarrenties on p1.Product_ID equals pwd.productId
                                         into element
                                         from item in element.DefaultIfEmpty()
                                         where p1.Product_ID == __proID
                                         select new ProductDetailDto
                                         {
                                             id = p1.Product_ID,
                                             pName = p1.Product_Name,
                                             newPrice = (((price * 1) - (float)p1.DiscountInAmount) - (((float)p1.Discount / 100) * (price * 1))),
                                             pWDetail = item.warrentyDescription,
                                             brandId = b.BrandId,
                                             bName = b.Brand_Name,
                                             pDetail = p1.Product_Description,
                                             wishlistId = (int?)_db.WishLists.FirstOrDefault(x => x.SellerProduct.productId == p1.Product_ID && x.GuestId == token).id,
                                             qty = p1.quantity,
                                             img1 = pim.Image_1,
                                             img2 = pim.Image_2,
                                             img3 = pim.Image_3,
                                             img4 = pim.Image_4,
                                             img5 = pim.Image_5,
                                             pPrice = price,
                                             pDiscountAmount = (float)p1.DiscountInAmount,
                                             pDiscountPercent = (float)p1.Discount
                                         };
                    ViewBag.pd = productDetails;
                }
                else
                {
                    //WithLogin
                    int userId = ((CustomerDto)Session["customerSession"]).Id;
                    TempData["ID"] = ((CustomerDto)Session["customerSession"]).Id.ToString();
                    ViewBag.relatedProductsDto = relatedProductsDto.relatedProducts(__proID).ToList();
                    int productID = __proID;
                    float price = (float)_db.ProductPrices.Where(x => x.ProductID == productID && x.EndDate == null).FirstOrDefault().Price;
                    var productDetails = from p1 in _db.Products
                                         join scb in _db.SubCatBrands on p1.brandCategoryId equals scb.Id
                                         join b in _db.Brands on scb.BrandID equals b.BrandId
                                         join pim in _db.Product_Image on p1.Product_ID equals pim.Product_ID
                                         join pwd in _db.ProductWarrenties on p1.Product_ID equals pwd.productId
                                         into element
                                         from item in element.DefaultIfEmpty()
                                         where p1.Product_ID == __proID
                                         select new ProductDetailDto
                                         {
                                             id = p1.Product_ID,
                                             pName = p1.Product_Name,
                                             newPrice = (((price * 1) - (float)p1.DiscountInAmount) - (((float)p1.Discount / 100) * (price * 1))),
                                             pWDetail = item.warrentyDescription,
                                             brandId = b.BrandId,
                                             bName = b.Brand_Name,
                                             pDetail = p1.Product_Description,
                                             wishlistId = (int?)_db.WishLists.FirstOrDefault(x => x.SellerProduct.productId == p1.Product_ID && x.customerId == userId).id,
                                             qty = p1.quantity,
                                             img1 = pim.Image_1,
                                             img2 = pim.Image_2,
                                             img3 = pim.Image_3,
                                             img4 = pim.Image_4,
                                             img5 = pim.Image_5,
                                             pPrice = price,
                                             pDiscountAmount = (float)p1.DiscountInAmount,
                                             pDiscountPercent = (float)p1.Discount
                                         };
                    ViewBag.pd = productDetails;
                }

                var RAndR = (from rr in _db.RatingsAndReviews
                             where rr.productId == __proID && rr.review != null
                             select new RatingsAndReviewsDto
                             {
                                 custName = rr.Customer.Cust_Name,
                                 Rating = (float)rr.rating,
                                 Review = rr.review
                             }).ToList();
                ViewBag.RAndR = RAndR;
                var product_feature = from pf in _db.Product_Feature
                                      join p in _db.Products on pf.product_id equals p.Product_ID
                                      where pf.product_id == __proID
                                      select new ProductDetailDto
                                      {
                                          f_name = pf.featureName,
                                          f_des = pf.featureDescription
                                      };
                ViewBag.pf = product_feature;
                ViewBag.sellerName = _db.SellerProducts.FirstOrDefault(x => x.productId == __proID).Seller.sellerName;
                ViewBag.sellerID = _db.SellerProducts.FirstOrDefault(x => x.productId == __proID).Seller.sellerId;
                ViewBag.pColor = (from pc in _db.ProductColors where pc.productId == __proID select new SizeColorStorageDto { colorid = (int)pc.colorid, colorName = pc.Color.colorName }).ToList();
                ViewBag.pStorage = (from ps in _db.ProductStorages where ps.productid == __proID select new SizeColorStorageDto { storageid = (int)ps.storageid, storageName = ps.Storage.Storage1 }).ToList();
                ViewBag.pSize = (from psize in _db.ProductSizes where psize.productId == __proID select new SizeColorStorageDto { sizeid = (int)psize.sizeid, sizeName = psize.Size.sizeName }).ToList();
                ViewBag.productId = __proID;
                int one = _db.RatingsAndReviews.Where(x => x.rating == 1 && x.productId == __proID).Count();
                int two = _db.RatingsAndReviews.Where(x => x.rating == 2 && x.productId == __proID).Count();
                int Three = _db.RatingsAndReviews.Where(x => x.rating == 3 && x.productId == __proID).Count();
                int Four = _db.RatingsAndReviews.Where(x => x.rating == 4 && x.productId == __proID).Count();
                int Five = _db.RatingsAndReviews.Where(x => x.rating == 5 && x.productId == __proID).Count();
                RatingsAndReviewsDto r = new RatingsAndReviewsDto()
                { One = one, Two = two, Three = Three, Four = Four, Five = Five };
                float aaa = ((float)one + (float)two + (float)Three + (float)Four + (float)Five) / (float)5;
                ViewBag.aaa = aaa;
                ViewBag.rew = r;
                if (Session["customerSession"] != null)
                {
                    int cid = (((CustomerDto)Session["customerSession"]).Id);
                    var posCheck = (from so in _db.SalesOrders
                                    join sol in _db.SalesOrderLines
                                    on so.order_ID equals sol.order_id
                                    where so.cust_Id == cid && sol.product_id == __proID
                                    select sol.isConfirmed).FirstOrDefault();
                    ViewBag.pos = posCheck;
                    //int cid = (((CustomerDto)Session["customerSession"]).Id);
                    var a = _db.RatingsAndReviews.FirstOrDefault(x => x.customerId == cid && x.productId == __proID);
                    if (a == null)
                    {
                        int sellerId = (from c in _db.SellerProducts
                                        where c.productId == __proID
                                        select c.sellerId).FirstOrDefault();
                        RatingsAndReviewsDto rat = new RatingsAndReviewsDto()
                        { customerId = cid, Rating = 0, sellerId = sellerId, productId = __proID };
                        //_db.RatingsAndReviews.Add(rat);
                        //_db.SaveChanges();
                        //var b = _db.RatingsAndReviews.FirstOrDefault(x => x.customerId == cid && x.productId == productID);
                        return View(rat);
                    }
                    else
                    {
                        int sellerId = (from c in _db.SellerProducts
                                        where c.productId == __proID
                                        select c.sellerId).FirstOrDefault();
                        RatingsAndReviewsDto rat = new RatingsAndReviewsDto()
                        { customerId = a.customerId, Rating = (float)a.rating, sellerId = a.sellerId, productId = a.productId };
                        return View(rat);
                    }
                }
                return View();
            }
            catch (Exception)
            {
                return Redirect("~/Customers/ErrorPage");
            }
        }
        [TrackExecution]
        public void Ratings(int productId, float Value)
        {
            try
            {
                if (Session["customerSession"] != null)
                {
                    TempData["ID"] = (((CustomerDto)Session["customerSession"]).Id);
                    int cid = (((CustomerDto)Session["customerSession"]).Id);
                    int sellerId = (from c in _db.SellerProducts
                                    where c.productId == productId
                                    select c.sellerId).FirstOrDefault();
                    var a = _db.RatingsAndReviews.FirstOrDefault(x => x.productId == productId && x.customerId == cid);
                    if (a != null)
                    {
                        a.rating = Value;
                        _db.Entry(a).State = EntityState.Modified;
                        _db.SaveChanges();
                    }
                    else
                    {
                        RatingsAndReview r = new RatingsAndReview()
                        { customerId = cid, rating = Value, sellerId = sellerId, productId = productId };
                        _db.RatingsAndReviews.Add(r);
                        _db.SaveChanges();
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine();
            }
        }
        [TrackExecution]
        [HttpGet]
        public ActionResult WithOutLoginSessionNull()
        {
            try
            {
                Session["WithoutLogin"] = null;
                Session.Abandon();
                Session.Remove("WithoutLogin");
                Session.Clear();
                return Redirect("~/Customers/Index");
            }
            catch (Exception)
            {
                return Redirect("~/Customers/ErrorPage");
            }
        }
        [TrackExecution]
        public void UserBehaviour(float spendedminutes, int behaviourtypeid, int relationId)//never comment this method
        {
            try
            {
                if (Session["customerSession"] == null)
                {
                    //Not logged in
                    HttpCookie cookie = Request.Cookies["SID"];
                    string token = string.Empty;
                    if (cookie == null)
                    {
                        Response.Cookies["SID"].Values["x"] = Session.SessionID;
                        Response.Cookies["SID"].Expires = DateTime.MaxValue;
                    }
                    else
                    {
                        Session.Add("cookieId", cookie.Values["x"].ToString());
                        token = cookie.Values["x"].ToString();
                    }
                    if (token == string.Empty)
                        token = Response.Cookies["SID"].Values["x"].ToString();
                    TempData["ID"] = token;
                    //pruneJOB();
                }
                else
                {
                    //With Login
                    int _cid = ((CustomerDto)Session["customerSession"]).Id;
                    TempData["ID"] = (((CustomerDto)Session["customerSession"]).Id);
                    //pruneJOB();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        [TrackExecution]
        [HttpGet]
        public ActionResult Logout()
        {
            try
            {
                Session["customerSession"] = null;
                Session.Abandon();
                Session.Remove("customerSession");
                Session.Clear();
                return Redirect("~/Customers/Index");
            }
            catch (Exception e)
            {
                return Redirect("~/Customers/Index");
            }
        }
        [TrackExecution]
        public ActionResult loadstates(int countryId)
        {
            try
            {
                return Json(_db.States.Where(s => s.Country_ID == countryId).Select(s => new
                {
                    SID = s.State_ID,
                    Name = s.State_Name
                }).ToList());
            }
            catch (Exception)
            {
                return Redirect("~/Customers/ErrorPage");
            }
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
            catch (Exception e)
            {
                return Redirect("~/Customers/ErrorPage");
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
            catch (Exception e)
            {
                return Redirect("~/Customers/ErrorPage");
            }
        }
        [OutputCache(NoStore = false, Duration = 500, VaryByParam = "None")]
        [TrackExecution]
        public ActionResult loadgender()
        {
            try
            {
                return Json(_db.Genders.Select(s => new
                {
                    AID = s.Gender_ID,
                    aName = s.Type
                }).ToList());
            }
            catch (Exception e)
            {
                return Redirect("~/Customers/ErrorPage");
            }
        }

        [MVCDecryptFilter(secret = "mySecret")]
        [TrackExecution]
        [HttpGet]
        public async Task<ActionResult> SellersProductsView(int sellerId)
        {
            try
            {
                if (Session["customerSession"] == null)
                {
                    //Without Login
                    HttpCookie cookie = Request.Cookies["SID"];
                    string token = string.Empty;
                    //If Cookie is null then create new Cookie
                    if (cookie == null)
                    {
                        Response.Cookies["SID"].Values["x"] = Session.SessionID;
                        Response.Cookies["SID"].Expires = DateTime.MaxValue;
                    }
                    else
                    {
                        Session.Add("cookieId", cookie.Values["x"].ToString());
                        token = cookie.Values["x"].ToString();
                    }
                    if (token == string.Empty)
                        token = Response.Cookies["SID"].Values["x"].ToString();
                    TempData["ID"] = token.ToString();
                    var sellerProducts = (from s in _db.Sellers
                                          join sp in _db.SellerProducts on s.sellerId equals sp.sellerId
                                          join p in _db.Products on sp.productId equals p.Product_ID
                                          join pp in _db.ProductPrices on p.Product_ID equals pp.ProductID
                                          where s.sellerId == sellerId && pp.EndDate == null
                                          select new ProductDetailDto
                                          {
                                              id = p.Product_ID,
                                              pName = p.Product_Name,
                                              pPrice = (float)pp.Price,
                                              newPrice = (float)(((pp.Price * 1) - (float)p.DiscountInAmount) - (((float)p.Discount / 100) * (pp.Price * 1))),
                                              pDiscountAmount = (float)p.DiscountInAmount,
                                              pDiscountPercent = (float)p.Discount,
                                              colorid = (int?)_db.ProductColors.FirstOrDefault(x => x.productId == p.Product_ID).Color.colorId,
                                              storageid = (int?)_db.ProductStorages.FirstOrDefault(x => x.productid == p.Product_ID).Storage.id,
                                              sizeid = (int?)_db.ProductSizes.FirstOrDefault(x => x.productId == p.Product_ID).Size.sizeId,
                                              wishlistId = (int?)_db.WishLists.FirstOrDefault(x => x.SellerProduct.productId == p.Product_ID && x.GuestId == token).id,
                                              cartId = (int?)_db.Carts.FirstOrDefault(x => x.GuestId == token && x.productId == p.Product_ID).cartId,
                                              rating = ((_db.RatingsAndReviews.Where(y => y.rating == 1).Count(x => x.productId == p.Product_ID) + _db.RatingsAndReviews.Where(y => y.rating == 2).Count(x => x.productId == p.Product_ID) + _db.RatingsAndReviews.Where(y => y.rating == 3).Count(x => x.productId == p.Product_ID) + _db.RatingsAndReviews.Where(y => y.rating == 4).Count(x => x.productId == p.Product_ID) + _db.RatingsAndReviews.Where(y => y.rating == 5).Count(x => x.productId == p.Product_ID)) / 5),
                                              img2 = _db.Product_Image.FirstOrDefault(x => x.Product_ID == p.Product_ID).Image_1,
                                              qty = p.quantity
                                          }).ToList();
                    if (sellerProducts.Count == 0)
                    {
                        ViewBag.MaxPrice = 0;
                    }
                    else
                    {
                        ViewBag.MaxPrice = sellerProducts.Max(x => x.newPrice);
                    }
                    var subsubcat = (from s in _db.Sellers
                                     join sp in _db.SellerProducts on s.sellerId equals sp.sellerId
                                     join p in _db.Products on sp.productId equals p.Product_ID
                                     join scb in _db.SubCatBrands on p.brandCategoryId equals scb.Id
                                     join ssc in _db.SubSubCategories on scb.subSubCatId equals ssc.SubSubCategoryID
                                     where s.sellerId == sellerId
                                     select new subsubcatimage
                                     {
                                         id = ssc.SubSubCategoryID,
                                         name = ssc.SubSubCategoryName
                                     }).Distinct();
                    var brands = (from s in _db.Sellers
                                  join sp in _db.SellerProducts on s.sellerId equals sp.sellerId
                                  join p in _db.Products on sp.productId equals p.Product_ID
                                  join scb in _db.SubCatBrands on p.brandCategoryId equals scb.Id
                                  join b in _db.Brands on scb.BrandID equals b.BrandId
                                  where s.sellerId == sellerId
                                  select new BrandDto
                                  {
                                      brandId = b.BrandId,
                                      brandName = b.Brand_Name
                                  }).Distinct().ToList();
                    SellerProductViewToCustomerDto sellerProductViewToCustomerDto = new SellerProductViewToCustomerDto();
                    sellerProductViewToCustomerDto.brandDtos = brands;
                    sellerProductViewToCustomerDto.subSubCategories = subsubcat.ToList();
                    sellerProductViewToCustomerDto.productDetailDtos = sellerProducts;
                    sellerProductViewToCustomerDto.sellerId = sellerId;
                    sellerProductViewToCustomerDto.sellerImage = _db.SellerImages.FirstOrDefault(x => x.sellerID == sellerId).image;
                    sellerProductViewToCustomerDto.sellerName = _db.Sellers.Find(sellerId).sellerName;

                    return View(sellerProductViewToCustomerDto);
                }
                else
                {
                    int cid = ((CustomerDto)Session["customerSession"]).Id;
                    TempData["ID"] = cid.ToString();
                    var sellerProducts = (from s in _db.Sellers
                                          join sp in _db.SellerProducts on s.sellerId equals sp.sellerId
                                          join p in _db.Products on sp.productId equals p.Product_ID
                                          join pp in _db.ProductPrices on p.Product_ID equals pp.ProductID
                                          where s.sellerId == sellerId && pp.EndDate == null
                                          select new ProductDetailDto
                                          {
                                              id = p.Product_ID,
                                              pName = p.Product_Name,
                                              pPrice = (float)pp.Price,
                                              newPrice = (float)(((pp.Price * 1) - (float)p.DiscountInAmount) - (((float)p.Discount / 100) * (pp.Price * 1))),
                                              pDiscountAmount = (float)p.DiscountInAmount,
                                              pDiscountPercent = (float)p.Discount,
                                              colorid = (int?)_db.ProductColors.FirstOrDefault(x => x.productId == p.Product_ID).Color.colorId,
                                              storageid = (int?)_db.ProductStorages.FirstOrDefault(x => x.productid == p.Product_ID).Storage.id,
                                              sizeid = (int?)_db.ProductSizes.FirstOrDefault(x => x.productId == p.Product_ID).Size.sizeId,
                                              wishlistId = (int?)_db.WishLists.FirstOrDefault(x => x.SellerProduct.productId == p.Product_ID && x.customerId == cid).id,
                                              cartId = (int?)_db.Carts.FirstOrDefault(x => x.customerId == cid && x.productId == p.Product_ID).cartId,
                                              rating = ((_db.RatingsAndReviews.Where(y => y.rating == 1).Count(x => x.productId == p.Product_ID) + _db.RatingsAndReviews.Where(y => y.rating == 2).Count(x => x.productId == p.Product_ID) + _db.RatingsAndReviews.Where(y => y.rating == 3).Count(x => x.productId == p.Product_ID) + _db.RatingsAndReviews.Where(y => y.rating == 4).Count(x => x.productId == p.Product_ID) + _db.RatingsAndReviews.Where(y => y.rating == 5).Count(x => x.productId == p.Product_ID)) / 5),
                                              img2 = _db.Product_Image.FirstOrDefault(x => x.Product_ID == p.Product_ID).Image_1,
                                              qty = p.quantity
                                          }).ToList();
                    if (sellerProducts.Count == 0)
                    {
                        ViewBag.MaxPrice = 0;
                    }
                    else
                    {
                        ViewBag.MaxPrice = sellerProducts.Max(x => x.newPrice);
                    }
                    var subsubcat = (from s in _db.Sellers
                                     join sp in _db.SellerProducts on s.sellerId equals sp.sellerId
                                     join p in _db.Products on sp.productId equals p.Product_ID
                                     join scb in _db.SubCatBrands on p.brandCategoryId equals scb.Id
                                     join ssc in _db.SubSubCategories on scb.subSubCatId equals ssc.SubSubCategoryID
                                     where s.sellerId == sellerId
                                     select new subsubcatimage
                                     {
                                         id = ssc.SubSubCategoryID,
                                         name = ssc.SubSubCategoryName
                                     }).Distinct();
                    var brands = (from s in _db.Sellers
                                  join sp in _db.SellerProducts on s.sellerId equals sp.sellerId
                                  join p in _db.Products on sp.productId equals p.Product_ID
                                  join scb in _db.SubCatBrands on p.brandCategoryId equals scb.Id
                                  join b in _db.Brands on scb.BrandID equals b.BrandId
                                  where s.sellerId == sellerId
                                  select new BrandDto
                                  {
                                      brandId = b.BrandId,
                                      brandName = b.Brand_Name
                                  }).Distinct().ToList();
                    SellerProductViewToCustomerDto sellerProductViewToCustomerDto = new SellerProductViewToCustomerDto();
                    sellerProductViewToCustomerDto.brandDtos = brands;
                    sellerProductViewToCustomerDto.subSubCategories = subsubcat.ToList();
                    sellerProductViewToCustomerDto.productDetailDtos = sellerProducts;
                    sellerProductViewToCustomerDto.sellerId = sellerId;
                    sellerProductViewToCustomerDto.sellerImage = _db.SellerImages.FirstOrDefault(x => x.sellerID == sellerId).image;
                    sellerProductViewToCustomerDto.sellerName = _db.Sellers.Find(sellerId).sellerName;

                    return View(sellerProductViewToCustomerDto);
                }
            }
            catch (Exception)
            {
                return Redirect("~/Customers/ErrorPage");
            }
        }
        [TrackExecution]
        public ActionResult ErrorPage()
        {
            return View();
        }

        static customerSignUpDto SignUpDto;

        [TrackExecution]
        [HttpPost]
        public JsonResult CustomerSignup(customerSignUpDto data)
        {
            try
            {
                SignUpDto = new customerSignUpDto();
                SignUpDto.custName = data.custName;
                SignUpDto.email = data.email;
                SignUpDto.gender = data.gender;
                SignUpDto.ph1 = data.ph1;
                if (data.custName == null && data.email == null && data.password == null && data.ph1 == null && data.gender == "-1")
                {
                    return Json("Your Form is empty!");
                }
                Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
                Regex regexPass = new Regex(@"^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9]).{8,}$");
                var emailCheck = _db.Customers.FirstOrDefault(x => x.Email == data.email);
                var match = regex.Match(data.email);
                var match1 = regexPass.Match(data.password);
                if (match == null)
                {
                    return Json("Your Email is empty!");
                }
                if (!match.Success)
                {
                    return Json("Invalid Email!");
                }
                else if (match1 == null)
                {
                    return Json("Your Password is empty!");
                }
                else if (!match1.Success)
                {
                    return Json("Invalid Password! You must use 1 Upper Case Letter, 1 Lower case Letter and 1 Digit!");
                }
                else if (data.password == null || data.password.Length < 8)
                {
                    return Json("Your Password is empty or doesn't fulfill our requirements!");
                }
                else if (data.gender == "-1" || data.gender == "0")
                {
                    return Json("Please Select gender!");
                }
                else if (data.ph1 == null)
                {
                    return Json("Your Phone Number field is empty!");
                }
                else if (data.ph1.Length > 11 || data.ph1.Length < 11 || data.ph1.Length == 0)
                {
                    return Json("Invalid Phone Number!");
                }

                else if (emailCheck != null)
                {
                    string e = "Sorry the email you entered already exist";
                    return Json(e);
                }
                else
                {
                    string activatioCode = string.Empty;
                    string hashPass = EncryptionClass.GetMD5Hash(data.password);
                    Random r = new Random();
                    activatioCode = r.Next(1001, 9999).ToString();
                    TempData["email"] = data.email;
                    Customer customer = new Customer()
                    { Cust_Name = data.custName, Email = data.email, Password = hashPass, Gender_ID = int.Parse(data.gender), Status = "unverified", ActivationCode = activatioCode, PasswordForCheck = data.password };
                    _db.Customers.Add(customer);
                    _db.SaveChanges();
                    int a = customer.Cust_ID;
                    Customer_Contact customer_Contact = new Customer_Contact()
                    { Customer_ID = a, PhoneNo1 = data.ph1 };
                    _db.Customer_Contact.Add(customer_Contact);
                    _db.SaveChanges();
                    SendCode(data.custName, activatioCode, data.email);
                    return Json("ActivateEmail");
                }
            }
            catch (Exception e)
            {
                string wentWrong = "Something went wrong. Check Your Fields Again!";
                return Json(wentWrong);
            }
        }
        [TrackExecution]
        [HttpPost]
        public void ReviewsAndRating(string review, float rating, int productId)
        {
            try
            {
                if (Session["customerSession"] != null)
                {
                    TempData["ID"] = (((CustomerDto)Session["customerSession"]).Id);
                    int cid = (((CustomerDto)Session["customerSession"]).Id);
                    int sellerId = (from c in _db.SellerProducts
                                    where c.productId == productId
                                    select c.sellerId).FirstOrDefault();
                    var a = _db.RatingsAndReviews.FirstOrDefault(x => x.productId == productId && x.customerId == cid);
                    if (a == null)
                    {
                        RatingsAndReview r = new RatingsAndReview()
                        { customerId = cid, rating = rating, sellerId = sellerId, productId = productId, review = review };
                        _db.RatingsAndReviews.Add(r);
                        try
                        {
                            _db.SaveChanges();
                        }
                        catch (DBConcurrencyException e)
                        {
                            Console.WriteLine(e);
                        }
                    }
                    else
                    {
                        a.rating = rating;
                        a.review = review;
                        _db.Entry(a).State = EntityState.Modified;
                        try
                        {
                            _db.Configuration.ValidateOnSaveEnabled = false;
                            _db.SaveChanges();
                            _db.Configuration.ValidateOnSaveEnabled = true;
                        }
                        catch (DBConcurrencyException e)
                        {
                            Console.WriteLine(e);
                        }
                    }
                }
            }
            catch (DBConcurrencyException e)
            {
                Console.WriteLine(e);
            }
        }
        //[TrackExecution]
        [HttpGet]
        public JsonResult CartCount()
        {
            int cartItemQty = 0;
            try
            {
                int cqty = 0;
                if (Session["customerSession"] != null)
                {
                    int custId = ((CustomerDto)Session["customerSession"]).Id;
                    var count = _db.Carts.Where(x => x.customerId == custId).ToList();
                    for (int i = 0; i < count.Count; i++)
                    {
                        int id = count[i].productId;
                        cqty = _db.Carts.FirstOrDefault(x => x.customerId == custId && x.productId == id).quantity;
                        cartItemQty = cartItemQty + cqty;
                    }
                    return Json(cartItemQty, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    HttpCookie cookie = Request.Cookies["SID"];
                    string token = string.Empty;
                    //If Cookie is null then create new Cookie
                    if (cookie == null)
                    { Response.Cookies["SID"].Values["x"] = Session.SessionID; Response.Cookies["SID"].Expires = DateTime.MaxValue; }
                    else
                    { Session.Add("cookieId", cookie.Values["x"].ToString()); token = cookie.Values["x"].ToString(); }
                    if (token == string.Empty)
                        token = Response.Cookies["SID"].Values["x"].ToString();
                    TempData["ID"] = token.ToString();
                    var count = _db.Carts.Where(x => x.GuestId == token).ToList();
                    for (int i = 0; i < count.Count; i++)
                    {
                        int id = count[i].productId;
                        cqty = _db.Carts.FirstOrDefault(x => x.GuestId == token && x.productId == id).quantity;
                        cartItemQty = cartItemQty + cqty;
                    }
                    return Json(cartItemQty, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception)
            {
                return Json(cartItemQty, JsonRequestBehavior.AllowGet);
            }
        }
        //[TrackExecution]
        [HttpGet]
        public JsonResult WishlistCount()
        {
            int wlCount = 0;
            try
            {
                if (Session["customerSession"] != null)
                {
                    int custId = ((CustomerDto)Session["customerSession"]).Id;
                    wlCount = _db.WishLists.Where(x => x.customerId == custId).Count();
                    return Json(wlCount.ToString(), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    HttpCookie cookie = Request.Cookies["SID"];
                    string token = string.Empty;
                    //If Cookie is null then create new Cookie
                    if (cookie == null)
                    { Response.Cookies["SID"].Values["x"] = Session.SessionID; Response.Cookies["SID"].Expires = DateTime.MaxValue; }
                    else
                    { Session.Add("cookieId", cookie.Values["x"].ToString()); token = cookie.Values["x"].ToString(); }
                    if (token == string.Empty)
                        token = Response.Cookies["SID"].Values["x"].ToString();
                    TempData["ID"] = token.ToString();
                    wlCount = _db.WishLists.Where(x => x.GuestId == token).Count();
                    return Json(wlCount.ToString(), JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception)
            {
                return Json(wlCount.ToString(), JsonRequestBehavior.AllowGet);
            }
        }
        [TrackExecution]
        [HttpGet]
        public JsonResult addtoCartByQtyPlusMinus(int proid, int qty)
        {
            int cartItemQty = 0;
            try
            {
                if (Session["customerSession"] != null)
                {
                    int cqty = 0;
                    int custId = ((CustomerDto)Session["customerSession"]).Id;
                    var product = _db.Carts.FirstOrDefault(x => x.productId == proid && x.customerId == custId);
                    product.quantity = qty;
                    _db.Entry(product).State = EntityState.Modified;
                    _db.SaveChanges();
                    var count = _db.Carts.Where(x => x.customerId == custId).ToList();
                    for (int i = 0; i < count.Count; i++)
                    {
                        int id = count[i].productId;
                        cqty = _db.Carts.FirstOrDefault(x => x.customerId == custId && x.productId == id).quantity;
                        cartItemQty = cartItemQty + cqty;
                    }
                    if (cartItemQty != 0)
                    {
                        return Json(cartItemQty, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json("&nbsp", JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    int cqty = 0;
                    HttpCookie cookie = Request.Cookies["SID"];
                    string token = string.Empty;
                    //If Cookie is null then create new Cookie
                    if (cookie == null)
                    { Response.Cookies["SID"].Values["x"] = Session.SessionID; Response.Cookies["SID"].Expires = DateTime.MaxValue; }
                    else
                    { Session.Add("cookieId", cookie.Values["x"].ToString()); token = cookie.Values["x"].ToString(); }
                    if (token == string.Empty)
                        token = Response.Cookies["SID"].Values["x"].ToString();
                    TempData["ID"] = token.ToString();
                    //int custId = ((CustomerDto)Session["customerSession"]).Id;
                    var product = _db.Carts.FirstOrDefault(x => x.productId == proid && x.GuestId == token);
                    product.quantity = qty;
                    _db.Entry(product).State = EntityState.Modified;
                    _db.SaveChanges();
                    var count = _db.Carts.Where(x => x.GuestId == token).ToList();
                    for (int i = 0; i < count.Count; i++)
                    {
                        int id = count[i].productId;
                        cqty = _db.Carts.FirstOrDefault(x => x.GuestId == token && x.productId == id).quantity;
                        cartItemQty = cartItemQty + cqty;
                    }
                    if (cartItemQty != 0)
                    {
                        return Json(cartItemQty, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json("&nbsp", JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception)
            {
                return Json(cartItemQty, JsonRequestBehavior.AllowGet);
            }
        }
        [TrackExecution]
        [HttpPost]
        public JsonResult addtoWishlistByPi(List<string> _itemToAdd)
        {
            int cartItemQty = 0;
            try
            {
                if (Session["customerSession"] != null)
                {
                    TempData["ID"] = ((CustomerDto)Session["customerSession"]).Id.ToString();
                    foreach (var item in _itemToAdd)
                    {

                        int custId = ((CustomerDto)Session["customerSession"]).Id;
                        string[] a = item.Split('|');
                        int pid = int.Parse(a[0].ToString());
                        var pdCheck = _db.Carts.FirstOrDefault(x => x.productId == pid && x.customerId == custId);
                        if (pdCheck != null)
                        {
                            return Json("Item already exist in Cart" + "+" + cartItemQty.ToString(), JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            int sellerProductID = _db.SellerProducts.FirstOrDefault(x => x.productId == pid).Id;
                            var pwCheck = _db.WishLists.FirstOrDefault(x => x.sellerProductId == sellerProductID && x.customerId == custId);
                            if (pwCheck != null)
                            {
                                return Json("Item already exist in Wishlist" + "+" + cartItemQty.ToString(), JsonRequestBehavior.AllowGet);
                            }
                            if (a[4] == "undefined" && a[2] != "undefined" && a[3] != "undefined")
                            {
                                int colorid = int.Parse(a[2].ToString());
                                int storageid = int.Parse(a[3].ToString());
                                WishList cart = new WishList()
                                {
                                    sellerProductId = sellerProductID,
                                    customerId = (((CustomerDto)Session["customerSession"]).Id),
                                    colorid = colorid,
                                    storageid = storageid
                                };
                                _db.WishLists.Add(cart);
                                _db.SaveChanges();
                                cartItemQty = _db.WishLists.Where(x => x.customerId == custId).Count();
                                int wishlistId = _db.WishLists.Max(x => x.id);
                                return Json("Item Added to the Wishlist Succesfully" + "+" + cartItemQty.ToString() + "+" + wishlistId.ToString(), JsonRequestBehavior.AllowGet);
                            }
                            else if (a[2] == "undefined" && a[4] != "undefined" && a[3] != "undefined")
                            {
                                int sizeid = int.Parse(a[4].ToString());
                                int storageid = int.Parse(a[3].ToString());
                                WishList cart = new WishList()
                                {
                                    sellerProductId = sellerProductID,
                                    customerId = (((CustomerDto)Session["customerSession"]).Id),
                                    sizeid = sizeid,
                                    storageid = storageid
                                };
                                _db.WishLists.Add(cart);
                                _db.SaveChanges();
                                cartItemQty = _db.WishLists.Where(x => x.customerId == custId).Count();
                                int wishlistId = _db.WishLists.Max(x => x.id);
                                return Json("Item Added to the Wishlist Succesfully" + "+" + cartItemQty.ToString() + "+" + wishlistId.ToString(), JsonRequestBehavior.AllowGet);
                            }
                            else if (a[3] == "undefined" && a[2] != "undefined" && a[4] != "undefined")
                            {
                                int sizeid = int.Parse(a[4].ToString());
                                int colorid = int.Parse(a[2].ToString());
                                WishList cart = new WishList()
                                {
                                    sellerProductId = sellerProductID,
                                    customerId = (((CustomerDto)Session["customerSession"]).Id),
                                    sizeid = sizeid,
                                    colorid = colorid
                                };
                                _db.WishLists.Add(cart);
                                _db.SaveChanges();
                                cartItemQty = _db.WishLists.Where(x => x.customerId == custId).Count();
                                int wishlistId = _db.WishLists.Max(x => x.id);
                                return Json("Item Added to the Wishlist Succesfully" + "+" + cartItemQty.ToString() + "+" + wishlistId.ToString(), JsonRequestBehavior.AllowGet);
                            }
                            else if (a[3] == "undefined" && a[2] == "undefined" && a[4] != "undefined")
                            {
                                int sizeid = int.Parse(a[4].ToString());
                                WishList cart = new WishList()
                                {
                                    sellerProductId = sellerProductID,
                                    customerId = (((CustomerDto)Session["customerSession"]).Id),
                                    sizeid = sizeid,
                                };
                                _db.WishLists.Add(cart);
                                _db.SaveChanges();
                                cartItemQty = _db.WishLists.Where(x => x.customerId == custId).Count();
                                int wishlistId = _db.WishLists.Max(x => x.id);
                                return Json("Item Added to the Wishlist Succesfully" + "+" + cartItemQty.ToString() + "+" + wishlistId.ToString(), JsonRequestBehavior.AllowGet);
                            }
                            else if (a[4] == "undefined" && a[3] == "undefined" && a[2] != "undefined")
                            {
                                int colorid = int.Parse(a[2].ToString());
                                WishList cart = new WishList()
                                {
                                    sellerProductId = sellerProductID,
                                    customerId = (((CustomerDto)Session["customerSession"]).Id),
                                    colorid = colorid
                                };
                                _db.WishLists.Add(cart);
                                _db.SaveChanges();
                                cartItemQty = _db.WishLists.Where(x => x.customerId == custId).Count();
                                int wishlistId = _db.WishLists.Max(x => x.id);
                                return Json("Item Added to the Wishlist Succesfully" + "+" + cartItemQty.ToString() + "+" + wishlistId.ToString(), JsonRequestBehavior.AllowGet);
                            }
                            else if (a[4] == "undefined" && a[2] == "undefined" && a[3] != "undefined")
                            {
                                int storageid = int.Parse(a[3].ToString());
                                WishList cart = new WishList()
                                {
                                    sellerProductId = sellerProductID,
                                    customerId = (((CustomerDto)Session["customerSession"]).Id),
                                    storageid = storageid,
                                };
                                _db.WishLists.Add(cart);
                                _db.SaveChanges();
                                cartItemQty = _db.WishLists.Where(x => x.customerId == custId).Count();
                                int wishlistId = _db.WishLists.Max(x => x.id);
                                return Json("Item Added to the Wishlist Succesfully" + " " + cartItemQty.ToString() + "+" + wishlistId.ToString(), JsonRequestBehavior.AllowGet);
                            }
                            else if (a[3] != "undefined" && a[2] != "undefined" && a[4] != "undefined")
                            {
                                int storageid = int.Parse(a[3].ToString());
                                int sizeid = int.Parse(a[4].ToString());
                                int colorid = int.Parse(a[2].ToString());
                                WishList cart = new WishList()
                                {
                                    sellerProductId = sellerProductID,
                                    customerId = (((CustomerDto)Session["customerSession"]).Id),
                                    colorid = colorid,
                                    storageid = storageid,
                                    sizeid = sizeid
                                };
                                _db.WishLists.Add(cart);
                                _db.SaveChanges();
                                cartItemQty = _db.WishLists.Where(x => x.customerId == custId).Count();
                                int wishlistId = _db.WishLists.Max(x => x.id);
                                return Json("Item Added to the Wishlist Succesfully" + "+" + cartItemQty.ToString() + "+" + wishlistId.ToString(), JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                WishList cart = new WishList()
                                {
                                    sellerProductId = sellerProductID,
                                    customerId = (((CustomerDto)Session["customerSession"]).Id),
                                };
                                _db.WishLists.Add(cart);
                                _db.SaveChanges();
                                cartItemQty = _db.WishLists.Where(x => x.customerId == custId).Count();
                                int wishlistId = _db.WishLists.Max(x => x.id);
                                return Json("Item Added to the Wishlist Succesfully" + "+" + cartItemQty.ToString() + "+" + wishlistId.ToString(), JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                    return Json("Item Added" + "+" + "&nbsp", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    HttpCookie cookie = Request.Cookies["SID"];
                    string token = string.Empty;
                    if (cookie == null)
                    { Response.Cookies["SID"].Values["x"] = Session.SessionID; Response.Cookies["SID"].Expires = DateTime.MaxValue; }
                    else { Session.Add("cookieId", cookie.Values["x"].ToString()); token = cookie.Values["x"].ToString(); }
                    if (token == string.Empty)
                        token = Response.Cookies["SID"].Values["x"].ToString();
                    TempData["ID"] = token;
                    //TempData["ID"] = ((CustomerDto)Session["customerSession"]).Id.ToString();
                    foreach (var item in _itemToAdd)
                    {
                        //int custId = ((CustomerDto)Session["customerSession"]).Id;
                        string[] a = item.Split('|');
                        int pid = int.Parse(a[0].ToString());
                        var pdCheck = _db.Carts.FirstOrDefault(x => x.productId == pid && x.GuestId == token);
                        if (pdCheck != null)
                        {
                            return Json("Item already exist in Cart" + "+" + cartItemQty.ToString(), JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            int sellerProductID = _db.SellerProducts.FirstOrDefault(x => x.productId == pid).Id;
                            var pwCheck = _db.WishLists.FirstOrDefault(x => x.sellerProductId == sellerProductID && x.GuestId == token);
                            if (pwCheck != null)
                            {
                                return Json("Item already exist in Wishlist" + "+" + cartItemQty.ToString(), JsonRequestBehavior.AllowGet);
                            }
                            if (a[4] == "undefined" && a[2] != "undefined" && a[3] != "undefined")
                            {
                                int colorid = int.Parse(a[2].ToString());
                                int storageid = int.Parse(a[3].ToString());
                                WishList cart = new WishList()
                                {
                                    sellerProductId = sellerProductID,
                                    GuestId = token,
                                    colorid = colorid,
                                    storageid = storageid
                                };
                                _db.WishLists.Add(cart);
                                _db.SaveChanges();
                                cartItemQty = _db.WishLists.Where(x => x.GuestId == token).Count();
                                int wishlistId = _db.WishLists.Max(x => x.id);
                                return Json("Item Added to the Wishlist Succesfully" + "+" + cartItemQty.ToString() + "+" + wishlistId.ToString(), JsonRequestBehavior.AllowGet);
                            }
                            else if (a[2] == "undefined" && a[4] != "undefined" && a[3] != "undefined")
                            {
                                int sizeid = int.Parse(a[4].ToString());
                                int storageid = int.Parse(a[3].ToString());
                                WishList cart = new WishList()
                                {
                                    sellerProductId = sellerProductID,
                                    GuestId = token,
                                    sizeid = sizeid,
                                    storageid = storageid
                                };
                                _db.WishLists.Add(cart);
                                _db.SaveChanges();
                                cartItemQty = _db.WishLists.Where(x => x.GuestId == token).Count();
                                int wishlistId = _db.WishLists.Max(x => x.id);
                                return Json("Item Added to the Wishlist Succesfully" + "+" + cartItemQty.ToString() + "+" + wishlistId.ToString(), JsonRequestBehavior.AllowGet);
                            }
                            else if (a[3] == "undefined" && a[2] != "undefined" && a[4] != "undefined")
                            {
                                int sizeid = int.Parse(a[4].ToString());
                                int colorid = int.Parse(a[2].ToString());
                                WishList cart = new WishList()
                                {
                                    sellerProductId = sellerProductID,
                                    GuestId = token,
                                    sizeid = sizeid,
                                    colorid = colorid
                                };
                                _db.WishLists.Add(cart);
                                _db.SaveChanges();
                                cartItemQty = _db.WishLists.Where(x => x.GuestId == token).Count();
                                int wishlistId = _db.WishLists.Max(x => x.id);
                                return Json("Item Added to the Wishlist Succesfully" + "+" + cartItemQty.ToString() + "+" + wishlistId.ToString(), JsonRequestBehavior.AllowGet);
                            }
                            else if (a[3] == "undefined" && a[2] == "undefined" && a[4] != "undefined")
                            {
                                int sizeid = int.Parse(a[4].ToString());
                                WishList cart = new WishList()
                                {
                                    sellerProductId = sellerProductID,
                                    GuestId = token,
                                    sizeid = sizeid,
                                };
                                _db.WishLists.Add(cart);
                                _db.SaveChanges();
                                cartItemQty = _db.WishLists.Where(x => x.GuestId == token).Count();
                                int wishlistId = _db.WishLists.Max(x => x.id);
                                return Json("Item Added to the Wishlist Succesfully" + "+" + cartItemQty.ToString() + "+" + wishlistId.ToString(), JsonRequestBehavior.AllowGet);
                            }
                            else if (a[4] == "undefined" && a[3] == "undefined" && a[2] != "undefined")
                            {
                                int colorid = int.Parse(a[2].ToString());
                                WishList cart = new WishList()
                                {
                                    sellerProductId = sellerProductID,
                                    GuestId = token,
                                    colorid = colorid
                                };
                                _db.WishLists.Add(cart);
                                _db.SaveChanges();
                                cartItemQty = _db.WishLists.Where(x => x.GuestId == token).Count();
                                int wishlistId = _db.WishLists.Max(x => x.id);
                                return Json("Item Added to the Wishlist Succesfully" + "+" + cartItemQty.ToString() + "+" + wishlistId.ToString(), JsonRequestBehavior.AllowGet);
                            }
                            else if (a[4] == "undefined" && a[2] == "undefined" && a[3] != "undefined")
                            {
                                int storageid = int.Parse(a[3].ToString());
                                WishList cart = new WishList()
                                {
                                    sellerProductId = sellerProductID,
                                    GuestId = token,
                                    storageid = storageid,
                                };
                                _db.WishLists.Add(cart);
                                _db.SaveChanges();
                                cartItemQty = _db.WishLists.Where(x => x.GuestId == token).Count();
                                int wishlistId = _db.WishLists.Max(x => x.id);
                                return Json("Item Added to the Wishlist Succesfully" + " " + cartItemQty.ToString() + "+" + wishlistId.ToString(), JsonRequestBehavior.AllowGet);
                            }
                            else if (a[3] != "undefined" && a[2] != "undefined" && a[4] != "undefined")
                            {
                                int storageid = int.Parse(a[3].ToString());
                                int sizeid = int.Parse(a[4].ToString());
                                int colorid = int.Parse(a[2].ToString());
                                WishList cart = new WishList()
                                {
                                    sellerProductId = sellerProductID,
                                    GuestId = token,
                                    colorid = colorid,
                                    storageid = storageid,
                                    sizeid = sizeid
                                };
                                _db.WishLists.Add(cart);
                                _db.SaveChanges();
                                cartItemQty = _db.WishLists.Where(x => x.GuestId == token).Count();
                                int wishlistId = _db.WishLists.Max(x => x.id);
                                return Json("Item Added to the Wishlist Succesfully" + "+" + cartItemQty.ToString() + "+" + wishlistId.ToString(), JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                WishList cart = new WishList()
                                {
                                    sellerProductId = sellerProductID,
                                    GuestId = token
                                };
                                _db.WishLists.Add(cart);
                                _db.SaveChanges();
                                cartItemQty = _db.WishLists.Where(x => x.GuestId == token).Count();
                                int wishlistId = _db.WishLists.Max(x => x.id);
                                return Json("Item Added to the Wishlist Succesfully" + "+" + cartItemQty.ToString() + "+" + wishlistId.ToString(), JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                    return Json("Item Added" + "+" + "&nbsp", JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception)
            {
                return Json("Item Not Added to the Wishlist" + "+" + cartItemQty.ToString() + "+" + "undefined", JsonRequestBehavior.AllowGet);
            }
        }
        [OutputCache(NoStore = false, Duration = int.MaxValue)]
        //[TrackExecution]
        [HttpGet]
        public JsonResult CatMenu()
        {
            string c = "";
            var cat = _db.Categories.ToList();
            for (int i = 0; i < cat.Count; i++)
            {
                string _cid = Url.ActionEnc("mySecret", "CatSubCat", new { catid = cat[i].Cat_ID });
                c = c + "<li class='li'>" + "<a href='" + _cid + "'>" + cat[i].Cat_Name + "</a>";
                int cid = cat[i].Cat_ID;
                var subcat = _db.Sub_Category.Where(x => x.Cat_ID == cid).ToList();
                c = c + "<ul class='ul'>";
                for (int j = 0; j < subcat.Count; j++)
                {
                    string _scid = Url.ActionEnc("mySecret", "SubSubCat", new { _ssc = subcat[j].Sub_Category_ID });
                    c = c + "<li class='li'>" + "<a href='" + _scid + "'>" + subcat[j].Sub_Name + "</a>";
                    int scid = subcat[j].Sub_Category_ID;
                    var subsubcat = _db.SubSubCategories.Where(x => x.Sub_Category_ID == scid).ToList();
                    c = c + "<ul class='ul'>";
                    for (int k = 0; k < subsubcat.Count; k++)
                    {
                        string _sscid = Url.ActionEnc("mySecret", "SubSubCatBrands", new { subsubcatId = subsubcat[k].SubSubCategoryID });
                        c = c + "<li class='li'>" + "<a href='" + _sscid + "'>" + subsubcat[k].SubSubCategoryName + "</a></li>";
                    }
                    c = c + " </ul></li>";
                }
                c = c + " </ul></li>";
            }
            return Json(c, JsonRequestBehavior.AllowGet);
        }

        [TrackExecution]
        [HttpPost]
        public JsonResult _RemovefromWishlist(int wishlistId)
        {
            int wCount = 0;
            try
            {
                if (Session["customerSession"] != null)
                {
                    int userid = ((CustomerDto)Session["customerSession"]).Id;
                    TempData["ID"] = userid.ToString();
                    var wishList = new WishList { id = wishlistId };
                    _db.Entry(wishList).State = EntityState.Deleted;
                    _db.SaveChanges();
                    wCount = _db.WishLists.Where(x => x.customerId == userid).Count();
                    return Json("Item is Removed from Wishlist" + "+" + wCount.ToString(), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    HttpCookie cookie = Request.Cookies["SID"];
                    string token = string.Empty;
                    if (cookie == null)
                    { Response.Cookies["SID"].Values["x"] = Session.SessionID; Response.Cookies["SID"].Expires = DateTime.MaxValue; }
                    else { Session.Add("cookieId", cookie.Values["x"].ToString()); token = cookie.Values["x"].ToString(); }
                    if (token == string.Empty)
                        token = Response.Cookies["SID"].Values["x"].ToString();
                    TempData["ID"] = token;
                    var wishList = new WishList { id = wishlistId };
                    _db.Entry(wishList).State = EntityState.Deleted;
                    _db.SaveChanges();
                    wCount = _db.WishLists.Where(x => x.GuestId == token).Count();
                    return Json("Item is Removed from Wishlist" + "+" + wCount.ToString(), JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception)
            {
                return Json("Item Not Removed From Cart." + "+" + wCount.ToString(), JsonRequestBehavior.AllowGet);
            }
        }
        [TrackExecution]
        [HttpPost]
        public JsonResult _RemovefromCart(int cartId)
        {
            int cCount = 0;
            try
            {
                if (Session["customerSession"] != null)
                {
                    int userid = ((CustomerDto)Session["customerSession"]).Id;
                    TempData["ID"] = userid.ToString();
                    var cart = new Cart { cartId = cartId };
                    _db.Entry(cart).State = EntityState.Deleted;
                    _db.SaveChanges();
                    cCount = _db.Carts.Where(x => x.customerId == userid).Count();
                    return Json("Item is Removed from Cart" + "+" + cCount.ToString(), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    HttpCookie cookie = Request.Cookies["SID"];
                    string token = string.Empty;
                    if (cookie == null)
                    { Response.Cookies["SID"].Values["x"] = Session.SessionID; Response.Cookies["SID"].Expires = DateTime.MaxValue; }
                    else { Session.Add("cookieId", cookie.Values["x"].ToString()); token = cookie.Values["x"].ToString(); }
                    if (token == string.Empty)
                        token = Response.Cookies["SID"].Values["x"].ToString();
                    TempData["ID"] = token;
                    var cart = new Cart { cartId = cartId };
                    _db.Entry(cart).State = EntityState.Deleted;
                    _db.SaveChanges();
                    cCount = _db.Carts.Where(x => x.GuestId == token).Count();
                    return Json("Item is Removed from Cart" + "+" + cCount.ToString(), JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception)
            {
                return Json("Item Not Removed from Cart" + "+" + cCount.ToString(), JsonRequestBehavior.AllowGet);
            }
        }

        [TrackExecution]
        [HttpPost]
        public void addnewAddrSession(string gName, string gPhone, string gEmail)
        {
            try
            {
                if (Session["customerSession"] == null)
                {
                    HttpCookie cookie = Request.Cookies["SID"];
                    string token = string.Empty;
                    if (cookie == null)
                    {
                        Response.Cookies["SID"].Values["x"] = Session.SessionID;
                        Response.Cookies["SID"].Expires = DateTime.MaxValue;
                    }
                    else
                    {
                        Session.Add("cookieId", cookie.Values["x"].ToString());
                        token = cookie.Values["x"].ToString();
                    }
                    if (token == string.Empty)
                        token = Response.Cookies["SID"].Values["x"].ToString();
                    TempData["ID"] = token.ToString();
                    ((SelectedProductDto)Session[token])._gEmail = gEmail;
                    ((SelectedProductDto)Session[token])._gName = gName;
                    ((SelectedProductDto)Session[token])._gPhone = gPhone;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        [TrackExecution]
        [HttpGet]
        public ActionResult ViewOrders()
        {
            try
            {
                if (Session["customerSession"] != null)
                {
                    int cid = ((CustomerDto)Session["customerSession"]).Id;
                    TempData["ID"] = cid.ToString();
                    return View();
                }
                return View();
            }
            catch (Exception e)
            {
                return Redirect("~/Customers/ErrorPage");
            }
        }
        [HttpGet]
        public JsonResult _viewOrders()
        {
            try
            {
                string c = "";
                if (Session["customerSession"] != null)
                {
                    int cid = ((CustomerDto)Session["customerSession"]).Id;
                    List<SalesOrderLineDto> orderDetailTrack = (from so in _db.SalesOrders
                                                                where so.cust_Id == cid
                                                                select new SalesOrderLineDto { soID = so.order_ID, OrderDate = so.order_date, OrderTot = (float)so.GrandTotal, trackID = so.trackingID, }).ToList();
                    foreach (var item in orderDetailTrack)
                    {
                        c = c + "<div class='container'><div class='OrderTextDiv1 z-depth-2 hoverable'><div class='row'><div class='col l5 m6 s6'><div class='row'><div class='col l12 m6 s6 black-text'><p class='trackIDNo'>Order Against # " + item.trackID + "</p><p class='orderDateTime'>" + item.OrderDate + "</p></div></div></div><div class='col l7 m6 s6'><p class='orderTot' style=''>Total: " + item.OrderTot.ToString("#,###") + "</p></div></div>";
                        List<SalesOrderLineDto> orderDetailTrack2 = (from sol in _db.SalesOrderLines
                                                                     where sol.order_id == item.soID
                                                                     select new SalesOrderLineDto { isConfirmed = (int)sol.isConfirmed, img = _db.Product_Image.FirstOrDefault(x => x.Product_ID == sol.product_id).Image_1, qty = sol.quantity, pName = _db.Products.FirstOrDefault(x => x.Product_ID == sol.product_id).Product_Name, proID = sol.product_id }).ToList();
                        string status = "";
                        string color = "";
                        string cancel = "";
                        string url = "";
                        foreach (var item1 in orderDetailTrack2)
                        {

                            if (item1.isConfirmed == 0) { status = "Not Confirmed"; color = "red-text"; cancel = "Cancel"; url = Url.ActionEnc("mySecret", "cancelOrder", new { so = item.soID, pid = item1.proID }); }
                            if (item1.isConfirmed == 1) { status = "Confirmed"; color = "yellow-text"; cancel = ""; url = ""; }
                            if (item1.isConfirmed == 2) { status = "Delivered"; color = "green-text"; cancel = ""; url = ""; }
                            c = c + "<div class='divider' style='margin-top:-20px;'></div><div class='row'><div class='col l2 m2 s2' style=''><img style='width:80px;height:80px; object-fit:scale-down' src=" + item1.img + " /></div><div class='col l4 m4 s4 flow-text' style='margin-top:10px; font-size:12px;'>" + item1.pName + "</div><div class='col l2 m2 s2 flow-text' style='margin-top:10px; font-size:15px; padding-top:25px; '>Qty:" + item1.qty + "</div><div class='col l2 m2 s2 flow-text " + color + "' style='margin-top:10px; font-size:15px; padding-top:25px;'>" + status + "</div><div class='col l2 m2 s2 flow-text black-text' style='margin-top:10px; font-size:15px; padding-top:25px;'><a href='" + url + "'>" + cancel + "</a></div></div>";
                        }
                        c = c + "</div></div>";
                    }
                    return Json(c, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("Please SignUp or Login", JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                return Json("Some Problem Occured While Retreiving Your Orders.", JsonRequestBehavior.AllowGet);
            }
        }
        [MVCDecryptFilter(secret = "mySecret")]
        [HttpGet]
        public ActionResult cancelOrder(int so, int pid)
        {
            try
            {
                if (Session["customerSession"] != null)
                {
                    int cid = ((CustomerDto)Session["customerSession"]).Id;
                    float grandTotal = (float)_db.SalesOrders.FirstOrDefault(x => x.cust_Id == cid && x.order_ID == so).GrandTotal;
                    float netAmount = (float)_db.SalesOrderLines.FirstOrDefault(x => x.product_id == pid && x.order_id == so).netAmount;
                    float newGrandTotal = grandTotal - netAmount;
                    var salesOrderLineCount = _db.SalesOrderLines.Where(x => x.order_id == so).Count();
                    if (salesOrderLineCount == 1)
                    {
                        var _salesOrder = _db.SalesOrders.FirstOrDefault(x => x.order_ID == so);
                        _db.SalesOrders.Remove(_salesOrder);
                        _db.SaveChanges();
                        CanceledOrder canceledOrder = new CanceledOrder()
                        { productId = pid };
                        _db.CanceledOrders.Add(canceledOrder);
                        _db.SaveChanges();
                    }
                    else
                    {
                        var salesOrderLine = _db.SalesOrderLines.FirstOrDefault(x => x.order_id == so && x.product_id == pid);
                        _db.Entry(salesOrderLine).State = EntityState.Deleted;
                        _db.SaveChanges();
                        var salesOrder = _db.SalesOrders.FirstOrDefault(x => x.order_ID == so);
                        salesOrder.GrandTotal = newGrandTotal;
                        _db.Entry(salesOrder).State = EntityState.Modified;
                        _db.SaveChanges();
                        var emartProfit = _db.EmartProfits.FirstOrDefault(x => x.SalesOrderID == so);
                        emartProfit.Amout = grandTotal * 0.1;
                        _db.Entry(emartProfit).State = EntityState.Modified;
                        _db.SaveChanges();
                        CanceledOrder canceledOrder = new CanceledOrder()
                        { productId = pid };
                        _db.CanceledOrders.Add(canceledOrder);
                        _db.SaveChanges();
                    }
                    return Redirect("~/Customers/ViewOrders");
                }
                else
                {
                    return View();
                }
            }
            catch (Exception e)
            {
                return Redirect("~/Customers/ErrorPage");
            }
        }
    }
}
