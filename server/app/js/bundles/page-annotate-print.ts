import "../../css/annotation.css";
import "../../css/text-view.css";
import "../../css/print.css";
import Annotator from "../Annotator";
import { Mode } from "../types/Mode";

// Annotate -> Print: the same text and annotation tables as the View page, one block per annotated section, with the
// highlights shown as numbers and underlines (black and white; see print.css) instead of colours. Once the page has
// rendered, open the browser's print dialog.
$(window).on('load',
    async () => {
        const { docAnnId, bookNumber } = JSON.parse($("#info").text());

        // Mode.Printing numbers the annotations in each section's table and marks the matching words
        new Annotator(Mode.Printing, [], docAnnId, bookNumber, 0).setup();

        // wait for web fonts and for the browser to lay out the numbered highlights before printing
        if (document.fonts && document.fonts.ready) {
            await document.fonts.ready;
        }
        await new Promise<void>((resolve) => requestAnimationFrame(() => requestAnimationFrame(() => resolve())));
        window.print();
    });
