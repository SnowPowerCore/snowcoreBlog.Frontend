// Scrolls the page back to the top with a fast, eased animation.
//
// The app layout scrolls an inner container (.root-layout-item) instead of
// the window, so we first resolve which element actually scrolls. Each frame
// writes scrollTop/scrollTo with behavior: 'instant' to bypass the CSS
// `scroll-behavior: smooth` set by bootstrap-reboot, otherwise every
// programmatic scroll would itself animate and the motion would lag.

function findScroller() {
  const doc = document.scrollingElement || document.documentElement;
  if (doc.scrollHeight > doc.clientHeight) {
    return null; // null = the window/document scrolls
  }

  let best = null;
  let bestOverflow = 0;
  for (const el of document.querySelectorAll('*')) {
    const cs = getComputedStyle(el);
    if (/(auto|scroll)/.test(cs.overflowY)) {
      const overflow = el.scrollHeight - el.clientHeight;
      if (overflow > bestOverflow) {
        best = el;
        bestOverflow = overflow;
      }
    }
  }
  return best;
}

export function scrollToTop() {
  if (window.matchMedia('(prefers-reduced-motion: reduce)').matches) {
    const el = findScroller();
    if (el) {
      el.scrollTo({ top: 0, behavior: 'instant' });
    } else {
      window.scrollTo({ top: 0, behavior: 'instant' });
    }
    return;
  }

  const scroller = findScroller();
  const startY = scroller ? scroller.scrollTop : window.scrollY;

  // Speedy: duration proportional to distance, hard-capped.
  const duration = Math.min(450, Math.max(150, startY / 3));
  const start = performance.now();
  let cancelled = false;

  const cancel = () => { cancelled = true; };
  // wheel/touchstart bubble up from the inner scroller; keydown lands on window.
  window.addEventListener('wheel', cancel, { passive: true, once: true });
  window.addEventListener('touchstart', cancel, { passive: true, once: true });
  window.addEventListener('keydown', cancel, { once: true });

  // easeOutQuart: fast departure, gentle arrival.
  const easeOutQuart = t => 1 - Math.pow(1 - t, 4);

  const step = now => {
    if (cancelled) {
      return;
    }
    const t = Math.min(1, (now - start) / duration);
    const y = Math.round(startY * (1 - easeOutQuart(t)));
    if (scroller) {
      scroller.scrollTo({ top: y, behavior: 'instant' });
    } else {
      window.scrollTo({ top: y, behavior: 'instant' });
    }
    if (t < 1) {
      requestAnimationFrame(step);
    }
  };

  requestAnimationFrame(step);
}
