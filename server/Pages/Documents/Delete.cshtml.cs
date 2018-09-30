using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocumentAnnotation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DocumentAnnotation.Pages.Documents
{
    public class DeleteModel : PageModel
    {
        private readonly AnnotationContext _context;

        public DeleteModel(AnnotationContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Models.DocumentAnnotation DocumentAnnotation { get; set; }

        public IList<TextData> Texts { get; set; }
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            DocumentAnnotation = await _context.DocumentAnnotations.SingleOrDefaultAsync(m => m.DocumentAnnotationId == id);
            Texts = _context.Texts.ToList();
            
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