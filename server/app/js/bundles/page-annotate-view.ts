import "../../css/annotation.css"
import "../../css/text-view.css"
import Annotator from "../Annotator";
import Annotation from "../types/Annotation";
import {Mode} from "../types/Mode";
let annotator:Annotator;
$(window).on('load',
    () => {

        let annotations = JSON.parse($("#annotations").text()) as Annotation[];
        let {docAnnId, bookNumber, sectionNumber, print} = JSON.parse($("#info").text());

        annotator = new Annotator(Mode.Viewing, annotations, docAnnId, bookNumber, sectionNumber);
        annotator.setup();
    });
