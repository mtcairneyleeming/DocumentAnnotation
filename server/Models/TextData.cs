using System.ComponentModel.DataAnnotations;

namespace DocumentAnnotation.Models
{
    public class TextData
    {
        [Key]
        public int TextId { get; set; }

        public string Title { get; set; }
        public string Author { get; set; }
        public string Abbreviation { get; set; } // abbreviation - should be same as perseus, e.g. "Cic. Mil." for the Pro Milone
        public string Identifier { get; set; } // identifier for a text on Perseus, e.g. 1999.02.0011
                
        /// <summary>
        /// Set to true if text should not organically continue after each group in this text - e.g. true for verse, but false for everything else.
        /// </summary>
        public bool AddNewLine { get; set; }

    }
}