using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using server.Models;

namespace server.Pages.Annotations
{
    public class CreateModel : PageModel
    {
        private readonly server.Models.AnnotationContext _context;

        public CreateModel(server.Models.AnnotationContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public Annotation Annotation { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Annotations.Add(Annotation);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}