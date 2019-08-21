using DocumentAnnotation.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DocumentAnnotation.Pages.Documents
{
    public class IndexModel : PageModel
    {
        public readonly AnnotationContext Context;
        public readonly UserManager<AppUser> UserManager;

        public IndexModel(AnnotationContext context, UserManager<AppUser> userManager)
        {
            Context = context;
            UserManager = userManager;
        }


        public void OnGet()
        {
          
        }
        
    }
}