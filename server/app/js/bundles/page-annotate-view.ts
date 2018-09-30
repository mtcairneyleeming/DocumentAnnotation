import "../../css/annotation.css"
import { Annotator, } from "../Annotator"
import { Annotation } from "../AnnotationTypes";


let annotator;
$(window).on('load',
    () => {

        let annotations = JSON.parse($("#annotations").text()) as Annotation[];
        let annotationColours = JSON.parse($("#annotation-colours").text()) as { [id: number]: string };
        let annotationClasses = JSON.parse($("#annotation-classes").text()) as { [id: number]: string };
        let { docAnnId, bookNumber, sectionNumber } = JSON.parse($("#info").text());

        annotator = new Annotator(annotations, annotationColours, annotationClasses, docAnnId, bookNumber, sectionNumber, false);

        annotator.displayAllCards();


        annotator.addCanvasHandlers();
        annotator.drawLines();
    });
$(window).on('resize',
    () => {
        annotator.drawLines();
    });