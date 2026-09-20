/* ============================================
   SCROLL PROGRESS BAR
   Version: 1.0.0 - TaskKarate v4
   
   ╔════════════════════════════════════════════╗
   ║  BELT PROGRESS INDICATOR                   ║
   ╠════════════════════════════════════════════╣
   ║  Features:                                 ║
   ║  • Fills progress bar on scroll            ║
   ║  • Colors change through belt ranks        ║
   ║  • Uses CSS custom properties              ║
   ╚════════════════════════════════════════════╝
   
   Belt Progression (0% → 100%):
   ┌───────────────────────────────────────────┐
   │  0-11%   │ White  │ Beginning             │
   │  11-22%  │ Gold   │ Discovery             │
   │  22-33%  │ Orange │ Growth                │
   │  33-44%  │ Green  │ Balance               │
   │  44-56%  │ Purple │ Wisdom                │
   │  56-67%  │ Blue   │ Skill                 │
   │  67-78%  │ Red    │ Power                 │
   │  78-89%  │ Brown  │ Mastery               │
   │  89-100% │ Black  │ Expertise             │
   └───────────────────────────────────────────┘
   
   📁 Related CSS: css/paper-fu-navigation.css
   📁 Related HTML: partials/navigation.html
   ============================================ */


/**
 * ScrollProgressController
 * ─────────────────────────────────────────────
 * Updates the scroll progress bar width and color.
 */
class ScrollProgressController {
  
  constructor() {
    // Belt colors in progression order
    // These match CSS custom properties in paper-fu-theme-hi-yah.css
    this.beltColors = [
      { threshold: 0,    color: '#f8fafc', name: 'white'  },
      { threshold: 11,   color: '#d4a84b', name: 'gold'   },
      { threshold: 22,   color: '#f97316', name: 'orange' },
      { threshold: 33,   color: '#22c55e', name: 'green'  },
      { threshold: 44,   color: '#8b5cf6', name: 'purple' },
      { threshold: 56,   color: '#3b82f6', name: 'blue'   },
      { threshold: 67,   color: '#ef4444', name: 'red'    },
      { threshold: 78,   color: '#78350f', name: 'brown'  },
      { threshold: 89,   color: '#0a0a0a', name: 'black'  }
    ];
    
    // Initialize when DOM is ready (don't wait for partials)
    if (document.readyState === 'loading') {
      document.addEventListener('DOMContentLoaded', () => this.init());
    } else {
      this.init();
    }
    
    // Also re-init if partials load later and bring the element
    document.addEventListener('partialsLoaded', () => {
      if (!this.progressBar) this.init();
    });
  }
  
  
  /**
   * init
   * ─────────────────────────────────────────────
   * Initialize progress bar after partials load.
   */
  init() {
    // Try multiple selectors to find the progress bar
    this.progressBar = document.querySelector('.scroll-progress') 
                    || document.querySelector('#scrollProgress')
                    || document.querySelector('.navbar-progress');
    
    // If no progress bar exists, create one
    if (!this.progressBar) {
      this.createProgressBar();
    }
    
    // Bind scroll event (with throttling)
    let ticking = false;
    window.addEventListener('scroll', () => {
      if (!ticking) {
        window.requestAnimationFrame(() => {
          this.updateProgress();
          ticking = false;
        });
        ticking = true;
      }
    });
    
    // Initial update
    this.updateProgress();
    
    console.log('ScrollProgress: Initialized');
  }
  
  
  /**
   * createProgressBar
   * ─────────────────────────────────────────────
   * Create the scroll progress bar dynamically.
   */
  createProgressBar() {
    this.progressBar = document.createElement('div');
    this.progressBar.className = 'navbar-progress';
    this.progressBar.id = 'scrollProgress';
    
    // Insert after the navbar (at bottom of viewport)
    document.body.appendChild(this.progressBar);
    
    console.log('ScrollProgress: Created progress bar');
  }
  
  
  /**
   * updateProgress
   * ─────────────────────────────────────────────
   * Calculate scroll percentage and update bar.
   */
  updateProgress() {
    // Calculate scroll percentage
    const scrollTop = window.scrollY;
    const docHeight = document.documentElement.scrollHeight - window.innerHeight;
    const scrollPercent = docHeight > 0 ? (scrollTop / docHeight) * 100 : 0;
    
    // Update width
    this.progressBar.style.width = `${scrollPercent}%`;
    
    // Update color based on belt progression
    const color = this.getBeltColor(scrollPercent);
    this.progressBar.style.background = color;
  }
  
  
  /**
   * getBeltColor
   * ─────────────────────────────────────────────
   * Get belt color based on scroll percentage.
   * 
   * @param {number} percent - Scroll percentage (0-100)
   * @returns {string} - CSS color value
   */
  getBeltColor(percent) {
    // Find the appropriate belt color for current progress
    let selectedColor = this.beltColors[0].color;
    
    for (const belt of this.beltColors) {
      if (percent >= belt.threshold) {
        selectedColor = belt.color;
      }
    }
    
    return selectedColor;
  }
}


// Create instance
new ScrollProgressController();


/* ============================================
   END OF SCROLL PROGRESS CONTROLLER
   ============================================ */
