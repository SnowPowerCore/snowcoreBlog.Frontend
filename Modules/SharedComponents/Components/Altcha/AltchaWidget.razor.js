export function invokeCustomVerifiedEventDelegate(ev) {
    const source = ev.target || ev.srcElement;
    const e = new CustomEvent('verifiedEvent', {
        bubbles: true,
        detail: ev.detail
    });
    source.dispatchEvent(e);
};

export function altchaScriptHookVerifiedEvent(id) {
    var item = document.getElementById(id);
    if (!!item) {
        item.addEventListener('verified', invokeCustomVerifiedEventDelegate);
    }
};