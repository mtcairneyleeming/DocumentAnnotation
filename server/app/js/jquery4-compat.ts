// jquery-validation-unobtrusive 4.0.0 still calls two helpers that jQuery 4 removed (it is otherwise compatible):
// $.parseJSON (to read data-valmsg-replace) and $.isFunction. Restore them for the validation bundle.
const jq = (window as any).jQuery;
if (!jq.parseJSON) {
    jq.parseJSON = JSON.parse;
}
if (!jq.isFunction) {
    jq.isFunction = (f: unknown) => typeof f === "function";
}
