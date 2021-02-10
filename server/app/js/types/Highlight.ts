import { Place } from "./Place";

export class Highlight {
    location: Place;
    annotationId: number;
    constructor(location: Place, annotationId: number) {
        this.location = location;
        this.annotationId = annotationId;
    }

}