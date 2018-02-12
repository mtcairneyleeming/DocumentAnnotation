using System.Collections.Generic;

namespace server.TextLoader.Models
{
    public class FullDocument
    {
        public DocumentInfo Info { get; set; }
        public List<Text> Texts { get; set; }
    }

    /// <summary>
    /// An individual text within an XML document, e.g. the Pro Milone of the Philippics in the document for all Cicero's Orationes
    /// </summary>
    public class TextOld
    {
        public string Name { get; set; }
        public string Abbreviation { get; set; }
        
        
        public List<Level> Sections { get; set; }
    }

    /// <summary>
    /// A limited subset of the information in a TEI.2 <titleStmt> element
    /// </summary>
    public class DocumentInfo
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Author { get; set; }
        
    }

    public class Level
    {
        public MilestoneType Type { get; set; }
        private List<Level> Sublevel { get; set; }
        private List<string> Text { get; set; }

        public (bool hasChildren, List<Level> children, List<string> text) GetData()
        {
            if (this.Sublevel is null || this.Sublevel.Count == 0)
            {
                return (false, null, this.Text);
            }
            else
            {
                return (true, this.Sublevel, null);
            }
        }
    }

    public enum MilestoneType
    {
        Chapter,
        Section
    }
    
}