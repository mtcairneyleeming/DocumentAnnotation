// noinspection JSMethodCanBeStatic

import { Text} from "./types/TextTypes"

export class SelectController {
    constructor(bookId: string, sectionId: string, textNames: Text) {
        this.bookId = bookId;
        this.sectionId = sectionId;
        this.textNames = textNames;
    }

    private bookId: string;
    private sectionId: string;
    private textNames: Text;

    init() {
        this.initialiseSelects();
        this.addHandlers();
    }

    addHandlers() {
        $("#bookSelect").on('change', () => this.updateBook());
        $("#sectionSelect").on('change', () => this.updateSection());
        $("#navigationButton").on('click', () => this.navigate())
    }

    private initialiseSelects() {
        SelectController.setSelectOptions("bookSelect", this.textNames.books.map(b => b.name), this.bookId);
        SelectController.setSelectOptions("sectionSelect", this.textNames.books.find(b => b.name === this.bookId).sections.map(s => s.name), this.sectionId);
    }

    updateBook() {
        // update the section dropdown
        const e = <HTMLSelectElement>document.getElementById("bookSelect");
        this.bookId = e.options[e.selectedIndex].value;
        this.initialiseSelects();
    }


    updateSection() {
        const e = <HTMLSelectElement>document.getElementById("sectionSelect");
        this.sectionId = e.options[e.selectedIndex].value;

    }

    navigate() {
        // navigate to the new location chosen
        this.updateBook();
        this.updateSection();
        let newUrl = SelectController.updateQueryStringParameter(window.location.href, "book", this.bookId);
        newUrl = SelectController.updateQueryStringParameter(newUrl, "section", this.sectionId);
        window.location.href = newUrl;
    }


    private static updateQueryStringParameter(uri, key, value) {
        const re = new RegExp("([?&])" + key + "=.*?(&|$)", "i");
        const separator = uri.indexOf('?') !== -1 ? "&" : "?";
        if (uri.match(re)) {
            return uri.replace(re, '$1' + key + "=" + value + '$2');
        } else {
            return uri + separator + key + "=" + value;
        }
    }

    private static setSelectOptions(id, options, currentSelection) {
        let select = $(`#${id}`);
        select.empty();
        for (let i = 0; i < options.length; i++) {

            if (options[i] === currentSelection) {
                select.append(`<option value="${options[i]}" selected>${options[i]}</option>`);
            } else {
                select.append(`<option value="${options[i]}">${options[i]}</option>`);
            }
        }
    }
}