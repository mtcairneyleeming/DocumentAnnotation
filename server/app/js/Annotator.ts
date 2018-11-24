import {Layer, Line, Node, Rect, Stage} from 'konva';
import axios from 'axios'
import * as showdown from 'showdown'
import * as $ from 'jquery'
import {Annotation, Highlight, Place} from "./AnnotationTypes";
import {isNullOrUndefined} from "util";
import {getAnnotationColour, getHighlightingData, HighlightType} from './colours'

class LineInfo {

    public annId: number;
    public line: Line;
}

enum BracketStatus {
    Start,
    Middle,
    End,
    Single
}

export class Annotator {

    // id of current document being edited
    private readonly docAnnId: number;
    // location in text
    private readonly bookNum: number;
    private readonly sectionNum: number;
    // annotations initially provided
    private readonly annotations: Annotation[];
    private highlightsToAdd: Highlight[];
    private highlightsToRemove: Highlight[];

    private colouredMode: boolean = true;
    private annotationNumbers: { [annId: number]: number } = {};

    // record of currently-edited annotation
    private annBeingEdited: number;
    // lines joining annotations to highlights
    private annLines: LineInfo[];


    // containers for various aspects of the UI
    private contentCtr: HTMLElement;
    private document: HTMLElement;
    private annCtr: HTMLElement;
    private backgroundCtr: HTMLElement;
    // stage and layer for the line-canvas
    private stage: Stage;
    private readonly layer: Layer;

    constructor(annotations,
                docAnnId: number,
                bookNum: number,
                sectionNum: number,
                printMode: boolean) {
        this.annotations = annotations;
        this.annotations.push(new Annotation([], "", "", 0, docAnnId));
        this.annBeingEdited = 0;

        this.annLines = [];

        this.highlightsToAdd = [];
        this.highlightsToRemove = [];

        this.docAnnId = docAnnId;
        this.bookNum = bookNum;
        this.sectionNum = sectionNum;


        this.contentCtr = document.getElementById("contentContainer");
        this.document = document.getElementById("document");
        this.annCtr = document.getElementById("annotationTable");
        this.backgroundCtr = document.getElementById("backgroundContainer");
        // initial set up of canvas
        this.stage = new Stage({
            container: "backgroundContainer",
            width: this.contentCtr.clientWidth,
            height: this.contentCtr.clientHeight
        });

        this.layer = new Layer();
        this.stage.add(this.layer);
        if (printMode) {
            this.colouredMode = false;
        } else {

            this.colouredMode = window.localStorage.getItem("displayMode") != "lines";
        }

        this.displayMarkdownOnAllRows();
    }

    // Canvas management ===============================================================================================

    private getAnnotationIdsFromElement(element: HTMLElement): number[] {
        return element.dataset.ids.split(",").map(id => parseInt(id, 10)).filter(id => !Number.isNaN(id));
    }

    public draw() {
        // draw lines from a highlight to the annotation comment

        const groups = document.getElementsByClassName("group");

        // clear layer
        this.layer.destroyChildren();
        this.annLines = [];
        const shapesToAdd: Node[] = [];
        for (let i = 0; i < groups.length; i++) {
            let group = groups[i];
            for (let j = 0; j < group.children.length; j++) {
                const span = <HTMLElement>group.children[j];
                if (this.colouredMode) {
                    this.hideTableAnnotationNumbers();
                    Annotator.hideDocumentNumbers(span);
                    let lines = this.createLines(span, <HTMLElement>group.children[j + 1]);
                    if (lines.length > 0) {
                        shapesToAdd.push(...lines);
                    }
                    let highlights = this.createHighlights(span);
                    if (highlights.length > 0) {
                        shapesToAdd.push(...highlights);
                    }

                    this.addHighlightHandlers(span);
                } else {
                    this.showTableAnnotationNumbers();
                    this.showDocumentNumbers(span, <HTMLSpanElement>group.children[j - 1]);


                }
            }
        }
        if (shapesToAdd.length > 0) {
            this.layer.add(...shapesToAdd);
        }
        this.updateCanvas();
    }

    private createHighlights(span: HTMLSpanElement): Node[] {
        const shapesToAdd = [];


        if (span.dataset.ids) { // if annotated
            const ids: number[] = this.getAnnotationIdsFromElement(span);


            for (let i = 0; i < ids.length; i++) {
                let id = ids[i];

                // HIGHLIGHT DRAWING 
                const spanRect = span.getBoundingClientRect();
                const canvasRect = this.contentCtr.getBoundingClientRect();

                const spanBottomLeft = {
                    x: spanRect.left - canvasRect.left + 15, // adapt for margins and for offset of highlight line
                    y: spanRect.bottom - canvasRect.top
                };
                const spanTopLeft = {
                    x: spanBottomLeft.x,
                    y: spanBottomLeft.y - spanRect.height + Annotator.convertRemToPixels(2)
                };
                const spanBottomRight = {
                    x: spanBottomLeft.x + spanRect.width + 4.4,
                    y: spanBottomLeft.y
                };
                const spanTopRight = {
                    x: spanBottomRight.x,
                    y: spanTopLeft.y
                };
                const spanWidth = spanBottomRight.x - spanBottomLeft.x;
                const spanHeight = spanBottomLeft.y - spanTopLeft.y;


                let data = getHighlightingData(id);
                switch (data[0]) {
                    case HighlightType.Brackets:
                        const status = this.getBracketStatus(span, id);
                        switch (status) {
                            case BracketStatus.Start:
                                shapesToAdd.push(new Line({
                                    points: [spanTopLeft.x + 10, spanTopLeft.y, spanTopLeft.x, spanTopLeft.y, spanBottomLeft.x, spanBottomLeft.y + 4, spanBottomLeft.x + 10, spanBottomLeft.y + 4],
                                    strokeWidth: 2,
                                    stroke: data[1]
                                }));

                                break;
                            case BracketStatus.Middle:
                                // do nothing normally
                                break;
                            case BracketStatus.End:
                                shapesToAdd.push(new Line({
                                    points: [spanTopRight.x - 10, spanTopRight.y, spanTopRight.x, spanTopRight.y, spanBottomRight.x, spanBottomRight.y + 4, spanBottomRight.x - 10, spanBottomRight.y + 4],
                                    strokeWidth: 2,
                                    stroke: data[1]
                                }));
                                break;
                            case BracketStatus.Single:
                                shapesToAdd.push(new Line({
                                    points: [spanTopLeft.x + 10, spanTopLeft.y, spanTopLeft.x, spanTopLeft.y, spanBottomLeft.x, spanBottomLeft.y + 4, spanBottomLeft.x + 10, spanBottomLeft.y + 4],
                                    strokeWidth: 2,
                                    stroke: data[1]
                                }));
                                shapesToAdd.push(new Line({
                                    points: [spanTopRight.x - 10, spanTopRight.y, spanTopRight.x, spanTopRight.y, spanBottomRight.x, spanBottomRight.y + 4, spanBottomRight.x - 10, spanBottomRight.y + 4],
                                    strokeWidth: 2,
                                    stroke: data[1]
                                }));
                                break;
                        }
                        const hoverTopLine = new Line({
                            points: [spanTopLeft.x, spanTopLeft.y, spanTopRight.x, spanTopRight.y],
                            strokeWidth: 2,
                            stroke: getAnnotationColour(id),
                            visible: false
                        });
                        shapesToAdd.push(hoverTopLine);
                        const hoverBottomLine = new Line({
                            points: [spanBottomLeft.x, spanBottomLeft.y + 4, spanBottomRight.x, spanBottomRight.y + 4],
                            strokeWidth: 2,
                            stroke: getAnnotationColour(id),
                            visible: false
                        });
                        shapesToAdd.push(hoverBottomLine);
                        $(span).hover(
                            () => {
                                hoverTopLine.visible(true);
                                hoverBottomLine.visible(true);
                                this.updateCanvas();
                            },
                            () => {
                                hoverTopLine.visible(false);
                                hoverBottomLine.visible(false);
                                this.updateCanvas();
                            });

                        break;
                    case HighlightType.Underline:
                        const underlineRect = new Rect({
                            x: spanBottomLeft.x,
                            y: spanBottomLeft.y + i * 4,
                            height: 4,
                            width: spanWidth,
                            fill: getAnnotationColour(id)
                        });
                        shapesToAdd.push(underlineRect);
                        break;
                    case HighlightType.Highlight:
                        const highlightRect = new Rect({
                            x: spanBottomLeft.x,
                            y: spanTopLeft.y,
                            height: spanHeight + 4,
                            width: spanWidth,
                            opacity: 0.5,
                            fill: getAnnotationColour(id)
                        });
                        shapesToAdd.push(highlightRect);
                        break;
                }
            }
        }
        return shapesToAdd;
    }

    private addHighlightHandlers(span: HTMLSpanElement) {
        if (span.dataset.ids) { // if annotated

            const ids: number[] = this.getAnnotationIdsFromElement(span);

            // add mouseover handlers to emphasise the links between highlights and annotations
            $(span).hover(
                () => {
                    const lines = this.annLines.filter(l => ids.indexOf(l.annId) >= 0);
                    if (lines.length == 0) {
                    } else {
                        for (let line of lines) {
                            line.line.dash([10, 0]);
                            line.line.opacity(100);
                            this.updateCanvas();
                        }

                    }
                },
                () => {
                    const lines = this.annLines.filter(l => ids.indexOf(l.annId) >= 0);
                    if (lines.length == 0) {
                    } else {
                        for (let line of lines) {
                            line.line.dash([5, 15]);
                            line.line.opacity(line.annId != 0 ? 0.5 : 1);
                            this.updateCanvas();
                        }
                    }
                });
        }
    }

    private createLines(span: HTMLSpanElement, next: HTMLSpanElement): Line[] {

        let shapesToAdd: Line[] = [];

        if (span.dataset.ids) { // if annotated

            const ids: number[] = this.getAnnotationIdsFromElement(span);


            for (let id of ids) {
                if (next) {
                    if (this.getAnnotationIdsFromElement(next).indexOf(id) != -1) {
                        return []; // only draw lines to the last character in the highlight
                    }
                }
                const spanRect = span.getBoundingClientRect();
                const canvasRect = this.contentCtr.getBoundingClientRect();
                const spanBottomLeft = {
                    x: spanRect.left - canvasRect.left + 15, // adapt for margins and for offset of highlight line
                    y: spanRect.bottom - canvasRect.top
                };

                const spanBottomRight = {
                    x: spanBottomLeft.x + spanRect.width + 5,
                    y: spanBottomLeft.y + 2
                };


                const tableRow = document.getElementById(`annotation${id}`).getBoundingClientRect();
                const annLeftMiddle = {
                    x: tableRow.left - canvasRect.left + 15, // adapt for margins
                    y: tableRow.top + 0.5 * (tableRow.height) - canvasRect.top
                };


                const line = new Line({
                    points: [spanBottomRight.x, spanBottomRight.y, annLeftMiddle.x, annLeftMiddle.y],
                    stroke: getAnnotationColour(id),
                    strokeWidth: 2,
                    dash: [5, 15],
                    opacity: id != 0 ? 0.5 : 1
                });
                shapesToAdd.push(line);
                this.annLines.push({annId: id, line: line});
            }
        }
        return shapesToAdd;

    }

    private showDocumentNumbers(span: HTMLSpanElement, prev: HTMLSpanElement) {
        if (span.dataset.ids) { // if annotated

            const ids: number[] = this.getAnnotationIdsFromElement(span);
            let prevIds = [];
            if (prev)
                prevIds = this.getAnnotationIdsFromElement(prev);

            if (ids.length > 0) {
                if (ids.sort().join(',') != prevIds.sort().join(',')) {
                    $(span).attr('data-before', ids.map(i => this.annotationNumbers[i]).sort((a, b) => a - b).join(","));
                }

                if (!$(span).hasClass("highlighted"))
                    $(span).addClass("highlighted");

                return;
            }
        }
        Annotator.hideDocumentNumbers(span);
    }

    private static hideDocumentNumbers(span: HTMLSpanElement) {
        $(span).attr('data-before', "");
        if ($(span).hasClass("highlighted"))
            $(span).removeClass("highlighted");
    }

    private static convertRemToPixels(rem: number): number {
        return rem * parseFloat(getComputedStyle(document.documentElement).fontSize);
    }

    private getBracketStatus(span: HTMLSpanElement, newId: number): BracketStatus {
        let prevGroup, prevWord, nextGroup, nextWord = 0;
        let start, end = false;
        let [groupNo, wordNo] = this.getHighlightIds(span);

        if (wordNo == 0 && groupNo == 0) {

            start = true;
        } else if (wordNo == 0) {
            prevWord = -1;
            prevGroup = groupNo - 1;
        } else {
            prevGroup = groupNo;
            prevWord = wordNo - 1;
        }

        const maxWords = this.getHighlightIds($(span).parent().children(":last")[0])[1];
        const maxGroups = this.getHighlightIds($(this.contentCtr).find("span").last()[0])[0];
        if (maxWords - wordNo == 0 && groupNo == maxGroups) {
            end = true;
        } else if (maxWords - wordNo == 0) {
            nextWord = 0;
            nextGroup = groupNo + 1;
        } else {
            nextGroup = groupNo;
            nextWord = wordNo + 1;
        }

        let isNextHighlighted = Annotator.isWordHighlighted(nextGroup, nextWord, newId);
        let isPrevHighlighted = Annotator.isWordHighlighted(prevGroup, prevWord, newId);

        // ends at the end of this <span> 
        if (!isNextHighlighted) {
            end = true;
        }

        // starts at the start of this <span> 
        if (!isPrevHighlighted) {
            start = true;
        }
        if (start && end)
            return BracketStatus.Single;
        if (start)
            return BracketStatus.Start;
        if (end)
            return BracketStatus.End;
        return BracketStatus.Middle;

    }


    private updateCanvas() {
        this.layer.draw();
    }

    private onResize() {
        this.stage.setWidth(this.contentCtr.clientWidth);
        this.stage.setHeight(this.contentCtr.clientHeight);
        this.draw();

    }

    public addCanvasHandlers() {
        $(this.annCtr).find("tr[id*=ann]").each((i: number, el: HTMLElement) => {
            const id = parseInt(el.dataset.annId, 10);
            $(el).hover(
                () => {
                    this.annLines.filter(l => l.annId === id).forEach(x => {
                        x.line.dash([10, 0]);
                        x.line.opacity(100);
                    });
                    this.updateCanvas();
                },
                () => {
                    this.annLines.filter(l => l.annId === id).forEach(x => {
                        x.line.dash([5, 15]);
                        x.line.opacity(50);
                    });
                    this.updateCanvas();
                });
        });
        $(window).on('resize',
            () => {
                this.onResize();
            });
        // @ts-ignore
        window.onbeforeprint = (r) => {
            console.log(r);
            this.colouredMode = false;
            this.onResize();
        };
        // @ts-ignore
        window.onafterprint = (r) => {
            console.log(r);
            this.colouredMode = window.localStorage.getItem("displayMode") != "lines";
            this.onResize();
        };
        $("#colouredModeToggle").on('click', () => {
            this.colouredMode = !this.colouredMode;
            if (this.colouredMode) {
                window.localStorage.setItem("displayMode", "colours");
            } else {
                window.localStorage.setItem("displayMode", "lines");
            }
            this.draw();

        });
    }

// Helper functions ====================================================================================================

    private getAnnotation(id: number): Annotation {
        return this.annotations.find(x => x.annotationId == id);
    }

    private getAnnotationIndex(id: number): number {
        for (let i = 0; i < this.annotations.length; i++) {
            if (this.annotations[i].annotationId == id) {
                return i;
            }
        }
        return -1;
    }


// Handlers ============================================================================================================

    public addEditHandlers() {
        $(this.annCtr).on('click',
            '.ann-edit-save',
            (e) => {
                let {annId: id} = $(e.currentTarget).closest("tr").data();
                this.readAnnotationFromForm(id);
                this.updateAnnotation(id, false).then((annotation: Annotation) => {
                    this.annBeingEdited = 0;
                    this.updateHighlightsByAnnotation(annotation.annotationId);
                });
                this.setEditingStatus(id, false);
                this.draw();
            });
        $(this.annCtr).on('click',
            '.ann-edit-cancel',
            (e) => {
                let {annId: id} = $(e.currentTarget).closest("tr").data();
                this.annBeingEdited = 0;
                this.setEditingStatus(id, false);
                this.resetHighlights();
                this.draw();
            });

        $(this.annCtr).on('click',
            '.ann-edit-delete',
            (e) => {
                let {annId: id} = $(e.currentTarget).closest("tr").data();
                this.annBeingEdited = 0;
                Annotator.deleteAnnotationFromServer(id, false).then(() => {
                    Annotator.removeTableRow(id);
                    this.removeHighlightsByAnnotation(id);
                    this.resetHighlights();
                    this.draw();
                });


            });
        $(this.annCtr).on('click',
            '.annotationEditLink',
            (e) => {
                let {annId: id} = $(e.currentTarget).closest("tr").data();


                if (this.annBeingEdited == 0) {
                    // we can ignore cases where the currently edited annotation is the new one and there are no highlights
                    if (this.highlightsToAdd.length != 0) {
                        alert("You have unsaved changes for your new annotation - please save them before editing others");
                        return;
                    }
                } else if (this.annBeingEdited > 0) {
                    alert("Please finish and save your edits to the other annotation you are working on first.");
                    return;
                }

                this.annBeingEdited = id;
                this.setEditingStatus(id, true);
                this.draw();

            });


        $(this.annCtr).on('click',
            '.ann-new-save',
            () => {
                let ann: Annotation = this.getAnnotation(0);

                const annDiv: JQuery<HTMLElement> = $(`.newAnnotationRow`);
                ann.body = <string>annDiv.find(`.annBody`).val();
                ann.title = <string>annDiv.find(`.annTitle`).val();

                Annotator.clearNewAnnotationForm();
                const promise = this.saveNewAnnotation(ann, false);
                promise.then((annotation: Annotation) => {
                    this.annotations[this.getAnnotationIndex(0)] = annotation; // also sets id
                    this.annotations.push(new Annotation([], "", "", 0, this.docAnnId));
                    this.annBeingEdited = 0;
                    this.updateHighlightsByAnnotation(annotation.annotationId);
                    this.addTableRow(annotation);
                    this.draw();
                })
            });
        $(this.annCtr).on('click',
            '.ann-new-cancel',
            () => {
                Annotator.clearNewAnnotationForm();
                this.resetHighlights();
                this.draw();
            });


        $(this.document).on('click',
            '.word',
            null,
            (e: JQuery.Event) => {
                this.onWordClick(e);
                this.draw();
            });


        $(document).on('keydown', '.annBody', function (e) {
            const keyCode = e.keyCode || e.which;

            if (keyCode == 9) {
                e.preventDefault();
                let t = (<HTMLTextAreaElement>this);
                const start = t.selectionStart;
                const end = t.selectionEnd;

                // set textarea value to: text before caret + tab + text after caret
                $(this).val($(this).val().toString().substring(0, start)
                    + "    "
                    + $(this).val().toString().substring(end));

                // put caret at right position again
                t.selectionStart =
                    t.selectionEnd = start + 4;
            }
        });
    }

// Table UI management =================================================================================================

    private setEditingStatus(annotationId: number, editing: boolean) {
        const element = document.getElementById(`annotation${annotationId}`);
        const ann = this.getAnnotation(annotationId);
        if (editing) {
            element.innerHTML = `
            <td style="background-color: ${getAnnotationColour(annotationId)}"></td>
                        <td><input type="text" class="form-control annTitle" placeholder="Annotation Title"
                   value="${ann.title}"></td>
                        <td>
                           <textarea rows="2" placeholder="Your annotation" class="form-control annBody">${ann.body}</textarea>
                        </td>
                        <td>
                            <button class="btn btn-primary ann-edit-save">Save</button>
                            <button class="btn btn-danger ann-edit-delete">Delete</button>
                            <button class="btn btn-outline-danger ann-edit-cancel">Cancel</button>
                        </td>`
        } else {
            element.innerHTML = `<td style="background-color: ${getAnnotationColour(annotationId)}"></td>
                        <td>${ann.title}</td>
                        <td>
                            <span class="annotation-text">${new showdown.Converter().makeHtml(ann.body)}</span>
                        </td>
                        <td>
                            <span class="annotationEditLink">Edit</span>
                        </td>`
        }

    }

    private addTableRow(ann: Annotation) {
        let tableBody = $("#annotationTable > tbody");
        tableBody.append(`<tr id="annotation${ann.annotationId}" data-ann-id="${ann.annotationId}">
        <td style="background-color: ${this.colouredMode ? getAnnotationColour(ann.annotationId) + "!important" : ""}"></td>
                        <td>${ann.title}</td>
                        <td>
                            <span class="annotation-text">${new showdown.Converter().makeHtml(ann.body)}</span>
                        </td>
                        <td>
                            <span class="annotationEditLink">Edit</span>
                        </td></tr>`);
        if (!this.colouredMode) {
            this.showTableAnnotationNumbers();
        } else {
            this.hideTableAnnotationNumbers();
        }
    }

    private static removeTableRow(annotationId: number) {
        let row = $(`#annotationTable > tbody > tr#annotation${annotationId}`);
        row.remove();
    }

    // update to user input
    private readAnnotationFromForm(annotationId: number) {
        let ann: Annotation = this.getAnnotation(annotationId);
        let listIndex = this.annotations.indexOf(ann);
        const annDiv: JQuery<HTMLElement> = $(`#annotation${annotationId}`);
        ann.body = <string>annDiv.find(`.annBody`).val();
        ann.title = <string>annDiv.find(`.annTitle`).val();
        this.annotations[listIndex] = ann;
    }

    private static clearNewAnnotationForm() {
        const annDiv: JQuery<HTMLElement> = $(`.newAnnotationRow`);
        annDiv.find(`.annBody`).val("");
        annDiv.find(`.annTitle`).val("");
    }

    // display numbers in front of each annotation (from 1 to n, rather than annotation ids) for use in print format/
    // colourless mode
    private showTableAnnotationNumbers() {
        let rows = $("#annotationTable >tbody > tr, #annotationTable > tfoot > tr");
        rows.each((i: number, el: HTMLTableRowElement) => {
            let colouredCell = <HTMLTableCellElement>el.cells[0];
            colouredCell.style.backgroundColor = "";
            colouredCell.textContent = `${i + 1}`;
            this.annotationNumbers[parseInt(el.dataset.annId)] = i + 1;
        })
    }

    // hide numbers for coloured mode, and show the colours instead
    private hideTableAnnotationNumbers() {
        let rows = $("#annotationTable >tbody > tr, #annotationTable > tfoot > tr");
        rows.each((i: number, el: HTMLTableRowElement) => {
            let colouredCell = <HTMLTableCellElement>el.cells[0];
            colouredCell.style.cssText = `background-color: ${getAnnotationColour(parseInt(el.dataset.annId))} !important`;
            colouredCell.textContent = "";
        })
    }

    private displayMarkdownOnAllRows() {
        let rows = $("#annotationTable >tbody > tr");
        let converter = new showdown.Converter();
        rows.each((i: number, el: HTMLTableRowElement) => {
            let textCell = <HTMLSpanElement>el.cells[2].firstElementChild;
            let text = $(textCell).data("raw");
            textCell.innerHTML = converter.makeHtml(text);
            console.log(el.cells[2].firstElementChild)
        })
    }
    

// Highlight management ================================================================================================

    private GetHighlightIndex(list: Highlight[], highlight: Highlight) {
        // @ts-ignore
        return list.findIndex(
            h => h.location.bookNumber == highlight.location.bookNumber &&
                h.location.sectionNumber == highlight.location.sectionNumber &&
                h.location.groupNumber == highlight.location.groupNumber &&
                h.location.wordNumber == highlight.location.wordNumber);
    }

    // handler that runs when a word is clicked on - if the word is already part of the current annotation, it is
    // removed, else it is added
    private onWordClick(event: JQuery.Event) {
        let clickedElement = <HTMLElement>event.target;

        //update DOM
        if (clickedElement.dataset.ids.indexOf(`,${this.annBeingEdited.toString()},`) >= 0) {
            clickedElement.dataset.ids = clickedElement.dataset.ids.replace(this.annBeingEdited.toString(), '')
        } else {
            clickedElement.dataset.ids += this.annBeingEdited.toString() + ",";
        }

        let [groupNum, wordNum] = this.getHighlightIds(clickedElement);

        let wordPlace = new Place(this.bookNum,
            this.sectionNum,
            groupNum,
            wordNum);
        let highlight = new Highlight(wordPlace, this.annBeingEdited);

        let currentAnn = this.annotations[this.getAnnotationIndex(this.annBeingEdited)];
        let highlightIndex = this.GetHighlightIndex(currentAnn.highlights, highlight);

        if (highlightIndex == -1) {
            let indexInToAdd = this.GetHighlightIndex(this.highlightsToAdd, highlight);
            if (indexInToAdd == -1) {
                this.highlightsToAdd.push(highlight);
            } else {
                this.highlightsToAdd.splice(indexInToAdd, 1)
            }

        } else {
            let existingHighlight = currentAnn.highlights[highlightIndex];

            let indexInToRemove = this.GetHighlightIndex(this.highlightsToRemove, existingHighlight);
            if (indexInToRemove == -1) {
                this.highlightsToRemove.push(existingHighlight);
            } else {
                this.highlightsToRemove.splice(indexInToRemove, 1)
            }
        }
    }

    // remove highlighting from the text - used when an annotation is deleted.
    private removeHighlightsByAnnotation(id: number) {
        $(this.document).find("span.word").each((i, span) => {
            if (span.dataset.ids.indexOf("," + id.toString() + ",") >= 0) {
                span.dataset.ids = span.dataset.ids.replace(id.toString(), '');
            }
        })
    }

    // update the highlighting in the text - used when an annotation is saved, so we can then display it with the right colour.
    private updateHighlightsByAnnotation(newId: number) {
        $(this.document).find("span.word").each((i, span) => {
            if (span.dataset.ids.indexOf(",0,") >= 0) {
                span.dataset.ids = span.dataset.ids.replace(",0,", `,${newId.toString()},`)
            }
        });

    }

    private getHighlightIds(span: HTMLSpanElement) {
        return span.id.split("-").map(x => parseInt(x, 10));
    }

    private static isWordHighlighted(groupNo: number, wordNo: number, annId: number): boolean {
        const span = $(`[id=${groupNo}-${wordNo}]`);
        const data = span.data("ids");
        if (isNullOrUndefined(data)) {
            return false;
        }
        return data.indexOf(`,${annId.toString()},`) >= 0;
    }

// remove highlighting from the text - used when an annotation is deleted.
    private removeHighlight(highlight: Highlight) {
        $(this.document).find(`#${highlight.location.groupNumber}-${highlight.location.wordNumber}`).each((i, p) => {
            p.dataset.ids = p.dataset.ids.replace(highlight.annotationId.toString(), '');
        });
    }

    // remove highlighting from the text - used when an annotation is deleted.
    private addHighlight(highlight: Highlight) {
        $(this.document).find(`#${highlight.location.groupNumber}-${highlight.location.wordNumber}`).each((i, p) => {
            p.dataset.ids += " " + highlight.annotationId.toString() + " ";
        });
    }

    private resetHighlights() {
        for (let i = 0; i < this.highlightsToAdd.length; i++) {
            this.removeHighlight(this.highlightsToAdd[i]);
        }
        for (let i = 0; i < this.highlightsToRemove.length; i++) {
            this.addHighlight(this.highlightsToRemove[i]);
        }
    }

// Server persistence ==================================================================================================

    private saveNewAnnotation(annotation: Annotation, reload = true): Promise<Annotation> {
        // add new highlights too
        return axios.post<Annotation>("/api/Annotations", {
            annotation: annotation,
            highlightsToAdd: this.highlightsToAdd
        }).then(
            (res): Annotation => {
                // reload page
                if (reload) {
                    window.location.reload(true);
                }
                this.highlightsToAdd = [];

                // @ts-ignore
                return res.data.annotation;
            });
    }

    private updateAnnotation(annotationId: number, reload = true): Promise<Annotation> {
        let annotation = this.getAnnotation(annotationId);
        annotation.highlights = [];
        return axios.put<Annotation>(`/api/Annotations/${annotationId}`,
            {
                annotation: annotation,
                highlightsToAdd: this.highlightsToAdd,
                highlightsToRemove: this.highlightsToRemove
            }).then(
            (res): Annotation => {
                // reload page
                if (reload) {
                    window.location.reload(true);
                }
                this.highlightsToAdd = [];
                this.highlightsToRemove = [];
                // @ts-ignore
                return res.data.annotation;
            });
    }

    private static deleteAnnotationFromServer(annotationId: number, reload = true) {
        return axios.delete(`/api/Annotations/${annotationId}`)
    }
}