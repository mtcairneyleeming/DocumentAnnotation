using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using DocumentAnnotation.Models;

namespace server.Pages.Admin.LinkShorteners
{
    public class CreateModel : PageModel
    {
        private readonly DocumentAnnotation.Models.AnnotationContext _context;

        public CreateModel(DocumentAnnotation.Models.AnnotationContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public LinkShortener LinkShortener { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.LinkShorteners.Add(LinkShortener);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}