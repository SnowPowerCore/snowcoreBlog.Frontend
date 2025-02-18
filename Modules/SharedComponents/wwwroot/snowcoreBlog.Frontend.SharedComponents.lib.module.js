export function afterWebStarted(blazor) {
    blazor.registerCustomEventType('verifiedEvent', {
        createEventArgs: event => {
            return {
                payload: event.detail.payload
            };
        }
    });
}

export function afterStarted(blazor) {
    blazor.registerCustomEventType('verifiedEvent', {
        createEventArgs: event => {
            return {
                payload: event.detail.payload
            };
        }
    });
}