using System.ComponentModel.DataAnnotations;

namespace server.Models
{
    public class TextData
    {
        [Key]
        public int TextId { get; set; }
        public string Title { get; set; }
        public string Abbreviation { get; set; } // abbreviation - should be same as perseus, e.g. "Cic. Mil." for the Pro Milone
        public string Identifier { get; set; } // identifier for a text on Perseus, e.g. 1999.02.0011
    }
}