import "../../css/text-view.css"
import { SelectController } from "../SelectController";

let select: SelectController;
$(window).on('load',
    () => {
        let textNames = JSON.parse($("#text-names").text());
        let { bookId, sectionId } = JSON.parse($("#info").text());
        select = new SelectController(bookId, sectionId, textNames);
        select.init();
    });