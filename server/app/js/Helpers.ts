

import { Annotation } from "./types/Annotation";
import { BracketStatus } from "./types/BracketStatus"
export class Helpers {

    static convertRemToPixels(rem: number) {
        return rem * parseFloat(getComputedStyle(document.documentElement).fontSize);
    };
    static getBracketStatus(span: HTMLSpanElement, newId: number) {
        let prevGroup: number, prevWord: number, nextGroup: number, nextWord = 0;
        let start: boolean, end = false;
        let _a = this.getHighlightIds(span), groupNo = _a[0], wordNo = _a[1];
        if (wordNo == 0 && groupNo == 0) {
            start = true;
        }
        else if (wordNo == 0) {
            prevWord = -1;
            prevGroup = groupNo - 1;
        }
        else {
            prevGroup = groupNo;
            prevWord = wordNo - 1;
        }
        let maxWords = this.getHighlightIds($(span).parent().children(":last")[0])[1];
        let maxGroups = this.getHighlightIds($(this.contentCtr).find("span").last()[0])[0];
        if (maxWords - wordNo == 0 && groupNo == maxGroups) {
            end = true;
        }
        else if (maxWords - wordNo == 0) {
            nextWord = 0;
            nextGroup = groupNo + 1;
        }
        else {
            nextGroup = groupNo;
            nextWord = wordNo + 1;
        }
        let isNextHighlighted = this.isWordHighlighted(nextGroup, nextWord, newId);
        let isPrevHighlighted = this.isWordHighlighted(prevGroup, prevWord, newId);
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
    };
    static getHighlightIds(span: HTMLSpanElement) {
        return span.id.split("-").map(x => { return parseInt(x, 10); });
    };
    static isWordHighlighted(groupNo: number, wordNo: number, annId: number) {
        let span = $("[id=" + groupNo + "-" + wordNo + "]");
        let data = span.data("ids");
        if (data === null || data === undefined) {
            return false;
        }
        return data.indexOf("," + annId.toString() + ",") >= 0;
    };
    static getAnnotationIdsFromElement(element: HTMLSpanElement) {
        return element.dataset.ids.split(",").map(id => parseInt(id, 10)).filter(id => !Number.isNaN(id));
    };
    static spanIsAnnotated(span: HTMLSpanElement) {
        return span.dataset.ids && this.getAnnotationIdsFromElement(span).length > 0;
    };
    static getAnnotation(id: number, annotations: Annotation[]) {
        return annotations.find((x: Annotation) => x.annotationId == id);
    };
    static getAnnotationIndex(id: number, annotations: Annotation[]) {
        for (let i = 0; i < annotations.length; i++) {
            if (annotations[i].annotationId == id) {
                return i;
            }
        }
        return -1;
    };
    static forAllGroups(fun: (arg0: Element, arg1: Element) => void) {
        let groups = $("span.group").toArray();
        for (let i = 0; i < groups.length; i++) {
            let group = groups[i];
            for (let j = 0; j < group.children.length; j++) {
                let span = group.children[j];
                let prev = group.children[j - 1];
                fun(span, prev);
            }
        }
    };
    static forAllGroupsInParent(f: (a: Element, b: Element) => void, parent: HTMLElement) {
        let groups = $(parent).find("span.group").toArray();
        for (let i = 0; i < groups.length; i++) {
            let group = groups[i];
            for (let j = 0; j < group.children.length; j++) {
                let span = group.children[j];
                let prev = group.children[j - 1];
                f(span, prev);
            }
        }
    };
    // containers for various aspects of the UI
    static contentCtr = document.getElementById("contentContainer");
    static document = document.getElementById("document");
    static annCtr = document.getElementsByClassName("annotationTable")[0];
    static backgroundCtr = document.getElementById("backgroundContainer");

}