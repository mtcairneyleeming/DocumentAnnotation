using System;
using System.Collections.Generic;

namespace DocumentAnnotation.TextLoader
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// 
        /// <param name="book"></param>
        /// <param name="section"></param>
        /// <returns>A <see cref="Text"/> containing only the requested book/section</returns>
        public Text CloneText(int book, int? section = null)
        {
            var ret = new Text
            {
                Identifier = Identifier,
                Name = Name,
                Books = new List<Book>()
            };
            ret.Books.Add(Books[book]);
            if (section is null)
            {
                return ret;
            }

            ret.Books[0].Sections = new List<Section> {ret.Books[0].Sections[section.Value]};

            return ret;
        }

        /// <summary>
        /// Return only the names of the books and sections within this text.
        /// </summary>
        /// <returns>A <see cref="Text"/> </returns>
        public Text CloneNames()
        {
            var ret = new Text
            {
                Identifier = Identifier,
                Name = Name,
                Books = new List<Book>()
            };
            foreach (var book in Books)
            {
                var newBook = new Book()
                {
                    Name = book.Name
                };
                foreach (var section in book.Sections)
                {
                    newBook.Sections.Add(new Section() {Name = section.Name});
                }

                ret.Books.Add(newBook);
            }

            return ret;
        }
        public int GetBookIndexFromName(string bookName)
        {
            for (int i = 0; i < Books.Count; i++)
            {
                if (Books[i].Name == bookName)
                {
                    return i;
                }
            }

            throw new ArgumentOutOfRangeException($"No book of name {bookName} exists in the text with identifier {Identifier}");
        }

        public (int book, int section) GetIndexesFromName(string bookName, string sectionName)
        {
            var book = GetBookIndexFromName(bookName);

            for (int i = 0; i < Books[book].Sections.Count; i++)
            {
                var testName = Books[book].Sections[i].Name;
                //Log.Information(text.Books[book].Sections[i].Name);

                if (testName == sectionName)
                {
                    return (book, i);
                }
            }

            throw new ArgumentOutOfRangeException(
                $"No section of name {sectionName} exists in the book {bookName} of the text with identifier {Identifier}");
        }
        
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
        public List<string> Data { get; set; }


    }
}