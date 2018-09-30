export interface Section {
    name: string;
    groups: any[];
}

export interface Book {
    name: string;
    sections: Section[];
}

export interface Text {
    identifier: string;
    name: string;
    books: Book[];
}