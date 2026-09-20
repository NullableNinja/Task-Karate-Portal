/* ============================================
   TESTIMONIALS CAROUSEL
   Version: 1.0.0 - TaskKarate v4
   
   ╔════════════════════════════════════════════╗
   ║  ROTATING TESTIMONIALS SLIDER              ║
   ╠════════════════════════════════════════════╣
   ║  Auto-rotating carousel with manual        ║
   ║  navigation controls.                      ║
   ║                                            ║
   ║  Features:                                 ║
   ║  • Auto-advance every 6 seconds            ║
   ║  • Pause on hover                          ║
   ║  • Arrow navigation                        ║
   ║  • Dot indicators                          ║
   ║  • Touch swipe gestures                    ║
   ║  • Keyboard controls                       ║
   ╚════════════════════════════════════════════╝
   
   📁 Related CSS: css/about.css
   ============================================ */


/**
 * TestimonialsCarouselController
 * ─────────────────────────────────────────────
 * Manages testimonials carousel functionality.
 */
class TestimonialsCarouselController {
  
  constructor() {
    // Configuration
    this.currentSlide = 0;
    this.totalSlides = 0;
    this.autoplayInterval = null;
    this.autoplayDelay = 6000; // 6 seconds
    this.isPaused = false;
    
    // Touch tracking
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
   * Find elements and start carousel.
   */
  init() {
    // Get elements
    this.carousel = document.getElementById('testimonials-carousel');
    this.track = document.getElementById('testimonials-track');
    this.slides = this.track?.querySelectorAll('.testimonial-card');
    this.dotsContainer = document.getElementById('testimonials-dots');
    this.dots = this.dotsContainer?.querySelectorAll('.testimonials-dot');
    this.prevBtn = document.getElementById('testimonials-prev');
    this.nextBtn = document.getElementById('testimonials-next');
    
    if (!this.carousel || !this.track || this.slides.length === 0) {
      console.log('TestimonialsCarousel: Not found on this page');
      return;
    }
    
    this.totalSlides = this.slides.length;
    
    // Bind events
    this.bindEvents();
    
    // Start autoplay
    this.startAutoplay();
    
    // Initial display
    this.goToSlide(0);
    
    console.log(`TestimonialsCarousel: Initialized with ${this.totalSlides} slides`);
  }
  
  
  /**
   * bindEvents
   * ─────────────────────────────────────────────
   * Attach event listeners.
   */
  bindEvents() {
    // Arrow buttons
    if (this.prevBtn) {
      this.prevBtn.addEventListener('click', () => this.prev());
    }
    
    if (this.nextBtn) {
      this.nextBtn.addEventListener('click', () => this.next());
    }
    
    // Dot indicators
    this.dots.forEach(dot => {
      dot.addEventListener('click', () => {
        const slideIndex = parseInt(dot.getAttribute('data-slide'));
        this.goToSlide(slideIndex);
      });
    });
    
    // Pause on hover
    this.carousel.addEventListener('mouseenter', () => this.pauseAutoplay());
    this.carousel.addEventListener('mouseleave', () => this.resumeAutoplay());
    
    // Touch gestures
    this.track.addEventListener('touchstart', (e) => this.handleTouchStart(e));
    this.track.addEventListener('touchend', (e) => this.handleTouchEnd(e));
    
    // Keyboard navigation
    document.addEventListener('keydown', (e) => this.handleKeyboard(e));
  }
  
  
  /**
   * goToSlide
   * ─────────────────────────────────────────────
   * Display specific slide.
   */
  goToSlide(index) {
    this.currentSlide = index;
    
    // Update track position
    const offset = -index * 100;
    this.track.style.transform = `translateX(${offset}%)`;
    
    // Update dots
    this.dots.forEach((dot, i) => {
      if (i === index) {
        dot.classList.add('active');
      } else {
        dot.classList.remove('active');
      }
    });
    
    // Update arrows
    if (this.prevBtn) {
      this.prevBtn.disabled = (index === 0);
    }
    if (this.nextBtn) {
      this.nextBtn.disabled = (index === this.totalSlides - 1);
    }
  }
  
  
  /**
   * next
   * ─────────────────────────────────────────────
   * Go to next slide.
   */
  next() {
    let nextIndex = this.currentSlide + 1;
    
    // Loop back to start
    if (nextIndex >= this.totalSlides) {
      nextIndex = 0;
    }
    
    this.goToSlide(nextIndex);
  }
  
  
  /**
   * prev
   * ─────────────────────────────────────────────
   * Go to previous slide.
   */
  prev() {
    let prevIndex = this.currentSlide - 1;
    
    // Loop to end
    if (prevIndex < 0) {
      prevIndex = this.totalSlides - 1;
    }
    
    this.goToSlide(prevIndex);
  }
  
  
  /**
   * startAutoplay
   * ─────────────────────────────────────────────
   * Begin automatic slide rotation.
   */
  startAutoplay() {
    this.autoplayInterval = setInterval(() => {
      if (!this.isPaused) {
        this.next();
      }
    }, this.autoplayDelay);
  }
  
  
  /**
   * pauseAutoplay
   * ─────────────────────────────────────────────
   * Pause automatic rotation.
   */
  pauseAutoplay() {
    this.isPaused = true;
  }
  
  
  /**
   * resumeAutoplay
   * ─────────────────────────────────────────────
   * Resume automatic rotation.
   */
  resumeAutoplay() {
    this.isPaused = false;
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
    const swipeThreshold = 50;
    const diff = this.touchStartX - this.touchEndX;
    
    if (Math.abs(diff) > swipeThreshold) {
      if (diff > 0) {
        // Swiped left - next
        this.next();
      } else {
        // Swiped right - prev
        this.prev();
      }
    }
  }
  
  
  /**
   * handleKeyboard
   * ─────────────────────────────────────────────
   * Keyboard controls for carousel.
   */
  handleKeyboard(e) {
    // Only respond if carousel is visible
    const carouselRect = this.carousel.getBoundingClientRect();
    const isVisible = (
      carouselRect.top < window.innerHeight &&
      carouselRect.bottom > 0
    );
    
    if (isVisible) {
      switch(e.key) {
        case 'ArrowLeft':
          this.prev();
          break;
        case 'ArrowRight':
          this.next();
          break;
      }
    }
  }
}


// Initialize testimonials carousel
new TestimonialsCarouselController();
