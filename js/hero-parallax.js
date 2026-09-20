/* ============================================
   HERO PARALLAX EFFECT
   Version: 1.0.0 - TaskKarate v4
   
   ╔════════════════════════════════════════════╗
   ║  SUBTLE PARALLAX SCROLLING                 ║
   ╠════════════════════════════════════════════╣
   ║  Gentle depth effect on hero section.      ║
   ║                                            ║
   ║  Features:                                 ║
   ║  • Subtle background movement              ║
   ║  • Keeps foreground static                 ║
   ║  • Smooth easing                           ║
   ║  • Disabled on mobile for performance      ║
   ║  • Respects reduced motion preference      ║
   ╚════════════════════════════════════════════╝
   
   📁 Related CSS: css/index.css
   ============================================ */


/**
 * HeroParallaxController
 * ─────────────────────────────────────────────
 * Creates subtle parallax effect on hero image.
 * 
 * CAUTIOUS IMPLEMENTATION:
 * - Only 30% movement speed (gentle)
 * - Only affects background image
 * - Disabled on mobile
 * - Smooth RAF for performance
 */
class HeroParallaxController {
  
  constructor() {
    // Configuration - VERY SUBTLE
    this.parallaxSpeed = 0.3; // 30% of scroll speed (gentle)
    this.isActive = false;
    this.ticking = false;
    this.scrollY = 0;
    
    // Check for reduced motion and mobile
    this.prefersReducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    this.isMobile = window.matchMedia('(max-width: 768px)').matches;
    
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
   * Find elements and set up parallax.
   */
  init() {
    // Skip if reduced motion or mobile
    if (this.prefersReducedMotion) {
      console.log('HeroParallax: Respecting reduced motion preference');
      return;
    }
    
    if (this.isMobile) {
      console.log('HeroParallax: Disabled on mobile for performance');
      return;
    }
    
    // Get hero elements
    this.hero = document.getElementById('hero');
    this.heroImage = this.hero?.querySelector('.hero-image');
    
    if (!this.hero || !this.heroImage) {
      console.log('HeroParallax: Hero not found');
      return;
    }
    
    // Bind events
    this.bindEvents();
    
    // Initial position
    this.update();
    
    this.isActive = true;
    console.log('HeroParallax: Initialized (subtle mode)');
  }
  
  
  /**
   * bindEvents
   * ─────────────────────────────────────────────
   * Attach scroll listener with RAF throttling.
   */
  bindEvents() {
    window.addEventListener('scroll', () => this.onScroll(), { passive: true });
    
    // Re-check on resize
    window.addEventListener('resize', () => {
      this.isMobile = window.matchMedia('(max-width: 768px)').matches;
      if (this.isMobile && this.isActive) {
        // Reset transform if switching to mobile
        this.heroImage.style.transform = 'translateY(0)';
        this.isActive = false;
      } else if (!this.isMobile && !this.isActive && !this.prefersReducedMotion) {
        this.isActive = true;
      }
    });
  }
  
  
  /**
   * onScroll
   * ─────────────────────────────────────────────
   * Request animation frame for smooth updates.
   */
  onScroll() {
    this.scrollY = window.scrollY;
    
    if (!this.ticking) {
      window.requestAnimationFrame(() => {
        this.update();
        this.ticking = false;
      });
      
      this.ticking = true;
    }
  }
  
  
  /**
   * update
   * ─────────────────────────────────────────────
   * Apply parallax transform.
   */
  update() {
    if (!this.isActive) return;
    
    // Get hero position
    const heroRect = this.hero.getBoundingClientRect();
    const heroBottom = heroRect.bottom;
    
    // Only apply parallax while hero is visible
    if (heroBottom > 0) {
      // Calculate offset (gentle, only 30% speed)
      const offset = this.scrollY * this.parallaxSpeed;
      
      // Apply transform with will-change for performance
      this.heroImage.style.willChange = 'transform';
      this.heroImage.style.transform = `translateY(${offset}px)`;
    } else {
      // Stop once scrolled past hero
      this.heroImage.style.willChange = 'auto';
    }
  }
}


// Initialize hero parallax
new HeroParallaxController();
