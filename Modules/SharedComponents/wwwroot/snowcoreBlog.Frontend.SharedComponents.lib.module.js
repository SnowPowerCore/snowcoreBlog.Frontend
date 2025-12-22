function ensureVerifiedEventRegistered(blazor) {
    // Both afterWebStarted and afterStarted can be invoked; ensure we only register once.
    const key = '__snowcore_customEvent_verifiedEvent_registered__';
    if (globalThis[key]) {
        return;
    }

    try {
        blazor.registerCustomEventType('verifiedEvent', {
            createEventArgs: event => {
                return {
                    payload: event.detail?.payload
                };
            }
        });
        globalThis[key] = true;
    } catch (error) {
        // Be resilient across framework versions that may throw on duplicates.
        const message = String(error?.message ?? error);
        if (message.includes("'verifiedEvent' is already registered")) {
            globalThis[key] = true;
            return;
        }
        console.error('Failed to register Blazor custom event type: verifiedEvent', error);
        return;
    }
}

export function afterWebStarted(blazor) {
    ensureVerifiedEventRegistered(blazor);
}

export function afterStarted(blazor) {
    ensureVerifiedEventRegistered(blazor);
}