using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using server.Models;

namespace server.Pages.Annotations
{
    public class EditModel : PageModel
    {
        private readonly server.Models.AnnotationContext _context;

        public EditModel(server.Models.AnnotationContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Annotation Annotation { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Annotation = await _context.Annotations.SingleOrDefaultAsync(m => m.AnnotationId == id);

            if (Annotation == null)
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

            _context.Attach(Annotation).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AnnotationExists(Annotation.AnnotationId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool AnnotationExists(int id)
        {
            return _context.Annotations.Any(e => e.AnnotationId == id);
        }
    }
}
