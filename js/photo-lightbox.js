/* ============================================
   PHOTO LIGHTBOX
   Version: 1.0.0 - TaskKarate v4
   
   ╔════════════════════════════════════════════╗
   ║  PHOTO GALLERY LIGHTBOX                    ║
   ╠════════════════════════════════════════════╣
   ║  Full-screen image viewer with navigation. ║
   ║                                            ║
   ║  Features:                                 ║
   ║  • Click photos to open lightbox           ║
   ║  • Smooth zoom transitions                 ║
   ║  • Prev/Next navigation                    ║
   ║  • Keyboard controls (arrow keys, ESC)     ║
   ║  • Touch gestures (swipe on mobile)        ║
   ║  • Caption display                         ║
   ║  • Theme-compliant design                  ║
   ╚════════════════════════════════════════════╝
   
   📁 Related CSS: css/index.css
   ============================================ */


/**
 * PhotoLightboxController
 * ─────────────────────────────────────────────
 * Manages photo gallery lightbox functionality.
 * 
 * USAGE:
 * Automatically initializes for .photo-tile elements.
 * No manual setup required.
 */
class PhotoLightboxController {
  
  constructor() {
    this.currentIndex = 0;
    this.photos = [];
    this.touchStartX = 0;
    this.touchEndX = 0;
    
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
   * Find photos and create lightbox structure.
   */
  init() {
    // Find all photo tiles
    const photoTiles = document.querySelectorAll('.photo-tile');
    
    if (photoTiles.length === 0) {
      console.log('PhotoLightbox: No photos found');
      return;
    }
    
    // Extract photo data
    photoTiles.forEach((tile, index) => {
      const img = tile.querySelector('.photo-img');
      const caption = tile.querySelector('.photo-caption');
      
      if (img) {
        this.photos.push({
          src: img.src,
          alt: img.alt || '',
          caption: caption ? caption.textContent : ''
        });
        
        // Add click handler
        tile.style.cursor = 'pointer';
        tile.addEventListener('click', () => this.open(index));
      }
    });
    
    // Create lightbox element
    this.createLightbox();
    
    console.log(`PhotoLightbox: Initialized with ${this.photos.length} photos`);
  }
  
  
  /**
   * createLightbox
   * ─────────────────────────────────────────────
   * Build lightbox HTML and inject into page.
   */
  createLightbox() {
    const lightbox = document.createElement('div');
    lightbox.id = 'photo-lightbox';
    lightbox.className = 'photo-lightbox';
    lightbox.setAttribute('aria-hidden', 'true');
    lightbox.setAttribute('role', 'dialog');
    lightbox.setAttribute('aria-label', 'Photo viewer');
    
    lightbox.innerHTML = `
      <div class="photo-lightbox__backdrop"></div>
      <div class="photo-lightbox__container">
        <!-- Close Button -->
        <button 
          class="photo-lightbox__close" 
          aria-label="Close lightbox"
          id="lightbox-close"
        >
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5">
            <line x1="18" y1="6" x2="6" y2="18"></line>
            <line x1="6" y1="6" x2="18" y2="18"></line>
          </svg>
        </button>
        
        <!-- Previous Button -->
        <button 
          class="photo-lightbox__nav photo-lightbox__prev" 
          aria-label="Previous photo"
          id="lightbox-prev"
        >
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3">
            <polyline points="15 18 9 12 15 6"></polyline>
          </svg>
        </button>
        
        <!-- Next Button -->
        <button 
          class="photo-lightbox__nav photo-lightbox__next" 
          aria-label="Next photo"
          id="lightbox-next"
        >
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3">
            <polyline points="9 18 15 12 9 6"></polyline>
          </svg>
        </button>
        
        <!-- Image -->
        <div class="photo-lightbox__content">
          <img 
            id="lightbox-image" 
            class="photo-lightbox__image" 
            src="" 
            alt=""
          >
          <div class="photo-lightbox__caption" id="lightbox-caption"></div>
          <div class="photo-lightbox__counter" id="lightbox-counter"></div>
        </div>
      </div>
    `;
    
    document.body.appendChild(lightbox);
    
    // Cache elements
    this.lightbox = lightbox;
    this.image = document.getElementById('lightbox-image');
    this.caption = document.getElementById('lightbox-caption');
    this.counter = document.getElementById('lightbox-counter');
    this.closeBtn = document.getElementById('lightbox-close');
    this.prevBtn = document.getElementById('lightbox-prev');
    this.nextBtn = document.getElementById('lightbox-next');
    this.backdrop = lightbox.querySelector('.photo-lightbox__backdrop');
    
    // Bind events
    this.bindEvents();
  }
  
  
  /**
   * bindEvents
   * ─────────────────────────────────────────────
   * Attach event listeners.
   */
  bindEvents() {
    // Close button
    this.closeBtn.addEventListener('click', () => this.close());
    
    // Backdrop click
    this.backdrop.addEventListener('click', () => this.close());
    
    // Navigation buttons
    this.prevBtn.addEventListener('click', () => this.prev());
    this.nextBtn.addEventListener('click', () => this.next());
    
    // Keyboard controls
    document.addEventListener('keydown', (e) => this.handleKeyboard(e));
    
    // Touch gestures
    this.lightbox.addEventListener('touchstart', (e) => this.handleTouchStart(e));
    this.lightbox.addEventListener('touchend', (e) => this.handleTouchEnd(e));
  }
  
  
  /**
   * open
   * ─────────────────────────────────────────────
   * Open lightbox and display photo at index.
   */
  open(index) {
    this.currentIndex = index;
    this.showPhoto();
    
    this.lightbox.setAttribute('aria-hidden', 'false');
    document.body.style.overflow = 'hidden';
    
    // Focus close button for accessibility
    setTimeout(() => this.closeBtn.focus(), 100);
    
    console.log(`PhotoLightbox: Opened photo ${index + 1}/${this.photos.length}`);
  }
  
  
  /**
   * close
   * ─────────────────────────────────────────────
   * Close lightbox.
   */
  close() {
    this.lightbox.setAttribute('aria-hidden', 'true');
    document.body.style.overflow = '';
    
    console.log('PhotoLightbox: Closed');
  }
  
  
  /**
   * showPhoto
   * ─────────────────────────────────────────────
   * Display current photo.
   */
  showPhoto() {
    const photo = this.photos[this.currentIndex];
    
    // Update image
    this.image.src = photo.src;
    this.image.alt = photo.alt;
    
    // Update caption
    this.caption.textContent = photo.caption;
    
    // Update counter
    this.counter.textContent = `${this.currentIndex + 1} / ${this.photos.length}`;
    
    // Update navigation button states
    this.prevBtn.disabled = (this.currentIndex === 0);
    this.nextBtn.disabled = (this.currentIndex === this.photos.length - 1);
  }
  
  
  /**
   * next
   * ─────────────────────────────────────────────
   * Show next photo.
   */
  next() {
    if (this.currentIndex < this.photos.length - 1) {
      this.currentIndex++;
      this.showPhoto();
    }
  }
  
  
  /**
   * prev
   * ─────────────────────────────────────────────
   * Show previous photo.
   */
  prev() {
    if (this.currentIndex > 0) {
      this.currentIndex--;
      this.showPhoto();
    }
  }
  
  
  /**
   * handleKeyboard
   * ─────────────────────────────────────────────
   * Keyboard controls.
   */
  handleKeyboard(e) {
    if (this.lightbox.getAttribute('aria-hidden') === 'false') {
      switch(e.key) {
        case 'Escape':
          this.close();
          break;
        case 'ArrowLeft':
          this.prev();
          break;
        case 'ArrowRight':
          this.next();
          break;
      }
    }
  }
  
  
  /**
   * handleTouchStart
   * ─────────────────────────────────────────────
   * Track touch start position.
   */
  handleTouchStart(e) {
    this.touchStartX = e.changedTouches[0].screenX;
  }
  
  
  /**
   * handleTouchEnd
   * ─────────────────────────────────────────────
   * Detect swipe gestures.
   */
  handleTouchEnd(e) {
    this.touchEndX = e.changedTouches[0].screenX;
    this.handleSwipe();
  }
  
  
  /**
   * handleSwipe
   * ─────────────────────────────────────────────
   * Process swipe direction.
   */
  handleSwipe() {
    const swipeThreshold = 50; // Minimum swipe distance
    const diff = this.touchStartX - this.touchEndX;
    
    if (Math.abs(diff) > swipeThreshold) {
      if (diff > 0) {
        // Swiped left - next photo
        this.next();
      } else {
        // Swiped right - previous photo
        this.prev();
      }
    }
  }
}


// Initialize photo lightbox
new PhotoLightboxController();
