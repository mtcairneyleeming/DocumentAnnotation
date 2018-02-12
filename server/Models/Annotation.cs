using System.Collections.Generic;

namespace server.Models {

    /// ownPOST<summary>
    /// Represents a comment such as:
    ///     Juxtaposition: contrasts Milo and Clodius to show their hatred for each other
    ///     on 
    /// </summary>
    public class Annotation {
        public int AnnotationId { get; set; }
        public string Title {get;set;}
        public string Body  {get;set;}
        public List<Highlight> Highlights {get;set;}
        
        public int DocumentAnnotationId { get; set; }
        public DocumentAnnotation DocumentAnnotation { get; set; }
    }
}