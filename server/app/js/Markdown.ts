import { marked } from "marked";
import DOMPurify = require("dompurify");

// Only these URL schemes (or relative URLs) survive in href/src. This matches the server, which sanitises with
// HtmlSanitizer configured for http, https, mailto and tel (Services/MarkdownRenderer.cs).
const ALLOWED_URI = /^(?:(?:https?|mailto|tel):|[^a-z]|[a-z+.\-]+(?:[^a-z+.\-:]|$))/i;

// DOMPurify lets images use data: URIs; the server does not, so drop them here too.
DOMPurify.addHook("afterSanitizeAttributes", (node) => {
    for (const attr of ["src", "href", "xlink:href"]) {
        const value = node.getAttribute && node.getAttribute(attr);
        if (value && /^\s*data:/i.test(value)) {
            node.removeAttribute(attr);
        }
    }
});

/**
 * Render user-written Markdown (annotation bodies) to sanitised HTML. The server renders the same Markdown with
 * Markdig when a page loads (and sanitises it equivalently); this is used to update the table without a reload after
 * saving.
 */
export function renderMarkdown(markdown: string): string {
    const html = marked.parse(markdown || "", { async: false }) as string;
    return DOMPurify.sanitize(html, {
        ALLOWED_URI_REGEXP: ALLOWED_URI,
        // not useful in a note, and they allow UI spoofing/styling of the page
        FORBID_TAGS: ["style", "form", "input", "button", "select", "textarea", "option", "label", "svg", "math"],
        FORBID_ATTR: ["style", "target"],
    });
}
