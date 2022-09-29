
import Konva from 'konva';
import { getAnnotationColour, getHighlightingData, HighlightType } from "./colours"
import { Helpers } from "./Helpers"
import { BracketStatus } from "./types/BracketStatus"

class Line {
    annId: number;
    line: Konva.Line;

    constructor(annId: number, line: Konva.Line) {
        this.annId = annId;
        this.line = line;
    }

}
export class ColourTextHighlighter {
    stage: Konva.Stage;
    layer: Konva.Layer;
    annLines: Line[];
    hoverLines: Line[];
    ColourTextHighlighter: ColourTextHighlighter;

    constructor() {
        // initial set up of canvas
        this.stage = new Konva.Stage({
            container: "backgroundContainer",
            width: Helpers.contentCtr.clientWidth,
            height: Helpers.contentCtr.clientHeight
        });
        this.layer = new Konva.Layer();
        this.stage.add(this.layer);
    }


    addHighlightHandlers(span: HTMLSpanElement) {
        $(span).off("mouseover");
        $(span).off("mouseout");
        if (Helpers.spanIsAnnotated(span)) { // if annotated
            var ids = Helpers.getAnnotationIdsFromElement(span);

            // add mouseover handlers to emphasise the links between highlights and annotations
            $(span).on("mouseover", () => {
                var lines = this.annLines.filter((l: { annId: number; }) => ids.indexOf(l.annId) >= 0);
                if (lines.length == 0) {
                }
                else {
                    for (var _i = 0, lines_1 = lines; _i < lines_1.length; _i++) {
                        var line = lines_1[_i];
                        line.line.dash([10, 0]);
                        line.line.opacity(100);
                    }
                    this.updateCanvas();
                }
                var bracketIds = ids.filter((v) => { return getHighlightingData(v)[0] == HighlightType.Brackets; });
                this.hoverLines.filter((l: Line) => { return bracketIds.indexOf(l.annId) != -1; }).forEach((x: Line) => {
                    x.line.show();
                });
                this.updateCanvas();
            });
            $(span).on("mouseout", () => {
                var lines = this.annLines.filter((l: Line) => { return ids.indexOf(l.annId) >= 0; });
                if (lines.length == 0) {
                }
                else {
                    for (var _i = 0, lines_2 = lines; _i < lines_2.length; _i++) {
                        var line = lines_2[_i];
                        line.line.dash([5, 15]);
                        line.line.opacity(line.annId != 0 ? 0.5 : 1);
                    }
                }
                var bracketIds = ids.filter((v) => { return getHighlightingData(v)[0] == HighlightType.Brackets; });
                this.hoverLines.filter((l: Line) => { return bracketIds.indexOf(l.annId) != -1; }).forEach((x: Line) => {
                    x.line.hide();
                });
                this.updateCanvas();
            });
        }
    };
    draw() {
        // draw lines from a highlight to the annotation comment
        var _a: Konva.Layer;
        var groups = document.getElementsByClassName("group");
        // clear layer
        this.layer.destroyChildren();
        this.annLines = [];
        this.hoverLines = [];
        var shapesToAdd = [];
        for (var i = 0; i < groups.length; i++) {
            var group = groups[i];
            for (var j = 0; j < group.children.length; j++) {
                var span = group.children[j] as HTMLSpanElement;
                var lines = this.createLines(span, group.children[j + 1] as HTMLSpanElement);
                if (lines.length > 0) {
                    shapesToAdd.push.apply(shapesToAdd, lines);
                }
                var highlights = this.createHighlights(span);
                if (highlights.length > 0) {
                    shapesToAdd.push.apply(shapesToAdd, highlights);
                }
                this.addHighlightHandlers(span);
            }
        }
        if (shapesToAdd.length > 0) {
            (_a = this.layer).add.apply(_a, shapesToAdd);
        }
        this.updateCanvas();
    }


    addAllHandlers() {
        this.addCanvasHandlers();
        Helpers.forAllGroups(this.addHighlightHandlers);
    }
    addCanvasHandlers() {

        $(window).on('resize', () => {
            this.onResize();
        });
        $(Helpers.annCtr).find("tr[id*=ann]").toArray().forEach((el, i) => {
            var id = parseInt(el.dataset.annId, 10);
            $(el).on("mouseover", () => {
                this.annLines.filter((l: Line) => { return l.annId === id; }).forEach((x: Line) => {
                    x.line.dash([10, 0]);
                    x.line.opacity(100);
                });
                this.updateCanvas();
            });
            $(el).on("mouseout", () => {
                this.annLines.filter((l: { annId: number; }) => { return l.annId === id; }).forEach((x: Line) => {
                    x.line.dash([5, 15]);
                    x.line.opacity(50);
                });
                this.updateCanvas();
            });
        });
    };


    createHighlights(span: HTMLSpanElement) {
        var shapesToAdd = [];
        if (span.dataset.ids) { // if annotated
            var ids = Helpers.getAnnotationIdsFromElement(span);
            for (var i = 0; i < ids.length; i++) {
                var id = ids[i];
                // HIGHLIGHT DRAWING 
                var spanRect = span.getBoundingClientRect();
                var canvasRect = Helpers.contentCtr.getBoundingClientRect();
                var spanBottomLeft = {
                    x: spanRect.left - canvasRect.left + 15,
                    y: spanRect.bottom - canvasRect.top
                };
                var spanTopLeft = {
                    x: spanBottomLeft.x,
                    y: spanBottomLeft.y - spanRect.height + Helpers.convertRemToPixels(2)
                };
                var spanBottomRight = {
                    x: spanBottomLeft.x + spanRect.width + 4.4,
                    y: spanBottomLeft.y
                };
                var spanTopRight = {
                    x: spanBottomRight.x,
                    y: spanTopLeft.y
                };
                var spanWidth = spanBottomRight.x - spanBottomLeft.x;
                var spanHeight = spanBottomLeft.y - spanTopLeft.y;
                var data = getHighlightingData(id);
                switch (data[0]) {
                    case HighlightType.Brackets:
                        var status = Helpers.getBracketStatus(span, id);
                        switch (status) {
                            case BracketStatus.Start:
                                shapesToAdd.push(new Konva.Line({
                                    points: [spanTopLeft.x + 10, spanTopLeft.y, spanTopLeft.x, spanTopLeft.y, spanBottomLeft.x, spanBottomLeft.y + 4, spanBottomLeft.x + 10, spanBottomLeft.y + 4],
                                    strokeWidth: 2,
                                    stroke: data[1]
                                }));
                                break;
                            case BracketStatus.Middle:
                                // do nothing normally
                                break;
                            case BracketStatus.End:
                                shapesToAdd.push(new Konva.Line({
                                    points: [spanTopRight.x - 10, spanTopRight.y, spanTopRight.x, spanTopRight.y, spanBottomRight.x, spanBottomRight.y + 4, spanBottomRight.x - 10, spanBottomRight.y + 4],
                                    strokeWidth: 2,
                                    stroke: data[1]
                                }));
                                break;
                            case BracketStatus.Single:
                                shapesToAdd.push(new Konva.Line({
                                    points: [spanTopLeft.x + 10, spanTopLeft.y, spanTopLeft.x, spanTopLeft.y, spanBottomLeft.x, spanBottomLeft.y + 4, spanBottomLeft.x + 10, spanBottomLeft.y + 4],
                                    strokeWidth: 2,
                                    stroke: data[1]
                                }));
                                shapesToAdd.push(new Konva.Line({
                                    points: [spanTopRight.x - 10, spanTopRight.y, spanTopRight.x, spanTopRight.y, spanBottomRight.x, spanBottomRight.y + 4, spanBottomRight.x - 10, spanBottomRight.y + 4],
                                    strokeWidth: 2,
                                    stroke: data[1]
                                }));
                                break;
                        }
                        var hoverTopLine = new Konva.Line({
                            points: [spanTopLeft.x, spanTopLeft.y, spanTopRight.x, spanTopRight.y],
                            strokeWidth: 2,
                            stroke: getAnnotationColour(id),
                            visible: false
                        });
                        shapesToAdd.push(hoverTopLine);
                        this.hoverLines.push({ annId: id, line: hoverTopLine });
                        var hoverBottomLine = new Konva.Line({
                            points: [spanBottomLeft.x, spanBottomLeft.y + 4, spanBottomRight.x, spanBottomRight.y + 4],
                            strokeWidth: 2,
                            stroke: getAnnotationColour(id),
                            visible: false
                        });
                        shapesToAdd.push(hoverBottomLine);
                        this.hoverLines.push({ annId: id, line: hoverBottomLine });
                        break;
                    case HighlightType.Underline:
                        var underlineRect = new Konva.Rect({
                            x: spanBottomLeft.x,
                            y: spanBottomLeft.y + i * 4,
                            height: 4,
                            width: spanWidth,
                            fill: getAnnotationColour(id)
                        });
                        shapesToAdd.push(underlineRect);
                        break;
                    case HighlightType.Highlight:
                        var highlightRect = new Konva.Rect({
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
    };
    createLines(span: HTMLSpanElement, next: HTMLSpanElement) {
        var shapesToAdd = [];
        if (span.dataset.ids) { // if annotated
            var ids = Helpers.getAnnotationIdsFromElement(span);
            for (var _i = 0, ids_2 = ids; _i < ids_2.length; _i++) {
                var id = ids_2[_i];
                if (next && Helpers.getAnnotationIdsFromElement(next).indexOf(id) != -1) {
                    continue; // only draw lines to the last character in the highlight
                }
                var spanRect = span.getBoundingClientRect();
                var canvasRect = Helpers.contentCtr.getBoundingClientRect();
                var spanBottomLeft = {
                    x: spanRect.left - canvasRect.left + 15,
                    y: spanRect.bottom - canvasRect.top
                };
                var spanBottomRight = {
                    x: spanBottomLeft.x + spanRect.width + 5,
                    y: spanBottomLeft.y + 2
                };
                var tableRow = document.getElementById("annotation" + id).getBoundingClientRect();
                var annLeftMiddle = {
                    x: tableRow.left - canvasRect.left + 15,
                    y: tableRow.top + 0.5 * (tableRow.height) - canvasRect.top
                };
                var line = new Konva.Line({
                    points: [spanBottomRight.x, spanBottomRight.y, annLeftMiddle.x, annLeftMiddle.y],
                    stroke: getAnnotationColour(id),
                    strokeWidth: 2,
                    dash: [5, 15],
                    opacity: id != 0 ? 0.5 : 1
                });
                shapesToAdd.push(line);
                this.annLines.push({ annId: id, line: line });
            }
        }
        return shapesToAdd;
    };
    onResize() {
        this.stage.width(Helpers.contentCtr.clientWidth);
        this.stage.height(Helpers.contentCtr.clientHeight);
        this.draw();
    };
    updateCanvas() {
        this.layer.draw();
    };
    hide() {
        this.layer.destroyChildren();
        this.updateCanvas();
    };
}