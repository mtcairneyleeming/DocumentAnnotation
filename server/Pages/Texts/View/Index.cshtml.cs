using System.Collections.Generic;
using System.Linq;
using DocumentAnnotation.Models;
using DocumentAnnotation.TextLoader;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DocumentAnnotation.Pages.Texts.View
{
    public class IndexModel : PageModel
    {
        private readonly AnnotationContext _context;
        private readonly TextLoader.TextLoader _loader;

        public IndexModel(AnnotationContext context, TextLoader.TextLoader loader)
        {
            _context = context;
            _loader = loader;
        }

        public TextData TextData { get; private set; }


        public Text Text { get; private set; }

        private int BookNum { get; set; }
        private int SectionNum { get; set; }

        public Book Book => Text.Books[BookNum];
        public Section Section => Book.Sections[SectionNum];
        public List<Group> Groups => Section.Groups;

        public IActionResult OnGet(string textIdentifier, string book, string section)
        {
            // load the text requested
            TextData = _context.Texts.SingleOrDefault(t => t.Identifier == textIdentifier);

            if (TextData == null)
            {
                return NotFound();
            }

            Text = _loader.LoadText(TextData.Identifier);
            
            if (book is null)
            {
                // make an assumption, open the first book
                BookNum = 0;
            }
            else if (section is null)
            {
                // load the first section of the chosen book
                BookNum = Text.GetBookIndexFromName(book);
                SectionNum = 0;
            }
            else
            {
                (BookNum, SectionNum) = Text.GetIndexesFromName(book, section);
            }



            return Page();
        }

    }
}