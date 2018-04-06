using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using server.Models;

namespace server.Pages.Documents
{
    public class EditModel : PageModel
    {
        private readonly AnnotationContext _context;

        public EditModel(AnnotationContext context)
        {
            _context = context;
        }

        [BindProperty]
        public DocumentAnnotation DocumentAnnotation { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            DocumentAnnotation = await _context.DocumentAnnotations.SingleOrDefaultAsync(m => m.DocumentAnnotationId == id);

            if (DocumentAnnotation == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(DocumentAnnotation).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DocumentAnnotationExists(DocumentAnnotation.DocumentAnnotationId))
                {
                    return NotFound();
                }

                throw;
            }

            return RedirectToPage("./Index");
        }

        private bool DocumentAnnotationExists(int id)
        {
            return _context.DocumentAnnotations.Any(e => e.DocumentAnnotationId == id);
        }
    }
}
