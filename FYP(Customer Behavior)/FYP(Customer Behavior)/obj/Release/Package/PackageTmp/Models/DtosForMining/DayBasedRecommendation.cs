using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FYP_Customer_Behavior_.Models.DtosForMining
{
    public class DayBasedRecommendation
    {
        public int Id { get; set; }
        public String dateTime { get; set; }
        public String Users { get; set; }
        public string Urls { get; set; }

        FYPEntities _db = new FYPEntities();
        List<int> vs = new List<int>();
        //List<int> ids = new List<int>();

        public List<int> dayBasedRecommendation(string day, string UserId)
        {
            var items = _db.dayBased(UserId, day).ToList();

            foreach (var item in items)
            {
                int id = int.Parse(item.Split('=').Last());
                vs.Add(id);
            }
            return vs;
        }
    }
}