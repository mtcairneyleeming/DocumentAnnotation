using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace DocumentAnnotation.Models
{
    /// <summary>
    /// Represents a comment such as:
    ///     Juxtaposition: contrasts Milo and Clodius to show their hatred for each other
    ///     on 
    /// </summary>
    public class Annotation
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AnnotationId { get; set; }

        public string Title { get; set; }
        public string Body { get; set; }
        
        public List<Highlight> Highlights { get; set; }

        public int DocumentAnnotationId { get; set; }

        [JsonIgnore]
        public DocumentAnnotation DocumentAnnotation { get; set; }
    }
}