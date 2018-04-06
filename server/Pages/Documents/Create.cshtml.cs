using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using server.Models;

namespace server.Pages.Documents
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
        public DocumentAnnotation DocumentAnnotation { get; set; }
        public IList<TextData> Texts { get; set; }
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            DocumentAnnotation.User = await _userManager.GetUserAsync(User);
            _context.DocumentAnnotations.Add(DocumentAnnotation);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}