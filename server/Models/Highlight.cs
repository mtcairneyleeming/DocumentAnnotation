using System;

namespace server.Models
{
    public class Highlight
    {
        public Highlight()
        {
        }

        public Highlight(Location start, Location end)
        {
            if (start > end)
            {
                throw new ArgumentException("The end of a highlight must come after it starts");
            }
            Start = start;
            End = end;
        }
        public int HighlightId { get; set; }
        
        public Location Start { get; set; }
        public Location End { get; set; }
        
        public Annotation Annotation { get; set; }
        public int AnnotationId { get; set; }
    }
}