using FYP_Customer_Behavior_.Models.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FYP_Customer_Behavior_.Models.DtosForMining
{
    public class DayNightAndTimeBasedRecommendation
    {
        public int Id { get; set; }
        public String DateTime { get; set; }
        public String Users { get; set; }
        public string Url { get; set; }
        FYPEntities _db = new FYPEntities();
        List<int> allBrandsId = new List<int>();
        List<int> _ids = new List<int>();
        List<int> vs = new List<int>();
        public List<int> dayAndnightBased(string datetime)
        {
            string format = String.Empty;
            if (datetime.Contains("AM"))
            { format = "AM"; }
            else if (datetime.Contains("PM"))
            { format = "PM"; }
            else { format = ""; };
            //_ids = new List<int>();
            var items = _db.dayAndNightBased(format).ToList();

            foreach (var item in items)
            {
                int id = int.Parse(item.Split('=').Last());
                vs.Add(id);
            }

            return vs;
        }
        List<int> ids = new List<int>();
        List<int> idsForBrand = new List<int>();
        public List<int> timeSpendBasedRecommendation(string UserId)
        {
            var items = _db.timeSpendBased(UserId).ToList();
            Dictionary<int, float> keyValuePairs = new Dictionary<int, float>();

            foreach (var item in items)
            {
                string[] arr0 = item.Split('=');

                int key = int.Parse(arr0.Last());
                float time = float.Parse(arr0[1].Split('&').First());
                if (!keyValuePairs.ContainsKey(key))
                {
                    keyValuePairs.Add(key, time);
                }
                else
                {
                    keyValuePairs[key] += time;
                }
            }

            idsForBrand = (from p in keyValuePairs orderby p.Value descending select p.Key).AsParallel().ToList();
            ids = (from p in keyValuePairs orderby p.Value descending select p.Key).Take(6).AsParallel().ToList();
            return ids;
        }
        List<int> rIds = new List<int>();
        RecommendationClass recommendationClass = new RecommendationClass();
        public List<int> RecommendFromTimeSpendProducts()
        {
            for (int i = 0; i < ids.Count; i++)
            {
                int d = ids[i];
                int scid = (from p in _db.Products where (p.Product_ID == d) select p.SubCatBrand.subSubCatId).FirstOrDefault();
                var products = (from Product in _db.Products where (Product.SubCatBrand.subSubCatId == scid) select Product);
                foreach (var itemToBeChecked in products)
                {
                    if (itemToBeChecked.Product_ID != ids[i])
                    {
                        var p1FeaturesCollection = _db.Product_Feature.Where(p => p.product_id == d);
                        string p1Features = string.Empty;
                        string p2Features = string.Empty;
                        foreach (var feature in p1FeaturesCollection)
                            p1Features = p1Features + "," + feature.featureDescription;
                        p1Features = p1Features.Remove(0, 1);
                        foreach (var feature in itemToBeChecked.Product_Feature)
                            p2Features = p2Features + "," + feature.featureDescription;
                        p2Features = p2Features.Remove(0, 1);
                        if (recommendationClass.CalculateSimilarity(p1Features, p2Features) > 0.3)
                        {
                            rIds.Add(itemToBeChecked.Product_ID);
                        }
                    }
                }
            }
            return rIds.Distinct().ToList();
        }
        List<int> bId = new List<int>();
        //List<BrandsDto> brands = new List<BrandsDto>();
        //List<int> brands = new List<int>();
        public void _BrandsFromTimeSpendProducts()
        {
            for (int i = 0; i < rIds.Count(); i++)
            {
                int id = rIds[i];
                int brandsId = (from p in _db.Products
                                join scb in _db.SubCatBrands on p.brandCategoryId equals scb.Id
                                where p.Product_ID == id
                                select scb.Brand.BrandId).FirstOrDefault();
                bId.Add(int.Parse(brandsId.ToString()));
            }
            bId = bId.Distinct().ToList();
            for (int i = 0; i < bId.Count(); i++)
            {
                int ___bid = bId[i];
                //var brand = (from br in _db.Brands
                //             where ___bid == br.BrandId
                //             select new BrandsDto
                //             {
                //                 brandID = ___bid,
                //                 brandName = br.Brand_Name,
                //                 brandImage = _db.Brand_Images.FirstOrDefault(x => x.BrandID == ___bid).Image,
                //                 proImg = _db.Product_Image.FirstOrDefault(x => x.Product.SubCatBrand.BrandID == ___bid).Image_1,
                //                 proImg1 = _db.Product_Image.FirstOrDefault(x => x.Product.SubCatBrand.BrandID == ___bid).Image_2
                //             }).FirstOrDefault();
                //brands.Add(brand);
                allBrandsId.Add(___bid);
            }
            //return brands.Distinct().Take(6).ToList();
        }
        //List<BrandsDto> brands = new List<BrandsDto>();
        //List<int> brands1 = new List<int>();
        public void _BrandsFromDayAndNight()//Still work to do
        {
            List<int> bId = new List<int>();
            for (int i = 0; i < vs.Count(); i++)
            {
                int id = vs[i];
                var brandsId = (from p in _db.Products
                                join scb in _db.SubCatBrands on p.brandCategoryId equals scb.Id
                                where p.Product_ID == id
                                select scb.Brand.BrandId).FirstOrDefault();
                bId.Add(int.Parse(brandsId.ToString()));
            }
            bId = bId.Distinct().ToList();
            for (int i = 0; i < bId.Count(); i++)
            {
                int ___bid = bId[i];
                //var brand = (from br in _db.Brands
                //             where br.BrandId == ___bid
                //             select new BrandsDto
                //             {
                //                 brandID = ___bid,
                //                 brandName = br.Brand_Name,
                //                 brandImage = _db.Brand_Images.FirstOrDefault(x => x.BrandID == ___bid).Image,
                //                 proImg = _db.Product_Image.FirstOrDefault(x => x.Product.SubCatBrand.BrandID == ___bid).Image_1,
                //                 proImg1 = _db.Product_Image.FirstOrDefault(x => x.Product.SubCatBrand.BrandID == ___bid).Image_2
                //             }).First();
                //brands1.Add(brand);
                allBrandsId.Add(___bid);
            }
            //return brands1.Distinct().Take(6).ToList();
        }

        public void _Brands()
        {
            var idsBrand = _db.Brands.Select(x => x.BrandId).ToList();
            //List<BrandsDto> brands = (from br in _db.Brands
            //                          select new BrandsDto
            //                          {
            //                              brandID = br.BrandId,
            //                              brandName = br.Brand_Name,
            //                              brandImage = _db.Brand_Images.FirstOrDefault(x => x.BrandID == br.BrandId).Image,
            //                              proImg = _db.Product_Image.FirstOrDefault(x => x.Product.SubCatBrand.BrandID == br.BrandId).Image_1,
            //                              proImg1 = _db.Product_Image.FirstOrDefault(x => x.Product.SubCatBrand.BrandID == br.BrandId).Image_2
            //                          }).ToList();
            for (int i = 0; i < idsBrand.Count; i++)
            {
                int ___bid = idsBrand[i];
                allBrandsId.Add(___bid);
            }
            //return brands.Distinct().Take(6).ToList();
        }
        List<BrandsDto> brands = new List<BrandsDto>();
        public List<BrandsDto> brandsResult()
        {
            allBrandsId = allBrandsId.Distinct().Take(6).ToList();
            foreach (var item in allBrandsId)
            {
                int id = item;
                var brand = (from br in _db.Brands
                             where br.BrandId == id
                             select new BrandsDto
                             {
                                 brandID = br.BrandId,
                                 brandName = br.Brand_Name,
                                 brandImage = _db.Brand_Images.FirstOrDefault(x => x.BrandID == id).Image,
                                 proImg = _db.Product_Image.FirstOrDefault(x => x.Product.SubCatBrand.BrandID == id).Image_1,
                                 proImg1 = _db.Product_Image.FirstOrDefault(x => x.Product.SubCatBrand.BrandID == id).Image_2
                             }).FirstOrDefault();
                brands.Add(brand);
            }
            return brands;
        }
    }
}