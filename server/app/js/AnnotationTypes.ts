import {isNullOrUndefined} from "util";

export class Annotation {
    public highlights: Highlight[];
    public title: string;
    public body: string;
    public annotationId: number;
    public documentId: number;

    constructor(highlights: Highlight[], title: string, body: string, annotationId: number, documentId: number) {
        this.highlights = highlights;
        this.title = title;
        this.body = body;
        this.annotationId = annotationId;
        this.documentId = documentId;
    }
}

// a location in the c#
export class Place {
    constructor(bookNumber: number, sectionNumber: number, groupNumber?: number, wordNumber?: number) {
        this.bookNumber = bookNumber;
        this.sectionNumber = sectionNumber;
        if (isNullOrUndefined(groupNumber)) {
            this.exact = false;
        } else {
            this.groupNumber = groupNumber;
            this.wordNumber = wordNumber;
            this.exact = true;
        }
    }


    public bookNumber: number;
    public sectionNumber: number;
    public groupNumber: number;
    public wordNumber: number;
    public exact: boolean;
}

export class Highlight {
    constructor(location: Place, annotationId: number) {
        this.location = location;
        this.annotationId = annotationId;
    }

    public location: Place;
    public annotationId: number;
}