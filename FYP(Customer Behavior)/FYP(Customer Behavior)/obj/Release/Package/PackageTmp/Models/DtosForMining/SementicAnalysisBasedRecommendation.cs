using System.Collections.Generic;
using System.Linq;
using VaderSharp;

namespace FYP_Customer_Behavior_.Models.DtosForMining
{
    public class SementicAnalysisBasedRecommendation
    {
        public int productId { get; set; }
        public string review { get; set; }

        FYPEntities _db = new FYPEntities();

        public List<int> reviewsBasedRecommendation()
        {
            var reviewRecommendation = (from rr in _db.RatingsAndReviews where rr.review != null
                                        select new SementicAnalysisBasedRecommendation
                                        {
                                            productId = rr.productId,
                                            review = rr.review
                                        }).ToList();

            SentimentIntensityAnalyzer analyzer = new SentimentIntensityAnalyzer();
            List<int> ids = new List<int>();
            for (int i = 0; i < reviewRecommendation.Count; i++)
            {
                var result = analyzer.PolarityScores(reviewRecommendation[i].review);
                if (result.Positive >= 0.3)
                {
                    ids.Add(reviewRecommendation[i].productId);
                }
            }
            return ids;
        }
    }
}