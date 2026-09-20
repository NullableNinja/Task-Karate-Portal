/**
 * Progressive Image Loading
 * ──────────────────────────────────────────────
 * Implements LQIP (Low Quality Image Placeholder)
 * strategy for smooth image loading experience.
 * 
 * Features:
 * - Blur-up effect (placeholder → full image)
 * - Intersection Observer for lazy loading
 * - Automatic fallback for browsers without support
 * - Respects user's data-saver preferences
 * - Handles loading errors gracefully
 * 
 * Usage:
 * Add data-progressive attribute to img elements:
 * <img data-progressive="path/to/full.jpg" 
 *      src="path/to/placeholder-tiny.jpg" 
 *      alt="Description">
 * 
 * @version 1.0.0
 * @file js/progressive-images.js
 */

(function() {
  'use strict';
  
  // Configuration
  const config = {
    // Root margin for Intersection Observer
    // Load images 200px before they enter viewport
    rootMargin: '200px 0px',
    
    // Intersection ratio threshold
    threshold: 0.01,
    
    // Blur-up transition duration (ms)
    transitionDuration: 600,
    
    // Class names
    classes: {
      loading: 'pf-img-loading',
      loaded: 'pf-img-loaded',
      error: 'pf-img-error'
    }
  };
  
  // Check if browser supports Intersection Observer
  const hasIntersectionObserver = 'IntersectionObserver' in window;
  
  // Check if user prefers reduced data usage
  const prefersReducedData = navigator.connection && 
    (navigator.connection.saveData || navigator.connection.effectiveType === 'slow-2g');
  
  /**
   * Load the full-quality image
   * @param {HTMLImageElement} img - The image element
   */
  function loadImage(img) {
    const fullSrc = img.dataset.progressive;
    
    if (!fullSrc || img.classList.contains(config.classes.loaded)) {
      return;
    }
    
    // Add loading class
    img.classList.add(config.classes.loading);
    
    // Create a new image to preload
    const fullImage = new Image();
    
    // Success handler
    fullImage.onload = function() {
      // Update src to full image
      img.src = fullSrc;
      
      // Remove loading, add loaded class
      img.classList.remove(config.classes.loading);
      img.classList.add(config.classes.loaded);
      
      // Remove data attribute to prevent re-loading
      delete img.dataset.progressive;
      
      // Dispatch custom event for potential analytics
      img.dispatchEvent(new CustomEvent('imageLoaded', {
        detail: { src: fullSrc }
      }));
    };
    
    // Error handler
    fullImage.onerror = function() {
      img.classList.remove(config.classes.loading);
      img.classList.add(config.classes.error);
      
      console.warn('Failed to load progressive image:', fullSrc);
      
      // Dispatch error event
      img.dispatchEvent(new CustomEvent('imageError', {
        detail: { src: fullSrc }
      }));
    };
    
    // Start loading the full image
    fullImage.src = fullSrc;
  }
  
  /**
   * Handle image intersection
   * @param {IntersectionObserverEntry[]} entries - Observer entries
   * @param {IntersectionObserver} observer - The observer instance
   */
  function handleIntersection(entries, observer) {
    entries.forEach(entry => {
      if (entry.isIntersecting) {
        const img = entry.target;
        
        // Load the image
        loadImage(img);
        
        // Stop observing this image
        observer.unobserve(img);
      }
    });
  }
  
  /**
   * Initialize progressive loading for all images
   */
  function init() {
    // Get all images with data-progressive attribute
    const images = document.querySelectorAll('img[data-progressive]');
    
    if (images.length === 0) {
      return;
    }
    
    // If user prefers reduced data, don't auto-load
    // Just add a class so they can be loaded on demand
    if (prefersReducedData) {
      images.forEach(img => {
        img.classList.add('pf-img-data-saver');
        
        // Add click handler to load on demand
        img.addEventListener('click', () => loadImage(img), { once: true });
      });
      return;
    }
    
    // If browser supports Intersection Observer, use it
    if (hasIntersectionObserver) {
      const observer = new IntersectionObserver(handleIntersection, {
        rootMargin: config.rootMargin,
        threshold: config.threshold
      });
      
      // Observe each image
      images.forEach(img => observer.observe(img));
    } else {
      // Fallback: Load all images immediately (no lazy loading)
      images.forEach(img => loadImage(img));
    }
  }
  
  /**
   * Add a new image to progressive loading
   * Useful for dynamically added images
   * @param {HTMLImageElement} img - The image element
   */
  function observeImage(img) {
    if (!img.dataset.progressive) {
      return;
    }
    
    if (hasIntersectionObserver && !prefersReducedData) {
      const observer = new IntersectionObserver(handleIntersection, {
        rootMargin: config.rootMargin,
        threshold: config.threshold
      });
      observer.observe(img);
    } else {
      loadImage(img);
    }
  }
  
  // Initialize when DOM is ready
  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', init);
  } else {
    init();
  }
  
  // Expose API for dynamic images
  window.ProgressiveImages = {
    observe: observeImage,
    load: loadImage
  };
  
})();
