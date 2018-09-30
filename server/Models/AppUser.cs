using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace DocumentAnnotation.Models
{
    public class AppUser : IdentityUser
    {
        public List<DocumentAnnotation> DocumentAnnotations { get; set; }
    }
}