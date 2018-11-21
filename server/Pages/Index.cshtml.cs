using DocumentAnnotation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DocumentAnnotation.Pages
{
    [AllowAnonymous]
    public class IndexModel : PageModel
    {
        public readonly AnnotationContext _context;

        public IndexModel(AnnotationContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            UserManager = userManager;
        }

        public UserManager<AppUser> UserManager { get; set; }

        public void OnGet()
        {
        }
    }
}