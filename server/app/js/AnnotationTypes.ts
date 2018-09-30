export class Annotation {
    public highlights: Highlight[];
    public title: string;
    public body: string;
    public annotationId: number;
    public documentAnnotationId: number;

    constructor(highlights: Highlight[], title: string, body: string, annotationId: number, documentAnnotationId: number) {
        this.highlights = highlights;
        this.title = title;
        this.body = body;
        this.annotationId = annotationId;
        this.documentAnnotationId = documentAnnotationId;
    }
}

// a location in the c#
export class Place {
    constructor(bookNumber: number, sectionNumber: number, groupNumber: number, wordNumber: number) {
        this.bookNumber = bookNumber;
        this.sectionNumber = sectionNumber;
        this.groupNumber = groupNumber;
        this.wordNumber = wordNumber;
    }

    public bookNumber: number;
    public sectionNumber: number;
    public groupNumber: number;
    public wordNumber: number;
}

export class Highlight {
    constructor(location: Place,annotationId: number) {
        this.location = location;
        this.annotationId = annotationId;
    }

    public location: Place;
    public annotationId: number;
}