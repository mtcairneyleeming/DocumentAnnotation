import "../../css/annotation.css";
import "../../css/text-view.css";
import Annotator from "../Annotator";
import Annotation from "../types/Annotation";
import { Mode } from "../types/Mode";

let annotator: Annotator;
$(window).on('load',
    () => {

        let annotations = JSON.parse($("#annotations").text()) as Annotation[];
        let { docAnnId, bookNumber, sectionNumber } = JSON.parse($("#info").text());
        console.log(bookNumber);
        annotator = new Annotator(Mode.Editing, annotations, docAnnId, bookNumber, sectionNumber);
        annotator.setup();
    });
