using DocumentAnnotation.Models;

namespace DocumentAnnotation.Pages.Partials
{
    public class TextDetails
    {
        public TextData TextData;
        public string BookName;
        public string SectionName;

        public TextDetails(TextData textData, string bookName, string sectionName)
        {
            TextData = textData;
            BookName = bookName;
            SectionName = sectionName;
        }
    }
}