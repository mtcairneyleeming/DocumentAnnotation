
import { Helpers } from "./Helpers"
type NumList = { [x: string]: number; };
export class PrintTextHighlighter {

    static showDocumentNumbersOnSpan(span: HTMLSpanElement, prev: HTMLSpanElement, annotationNumbers: NumList) {
        if (span.dataset.ids) { // if annotated
            var ids = Helpers.getAnnotationIdsFromElement(span);
            var prevIds = [];
            if (prev)
                prevIds = Helpers.getAnnotationIdsFromElement(prev);
            if (ids.length > 0) {
                console.log(ids.sort());
                if (ids.sort().join(',') != prevIds.sort().join(',')) {
                    $(span).attr('data-before', ids.map(i => annotationNumbers[i]).sort((a, b) => a - b).join(","));
                }
                else {
                    $(span).attr('data-before', "");
                }
                if (!$(span).hasClass("highlighted"))
                    $(span).addClass("highlighted");
                return;
            }
        }
        this.hideDocumentNumbersOnSpan(span);
    };
    static hideDocumentNumbersOnSpan(span: HTMLSpanElement) {
        $(span).attr('data-before', "");
        if ($(span).hasClass("highlighted"))
            $(span).removeClass("highlighted");
    };
    static showDocumentNumbers(show: boolean, annotationNumbers: NumList) {
        Helpers.forAllGroups((span: HTMLSpanElement, prev: HTMLSpanElement) => {
            if (show)
                PrintTextHighlighter.showDocumentNumbersOnSpan(span, prev, annotationNumbers);
            else
                PrintTextHighlighter.hideDocumentNumbersOnSpan(span);
        });
    };
    static showDocumentNumbersOnAllTables(show: boolean, annotationNumberLists: NumList[]) {
        var foregroundContainers = $(".foregroundContainer").toArray();

        for (var i = 0; i < foregroundContainers.length; i++) {
            Helpers.forAllGroupsInParent((span: HTMLSpanElement, prev: HTMLSpanElement) => {
                console.log(span);
                if (show)
                    PrintTextHighlighter.showDocumentNumbersOnSpan(span, prev, annotationNumberLists[i]);
                else
                    PrintTextHighlighter.hideDocumentNumbersOnSpan(span);
            }, foregroundContainers[i]);
        }
    };

}