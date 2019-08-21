using System.Collections.Generic;
using DocumentAnnotation.TextLoader;

namespace DocumentAnnotation.Pages.Partials
{
    public class TextPartial
    {
        public TextPartial(List<Group> groups, bool addNewLine)
        {
            Groups = groups;
            AddNewLine = addNewLine;
        }

        public List<Group> Groups { get; set; }
        public bool AddNewLine { get; }
    }
    
}