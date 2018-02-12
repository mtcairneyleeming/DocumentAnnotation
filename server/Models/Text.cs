namespace server.Models
{
    public class Text
    {
        public int TextId { get; set; }
        public string Title { get; set; }
        public string Abbreviation { get; set; } // abbreviation - should be same as perseus, e.g. "Cic. Mil." for the Pro Milone
    }
}