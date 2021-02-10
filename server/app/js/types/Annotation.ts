import { Highlight } from "./Highlight";

export class Annotation {
    highlights: Highlight[];
    title: string;
    body: string;
    annotationId: number;
    documentId: number;
    constructor(highlights: Highlight[], title: string, body: string, annotationId: number, documentId: number) {
        this.highlights = highlights;
        this.title = title;
        this.body = body;
        this.annotationId = annotationId;
        this.documentId = documentId;
    }

}