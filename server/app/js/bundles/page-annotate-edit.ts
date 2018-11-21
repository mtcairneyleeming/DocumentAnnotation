import "../../css/annotation.css"
import "../../css/text-view.css"
import {Annotator} from "../Annotator"
import {Annotation} from "../AnnotationTypes";

let annotator;
$(window).on('load',
    () => {

        let annotations = JSON.parse($("#annotations").text()) as Annotation[];
        let {docAnnId, bookNumber, sectionNumber, print} = JSON.parse($("#info").text());

        annotator = new Annotator(annotations, docAnnId, bookNumber, sectionNumber, print);
        annotator.addCanvasHandlers();
        annotator.addEditHandlers();
        annotator.draw();
    });
