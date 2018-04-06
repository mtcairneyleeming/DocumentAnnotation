using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using server.Models;

namespace server.Pages.Documents
{
    public class DeleteModel : PageModel
    {
        private readonly AnnotationContext _context;

        public DeleteModel(AnnotationContext context)
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

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            DocumentAnnotation = await _context.DocumentAnnotations.FindAsync(id);

            if (DocumentAnnotation != null)
            {
                _context.DocumentAnnotations.Remove(DocumentAnnotation);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
