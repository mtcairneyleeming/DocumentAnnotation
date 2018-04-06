using System.Collections.Generic;

namespace server.Pages.Annotate
{
    public struct SimpleHighlight
    {
        public int Start;
        public int End;
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

    public struct Pair
    {
        public Pair(int start, int end)
        {
            Start = start;
            End = end;
        }

        public int Start { get; set; }
        public int End { get; set; }
    }

    public struct PairWithData
    {
        public Pair Pair;
        public List<string> Classes;
        public List<int> AnnotationIds;

        public PairWithData(Pair pair, List<string> classes, List<int> annotationIds)
        {
            Pair = pair;
            Classes = classes;
            AnnotationIds = annotationIds;
        }
    }
}