/**
 * Page Transition Animations
 * ──────────────────────────────────────────────
 * Smooth transitions between page navigation
 * with Paper-Fu slide effect.
 * 
 * Features:
 * - Fade out + slide effect on navigation
 * - Maintains scroll position on back button
 * - Preloads next page on hover
 * - Loading indicator for slow connections
 * - Fallback for browsers without support
 * - Respects reduced motion preference
 * 
 * @version 1.0.0
 * @file js/page-transitions.js
 */

(function() {
  'use strict';
  
  // Configuration
  const config = {
    // Transition duration (ms)
    duration: 400,
    
    // Enable preloading on link hover
    enablePreload: true,
    
    // Show loading indicator after this delay (ms)
    loadingDelay: 500,
    
    // Class names
    classes: {
      transitioning: 'pf-page-transitioning',
      fadeOut: 'pf-page-fade-out',
      loading: 'pf-page-loading'
    }
  };
  
  // Check for reduced motion preference
  const prefersReducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
  
  // Track if a transition is in progress
  let isTransitioning = false;
  
  // Preload cache
  const preloadCache = new Set();
  
  /**
   * Get all internal navigation links
   * @returns {NodeList} - Internal links
   */
  function getInternalLinks() {
    return document.querySelectorAll('a[href^="/"]:not([target="_blank"]), a[href^="./"]:not([target="_blank"]), a[href$=".html"]:not([target="_blank"])');
  }
  
  /**
   * Check if a link is internal
   * @param {string} href - The href to check
   * @returns {boolean} - True if internal
   */
  function isInternalLink(href) {
    if (!href) return false;
    
    // Check if it's a relative path or same origin
    try {
      const url = new URL(href, window.location.origin);
      return url.origin === window.location.origin;
    } catch {
      // If URL constructor fails, assume internal if it doesn't start with http
      return !href.startsWith('http') && !href.startsWith('//');
    }
  }
  
  /**
   * Preload a page
   * @param {string} href - The URL to preload
   */
  function preloadPage(href) {
    if (!config.enablePreload || preloadCache.has(href)) {
      return;
    }
    
    // Create a link element for preloading
    const link = document.createElement('link');
    link.rel = 'prefetch';
    link.href = href;
    document.head.appendChild(link);
    
    // Add to cache
    preloadCache.add(href);
  }
  
  /**
   * Show loading indicator
   */
  function showLoading() {
    document.body.classList.add(config.classes.loading);
  }
  
  /**
   * Hide loading indicator
   */
  function hideLoading() {
    document.body.classList.remove(config.classes.loading);
  }
  
  /**
   * Start page transition animation
   * @param {Function} callback - Called when animation completes
   */
  function startTransition(callback) {
    if (prefersReducedMotion) {
      // No animation, just call callback immediately
      if (callback) callback();
      return;
    }
    
    // Add transitioning class
    document.body.classList.add(config.classes.transitioning);
    document.body.classList.add(config.classes.fadeOut);
    
    // Call callback after animation duration
    setTimeout(() => {
      if (callback) callback();
    }, config.duration);
  }
  
  /**
   * Handle navigation click
   * @param {Event} e - Click event
   */
  function handleNavigation(e) {
    // Get the clicked link
    const link = e.target.closest('a');
    
    if (!link) return;
    
    const href = link.getAttribute('href');
    
    // Check if it's an internal navigation link
    if (!isInternalLink(href)) {
      return;
    }
    
    // Ignore if already transitioning
    if (isTransitioning) {
      e.preventDefault();
      return;
    }
    
    // Prevent default navigation
    e.preventDefault();
    
    // Mark as transitioning
    isTransitioning = true;
    
    // Set loading timeout
    const loadingTimeout = setTimeout(showLoading, config.loadingDelay);
    
    // Save current scroll position
    sessionStorage.setItem('scrollPosition', window.scrollY.toString());
    
    // Start transition animation
    startTransition(() => {
      // Clear loading timeout
      clearTimeout(loadingTimeout);
      
      // Navigate to new page
      window.location.href = href;
    });
  }
  
  /**
   * Restore scroll position on page load
   */
  function restoreScrollPosition() {
    // Check if we're coming from a back/forward navigation
    if (window.performance && window.performance.navigation.type === 2) {
      const savedPosition = sessionStorage.getItem('scrollPosition');
      
      if (savedPosition) {
        // Wait for page to fully load
        window.addEventListener('load', () => {
          window.scrollTo(0, parseInt(savedPosition, 10));
          sessionStorage.removeItem('scrollPosition');
        });
      }
    }
  }
  
  /**
   * Fade in page on load
   */
  function fadeInPage() {
    if (prefersReducedMotion) {
      return;
    }
    
    // Remove transitioning classes if they exist
    document.body.classList.remove(config.classes.transitioning);
    document.body.classList.remove(config.classes.fadeOut);
    document.body.classList.remove(config.classes.loading);
    
    // Reset transitioning flag
    isTransitioning = false;
  }
  
  /**
   * Handle link hover for preloading
   * @param {Event} e - Mouseenter event
   */
  function handleLinkHover(e) {
    const link = e.target.closest('a');
    
    if (!link) return;
    
    const href = link.getAttribute('href');
    
    if (isInternalLink(href)) {
      preloadPage(href);
    }
  }
  
  /**
   * Initialize page transitions
   */
  function init() {
    // Restore scroll position if needed
    restoreScrollPosition();
    
    // Fade in the page
    fadeInPage();
    
    // Add click handlers to internal links
    document.addEventListener('click', handleNavigation);
    
    // Add hover handlers for preloading
    if (config.enablePreload) {
      document.addEventListener('mouseenter', handleLinkHover, true);
    }
    
    // Handle popstate (back/forward buttons)
    window.addEventListener('popstate', () => {
      // Allow browser's native back/forward navigation
      // Just fade in when we arrive
      fadeInPage();
    });
  }
  
  // Initialize when DOM is ready
  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', init);
  } else {
    init();
  }
  
  // Expose API
  window.PageTransitions = {
    navigate: function(href) {
      if (isInternalLink(href)) {
        isTransitioning = true;
        startTransition(() => {
          window.location.href = href;
        });
      }
    }
  };
  
})();
