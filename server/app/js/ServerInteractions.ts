import axios from "axios"
import Annotation from "./types/Annotation";
import Highlight from "./types/Highlight";

export class ServerInteractions {

    static saveNewAnnotation(annotation: Annotation, highlightsToAdd: Highlight[], reload: boolean = true) {

        // add new highlights too
        return axios.post("/api/Annotations", {
            annotation: annotation,
            highlightsToAdd: highlightsToAdd
        }).then((res) => {
            // reload page
            if (reload) {
                window.location.reload();
            }
            highlightsToAdd = [];
            // @ts-ignore
            return res.data.annotation;
        });
    };
    static updateAnnotation(annotation: Annotation, highlightsToAdd: Highlight[], highlightsToRemove: Highlight[], reload: boolean = true) {

        //let annotation = Helpers.getAnnotation(annotationId, annotation);
        annotation.highlights = [];
        return axios.put("/api/Annotations/" + annotation.annotationId, {
            annotation: annotation,
            highlightsToAdd: highlightsToAdd,
            highlightsToRemove: highlightsToRemove
        }).then((res) => {
            // reload page
            if (reload) {
                window.location.reload();
            }
            highlightsToAdd = [];
            highlightsToRemove = [];
            // @ts-ignore
            return res.data.annotation;
        });
    };
    static deleteAnnotationFromServer(annotationId: string, reload: boolean = true) {
        return axios.delete("/api/Annotations/" + annotationId);
    };

}