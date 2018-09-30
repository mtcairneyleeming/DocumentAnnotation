import * as Konva from 'konva';
import axios, {AxiosPromise, AxiosResponse} from 'axios'
import * as $ from 'jquery'
import {Annotation, Highlight, Place} from "./AnnotationTypes";

class LineInfo {

    public annId: number;
    public line: Konva.Line;
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

    // colour info per annotation
    private readonly annColours: { [id: number]: string } = {};
    // class info per annotation
    private readonly annClasses: { [id: number]: string } = {};
    // record of currently-edited annotation
    private annBeingEdited: number;
    // lines joining annotations to highlights
    private readonly annLines: LineInfo[];

    private readonly editing

    // containers for various aspects of the UI
    private contentCtr: HTMLElement;
    private annCtr: HTMLElement;
    private backgroundCtr: HTMLElement;
    // stage and layer for the line-canvas
    private stage: Konva.Stage;
    private readonly layer: Konva.Layer;
    private newAnnotation: Annotation;

    constructor(annotations,
                annColours: { [id: number]: string },
                annClasses: { [id: number]: string },
                docAnnId: number,
                bookNum: number,
                sectionNum: number,
                editing = true) {
        this.annotations = annotations;
        this.annotations.push(new Annotation([], "", "", 0, docAnnId));
        this.annBeingEdited = 0;

        this.annLines = [];
        this.annColours = annColours;
        this.annClasses = annClasses;
        this.annClasses[0] = "annH1";

        this.highlightsToAdd = [];
        this.highlightsToRemove = [];

        this.docAnnId = docAnnId;
        this.bookNum = bookNum;
        this.sectionNum = sectionNum;

        this.editing = editing;


        this.contentCtr = document.getElementById("document");
        this.annCtr = document.getElementById("annotationTable");
        this.backgroundCtr = document.getElementById("backgroundContainer");
        // initial set up of canvas
        let foreground = document.getElementById("foregroundContainer");
        this.stage = new Konva.Stage({
            container: "backgroundContainer",
            width: foreground.clientWidth,
            height: foreground.clientHeight
        });

        this.layer = new Konva.Layer();
        this.stage.add(this.layer);
    }

    // Canvas management ===============================================================================================

    getAnnotationIdsFromElement(element: HTMLElement): number[] {
        return element.dataset.ids.split(",").map(id => parseInt(id, 10)).filter(id => !Number.isNaN(id));
    }

    drawLines() {
        // draw lines from a highlight to the annotation comment

        const groups = document.getElementsByClassName("group");

        // clear previous lines
        for (let line of this.annLines) {
            line.line.destroy();
            this.annLines.splice(this.annLines.indexOf(line), 1)
        }
        for (let i = 0; i < groups.length; i++) {
            let group = groups[i];
            for (let j = 0; j < group.children.length; j++) {
                this.drawLine(<HTMLElement>group.children[j], <HTMLElement>group.children[j + 1])
            }
        }
    }

    drawLine(span: HTMLElement, next: HTMLElement) {

        if (span.dataset.ids) { // if annotated

            const ids: number[] = this.getAnnotationIdsFromElement(span);


            // add mouseover handlers to emphasise the links between highlights and annotations
            $(span).hover(
                () => {
                    var lines = this.annLines.filter(l => ids.indexOf(l.annId) >= 0);
                    if (lines.length == 0) {
                        console.log(span.id);
                        console.log(this.annLines)
                    } else {
                        for (var line of lines) {
                            line.line.dash([10, 0]);
                            this.updateCanvas();
                        }

                    }
                },
                () => {
                    var lines = this.annLines.filter(l => ids.indexOf(l.annId) >= 0);
                    if (lines.length == 0) {
                        console.log(span.id);
                        console.log(this.annLines)
                    } else {
                        for (var line of lines) {
                            line.line.dash([5, 15]);
                            this.updateCanvas();
                        }
                    }
                });

            for (let id of ids) {
                if (next) {
                    if (this.getAnnotationIdsFromElement(next).indexOf(id) != -1) {
                        return; // only draw lines to the last character in the highlight
                    }
                }
                const spanRect = span.getBoundingClientRect();
                const canvasRect = this.contentCtr.getBoundingClientRect();
                const titleRect = document.getElementById("title").getBoundingClientRect();
                // drawing from bottom right corner:
                const spanBottomRight = {
                    x: spanRect.right - canvasRect.left + 30 - 1, // adapt for margins and for offset of highlight line
                    y: spanRect.bottom - canvasRect.top + 15
                };

                const annRect = document.getElementById(`annotation${id}`).getBoundingClientRect();
                const annLeftMiddle = {
                    x: annRect.left - canvasRect.left + 30, // adapt for margins
                    y: annRect.top - (0.5 * (annRect.top - annRect.bottom)) - canvasRect.top + 15
                };

                const line = new Konva.Line({
                    points: [spanBottomRight.x, spanBottomRight.y + titleRect.height, annLeftMiddle.x, annLeftMiddle.y + titleRect.height],
                    stroke: this.getAnnotationColour(id),
                    strokeWidth: 2,
                    dash: [5, 15]
                });
                this.layer.add(line);
                this.annLines.push({annId: id, line: line});
            }
        }

        this.updateCanvas();

    }

    updateCanvas() {
        this.layer.draw();
        this.stage.draw();
    }

    onResize() {
        this.stage.setWidth(this.contentCtr.clientWidth);
        this.stage.setHeight(this.contentCtr.clientHeight);
        this.drawLines();

    }

    addCanvasHandlers() {
        $(this.annCtr).find("tr[id*=ann]").each((i: number, el: HTMLElement) => {
            const id = parseInt(el.dataset.annId, 10);
            $(el).hover(
                () => {
                    this.annLines.filter(l => l.annId === id).forEach(x => x.line.dash([10, 0]));
                    this.updateCanvas();
                },
                () => {
                    this.annLines.filter(l => l.annId === id).forEach(x => x.line.dash([5, 15]));
                    this.updateCanvas();
                });
        });
        $(window).on('resize',
            () => {
                this.onResize();
            });
    }


// Helper functions ================================================================================================

    isAnnotationBeingEdited(annId): boolean {
        return this.annBeingEdited == annId;
    }

    getAnnotation(id: number): Annotation {
        return this.annotations.find(x => x.annotationId == id);
    }

    getAnnotationIndex(id: number): number {
        for (let i = 0; i < this.annotations.length; i++) {
            if (this.annotations[i].annotationId == id) {
                return i;
            }
        }
        return -1;
    }

    deleteAnnotation(id: number) {
        for (let i = 0; i < this.annotations.length; i++) {
            if (this.annotations[i].annotationId == id) {
                this.annotations.splice(i, 1);
                return;
            }
        }
    }

    getAnnotationColour(id: number): string {
        return this.annColours[id];
    }

// Handlers ========================================================================================================
    addEditHandlers() {
        $(this.annCtr).on('click',
            '.ann-edit-save',
            (e) => {
                let {annId: id} = $(e.currentTarget).closest("tr").data();
                this.readAnnotationFromForm(id);
                this.updateAnnotation(id);
                this.setEditingStatus(id, false);
                this.drawLines();
            });
        $(this.annCtr).on('click',
            '.ann-edit-cancel',
            (e) => {
                let {annId: id} = $(e.currentTarget).closest("tr").data();
                this.annBeingEdited = 0;
                this.setEditingStatus(id, false);
                this.resetHighlights();
                this.drawLines();
            });

        $(this.annCtr).on('click',
            '.ann-edit-delete',
            (e) => {
                let {annId: id} = $(e.currentTarget).closest("tr").data();
                this.annBeingEdited = 0;
                this.deleteAnnotationFromServer(id, false).then((res) => {
                    this.removeTableRow(id);
                    this.removeHighlightsByAnnotation(id);
                    this.resetHighlights();
                    this.drawLines();
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
                this.drawLines();

            });


        $(this.annCtr).on('click',
            '.ann-new-save',
            (e) => {
                let ann: Annotation = this.getAnnotation(0);

                const annDiv: JQuery<HTMLElement> = $(`.newAnnotationRow`);
                ann.body = <string>annDiv.find(`.annBody`).val();
                ann.title = <string>annDiv.find(`.annTitle`).val();

                this.clearNewAnnotationForm();
                const promise = this.saveNewAnnotation(ann, false);
                promise.then((annotation: Annotation) => {
                    this.annotations[this.getAnnotationIndex(0)] = annotation;
                    this.annotations.push(new Annotation([], "", "", 0, this.docAnnId));
                    this.annBeingEdited = 0;

                    var id = annotation.annotationId;
                    this.getAnnotationColourFromServer(id).then((data) => {
                        this.annClasses[id] = data.colourClassName;
                        this.annColours[id] = data.colour;
                        this.updateHighlightsByAnnotation(id);
                        this.addTableRow(annotation);
                        this.drawLines();
                        this.drawLines();
                    })

                })


            });
        $(this.annCtr).on('click',
            '.ann-new-cancel',
            (e) => {
                this.clearNewAnnotationForm();
                this.resetHighlights();
                this.drawLines();
            });


        $(this.contentCtr).on('click',
            '.word',
            null,
            (e: JQuery.Event) => {
                console.log(this.annBeingEdited);
                this.onWordClick(e);
                this.drawLines();
                console.log("Drawn lines")
            });
    }


// Table UI management ==============================================================================================

    setEditingStatus(annotationId: number, editing: boolean) {
        const element = document.getElementById(`annotation${annotationId}`);
        const ann = this.getAnnotation(annotationId);
        if (editing) {
            element.innerHTML = `
            <td style="background-color: ${this.getAnnotationColour(annotationId)}"></td>
                        <td><input type="text" class="form-control annTitle" placeholder="Annotation Title"
                   value="${ann.title}"></td>
                        <td>
                           <textarea rows="5" placeholder="Your annotation" class="form-control annBody">${ann.body}</textarea>
                        </td>
                        <td>
                            <button class="btn btn-primary ann-edit-save">Save</button>
                            <button class="btn btn-danger ann-edit-delete">Delete</button>
                            <button class="btn btn-outline-danger ann-edit-cancel">Cancel</button>
                        </td>`
        } else {
            element.innerHTML = `<td style="background-color: ${this.getAnnotationColour(annotationId)}"></td>
                        <td>${ann.title}</td>
                        <td>
                            <p>${ann.body}</p>
                        </td>
                        <td>
                            <a class="annotationEditLink btn btn-light">Edit</a>
                        </td>`
        }

    }

    addTableRow(ann: Annotation) {
        let tableBody = $("#annotationTable > tbody");
        tableBody.append(`<tr id="annotation${ann.annotationId}" data-ann-id="${ann.annotationId}"><td style="background-color: ${this.getAnnotationColour(ann.annotationId)}"></td>
                        <td>${ann.title}</td>
                        <td>
                            <p>${ann.body}</p>
                        </td>
                        <td>
                            <a class="annotationEditLink btn btn-light">Edit</a>
                        </td></tr>`)
    }

    removeTableRow(annotationId: number) {
        let row = $(`#annotationTable > tbody > tr#annotation${annotationId}`);
        row.remove();
    }

    // update to user input
    readAnnotationFromForm(annotationId: number) {
        let ann: Annotation = this.getAnnotation(annotationId);
        let listIndex = this.annotations.indexOf(ann);
        const annDiv: JQuery<HTMLElement> = $(`#annotation${annotationId}`);
        ann.body = <string>annDiv.find(`.annBody`).val();
        ann.title = <string>annDiv.find(`.annTitle`).val();
        this.annotations[listIndex] = ann;
    }

    clearNewAnnotationForm() {
        const annDiv: JQuery<HTMLElement> = $(`.newAnnotationRow`);
        annDiv.find(`.annBody`).val("");
        annDiv.find(`.annTitle`).val("");
    }

// Highlight management ============================================================================================

    private GetHighlightIndex(list: Highlight[], highlight: Highlight) {
        return list.findIndex(
            h => h.location.bookNumber == highlight.location.bookNumber &&
                h.location.sectionNumber == highlight.location.sectionNumber &&
                h.location.groupNumber == highlight.location.groupNumber &&
                h.location.wordNumber == highlight.location.wordNumber);
    }

    // handler that runs when a word is clicked on - if the word is already part of the current annotation, it is
    // removed, else it is added
    onWordClick(event: JQuery.Event) {
        let clickedElement = <HTMLElement>event.target;

        //update DOM
        if (clickedElement.dataset.ids.indexOf(this.annBeingEdited.toString()) >= 0) {
            console.log(this.annBeingEdited.toString());
            clickedElement.dataset.ids = clickedElement.dataset.ids.replace(this.annBeingEdited.toString(), '')
        } else {
            clickedElement.dataset.ids += this.annBeingEdited.toString();
        }
        clickedElement.classList.toggle(this.annClasses[this.annBeingEdited]); // yellow highlight

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

        }
        else {
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
    removeHighlightsByAnnotation(id: number) {
        $(this.contentCtr).find("span").each((i, span) => {
            if (span.classList.contains(this.annClasses[id])) {
                span.classList.toggle(this.annClasses[id]);
                span.dataset.ids = span.dataset.ids.replace(id.toString(), '');
            }
        })
    }

    // update the highlighting in the text - used when an annotation is saved, so we can then display it with the right colour.
    updateHighlightsByAnnotation(newId: number) {
        $(this.contentCtr).find("span").each((i, span) => {
            if (span.classList.contains("annH1")) {
                var newClass = this.annClasses[newId];

                if (newClass == "annB1" || newClass == "annB2") {
                    // brackets need special handling

                    let prevGroup, prevWord, nextGroup, nextWord = 0;
                    let [groupNo, wordNo] = this.getHighlightIds(span);

                    if (wordNo == 0 && groupNo == 0) {

                        newClass += "Start"
                    } else if (wordNo == 0) {
                        prevWord = -1;
                        prevGroup = groupNo - 1;
                    } else {
                        prevGroup = groupNo;
                        prevWord = wordNo - 1;
                    }

                    var maxWords = this.getHighlightIds($(span).siblings(":last"))[1];
                    var maxGroups = this.getHighlightIds($(this.contentCtr).find("span").last())[0]
                    if (maxWords - wordNo == 1 && groupNo == maxGroups) {
                        newClass += "End";
                    } else if (maxWords - wordNo === 1) {
                        nextWord = 0;
                        nextGroup = groupNo + 1;
                    } else {
                        nextGroup = groupNo;
                        nextWord = wordNo + 1;
                    }

                    var isNextHighlighted = this.isWordHighlighted(nextGroup, nextWord, newId);
                    var isPrevHighlighted = this.isWordHighlighted(prevGroup, prevWord, newId);

                    // ends at the end of this <span> 
                    if (!isNextHighlighted) {
                        newClass += "End";
                    }

                    // starts at the start of this <span> 
                    if (!isPrevHighlighted) {
                        newClass += "Start";
                    }

                    // goes straight through this <span>
                    if (isPrevHighlighted && isNextHighlighted) {
                        newClass += "Middle";
                    }
                    span.classList.add(newClass);
                } else {
                    span.classList.add(this.annClasses[newId]);
                }
                span.classList.remove("annH1");
                span.dataset.ids = span.dataset.ids.replace("0", newId.toString())
            }
        });

    }

    private getHighlightIds(span) {
        return span.id.split("-").map(x => parseInt(x, 10));
    }

    private isWordHighlighted(groupNo: number, wordNo: number, annId: number) {
        var span = $(`.${groupNo}-${wordNo}`);
        return span.data("ids").indexOf(annId.toString()) >= 0;
    }

// remove highlighting from the text - used when an annotation is deleted.
    removeHighlight(highlight: Highlight) {
        $(this.contentCtr).find(`#${highlight.location.groupNumber}-${highlight.location.wordNumber}`).each((i, p) => {
            p.classList.toggle(this.annClasses[highlight.annotationId]);
            p.dataset.ids = p.dataset.ids.replace(highlight.annotationId.toString(), '');
        });
    }

    // remove highlighting from the text - used when an annotation is deleted.
    addHighlight(highlight: Highlight) {
        $(this.contentCtr).find(`#${highlight.location.groupNumber}-${highlight.location.wordNumber}`).each((i, p) => {
            p.classList.toggle(this.annClasses[highlight.annotationId]);
            p.dataset.ids += highlight.annotationId.toString();
        });
    }

    resetHighlights() {
        for (let i = 0; i < this.highlightsToAdd.length; i++) {
            this.removeHighlight(this.highlightsToAdd[i]);
        }
        for (let i = 0; i < this.highlightsToRemove.length; i++) {
            this.addHighlight(this.highlightsToRemove[i]);
        }
    }

// save to server ==================================================================================================

    saveNewAnnotation(annotation: Annotation, reload = true): Promise<object> {
        // add new highlights too
        let p: Promise<object> = axios.post<object>("/api/Annotations", {
            annotation: annotation,
            highlightsToAdd: this.highlightsToAdd
        }).then(
            (res): object => {
                // reload page
                if (reload) {
                    window.location.reload(true);
                }
                console.log(res);
                this.highlightsToAdd = [];

                // @ts-ignore
                return res.data.annotation;
            });
        return p;
    }

    updateAnnotation(annotationId: number, reload = true) {
        let annotation = this.getAnnotation(annotationId);
        annotation.highlights = [];
        let ann = axios.put(`/api/Annotations/${annotationId}`,
            {
                annotation: annotation,
                highlightsToAdd: this.highlightsToAdd,
                highlightsToRemove: this.highlightsToRemove
            });
        ann.then(
            () => {
                // reload page
                if (reload) {
                    window.location.reload(true);
                }
                this.highlightsToAdd = [];
                this.highlightsToRemove = [];
            });
        return ann;
    }

    deleteAnnotationFromServer(annotationId: number, reload = true) {
        return axios.delete(`/api/Annotations/${annotationId}`)
    }

    getAnnotationColourFromServer(annotationId: number) {
        return axios.get(`/api/Annotations/${annotationId}/Colour`).then((res) => res.data);
    }


}