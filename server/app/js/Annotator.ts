
import { PrintTextHighlighter } from "./PrintTextHighlighter";
// var $ = __webpack_require__(/*! jquery */ "../node_modules/jquery/dist/jquery.js");
import { ColourTextHighlighter } from "./ColourTextHighlighter";
import { TableDisplay } from "./TableDisplay";
import { AnnotationEditor } from "./AnnotationEditor";
import { Mode } from "./types/Mode";
import Annotation from "./types/Annotation";
export default class Annotator {
    annotations: Annotation[];
    mode: Mode;
    docAnnId: number;
    bookNum: number;
    sectionNum: number;
    colouredMode: boolean;
    colourTextHighlighter: ColourTextHighlighter;
    annotationEditor: AnnotationEditor;
    constructor(mode: Mode, annotations: Annotation[], docAnnId: number, bookNum: number, sectionNum: number) {
        this.mode = mode;
        this.annotations = annotations;
        this.docAnnId = docAnnId;
        this.bookNum = bookNum;
        this.sectionNum = sectionNum;
    }

    display() {
        switch (this.mode) {
            case Mode.Editing: // this needs both highlighting (coloured or numbered) plus annotation display and management
            case Mode.Viewing: // this needs highlighting (coloured or numbered), but not annotation display as that is all fixed
                if (this.colouredMode) {
                    TableDisplay.hideTableAnnotationNumbers();
                    PrintTextHighlighter.showDocumentNumbers(false, null);
                    this.colourTextHighlighter.draw();
                }
                else {
                    this.colourTextHighlighter.hide();
                    var annotationNumbers = TableDisplay.showTableAnnotationNumbers();
                    PrintTextHighlighter.showDocumentNumbers(true, annotationNumbers);
                }
                break;
            case Mode.Printing:
                // all this needs is displaying numbered highlights on the text
                var annNumLists = TableDisplay.getAnnotationNumbers();
                console.log(annNumLists);
                PrintTextHighlighter.showDocumentNumbersOnAllTables(true, annNumLists);
                break;
        }
    };


    setup() {
        this.colouredMode = window.localStorage.getItem("displayMode") != "lines";
        if (this.mode == Mode.Editing) {
            this.annotationEditor = new AnnotationEditor(this.annotations, this.docAnnId, this.bookNum, this.sectionNum, this.display);
            this.annotationEditor.addEditHandlers();
        }
        if (this.mode == Mode.Editing || this.mode == Mode.Viewing) {
            this.addEventHandlers();
            this.colourTextHighlighter = new ColourTextHighlighter();
            if (this.colouredMode)
                this.colourTextHighlighter.addAllHandlers();
        }
        this.display();
    };
    addEventHandlers() {

        $("#colouredModeToggle").on('click', () => {
            this.colouredMode = !this.colouredMode;
            if (this.colouredMode) {
                window.localStorage.setItem("displayMode", "colours");
            }
            else {
                window.localStorage.setItem("displayMode", "lines");
            }
            this.display();
        });
        // @ts-ignore
        window.onbeforeprint = () => {
            this.colouredMode = false;
            this.display();
        };
        // @ts-ignore
        window.onafterprint = () => {
            this.colouredMode = window.localStorage.getItem("displayMode") != "lines";
            this.display();
        };
    };

}
