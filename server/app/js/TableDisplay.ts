import { getAnnotationColour } from "./colours"
export class TableDisplay {

    // display numbers in front of each annotation (from 1 to n, rather than annotation ids) for use in print format/
    // colourless mode
    static showTableAnnotationNumbers() {
        var annotationNumbers = {};
        var rows = $(".annotationTable >tbody > tr, .annotationTable > tfoot > tr");
        rows.each((i, el: HTMLTableRowElement) => {

            var colouredCell = el.cells[0];
            colouredCell.style.backgroundColor = "";
            colouredCell.textContent = "" + (i + 1);
            annotationNumbers[parseInt(el.dataset.annId)] = i + 1;
        });
        return annotationNumbers;
    };
    static getAnnotationNumbers() {
        var annotationNumberLists = [];
        var tables = $(".annotationTable");
        tables.each((j, table: HTMLTableElement) => {
            var rows = $(table).find("tbody > tr, tfoot > tr");
            annotationNumberLists[j] = [];
            rows.each((i, el: HTMLTableRowElement) => {

                var colouredCell = el.cells[0];
                colouredCell.style.backgroundColor = "";
                colouredCell.textContent = "" + (i + 1);
                annotationNumberLists[j][parseInt(el.dataset.annId)] = i + 1;
            });
        });
        return annotationNumberLists;
    };
    // hide numbers for coloured mode, before showing the colours instead
    static hideTableAnnotationNumbers() {
        var rows = $(".annotationTable >tbody > tr, .annotationTable > tfoot > tr");
        rows.each((i, el: HTMLTableRowElement) => {

            var colouredCell = el.cells[0];
            colouredCell.style.cssText = "background-color: " + getAnnotationColour(parseInt(el.dataset.annId)) + " !important";
            colouredCell.textContent = "";
        });
    };

}
