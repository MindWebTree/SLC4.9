using System;

namespace MWT.Plugin.Misc.MwtStorefront.Models.Custom
{
    public class ReviewModel:ICloneable
    {
        public int Rating { get; set; }
        public string Suggestion { get; set; }
        public DateTime FeedBackDate { get; set; }
        public string CustomerName { get; set; }
        public string FeedbackImages { get; set; }
        public object Tags { get; set; }
        public int OrderID { get; set; }
        public string ProductImageUrl { get; set; }
        public string Albumn { get; set; }
        public string State { get; set; }
        public string City { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
