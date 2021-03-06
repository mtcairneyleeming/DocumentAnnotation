using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocumentAnnotation.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DocumentAnnotation.Pages.Documents
{
    public class CreateModel : PageModel
    {
        private readonly AnnotationContext _context;
        private readonly UserManager<AppUser> _userManager;

        public CreateModel(AnnotationContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult OnGet()
        {
            // load texts for select dropdown
            Texts = _context.Texts.ToList();


            return Page();
        }

        [BindProperty]
        public Document Document { get; set; }

        public IList<TextData> Texts { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            Document.LastLocation = new Location(0, 0);
            Document.User = await _userManager.GetUserAsync(User);
            _context.DocumentAnnotations.Add(Document);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}