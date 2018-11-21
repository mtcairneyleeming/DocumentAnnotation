using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DocumentAnnotation.Models
{
    /// <summary>
    /// A record that represents a user's annotation of a document, and multiple can exist for a document-user combination.
    /// </summary>
    public class Document
    {
        public int DocumentId { get; set; }

        public int TextId { get; set; }
        public TextData Text { get; set; }

        public Location LastLocation { get; set; }
        public DocumentVisibility Visibility { get; set; }
        // a list of the users allowed to view this document
        public string[] AllowedUsers { get; set; }
        
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public AppUser User { get; set; }


        public string Name { get; set; }

        public List<Annotation> Annotations { get; set; }
    }
}