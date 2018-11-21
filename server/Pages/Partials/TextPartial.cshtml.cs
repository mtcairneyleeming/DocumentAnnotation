using System.Collections;
using System.Collections.Generic;
using DocumentAnnotation.TextLoader;

namespace DocumentAnnotation.Pages.Partials
{
    public class TextPartial
    {
        public TextPartial(List<Group> groups, bool AddNewLine)
        {
            Groups = groups;
            this.AddNewLine = AddNewLine;
        }

        public List<Group> Groups { get; set; }
        public bool AddNewLine { get; }
    }
    
}