using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using server.Models;

namespace server.Pages.Texts
{
    public class DeleteModel : PageModel
    {
        private readonly server.Models.AnnotationContext _context;

        public DeleteModel(server.Models.AnnotationContext context)
        {
            _context = context;
        }

        [BindProperty]
        public TextData Text { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Text = await _context.Texts.SingleOrDefaultAsync(m => m.TextId == id);

            if (Text == null)
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

            Text = await _context.Texts.FindAsync(id);

            if (Text != null)
            {
                _context.Texts.Remove(Text);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
