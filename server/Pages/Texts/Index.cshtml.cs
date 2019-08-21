using DocumentAnnotation.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DocumentAnnotation.Pages.Texts
{
    public class IndexModel : PageModel
    {
        public readonly AnnotationContext Context;

        public IndexModel(AnnotationContext context)
        {
            Context = context;
        }

        public void OnGet()
        {
            
        }
    }
}