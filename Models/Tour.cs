namespace AstrakhanExcursions.Models
{
    public class Tour
    {
        public int TourId { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public User User { get; set; }
    }
}