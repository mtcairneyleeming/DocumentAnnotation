var underLineColours = [
    "#e6194B",
    "#3cb44b",
    "#ffe119",
    "#4363d8",
    "#f58231",
    "#911eb4",
    "#42d4f4",
    "#f032e6",
    "#7def00",
    "#fabebe",
    "#469990",
    "#e6beff",
    "#9A6324",
    "#800000",
]; //14
var highlightColours = [
    "#ff0000",
    "#00ff00",
    "#42d4f4",
    "#9c70ff",
    "#ff30de",
    "#ff5e00",
    "#ffff00"
]; //6
var bracketColours = [
    "#000000",
    "#000075",
    "#800000",
    "#911eb4"
]; //4


export enum HighlightType {
    Underline,
    Highlight,
    Brackets
}
type HighlightedId = [HighlightType, number]

var colourMap: HighlightedId[] = [
    [HighlightType.Highlight, 0],
    [HighlightType.Underline, 0],
    [HighlightType.Underline, 1],
    [HighlightType.Underline, 2],
    [HighlightType.Highlight, 1],
    [HighlightType.Brackets, 0],
    [HighlightType.Underline, 3],
    [HighlightType.Underline, 4],
    [HighlightType.Highlight, 2],
    [HighlightType.Brackets, 1],
    [HighlightType.Underline, 5],
    [HighlightType.Underline, 6],
    [HighlightType.Highlight, 3],
    [HighlightType.Brackets, 2],
    [HighlightType.Underline, 7],
    [HighlightType.Underline, 8],
    [HighlightType.Highlight, 4],
    [HighlightType.Underline, 9],
    [HighlightType.Underline, 10],
    [HighlightType.Brackets, 3],
    [HighlightType.Highlight, 5],
    [HighlightType.Underline, 11],
    [HighlightType.Underline, 12],
    [HighlightType.Underline, 13],
];
export function getAnnotationData(annId: number): HighlightedId {
    if (annId == 0) {
        return [HighlightType.Highlight, 6];
    }
    var index = annId % 24;
    return colourMap[index];
}
export function getAnnotationColour(annId: number): string {
    var data = getAnnotationData(annId);
    switch (data[0]) {
        case HighlightType.Brackets:
            return bracketColours[data[1]];
        case HighlightType.Underline:
            return underLineColours[data[1]];
        case HighlightType.Highlight:
            return highlightColours[data[1]];
    }
}
export function getHighlightingData(annId: number): [HighlightType, string] {
    var data = getAnnotationData(annId);
    return [data[0], getAnnotationColour(annId)];
}