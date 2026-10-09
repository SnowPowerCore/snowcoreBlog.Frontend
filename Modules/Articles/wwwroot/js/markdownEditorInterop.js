// Minimal interop for the MarkdownEditor component: reads/restores the text
// caret+selection of the native <textarea> rendered inside a FluentTextArea host.
export function getSelection(host) {
    const ta = host.querySelector('textarea') || host;
    return { start: ta.selectionStart, end: ta.selectionEnd, length: ta.value.length };
}

export function setSelection(host, start, end) {
    const ta = host.querySelector('textarea') || host;
    ta.focus();
    ta.setSelectionRange(start, end);
}
