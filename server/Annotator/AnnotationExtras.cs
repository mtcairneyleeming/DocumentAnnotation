using System.Collections.Generic;

namespace DocumentAnnotation.Annotator
{
    public struct SimpleHighlight
    {
        public int WordNumber;
        public int AnnotationId;
    }

    public enum HighlightType
    {
        Underline,
        Highlight,
        Bracket
    }

    public struct AnnotationData
    {
        public HighlightType Type { get; set; }
        public int Colour { get; set; }
    }
}