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
    public class DocumentListModel : PageModel
    {
        public readonly AnnotationContext _context;
        public readonly UserManager<AppUser> _userManager;

        public DocumentListModel(AnnotationContext context, UserManager<AppUser> userManager, ClaimsPrincipal user)
        {
            _context = context;
            _currentUser = user;
            _userManager = userManager;
        }

        private readonly ClaimsPrincipal _currentUser;

        public List<Document> MyDocuments { get; set; }
        public List<Document> DocumentsSharedWithMe { get; set; }

        public async Task OnGetAsync()
        {
            MyDocuments = await _context.DocumentAnnotations.Include(da=> da.User).Include(da=> da.Text).Where(da => da.UserId == GetCurrentUser())
                .ToListAsync();
            AppUser user = await _userManager.GetUserAsync(_currentUser);
            DocumentsSharedWithMe = await _context.DocumentAnnotations.Include(da=> da.User).Include(da=> da.Text).Where(da => da.Visibility == DocumentVisibility.Limited && da.AllowedUsers.Contains(user.Email))
                .ToListAsync();
        }
        
        private string GetCurrentUser()
        {
            return _currentUser.GetUserId();
        }
    }
}