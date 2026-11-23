using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DocumentAnnotation.Authorization;
using DocumentAnnotation.Extensions;
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
    public class ViewModel : PageModel
    {
        private readonly AnnotationContext _context;
        private readonly TextLoader.TextLoader _loader;
        private readonly IAuthorizationService _authorization;

        public ViewModel(AnnotationContext context, TextLoader.TextLoader loader, IAuthorizationService authorization)
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
        public int SectionNum { get; private set; }

        public Book Book => Text.Books[BookNum];
        public Section Section => Book.Sections[SectionNum];
        public List<Group> Groups => Section.Groups;
        public Document DocAnn { get; private set; }

        public Annotator.Annotator Annotator { get; private set; }


        public async Task<IActionResult> OnGet(int? docAnnId, string book, string section, bool print = false)
        {
            if (docAnnId == null)
            {
                return NotFound();
            }

            // load the document requested, checking that the current user created it.
            var docAnn= _context.DocumentAnnotations.Include(da => da.User).Include(da => da.Text).SingleOrDefault(da =>
                da.DocumentId == docAnnId);

            // public documents: anyone; otherwise the owner and (for limited documents) the users it is shared with
            switch (await _authorization.CheckDocumentAsync(User, docAnn, DocumentOperations.View))
            {
                case DocumentAccess.Allowed:
                    break;
                case DocumentAccess.Unauthenticated:
                    return Challenge();
                default:
                    return NotFound();
            }

            TextData = docAnn.Text;

            if (TextData == null)
            {
                Log.Warning("A user attempted to load a document, {docId} that did not have a valid text, {textId}.", docAnn.DocumentId, docAnn.TextId);
                return NotFound();
            }
            
            Text = _loader.LoadText(TextData.Identifier);

            if (book is null)
            {
                // make an assumption, open the last-viewed section of this document
                BookNum = docAnn.LastLocation.BookNumber;
                SectionNum = docAnn.LastLocation.SectionNumber;
            }
            else if (section is null) // but book is not
            {
                // load the first section of the chosen book
                BookNum = Text.GetBookIndexFromName(book);
                SectionNum = 0;
            }
            else
            {
                (BookNum, SectionNum) = Text.GetIndexesFromName(book, section);
            }



            // load appropriate annotations
            var annotations = _context.Annotations
                .Include(a => a.Highlights).ThenInclude(h => h.Location)
                .Where(a =>
                    a.DocumentId == docAnnId &&
                    a.Highlights.Any(h => h.Location.BookNumber == BookNum &&
                                          h.Location.SectionNumber == SectionNum
                    ))
                .ToList();

            Annotator = new Annotator.Annotator(annotations, Groups);


            // remember the last-viewed location, but only for the owner: other viewers must not move it
            if (User.GetUserId() == docAnn.UserId)
            {
                docAnn.LastLocation = new Location(BookNum, SectionNum);
                _context.SaveChanges();
            }
            DocAnn = docAnn;

            Print = print;

            return Page();
        }

        public bool Print { get; set; }
    }
}