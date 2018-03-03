using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using server.Models;

namespace server.Pages.Texts
{
    public class EditModel : PageModel
    {
        private readonly server.Models.AnnotationContext _context;

        public EditModel(server.Models.AnnotationContext context)
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

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(Text).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TextExists(Text.TextId))
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

        private bool TextExists(int id)
        {
            return _context.Texts.Any(e => e.TextId == id);
        }
    }
}