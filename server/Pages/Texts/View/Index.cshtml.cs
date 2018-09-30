using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using DocumentAnnotation.Models;
using DocumentAnnotation.TextLoader.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DocumentAnnotation.Pages.Texts.View
{
    public class IndexModel : PageModel
    {
        private readonly AnnotationContext _context;
        public readonly TextLoader.TextLoader _loader;

        public IndexModel(AnnotationContext context, TextLoader.TextLoader loader)
        {
            _context = context;
            _loader = loader;
        }

        public TextData TextData { get; set; }

        /// <summary>
        /// The text we are annotating - I believe this is passed by reference, so the whole thing doesn't have to be
        /// copied
        /// </summary>
        public Text FullText { get; set; }

        private int BookNum { get; set; }
        private int SectionNum { get; set; }

        public Book Book => FullText.Books[BookNum];
        public Section Section => Book.Sections[SectionNum];
        public List<Group> Groups => Section.Groups;
        public Models.DocumentAnnotation DocAnn { get; set; }

        public Annotator.Annotator Annotator { get; private set; }


        public IActionResult OnGet(string textIdentifier, string book, string section)
        {
            // load the text requested
            TextData = _context.Texts.SingleOrDefault(t => t.Identifier == textIdentifier);

            if (TextData == null)
            {
                return NotFound();
            }

            if (book is null)
            {
                // make an assumption, open the first book
                BookNum = 0;
            }
            else if (section is null)
            {
                // load the first section of the chosen book
                BookNum = _loader.GetIndexFromName(TextData.Identifier, book);
                SectionNum = 0;
            }
            else
            {
                (BookNum, SectionNum) = _loader.GetIndexFromName(TextData.Identifier, book, section);
            }

            FullText = _loader.LoadText(TextData.Identifier);

            return Page();
        }

        private string GetCurrentUser()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier).Value;
        }
    }
}