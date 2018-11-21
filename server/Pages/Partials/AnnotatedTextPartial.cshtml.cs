using System.Collections;
using System.Collections.Generic;
using DocumentAnnotation.TextLoader;

namespace DocumentAnnotation.Pages.Partials
{
    public class AnnotatedTextPartial
    {
        public AnnotatedTextPartial(List<Group> groups, List<List<List<int>>> highlightingByWord, bool AddNewLine)
        {
            Groups = groups;
            HighlightingByWord = highlightingByWord;
            this.AddNewLine = AddNewLine;
        }

        public List<Group> Groups { get; set; }
        public List<List<List<int>>> HighlightingByWord { get; set; }
        public bool AddNewLine { get; }
    }
}