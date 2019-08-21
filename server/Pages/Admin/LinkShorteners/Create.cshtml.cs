using System.Threading.Tasks;
using DocumentAnnotation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DocumentAnnotation.Pages.Admin.LinkShorteners
{
    public class CreateModel : PageModel
    {
        private readonly AnnotationContext _context;

        public CreateModel(AnnotationContext context)
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