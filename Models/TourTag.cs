namespace AstrakhanExcursions.Models
{
    public class TourTag
    {
        public int TourId { get; set; }
        public int TagId { get; set; }
        public Tour Tour { get; set; }
        public Tag Tag { get; set; }
    }
}