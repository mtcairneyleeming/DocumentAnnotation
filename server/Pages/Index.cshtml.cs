using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using server.Models;

namespace server.Pages
{
    [AllowAnonymous]
    public class IndexModel : PageModel
    {
        public readonly AnnotationContext _context;

        public IndexModel(AnnotationContext context)
        {
            _context = context;
        }

        public void OnGet()
        {
        }
    }
}