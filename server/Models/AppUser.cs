using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace DocumentAnnotation.Models
{
    public class AppUser : IdentityUser
    {
        public List<Document> DocumentAnnotations { get; set; }
    }
}