// Drives the mobile "raised" shadow of FloatingCard: the hover shadow is applied
// automatically while the card is "sufficiently" visible and removed once less
// than half of it is visible (e.g. scrolled out). Sufficiently visible means the
// card is fully displayed when it fits the screen vertically, or it occupies
// most of the screen when it is taller than one screen. Active only on mobile
// viewport sizes; the desktop keeps the plain :hover behavior.
const MOBILE_QUERY = '(max-width: 575.98px)';
const RAISED_CLASS = 'is-raised';
const MOST_OF_VIEWPORT_RATIO = 0.8; // "most of the screen" for taller-than-viewport cards

// element -> state (survives Blazor re-renders without leaking on navigation)
const states = new WeakMap();

function onIntersection(state, entries) {
    for (const entry of entries) {
        const cardHeight = entry.boundingClientRect.height;
        const visibleHeight = entry.intersectionRect.height;
        const viewportHeight = (entry.rootBounds && entry.rootBounds.height) || window.innerHeight;

        if (cardHeight <= viewportHeight) {
            // Card fits the screen vertically -> raised when fully visible.
            if (visibleHeight >= cardHeight - 1) {
                state.raised = true;
            }
        } else {
            // Card is taller than the screen -> raised when it occupies most of the viewport.
            if (visibleHeight >= MOST_OF_VIEWPORT_RATIO * viewportHeight) {
                state.raised = true;
            }
        }

        // Less than half of the effective reference visible -> dropped.
        const referenceHeight = Math.min(cardHeight, viewportHeight);
        if (visibleHeight < 0.5 * referenceHeight) {
            state.raised = false;
        }

        state.element.classList.toggle(RAISED_CLASS, state.raised);
    }
}

// One threshold per 5% of the card: the decisions above are pixel-exact at every
// callback, so finer thresholds only make tall cards (whose intersection ratio
// stays far below 1) update their state at a finer scroll granularity.
const THRESHOLDS = Array.from({ length: 21 }, (_, i) => i / 20);

function start(state) {
    if (state.observer) {
        return;
    }
    state.observer = new IntersectionObserver(
        (entries) => onIntersection(state, entries),
        { threshold: THRESHOLDS });
    // The observer fires an initial callback with the current ratio,
    // so the class is set correctly right when the card appears.
    state.observer.observe(state.element);
}

function stop(state) {
    if (state.observer) {
        state.observer.disconnect();
        state.observer = null;
    }
    state.raised = false;
    state.element.classList.remove(RAISED_CLASS);
}

export function initializeFloatingCard(element) {
    if (states.has(element)) {
        return;
    }

    const state = {
        element: element,
        observer: null,
        raised: false,
        mediaQuery: window.matchMedia(MOBILE_QUERY),
        onModeChange: null
    };

    state.onModeChange = () => {
        if (state.mediaQuery.matches) {
            start(state);
        } else {
            stop(state);
        }
    };

    state.mediaQuery.addEventListener('change', state.onModeChange);
    state.onModeChange();

    states.set(element, state);
}

export function disposeFloatingCard(element) {
    const state = states.get(element);
    if (!state) {
        return;
    }
    state.mediaQuery.removeEventListener('change', state.onModeChange);
    stop(state);
    states.delete(element);
}
