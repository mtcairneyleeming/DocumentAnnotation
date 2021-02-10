
// a location in the c#
export class Place {
    bookNumber: number;
    sectionNumber: number;
    exact: boolean;
    groupNumber: number;
    wordNumber: number;
    constructor(bookNumber: number, sectionNumber: number, groupNumber: number, wordNumber: number) {
        this.bookNumber = bookNumber;
        this.sectionNumber = sectionNumber;
        if (groupNumber === null || groupNumber === undefined) {
            this.exact = false;
        }
        else {
            this.groupNumber = groupNumber;
            this.wordNumber = wordNumber;
            this.exact = true;
        }
    }

}