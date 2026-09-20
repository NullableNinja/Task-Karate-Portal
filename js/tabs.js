/* ============================================
   PROGRAM TABS CONTROLLER
   Version: 1.1.0 - TaskKarate v4
   
   ╔════════════════════════════════════════════╗
   ║  TABBED CONTENT WITH AUTO-ROTATION         ║
   ╠════════════════════════════════════════════╣
   ║  Features:                                 ║
   ║  • Show/hide content panels                ║
   ║  • Auto-rotate every 30 seconds            ║
   ║  • Pause on hover, resume on leave         ║
   ║  • ARIA accessibility support              ║
   ║  • Keyboard navigation                     ║
   ╚════════════════════════════════════════════╝
   
   📁 Related CSS: css/index.css
   ============================================ */


/**
 * TabController
 * ─────────────────────────────────────────────
 * Manages tabbed content with auto-rotation.
 */
class TabController {
  
  constructor() {
    // Auto-rotation settings
    this.ROTATION_INTERVAL = 30000; // 30 seconds
    this.RESUME_DELAY = 60000; // 60 seconds after manual click
    this.autoRotateTimer = null;
    this.isPaused = false;
    this.currentIndex = 0;
    this.tabIds = [];
    
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
   * Find the programs section and initialize tabs.
   */
  init() {
    // Find the programs section
    this.root = document.getElementById('programs');
    
    if (!this.root) {
      // No programs section on this page
      return;
    }
    
    // Find tabs and panels
    this.tabs = this.root.querySelectorAll('.tab, [role="tab"]');
    this.panels = this.root.querySelectorAll('.tab-panel, [role="tabpanel"]');
    
    if (this.tabs.length === 0 || this.panels.length === 0) {
      console.warn('Tabs: No tabs or panels found');
      return;
    }
    
    // Build tab IDs array
    this.tabIds = Array.from(this.panels).map(p => p.id);
    
    // Add click handlers to tabs
    this.tabs.forEach(tab => {
      tab.addEventListener('click', (e) => {
        e.preventDefault();
        this.handleManualClick(tab);
      });
      
      // Keyboard navigation
      tab.addEventListener('keydown', (e) => {
        this.handleKeydown(e, tab);
      });
    });
    
    // Pause on hover, resume on leave
    this.root.addEventListener('mouseenter', () => {
      this.isPaused = true;
    });
    
    this.root.addEventListener('mouseleave', () => {
      this.isPaused = false;
    });
    
    // Show first tab
    this.show(this.tabIds[0]);
    
    // Start auto-rotation
    this.startAutoRotate();
    
    console.log(`Tabs: Initialized with ${this.tabs.length} tabs, auto-rotate enabled`);
  }
  
  
  /**
   * show
   * ─────────────────────────────────────────────
   * Show a specific tab panel.
   * 
   * @param {string} panelId - The ID of the panel to show
   */
  show(panelId) {
    // Update tab states
    this.tabs.forEach(t => {
      const isActive = t.getAttribute('aria-controls') === panelId;
      t.classList.toggle('is-active', isActive);
      t.setAttribute('aria-selected', isActive);
      t.setAttribute('tabindex', isActive ? '0' : '-1');
    });
    
    // Update panel visibility
    this.panels.forEach(p => {
      if (p.id === panelId) {
        p.classList.remove('hidden');
        p.removeAttribute('hidden');
        p.classList.add('fade-in');
        // Remove animation class after it completes
        setTimeout(() => p.classList.remove('fade-in'), 300);
      } else {
        p.classList.add('hidden');
        p.setAttribute('hidden', 'true');
      }
    });
    
    // Update current index
    this.currentIndex = this.tabIds.indexOf(panelId);
  }
  
  
  /**
   * handleManualClick
   * ─────────────────────────────────────────────
   * Handle when user manually clicks a tab.
   * Pauses auto-rotation temporarily.
   */
  handleManualClick(tab) {
    const panelId = tab.getAttribute('aria-controls');
    this.show(panelId);
    
    // Stop auto-rotation
    this.stopAutoRotate();
    this.isPaused = true;
    
    // Resume auto-rotation after delay
    setTimeout(() => {
      this.isPaused = false;
      this.startAutoRotate();
    }, this.RESUME_DELAY);
  }
  
  
  /**
   * rotateToNext
   * ─────────────────────────────────────────────
   * Rotate to the next tab (auto-rotation).
   */
  rotateToNext() {
    if (!this.isPaused && this.tabIds.length > 0) {
      this.currentIndex = (this.currentIndex + 1) % this.tabIds.length;
      this.show(this.tabIds[this.currentIndex]);
    }
  }
  
  
  /**
   * startAutoRotate
   * ─────────────────────────────────────────────
   * Start the auto-rotation timer.
   */
  startAutoRotate() {
    this.stopAutoRotate();
    this.autoRotateTimer = setInterval(() => this.rotateToNext(), this.ROTATION_INTERVAL);
  }
  
  
  /**
   * stopAutoRotate
   * ─────────────────────────────────────────────
   * Stop the auto-rotation timer.
   */
  stopAutoRotate() {
    if (this.autoRotateTimer) {
      clearInterval(this.autoRotateTimer);
      this.autoRotateTimer = null;
    }
  }
  
  
  /**
   * handleKeydown
   * ─────────────────────────────────────────────
   * Handle keyboard navigation between tabs.
   */
  handleKeydown(e, currentTab) {
    const tabsArray = Array.from(this.tabs);
    const currentIndex = tabsArray.indexOf(currentTab);
    let newIndex = null;
    
    switch (e.key) {
      case 'ArrowLeft':
      case 'ArrowUp':
        newIndex = currentIndex > 0 ? currentIndex - 1 : tabsArray.length - 1;
        break;
      case 'ArrowRight':
      case 'ArrowDown':
        newIndex = currentIndex < tabsArray.length - 1 ? currentIndex + 1 : 0;
        break;
      case 'Home':
        newIndex = 0;
        break;
      case 'End':
        newIndex = tabsArray.length - 1;
        break;
      case 'Enter':
      case ' ':
        this.handleManualClick(currentTab);
        e.preventDefault();
        return;
      default:
        return;
    }
    
    if (newIndex !== null) {
      e.preventDefault();
      tabsArray[newIndex].focus();
      this.handleManualClick(tabsArray[newIndex]);
    }
  }
}


// Create instance
new TabController();


/* ============================================
   END OF TAB CONTROLLER
   ============================================ */
