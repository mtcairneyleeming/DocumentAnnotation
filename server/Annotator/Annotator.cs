using System.Collections.Generic;
using System.Linq;
using DocumentAnnotation.Models;
using DocumentAnnotation.TextLoader.Models;
using Npgsql.TypeHandlers;

namespace DocumentAnnotation.Annotator
{
    public class Annotator
    {
        public Annotator(List<Annotation> annotations, List<Group> groups)
        {
            Annotations = annotations.Where(a=> a.Highlights.Count > 0).OrderBy(a=> a.Highlights[0].Location).ToList();
            Annotations.AddRange(annotations.Where(a=> a.Highlights.Count == 0));
            Groups = groups;
            Run();
        }


        // inputs
        public List<Annotation> Annotations { get; }
        private List<Group> Groups { get; }

        

        // internal working 
        /// <summary>
        /// Map of highlight type to the initial used in the CSS class
        /// </summary>
        private Dictionary<HighlightType, string> HighlightInitials { get; } =
            new Dictionary<HighlightType, string>
            {
                {HighlightType.Bracket, "B"},
                {HighlightType.Underline, "U"},
                {HighlightType.Highlight, "H"}
            };

        private Dictionary<HighlightType, Dictionary<int, string>> Colours { get; } =
            new Dictionary<HighlightType, Dictionary<int, string>>
            {
                {
                    HighlightType.Bracket, new Dictionary<int, string>
                    {
                        {1, "#666666"},
                        {2, "#666666"}
                    }
                },
                {
                    HighlightType.Underline, new Dictionary<int, string>
                    {
                        {1, "#008000"},
                        {2, "#0000ff"},
                        {3, "#ff0000"},
                        {4, "#9370db"}
                    }
                },
                {
                    HighlightType.Highlight, new Dictionary<int, string>
                    {
                        {2, "#48CCC9"},
                        {3, "#FFA858"},
                        {4, "#74FFAD"},
                        {5, "#FF8CAC"},
                        {6, "#CC54B1"}
                    }
                },
            };

        /// <summary>
        /// Data to determine class, colour and the like for each annotation
        /// </summary>
        private Dictionary<int, AnnotationData> AnnotationData { get; } = new Dictionary<int, AnnotationData>();

        /// <summary>
        /// a list of highlights, sorted by group.
        /// </summary>
        public List<List<SimpleHighlight>> Highlights { get; } = new List<List<SimpleHighlight>>();

        /// <summary>
        /// Generate a list of (hopefully) distinct colours with which to highlight stuff
        /// </summary>
        private void GenerateHighlightData()
        {
            for (var i = 0; i < Annotations.Count; i++)
            {
                var h = Annotations[i].AnnotationId % 11; // number of the highlight this annotation will receive
                if (h < 4) // leads to annU2...5
                {
                    AnnotationData.Add(Annotations[i].AnnotationId, new AnnotationData
                    {
                        Colour = h + 1,
                        Type = HighlightType.Underline
                    });
                }
                else if (h < 9) // leads to annH2...6
                {
                    AnnotationData.Add(Annotations[i].AnnotationId, new AnnotationData
                    {
                        Colour = h - 2,
                        Type = HighlightType.Highlight
                    });
                }
                else // leads to annU1...2
                {
                    AnnotationData.Add(Annotations[i].AnnotationId, new AnnotationData
                    {
                        Colour = h - 8,
                        Type = HighlightType.Bracket
                    });
                }
            }
        }

        private void SimplifyHighlights()
        {
            Highlights.Clear();
            for (var i = 0; i < Groups.Count; i++)
            {
                Highlights.Add(new List<SimpleHighlight>());
                foreach (var annotation in Annotations)
                {
                    foreach (var h in annotation.Highlights)
                    {
                        if (h.Location.GroupNumber == i)
                        {
                            Highlights[i].Add(new SimpleHighlight
                            {
                                WordNumber = h.Location.WordNumber,
                                AnnotationId = annotation.AnnotationId
                            });
                        }
                    }
                }
            }
        }

        private bool IsWordHighlighted(int groupNo, int wordNo, int annId)
        {
            foreach (var highlight in Highlights[groupNo])
            {
                if (highlight.WordNumber == wordNo && highlight.AnnotationId == annId)
                {
                    return true;
                }
            }

            return false;
        }


        public List<string> GetClasses(int groupNo, int wordNumber)
        {
            var classes = new List<string>();
            foreach (var highlight in Highlights[groupNo])
            {
                
                if (highlight.WordNumber == wordNumber)
                {
                    var className = GetClassName(highlight.AnnotationId);
                    if (AnnotationData[highlight.AnnotationId].Type == HighlightType.Bracket)
                    {
                        int prevGroup, prevWord, nextGroup, nextWord;
                        switch (wordNumber)
                        {
                            case 0 when groupNo == 0:
                                prevGroup = 0;
                                prevWord = -1; // there is no word before this one (in this section)
                                break;
                            case 0:
                                prevWord = Groups[groupNo - 1].Data.Count - 1;
                                prevGroup = groupNo - 1;
                                break;
                            default:
                                prevGroup = groupNo;
                                prevWord = wordNumber - 1;
                                break;
                        }

                        var maxWords = Groups[groupNo].Data.Count;
                        switch (maxWords - wordNumber)
                        {
                            case 1 when groupNo == Groups.Count - 1:
                                nextGroup = groupNo;
                                nextWord = -1; // there is no word after this one (in this section)
                                break;
                            case 1:
                                nextWord = 0;
                                nextGroup = groupNo + 1;
                                break;
                            default:
                                nextGroup = groupNo;
                                nextWord = wordNumber + 1;
                                break;
                        }

                        var isNextHighlighted = IsWordHighlighted(nextGroup, nextWord, highlight.AnnotationId);
                        var isPrevHighlighted = IsWordHighlighted(prevGroup, prevWord, highlight.AnnotationId);

                        // ends at the end of this <span> 
                        if (!isNextHighlighted)
                        {
                            className += "End";
                        }

                        // starts at the start of this <span> 
                        if (!isPrevHighlighted)
                        {
                            className += "Start";
                        }

                        // goes straight through this <span>
                        if (isPrevHighlighted && isNextHighlighted)
                        {
                            className += "Middle";
                        }
                    }


                        classes.Add(className);
                    
                }

                // 
            }

            return classes;
        }

        public string GetClassName(int annId)
        {
            var annDetails = AnnotationData[annId];
            var typeInitial = HighlightInitials[annDetails.Type];
            var className = $"ann{typeInitial}{annDetails.Colour}";
            return className;
        }
        


        public List<int> GetAnnotationIds(int groupNo, int character)
        {
            var ids = new List<int>();
            foreach (var highlight in Highlights[groupNo])
            {
                // any relation at all to this span
                if (highlight.WordNumber == character)
                {
                    ids.Add(highlight.AnnotationId);
                }
            }

            return ids;
        }


        public void Run()
        {
            GenerateHighlightData();
            SimplifyHighlights();

        }

        public string GetColourName(int annotationId)
        {
            var annData = AnnotationData[annotationId];

            return Colours[annData.Type][annData.Colour];
        }
    }
}