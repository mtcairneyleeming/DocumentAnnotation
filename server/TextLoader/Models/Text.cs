
using System.Collections.Generic;

namespace server.TextLoader.Models
{
    /// <summary>
    /// Represents an entire text, such as Cicero's Orationes
    /// </summary>
    public class Text
    {
        /// <summary>
        /// Identifier for the text in the Database
        /// </summary>
        public string Identifier { get; set; }
        public string Name { get; set; }
        public List<Book> Books { get; set; } = new List<Book>();
    }

    /// <summary>
    /// Represents a book within a text, eg.g the Pro Milone, or the eighth book of the Aeneid. Has a name for user-facing displays, but is identified by index in the containing list.
    /// </summary>
    public class Book
    {
        public string Name { get; set; }
        public List<Section> Sections { get; set; } = new List<Section>();
    }

    /// <summary>
    /// Represents a section in a book, e.g. chapter 24 in the Pro Mil., or lines 102-125 in Book 8 of the Aeneid. Has a name for user-facing displays, but is identified by index in the containing list.
    /// </summary>
    public class Section
    {
        public string Name { get; set; }
        public List<Group> Groups { get; set; } = new List<Group>();
    }

    /// <summary>
    /// Represents a group of words in a section, e.g. a line in the Aeneid, and an arbitary number of words in the Pro Milone. Has a name for user-facing displays, but is identified by index in the containing list.
    /// </summary>
    public class Group
    {
        public string Name { get; set; }
        /// <summary>
        /// contains all text within this group
        /// </summary>
        public string Data { get; set; }
        /// <summary>
        /// Set to true if text should not organically continue after this group - e.g. true for verse, but false for everything else.
        /// </summary>
        public bool AddNewLine { get; set; }
    }
    
}