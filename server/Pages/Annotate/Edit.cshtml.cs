using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using DocumentAnnotation.Models;
using DocumentAnnotation.TextLoader.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace DocumentAnnotation.Pages.Annotate
{
    public class EditModel : PageModel
    {
        private readonly AnnotationContext _context;
        public readonly TextLoader.TextLoader _loader;

        public EditModel(AnnotationContext context, TextLoader.TextLoader loader)
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

        public int BookNum { get; set; }
        public int SectionNum { get; set; }

        public Book Book => FullText.Books[BookNum];
        public Section Section => Book.Sections[SectionNum];
        public List<Group> Groups => Section.Groups;
        public Models.DocumentAnnotation DocAnn { get; set; }

        public Annotator.Annotator Annotator { get; private set; }

        [BindProperty]
        public Annotation NewAnnotation { get; set; }

        public IActionResult OnGet(int? docAnnId, string book, string section)
        {
            if (docAnnId == null)
            {
                return NotFound();
            }

            // load the document requested, checking that the current user created it.
            DocAnn = _context.DocumentAnnotations.Include(da => da.User).Include(da => da.Text).SingleOrDefault(da =>
                da.DocumentAnnotationId == docAnnId && da.UserId == GetCurrentUser());

            if (DocAnn is null)
            {
                return NotFound();
            }

            TextData = DocAnn.Text;

            if (TextData == null)
            {
                return NotFound();
            }

            if (book is null)
            {
                // make an assumption, open the first book
                BookNum = 0;
                SectionNum = 0;
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

            // load appropriate annotations
            var annotations = _context.Annotations
                .Include(a => a.Highlights).ThenInclude(h => h.Location)
                .Where(a =>
                    a.DocumentAnnotationId == docAnnId &&
                    a.Highlights.Any(h => h.Location.BookNumber == BookNum &&
                                          h.Location.SectionNumber == SectionNum
                    ))
                .Include(a => a.Highlights)
                .ToList();

            Annotator = new Annotator.Annotator(annotations, Groups);
            return Page();
        }

        private string GetCurrentUser()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier).Value;
        }
    }
}