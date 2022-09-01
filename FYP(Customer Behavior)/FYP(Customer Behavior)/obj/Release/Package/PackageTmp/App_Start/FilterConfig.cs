using System.Web;
using System.Web.Mvc;

namespace FYP_Customer_Behavior_
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
