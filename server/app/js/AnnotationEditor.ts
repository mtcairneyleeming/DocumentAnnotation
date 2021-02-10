import showdown = require("showdown");
import { getAnnotationColour } from "./colours";
import { Helpers } from "./Helpers";
import { ServerInteractions } from "./ServerInteractions";
import { Annotation } from "./types/Annotation";
import { Highlight } from "./types/Highlight";
import { Place } from "./types/Place";

export class AnnotationEditor {
    annotations: Annotation[];
    annBeingEdited: number;
    highlightsToAdd: Highlight[];
    highlightsToRemove: Highlight[];
    docAnnId: number;
    bookNum: number;
    sectionNum: number;
    draw: Function;
    constructor(annotations: Annotation[], docAnnId: number, bookNum: number, sectionNum: number, draw: Function) {
        this.annotations = annotations;
        this.annotations.push(new Annotation([], "", "", 0, docAnnId));
        this.annBeingEdited = 0;
        this.highlightsToAdd = [];
        this.highlightsToRemove = [];
        this.docAnnId = docAnnId;
        this.bookNum = bookNum;
        console.log("constructor :" + bookNum);
        this.sectionNum = sectionNum;
        this.draw = draw;
    }
    addEditHandlers() {

        console.log("adding edit handlers");
        console.log(Helpers.annCtr);
        console.log("after");
        $(Helpers.annCtr).on('click', '.ann-edit-save', (e) => {
            var id = $(e.currentTarget).closest("tr").data().annId;
            this.readAnnotationFromForm(id);
            ServerInteractions.updateAnnotation(Helpers.getAnnotation(id, this.annotations), this.highlightsToAdd, this.highlightsToRemove, false).then((annotation) => {
                this.annBeingEdited = 0;
                this.updateHighlightsByAnnotation(annotation.annotationId);
            });
            this.setEditingStatus(id, false);
            this.draw();
        });
        $(Helpers.annCtr).on('click', '.ann-edit-cancel', (e) => {
            var id = $(e.currentTarget).closest("tr").data().annId;
            this.annBeingEdited = 0;
            this.setEditingStatus(id, false);
            this.resetHighlights();
            this.draw();
        });
        $(Helpers.annCtr).on('click', '.ann-edit-delete', (e) => {
            var id = $(e.currentTarget).closest("tr").data().annId;
            this.annBeingEdited = 0;
            ServerInteractions.deleteAnnotationFromServer(id, false).then(() => {
                AnnotationEditor.removeTableRow(id);
                this.removeHighlightsByAnnotation(id);
                this.resetHighlights();
                this.draw();
            });
        });
        $(Helpers.annCtr).on('click', '.annotationEditLink', (e) => {
            var id = $(e.currentTarget).closest("tr").data().annId;
            if (this.annBeingEdited == 0) {
                // we can ignore cases where the currently edited annotation is the new one and there are no highlights
                if (this.highlightsToAdd.length != 0) {
                    alert("You have unsaved changes for your new annotation - please save them before editing others");
                    return;
                }
            }
            else if (this.annBeingEdited > 0) {
                alert("Please finish and save your edits to the other annotation you are working on first.");
                return;
            }
            this.annBeingEdited = id;
            this.setEditingStatus(id, true);
            this.draw();
        });
        $(Helpers.annCtr).on('click', '.ann-new-save', () => {
            var ann = Helpers.getAnnotation(0, this.annotations);
            var annDiv = $(".newAnnotationRow");
            ann.body = annDiv.find(".annBody").val().toString();
            ann.title = annDiv.find(".annTitle").val().toString();
            AnnotationEditor.clearNewAnnotationForm();
            var promise = ServerInteractions.saveNewAnnotation(ann, this.highlightsToAdd, false);
            promise.then((annotation: Annotation) => {
                this.annotations[Helpers.getAnnotationIndex(0, this.annotations)] = annotation; // also sets id
                this.annotations.push(new Annotation([], "", "", 0, this.docAnnId));
                this.annBeingEdited = 0;
                this.updateHighlightsByAnnotation(annotation.annotationId);
                AnnotationEditor.addTableRow(annotation);
                this.draw();
            });
        });
        $(Helpers.annCtr).on('click', '.ann-new-cancel', () => {
            AnnotationEditor.clearNewAnnotationForm();
            this.resetHighlights();
            this.draw();
        });
        $(Helpers.document).on('click', '.word', null, (e: Event) => {
            console.log("word clicked");
            this.onWordClick(e);
            this.draw();
            console.log("redrawn");
        });
        $(".annBody").on('keydown', (e) => {
            var keyCode = e.keyCode || e.which;
            if (keyCode == 9) {
                e.preventDefault();
                var t = e.target as HTMLTextAreaElement;
                var start = t.selectionStart;
                var end = t.selectionEnd;
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

    setEditingStatus(annotationId: number, editing: boolean) {
        var element = document.getElementById("annotation" + annotationId);
        var ann = Helpers.getAnnotation(annotationId, this.annotations);
        if (editing) {
            element.innerHTML =
                "\n            <td style=\"background-color: " + getAnnotationColour(annotationId) + "\"></td>\n                        <td><input type=\"text\" class=\"form-control annTitle\" placeholder=\"Annotation Title\"\n                   value=\"" + ann.title + "\"></td>\n                        <td>\n                           <textarea rows=\"5\" placeholder=\"Your annotation\" class=\"form-control annBody\">" + ann.body + "</textarea>\n                        </td>\n                        <td>\n                            <button class=\"btn btn-primary ann-edit-save\">Save</button>\n                            <button class=\"btn btn-danger ann-edit-delete\">Delete</button>\n                            <button class=\"btn btn-outline-danger ann-edit-cancel\">Cancel</button>\n                        </td>";
        }
        else {
            element.innerHTML = "<td style=\"background-color: " + getAnnotationColour(annotationId) + "\"></td>\n                        <td>" + ann.title + "</td>\n                        <td>\n                            <span class=\"annotation-text\">" + new showdown.Converter().makeHtml(ann.body) + "</span>\n                        </td>\n                        <td>\n                            <span class=\"annotationEditLink\">Edit</span>\n                        </td>";
        }
    };
    static addTableRow(ann: Annotation) {
        var tableBody = $(".annotationTable > tbody");
        console.log("fsknjdfasnkjfadskjnsdfankjdfskjnf");
        tableBody.append("<tr id=\"annotation" + ann.annotationId + "\" data-ann-id=\"" + ann.annotationId + "\">\n        <td style=\"background-color: " + (window.localStorage.getItem("displayMode") != "lines" ? getAnnotationColour(ann.annotationId) + "!important" : "") + "\"></td>\n                        <td>" + ann.title + "</td>\n                        <td>\n                            <span class=\"annotation-text\">" + new showdown.Converter().makeHtml(ann.body) + "</span>\n                        </td>\n                        <td>\n                            <span class=\"annotationEditLink\">Edit</span>\n                        </td></tr>");
    };
    static removeTableRow(annotationId: string) {
        var row = $(".annotationTable > tbody > tr#annotation" + annotationId);
        row.remove();
    };
    // update to user input
    readAnnotationFromForm(annotationId: number) {
        var ann = Helpers.getAnnotation(annotationId, this.annotations);
        var listIndex = this.annotations.indexOf(ann);
        var annDiv = $("#annotation" + annotationId);
        ann.body = annDiv.find(".annBody").val().toString();
        ann.title = annDiv.find(".annTitle").val().toString();
        this.annotations[listIndex] = ann;
    };
    static clearNewAnnotationForm() {
        var annDiv = $(".newAnnotationRow");
        annDiv.find(".annBody").val("");
        annDiv.find(".annTitle").val("");
    };
    // remove highlighting from the text - used when an annotation is deleted.
    removeHighlight(highlight: Highlight) {
        $(Helpers.document).find("#" + highlight.location.groupNumber + "-" + highlight.location.wordNumber).each((i, span) => {
            span.dataset.ids = span.dataset.ids.replace(highlight.annotationId.toString(), '');
        });
    };
    // remove highlighting from the text - used when an annotation is deleted.
    addHighlight(highlight: Highlight) {
        $(Helpers.document).find("#" + highlight.location.groupNumber + "-" + highlight.location.wordNumber).each((i, span) => {
            span.dataset.ids += " " + highlight.annotationId.toString() + " ";
        });
    };
    resetHighlights() {
        for (var i = 0; i < this.highlightsToAdd.length; i++) {
            this.removeHighlight(this.highlightsToAdd[i]);
        }
        for (var i = 0; i < this.highlightsToRemove.length; i++) {
            this.addHighlight(this.highlightsToRemove[i]);
        }
    };
    GetHighlightIndex(list: Highlight[], highlight: Highlight) {
        return list.findIndex((h: Highlight) =>
            h.location.bookNumber == highlight.location.bookNumber &&
            h.location.sectionNumber == highlight.location.sectionNumber &&
            h.location.groupNumber == highlight.location.groupNumber &&
            h.location.wordNumber == highlight.location.wordNumber
        );
    };
    // handler that runs when a word is clicked on - if the word is already part of the current annotation, it is
    // removed, else it is added
    onWordClick(event: Event) {
        var clickedElement = event.target as HTMLSpanElement;
        //update DOM
        if (clickedElement.dataset.ids.indexOf("," + this.annBeingEdited.toString() + ",") >= 0) {
            clickedElement.dataset.ids = clickedElement.dataset.ids.replace(this.annBeingEdited.toString() + ",", '');
        }
        else {
            clickedElement.dataset.ids += this.annBeingEdited.toString() + ",";
        }
        var _a = Helpers.getHighlightIds(clickedElement), groupNum = _a[0], wordNum = _a[1];
        var wordPlace = new Place(this.bookNum, this.sectionNum, groupNum, wordNum);
        console.log(this.bookNum);
        var highlight = new Highlight(wordPlace, this.annBeingEdited);
        var currentAnn = this.annotations[Helpers.getAnnotationIndex(this.annBeingEdited, this.annotations)]; // this is ridiculous!
        var highlightIndex = this.GetHighlightIndex(currentAnn.highlights, highlight);
        if (highlightIndex == -1) {
            var indexInToAdd = this.GetHighlightIndex(this.highlightsToAdd, highlight);
            if (indexInToAdd == -1) {
                this.highlightsToAdd.push(highlight);
            }
            else {
                this.highlightsToAdd.splice(indexInToAdd, 1);
            }
        }
        else {
            var existingHighlight = currentAnn.highlights[highlightIndex];
            var indexInToRemove = this.GetHighlightIndex(this.highlightsToRemove, existingHighlight);
            if (indexInToRemove == -1) {
                this.highlightsToRemove.push(existingHighlight);
            }
            else {
                this.highlightsToRemove.splice(indexInToRemove, 1);
            }
        }
    };
    // remove highlighting from the text - used when an annotation is deleted.
    removeHighlightsByAnnotation(id: number) {
        $(Helpers.document).find("span.word").each((i, span) => {
            if (span.dataset.ids.indexOf("," + id.toString() + ",") >= 0) {
                span.dataset.ids = span.dataset.ids.replace(id.toString(), '');
            }
        });
    };
    // update the highlighting in the text - used when an annotation is saved, so we can then display it with the right colour.
    updateHighlightsByAnnotation(annId: number) {
        $(Helpers.document).find("span.word").each((i, span) => {
            if (span.dataset.ids.indexOf(",0,") >= 0) {
                span.dataset.ids = span.dataset.ids.replace(",0,", "," + annId.toString() + ",");
            }
        });
    };

}