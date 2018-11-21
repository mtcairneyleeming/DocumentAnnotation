using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using DocumentAnnotation.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DocumentAnnotation.Pages.Texts
{
    public class TextListModel:PageModel
    {
        public ClaimsPrincipal CurrentUser { get; }
        private readonly AnnotationContext _context;

        public TextListModel(AnnotationContext context, ClaimsPrincipal currentUser)
        {
            CurrentUser = currentUser;
            _context = context;
        }

        public IList<TextData> Text { get; set; }

        public async Task OnGetAsync()
        {
            Text = await _context.Texts.ToListAsync();
        }
    }
}