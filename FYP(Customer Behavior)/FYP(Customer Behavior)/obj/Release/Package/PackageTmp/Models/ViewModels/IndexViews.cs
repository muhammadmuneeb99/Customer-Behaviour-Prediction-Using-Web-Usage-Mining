using FYP_Customer_Behavior_.Models.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FYP_Customer_Behavior_.Models.ViewModels
{
    public class IndexViews
    {
        public List<ProductPriceImageDto> productPriceImageDto { get; set; }
        public List<ProductPriceImageDto> productPriceImageDtoMultiR { get; set; }
        public List<CatNameImage> catNameImages { get; set; }
        public List<BrandsDto> brandsDtos { get; set; }
    }
}