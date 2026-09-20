/* ============================================
   BACK TO TOP BUTTON
   Version: 1.0.0 - TaskKarate v4
   
   ╔════════════════════════════════════════════╗
   ║  SCROLL TO TOP FUNCTIONALITY               ║
   ╠════════════════════════════════════════════╣
   ║  Features:                                 ║
   ║  • Shows button after scrolling down       ║
   ║  • Smooth scroll to top on click           ║
   ║  • Accessible keyboard support             ║
   ╚════════════════════════════════════════════╝
   
   📁 Related CSS: css/paper-fu-core.css
   ============================================ */


/**
 * BackToTopController
 * ─────────────────────────────────────────────
 * Manages the back-to-top button visibility
 * and scroll functionality.
 */
class BackToTopController {
  
  constructor() {
    // Configuration
    this.showThreshold = 300; // Pixels scrolled before showing button
    
    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
      document.addEventListener('DOMContentLoaded', () => this.init());
    } else {
      this.init();
    }
  }
  
  
  /**
   * init
   * ─────────────────────────────────────────────
   * Find or create the button and bind events.
   */
  init() {
    // Look for existing button
    this.button = document.querySelector('.back-to-top');
    
    // If no button exists, create one
    if (!this.button) {
      this.createButton();
    }
    
    // Bind events
    this.bindEvents();
    
    // Initial visibility check
    this.updateVisibility();
    
    console.log('BackToTop: Initialized');
  }
  
  
  /**
   * createButton
   * ─────────────────────────────────────────────
   * Create the back-to-top button dynamically.
   */
  createButton() {
    this.button = document.createElement('button');
    this.button.className = 'back-to-top';
    this.button.setAttribute('aria-label', 'Back to top');
    this.button.setAttribute('title', 'Back to top');
    this.button.innerHTML = `
      <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
        <path d="M18 15l-6-6-6 6"/>
      </svg>
    `;
    
    document.body.appendChild(this.button);
  }
  
  
  /**
   * bindEvents
   * ─────────────────────────────────────────────
   * Attach event listeners.
   */
  bindEvents() {
    // Click to scroll
    this.button.addEventListener('click', () => this.scrollToTop());
    
    // Keyboard support
    this.button.addEventListener('keydown', (e) => {
      if (e.key === 'Enter' || e.key === ' ') {
        e.preventDefault();
        this.scrollToTop();
      }
    });
    
    // Scroll event with throttling
    let ticking = false;
    window.addEventListener('scroll', () => {
      if (!ticking) {
        window.requestAnimationFrame(() => {
          this.updateVisibility();
          ticking = false;
        });
        ticking = true;
      }
    });
  }
  
  
  /**
   * updateVisibility
   * ─────────────────────────────────────────────
   * Show or hide button based on scroll position.
   */
  updateVisibility() {
    if (window.scrollY > this.showThreshold) {
      this.button.classList.add('visible');
      this.button.setAttribute('aria-hidden', 'false');
    } else {
      this.button.classList.remove('visible');
      this.button.setAttribute('aria-hidden', 'true');
    }
  }
  
  
  /**
   * scrollToTop
   * ─────────────────────────────────────────────
   * Smooth scroll to the top of the page.
   */
  scrollToTop() {
    window.scrollTo({
      top: 0,
      behavior: 'smooth'
    });
    
    // Focus on first focusable element for accessibility
    setTimeout(() => {
      const firstFocusable = document.querySelector('a, button, input, [tabindex]:not([tabindex="-1"])');
      if (firstFocusable) {
        firstFocusable.focus({ preventScroll: true });
      }
    }, 500);
  }
}


// Create instance
new BackToTopController();


/* ============================================
   BACK TO TOP BUTTON STYLES (injected CSS)
   ─────────────────────────────────────────────
   These styles are injected if the button is
   dynamically created. Override in your CSS.
   ============================================ */

(function injectBackToTopStyles() {
  // Check if styles already exist
  if (document.getElementById('back-to-top-styles')) return;
  
  const styles = document.createElement('style');
  styles.id = 'back-to-top-styles';
  styles.textContent = `
    .back-to-top {
      position: fixed;
      bottom: 2rem;
      right: 2rem;
      width: 48px;
      height: 48px;
      border-radius: 50%;
      background: var(--pf-color-accent, #0f4c81);
      color: white;
      border: none;
      cursor: pointer;
      display: flex;
      align-items: center;
      justify-content: center;
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.3);
      opacity: 0;
      visibility: hidden;
      transform: translateY(20px);
      transition: opacity 0.3s ease, visibility 0.3s ease, transform 0.3s ease;
      z-index: 1000;
    }
    
    .back-to-top.visible {
      opacity: 1;
      visibility: visible;
      transform: translateY(0);
    }
    
    .back-to-top:hover {
      background: var(--pf-color-accent-light, #1a5fa3);
      transform: translateY(-4px);
    }
    
    .back-to-top:focus {
      outline: 2px solid white;
      outline-offset: 2px;
    }
    
    .back-to-top svg {
      width: 24px;
      height: 24px;
    }
    
    @media (max-width: 768px) {
      .back-to-top {
        bottom: 1rem;
        right: 1rem;
        width: 40px;
        height: 40px;
      }
      
      .back-to-top svg {
        width: 20px;
        height: 20px;
      }
    }
  `;
  
  document.head.appendChild(styles);
})();


/* ============================================
   END OF BACK TO TOP CONTROLLER
   ============================================ */
