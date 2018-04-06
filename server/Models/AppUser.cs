using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace server.Models
{
    public class AppUser : IdentityUser
    {
        public List<DocumentAnnotation> DocumentAnnotations { get; set; }
    }
}