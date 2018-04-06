// Write your Javascript code.

function setSelectOptions(id, options, currentSelection) {
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

function initialiseSelects() {
    setSelectOptions("bookSelect", textNames.books.map(b => b.name), bookId);
    setSelectOptions("sectionSelect", textNames.books.find(b => b.name === bookId).sections.map(s => s.name),
        sectionId);
}

function updateBook() {
    // update the section dropdown
    const e = document.getElementById("bookSelect");
    let bookId = e.options[e.selectedIndex].value;
    initialiseSelects();
}

function updateSection() {
    const e = document.getElementById("sectionSelect");
    sectionId = e.options[e.selectedIndex].value;

}

function navigate() {
    // navigate to the new location chosen
    updateBook();
    updateSection();
    let newUrl = updateQueryStringParameter(window.location.href, "book", bookId);
    newUrl = updateQueryStringParameter(newUrl, "section", sectionId);
    window.location.href = newUrl;
}

function updateQueryStringParameter(uri, key, value) {
    const re = new RegExp("([?&])" + key + "=.*?(&|$)", "i");
    const separator = uri.indexOf('?') !== -1 ? "&" : "?";
    if (uri.match(re)) {
        return uri.replace(re, '$1' + key + "=" + value + '$2');
    } else {
        return uri + separator + key + "=" + value;
    }
}
$("#bookSelect").on('change', updateBook);
$("#sectionSelect").on('change', updateSection);