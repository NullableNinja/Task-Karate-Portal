/* ============================================
   NAVIGATION CONTROLLER
   Version: 1.0.0 - TaskKarate v4
   
   ╔════════════════════════════════════════════╗
   ║  MOBILE NAVIGATION & LOGO SPIN             ║
   ╠════════════════════════════════════════════╣
   ║  Controls:                                 ║
   ║  • Hamburger menu toggle (mobile)          ║
   ║  • Logo spin animation on click            ║
   ║  • Escape key to close menu                ║
   ╚════════════════════════════════════════════╝
   
   📁 Related CSS: css/paper-fu-navigation.css
   📁 Related HTML: partials/navigation.html
   ============================================ */


/**
 * NavigationController
 * ─────────────────────────────────────────────
 * Handles all navigation-related functionality.
 * Waits for partials to load before initializing.
 */
class NavigationController {
  
  constructor() {
    // Wait for navigation partial to be loaded
    document.addEventListener('partialsLoaded', () => this.init());
  }
  
  
  /**
   * init
   * ─────────────────────────────────────────────
   * Initialize navigation after partials are loaded.
   */
  init() {
    // Get DOM elements
    this.hamburger = document.querySelector('.hamburger');
    this.navLinks = document.querySelector('.nav-links');
    this.logo = document.querySelector('.tk-logo-img');
    
    if (!this.hamburger || !this.navLinks) {
      console.warn('Navigation: Required elements not found');
      return;
    }
    
    // Bind event handlers
    this.bindEvents();
    
    console.log('Navigation: Initialized');
  }
  
  
  /**
   * bindEvents
   * ─────────────────────────────────────────────
   * Attach all event listeners.
   */
  bindEvents() {
    // Hamburger click
    this.hamburger.addEventListener('click', () => this.toggleMenu());
    
    // Close menu when clicking a link
    this.navLinks.querySelectorAll('.nav-link').forEach(link => {
      link.addEventListener('click', () => this.closeMenu());
    });
    
    // Escape key closes menu
    document.addEventListener('keydown', (e) => {
      if (e.key === 'Escape') this.closeMenu();
    });
    
    // Click outside closes menu
    document.addEventListener('click', (e) => {
      if (!this.hamburger.contains(e.target) && !this.navLinks.contains(e.target)) {
        this.closeMenu();
      }
    });
    
    // Logo spin on click
    if (this.logo) {
      this.logo.addEventListener('click', () => this.spinLogo());
    }
  }
  
  
  /**
   * toggleMenu
   * ─────────────────────────────────────────────
   * Toggle mobile menu open/closed state.
   */
  toggleMenu() {
    const isOpen = this.navLinks.classList.toggle('open');
    this.hamburger.classList.toggle('open', isOpen);
    this.hamburger.setAttribute('aria-expanded', isOpen);
  }
  
  
  /**
   * closeMenu
   * ─────────────────────────────────────────────
   * Close the mobile menu.
   */
  closeMenu() {
    this.navLinks.classList.remove('open');
    this.hamburger.classList.remove('open');
    this.hamburger.setAttribute('aria-expanded', 'false');
  }
  
  
  /**
   * spinLogo
   * ─────────────────────────────────────────────
   * Add spin animation to logo when clicked.
   */
  spinLogo() {
    // Don't start new animation if one is running
    if (this.logo.classList.contains('spinning')) return;
    
    this.logo.classList.add('spinning');
    
    // Remove class after animation completes
    setTimeout(() => {
      this.logo.classList.remove('spinning');
    }, 1200); // Match CSS animation duration
  }
}


// Create instance
new NavigationController();


/* ============================================
   END OF NAVIGATION CONTROLLER
   ============================================ */
