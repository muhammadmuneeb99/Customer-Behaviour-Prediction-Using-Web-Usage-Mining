using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FYP_Customer_Behavior_.Models.Dtos
{
    public class CountryStateCityAreaBind
    {
        public int countryID { get; set; }
        public int stateID { get; set; }
        public int cityID { get; set; }
        public int areaID { get; set; }

    }
}