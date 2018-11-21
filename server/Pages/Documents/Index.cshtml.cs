using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DocumentAnnotation.Extensions;
using DocumentAnnotation.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DocumentAnnotation.Pages.Documents
{
    public class IndexModel : PageModel
    {
        public readonly AnnotationContext _context;
        public readonly UserManager<AppUser> _userManager;

        public IndexModel(AnnotationContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        public void OnGet()
        {
          
        }
        
    }
}