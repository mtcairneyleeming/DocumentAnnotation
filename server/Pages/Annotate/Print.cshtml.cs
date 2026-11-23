using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DocumentAnnotation.Authorization;
using DocumentAnnotation.Models;
using DocumentAnnotation.TextLoader;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Serilog;

// ReSharper disable RedundantCaseLabel

namespace DocumentAnnotation.Pages.Annotate
{
    public class PrintModel : PageModel
    {
        private readonly AnnotationContext _context;
        private readonly TextLoader.TextLoader _loader;
        private readonly IAuthorizationService _authorization;

        public PrintModel(AnnotationContext context, TextLoader.TextLoader loader, IAuthorizationService authorization)
        {
            _context = context;
            _loader = loader;
            _authorization = authorization;
        }

        public TextData TextData { get; set; }

        /// <summary>
        /// The text we are annotating - I believe this is passed by reference, so the whole thing doesn't have to be
        /// copied
        /// </summary>
        public Text Text { get; private set; }

        public int BookNum { get; private set; }


        public Book Book => Text.Books[BookNum];

        public Document DocAnn { get; private set; }

        public List<Annotator.Annotator> Annotators { get; private set; }


        public async Task<IActionResult> OnGet(int? docAnnId, string book)
        {
            if (docAnnId == null)
            {
                return NotFound();
            }

            // load the document requested, checking that the current user created it.
            DocAnn = _context.DocumentAnnotations.Include(da => da.User).Include(da => da.Text).SingleOrDefault(
                da =>
                    da.DocumentId == docAnnId);

            // same rules as the View page
            switch (await _authorization.CheckDocumentAsync(User, DocAnn, DocumentOperations.View))
            {
                case DocumentAccess.Allowed:
                    break;
                case DocumentAccess.Unauthenticated:
                    return Challenge();
                default:
                    return NotFound();
            }

            TextData = DocAnn.Text;

            if (TextData == null)
            {
                Log.Warning("A user attempted to load a document, {docId} that did not have a valid text, {textId}.",
                    DocAnn.DocumentId, DocAnn.TextId);
                return NotFound();
            }

            Text = _loader.LoadText(TextData.Identifier);

            if (book is null)
            {
                // make an assumption, open the last-viewed section of this document
                BookNum = DocAnn.LastLocation.BookNumber;
            }
            else // but book is not
            {
                // load the first section of the chosen book
                BookNum = Text.GetBookIndexFromName(book);
            }


            // load appropriate annotations
            var sectionsWithAnnotations = Book.Sections.Select((s, sectionIndex) => (s, _context.Annotations
                .Include(a => a.Highlights).ThenInclude(h => h.Location)
                .Where(a =>
                    a.DocumentId == docAnnId &&
                    a.Highlights.Any(h => h.Location.BookNumber == BookNum &&
                                          h.Location.SectionNumber == sectionIndex
                    ))
                .ToList()));

            Annotators = sectionsWithAnnotations.Select(swa => new Annotator.Annotator(swa.Item2, swa.s.Groups))
                .ToList();

            
            return Page();
        }
    }
}