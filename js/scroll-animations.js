/* ============================================
   SCROLL ANIMATIONS
   Version: 1.0.0 - TaskKarate v4
   
   ╔════════════════════════════════════════════╗
   ║  SCROLL-TRIGGERED ANIMATIONS               ║
   ╠════════════════════════════════════════════╣
   ║  Brings content to life as user scrolls.   ║
   ║                                            ║
   ║  Features:                                 ║
   ║  • Intersection Observer API               ║
   ║  • Multiple animation types                ║
   ║  • Stagger delays for lists                ║
   ║  • Respects prefers-reduced-motion         ║
   ║  • Mobile-friendly performance             ║
   ╚════════════════════════════════════════════╝
   
   📁 Related CSS: paper-fu-core.css (animation utilities)
   ============================================ */


/**
 * ScrollAnimationController
 * ─────────────────────────────────────────────
 * Manages scroll-triggered reveal animations.
 * 
 * USAGE:
 * Add data-animate attribute to elements:
 * 
 * <div data-animate="fade-up">Content</div>
 * <div data-animate="fade-left" data-delay="200">Content</div>
 * <div data-animate="scale" data-stagger="100">Content</div>
 */
class ScrollAnimationController {
  
  constructor(options = {}) {
    // Configuration
    this.threshold = options.threshold || 0.15; // 15% visible before animating
    this.rootMargin = options.rootMargin || '0px 0px -50px 0px';
    this.animateOnce = options.animateOnce !== false; // Default: animate only once
    
    // Check for reduced motion preference
    this.prefersReducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    
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
   * Set up Intersection Observer and find elements.
   */
  init() {
    // Skip animations if user prefers reduced motion
    if (this.prefersReducedMotion) {
      console.log('ScrollAnimations: Reduced motion detected, animations disabled');
      this.showAllElements();
      return;
    }
    
    // Find all elements with data-animate
    this.elements = document.querySelectorAll('[data-animate]');
    
    if (this.elements.length === 0) {
      console.log('ScrollAnimations: No animated elements found');
      return;
    }
    
    // Create Intersection Observer
    this.observer = new IntersectionObserver(
      (entries) => this.handleIntersection(entries),
      {
        threshold: this.threshold,
        rootMargin: this.rootMargin
      }
    );
    
    // Observe all elements
    this.elements.forEach((el, index) => {
      // Add initial hidden state
      el.classList.add('scroll-animate-hidden');
      
      // Handle stagger delays
      if (el.hasAttribute('data-stagger')) {
        const stagger = parseInt(el.getAttribute('data-stagger'));
        const delay = stagger * index;
        el.style.transitionDelay = `${delay}ms`;
      } else if (el.hasAttribute('data-delay')) {
        const delay = parseInt(el.getAttribute('data-delay'));
        el.style.transitionDelay = `${delay}ms`;
      }
      
      // Start observing
      this.observer.observe(el);
    });
    
    console.log(`ScrollAnimations: Observing ${this.elements.length} elements`);
  }
  
  
  /**
   * handleIntersection
   * ─────────────────────────────────────────────
   * Called when elements enter/exit viewport.
   */
  handleIntersection(entries) {
    entries.forEach(entry => {
      if (entry.isIntersecting) {
        // Element is visible - animate it
        this.animateElement(entry.target);
        
        // Stop observing if animateOnce is true
        if (this.animateOnce) {
          this.observer.unobserve(entry.target);
        }
      } else if (!this.animateOnce) {
        // Element left viewport and we're repeating animations
        entry.target.classList.remove('scroll-animate-visible');
        entry.target.classList.add('scroll-animate-hidden');
      }
    });
  }
  
  
  /**
   * animateElement
   * ─────────────────────────────────────────────
   * Apply animation classes to element.
   */
  animateElement(element) {
    element.classList.remove('scroll-animate-hidden');
    element.classList.add('scroll-animate-visible');
    
    // Get animation type
    const animationType = element.getAttribute('data-animate');
    element.classList.add(`scroll-animate-${animationType}`);
  }
  
  
  /**
   * showAllElements
   * ─────────────────────────────────────────────
   * Immediately show all elements (reduced motion fallback).
   */
  showAllElements() {
    const elements = document.querySelectorAll('[data-animate]');
    elements.forEach(el => {
      el.classList.add('scroll-animate-visible');
      el.style.opacity = '1';
      el.style.transform = 'none';
    });
  }
  
  
  /**
   * destroy
   * ─────────────────────────────────────────────
   * Clean up observer (useful for SPA page changes).
   */
  destroy() {
    if (this.observer) {
      this.observer.disconnect();
    }
  }
}


// Initialize scroll animations
const scrollAnimations = new ScrollAnimationController({
  threshold: 0.15,
  rootMargin: '0px 0px -50px 0px',
  animateOnce: true
});


// Export for potential use in other scripts
if (typeof module !== 'undefined' && module.exports) {
  module.exports = ScrollAnimationController;
}
