namespace AstrakhanExcursions.Models
{
    public class Route
    {
        public int RouteId { get; set; }
        public int TourId { get; set; }
        public int PlaceId { get; set; }
        public int OrderInRoute { get; set; }
        public Tour Tour { get; set; }
        public Place Place { get; set; }
    }
}