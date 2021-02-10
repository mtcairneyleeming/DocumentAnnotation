export class Text {
    name: string
    books: Book[];
    constructor(name: string, books: Book[]) {
        this.name = name;
        this.books = books;
    }
}

export class Book {
    name: string
    sections: Section[];
    constructor(name: string, sections: Section[]) {
        this.name = name;
        this.sections = sections;
    }
}

export class Section {
    name: string
    constructor(name: string) {
        this.name = name;

    }
}