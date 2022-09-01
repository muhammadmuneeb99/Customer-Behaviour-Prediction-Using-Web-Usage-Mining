using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace FYP_Customer_Behavior_.Common
{
    public class TrackExecution : ActionFilterAttribute
    {
        public string Identifier = "";
        FYPEntities _db = new FYPEntities();
        //public override void OnActionExecuting(ActionExecutingContext filterContext)
        //{
        //    string message = DateTime.Now.ToString()
        //        + "\t" + filterContext.Controller.TempData["ID"]
        //        + "\t" + filterContext.HttpContext.Request.Url;
        //    LogExecutionTime(message);
        //}
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            string message = DateTime.Now.ToString()
                + "\t" + filterContext.Controller.TempData["ID"]
                + "\t" + filterContext.HttpContext.Request.Url;
            LogExecutionTime(message);
        }
        //public override void OnResultExecuting(ResultExecutingContext filterContext)
        //{
        //    stopWatch.Reset();
        //    stopWatch.Start();

        //    string message = "\n" + filterContext.RouteData.Values["controller"].ToString() + "/" +
        //         filterContext.RouteData.Values["action"].ToString() + " -> OnResultExecuting \t- " + DateTime.Now.ToString() + "\n";
        //    LogExecutionTime(message);
        //}

        //public override void OnResultExecuted(ResultExecutedContext filterContext)
        //{
        //    stopWatch.Stop();
        //    var executionTime = stopWatch.ElapsedMilliseconds;

        //    string message = "\n" + filterContext.RouteData.Values["controller"].ToString() + "/" +
        //         filterContext.RouteData.Values["action"].ToString() + " -> OnResultExecuting \t- " + DateTime.Now.ToString() + "\t" + executionTime + "\n";
        //    LogExecutionTime(message);
        //    LogExecutionTime("-----------------------------------------------------------");
        //}
        public void OnException(ExceptionContext exceptionContext)
        {
            string message = "\n" + exceptionContext.RouteData.Values["controller"].ToString() + "/" +
                 exceptionContext.RouteData.Values["action"].ToString() + " -> " + exceptionContext.Exception.Message + " \t- " + DateTime.Now.ToString() + "\n";
        }

        private async Task LogExecutionTime(string data)
        {
            string[] dataArr = data.Split('\t');
            Log log = new Log()
            {
                DateTime = dataArr[0],
                Users = dataArr[1],
                Url = dataArr[2]
            };
            _db.Logs.Add(log);
            await _db.SaveChangesAsync();
        }

    }
}