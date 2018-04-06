using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace server.Models
{
    /// <summary>
    /// A record that represents a user's annotation of a document, and multiple can exist for a document-user combination.
    /// </summary>
    public class DocumentAnnotation
    {
        public int DocumentAnnotationId { get; set; }
        
        public int TextId { get; set; }
        public TextData Text { get; set; }

        
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public AppUser User { get; set; }
        
        
        public string Name { get; set; }
        
        public List<Annotation> Annotations { get; set; }
        
        }
}