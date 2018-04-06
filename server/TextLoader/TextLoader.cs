using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using server.TextLoader.Models;
using Serilog;

namespace server.TextLoader
{
    /// <summary>
    /// Load texts, e.g. the Pro Milone from local xml files, as stored by Perseus. This keeps a cache of files, and is used as a singleton.
    /// </summary>
    public class TextLoader
    {
        private readonly List<Text> _texts = new List<Text>();
        private readonly IOptions<Config> _config;

        public TextLoader(IOptions<Config> config)
        {
            _config = config;
        }
        /// <summary>
        /// Clear the currently loaded texts so that new loads must use the file system
        /// </summary>
        public void ForceReload()
        {
            Log.Information("Forced reload of texts");
            _texts.Clear();
        }

        public Text LoadText(string textIdentifier)
        {
            if (IsLoaded(textIdentifier))
            {
                Log.Information($"Loaded text {textIdentifier} from memory");
                return GetLoaded(textIdentifier);
            }

            using (var originalFile = File.OpenRead($"{_config.Value.ProcessedTexts}/{textIdentifier}.json.gz"))
            {
                using (GZipStream decompressionStream = new GZipStream(originalFile, CompressionMode.Decompress))
                {
                    var text = DeserializeFromStream(decompressionStream);
                    text.Identifier = textIdentifier;
                    _texts.Add(text);
                    Log.Information($"Loaded text {textIdentifier} from disk");
                    return text;
                }
            }
        }

        private bool IsLoaded(string textIdentifier)
        {
            return _texts.Any(t => t.Identifier == textIdentifier);
            
        }
        
        private Text GetLoaded(string textIdentifier){
            return _texts.FirstOrDefault(t => t.Identifier == textIdentifier);
        }

        public int GetIndexFromName(string textIdentifier, string bookName)
        {
            var text = LoadText(textIdentifier);
            for (int i = 0; i < text.Books.Count; i++)
            {
                if (text.Books[i].Name == bookName)
                {
                    return i;
                }
            }
            throw new ArgumentOutOfRangeException($"No book of name {bookName} exists in the text with identifier {textIdentifier}");
        }

        public (int book, int section) GetIndexFromName(string textIdentifier, string bookName, string sectionName)
        {
            var text = LoadText(textIdentifier);
            var book = GetIndexFromName(textIdentifier, bookName);

            for (int i = 0; i < text.Books[book].Sections.Count; i++)
            {
                var testName = text.Books[book].Sections[i].Name;
                //Log.Information(text.Books[book].Sections[i].Name);
                
                if (testName == sectionName)
                {
                    
                    return (book,i);
                }
            }
            
            throw new ArgumentOutOfRangeException($"No section of name {sectionName} exists in the book {bookName} of the text with identifier {textIdentifier}");
           
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="textIdentifer"></param>
        /// <param name="book"></param>
        /// <param name="section"></param>
        /// <returns>A <see cref="Text"/> containing only the requested book/section</returns>
        private Text CloneText(string textIdentifer, int book, int? section = null)
        {
            var text = LoadText(textIdentifer);
            var ret = new Text
            {
                Identifier = text.Identifier,
                Name = text.Name,
                Books = new List<Book>()
            };
            ret.Books.Add(text.Books[book]);
            if (section is null)
            {
                return ret;
            }
            ret.Books[0].Sections = new List<Section> { ret.Books[0].Sections[section.Value]};

            return ret;

        } 

        /// <summary>
        /// Load a full text and return only the names of the books and sections within it.
        /// </summary>
        /// <param name="textIdentifier"></param>
        /// <returns>A <see cref="Text"/> </returns>
        public Text CloneNames(string textIdentifier)
        {
            var text = LoadText(textIdentifier);
            var ret = new Text
            {
                Identifier = textIdentifier,
                Name = text.Name,
                Books = new List<Book>()
            };
            foreach (var book in text.Books)
            {
                var newBook = new Book()
                {
                    Name = book.Name
                        
                };
                foreach (var section in book.Sections)
                {
                    newBook.Sections.Add(new Section(){Name = section.Name});
                }
                ret.Books.Add(newBook);
            }
            return ret;

        } 

        public Text LoadBook(string textIdentifier, string book)
        {
            var id = GetIndexFromName(textIdentifier, book);
            return CloneText(textIdentifier, id);
        }
        
        public Text LoadBook(string textIdentifier, int bookNum)
        {
            return CloneText(textIdentifier, bookNum);
        }
        public Text LoadSection(string textIdentifier, string book, string section)
        {
            var id = GetIndexFromName(textIdentifier, book, section);
            return CloneText(textIdentifier, id.book, id.section);
        }
        
        public Text LoadSection(string textIdentifier, int bookNum, int sectionNum)
        {
            return CloneText(textIdentifier, bookNum, sectionNum);
        }
        
        public Text LoadSection(string textIdentifier, string book, int sectionNum)
        {
            var id = GetIndexFromName(textIdentifier, book);
            return CloneText(textIdentifier, id, sectionNum);
        }


        private static Text DeserializeFromStream(Stream stream)
        {
            var serializer = new JsonSerializer();

            using (var sr = new StreamReader(stream))
            using (var jsonTextReader = new JsonTextReader(sr))
            {
                return serializer.Deserialize<Text>(jsonTextReader);
            }
        }
    }
}