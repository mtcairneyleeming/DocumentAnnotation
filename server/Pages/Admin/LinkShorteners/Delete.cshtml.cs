﻿using System.Threading.Tasks;
using DocumentAnnotation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DocumentAnnotation.Pages.Admin.LinkShorteners
{
    public class DeleteModel : PageModel
    {
        private readonly AnnotationContext _context;

        public DeleteModel(AnnotationContext context)
        {
            _context = context;
        }

        [BindProperty]
        public LinkShortener LinkShortener { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            LinkShortener = await _context.LinkShorteners.FirstOrDefaultAsync(m => m.LinkShortenerId == id);

            if (LinkShortener == null)
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

            LinkShortener = await _context.LinkShorteners.FindAsync(id);

            if (LinkShortener != null)
            {
                _context.LinkShorteners.Remove(LinkShortener);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
