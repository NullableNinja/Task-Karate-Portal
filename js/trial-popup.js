/* ============================================
   FREE TRIAL POPUP
   Version: 1.0.0 - TaskKarate v4
   
   ╔════════════════════════════════════════════╗
   ║  2-WEEK FREE TRIAL NOTIFICATION            ║
   ╠════════════════════════════════════════════╣
   ║  Features:                                 ║
   ║  • Shows on page load (delay for UX)       ║
   ║  • Over-the-top Hi-Yah! design            ║
   ║  • Dismissible with localStorage           ║
   ║  • Smooth animations & interactions        ║
   ╚════════════════════════════════════════════╝
   
   📁 Related CSS: css/index.css
   ============================================ */


/**
 * TrialPopupController
 * ─────────────────────────────────────────────
 * Manages the free trial popup display,
 * dismissal, and localStorage persistence.
 */
class TrialPopupController {
  
  constructor() {
    // Configuration
    this.showDelay = 2500; // Milliseconds before showing popup
    this.storageKey = 'taskkarate_trial_popup_dismissed';
    this.storageExpiry = 30 * 24 * 60 * 60 * 1000; // 30 days in milliseconds
    
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
   * Find modal and bind events.
   */
  init() {
    // Get modal element
    this.modal = document.getElementById('trial-popup');
    this.closeBtn = document.getElementById('trial-popup-close');
    this.ctaLink = document.querySelector('.trial-popup__cta');
    
    if (!this.modal) {
      console.warn('TrialPopup: Modal not found');
      return;
    }
    
    // Bind events
    this.bindEvents();
    
    // Check if we should show
    this.checkAndShow();
    
    console.log('TrialPopup: Initialized');
  }
  
  
  /**
   * bindEvents
   * ─────────────────────────────────────────────
   * Attach event listeners to buttons.
   */
  bindEvents() {
    // Close button
    if (this.closeBtn) {
      this.closeBtn.addEventListener('click', () => this.dismiss());
    }
    
    // CTA link - close then scroll to contact
    if (this.ctaLink) {
      this.ctaLink.addEventListener('click', (e) => {
        e.preventDefault();
        this.dismiss();
        // Scroll to contact form
        document.getElementById('contact-modal')?.scrollIntoView({ behavior: 'smooth' });
      });
    }
    
    // Close on backdrop click (optional - adjust based on UX preference)
    const backdrop = this.modal.querySelector('.trial-popup__backdrop');
    if (backdrop) {
      backdrop.addEventListener('click', () => this.dismiss());
    }
  }
  
  
  /**
   * checkAndShow
   * ─────────────────────────────────────────────
   * Check if popup should be shown, then show it.
   */
  checkAndShow() {
    // Check localStorage
    const savedDismissal = this.getStoredDismissal();
    
    if (savedDismissal && !this.isDismissalExpired(savedDismissal)) {
      console.log('TrialPopup: Already dismissed recently, skipping');
      return;
    }
    
    // Show after delay
    setTimeout(() => this.show(), this.showDelay);
  }
  
  
  /**
   * show
   * ─────────────────────────────────────────────
   * Display the popup.
   */
  show() {
    this.modal.setAttribute('aria-hidden', 'false');
    console.log('TrialPopup: Shown');
  }
  
  
  /**
   * dismiss
   * ─────────────────────────────────────────────
   * Hide the popup and save dismissal to storage.
   */
  dismiss() {
    this.modal.setAttribute('aria-hidden', 'true');
    
    // Save dismissal with timestamp
    const dismissalData = {
      timestamp: Date.now(),
      dismissed: true
    };
    localStorage.setItem(this.storageKey, JSON.stringify(dismissalData));
    
    console.log('TrialPopup: Dismissed and saved to storage');
  }
  
  
  /**
   * getStoredDismissal
   * ─────────────────────────────────────────────
   * Retrieve dismissal data from localStorage.
   */
  getStoredDismissal() {
    try {
      const stored = localStorage.getItem(this.storageKey);
      return stored ? JSON.parse(stored) : null;
    } catch (e) {
      console.warn('TrialPopup: Error reading localStorage', e);
      return null;
    }
  }
  
  
  /**
   * isDismissalExpired
   * ─────────────────────────────────────────────
   * Check if dismissal timestamp has expired.
   */
  isDismissalExpired(dismissalData) {
    const now = Date.now();
    const storedTime = dismissalData.timestamp || 0;
    const age = now - storedTime;
    
    return age > this.storageExpiry;
  }
  
  
  /**
   * resetDismissal (Public method)
   * ─────────────────────────────────────────────
   * For testing: Clear the dismissal and show again.
   * Usage: window.trialPopup.resetDismissal()
   */
  resetDismissal() {
    localStorage.removeItem(this.storageKey);
    console.log('TrialPopup: Dismissal cleared, will show again');
    this.show();
  }
}


// Initialize on page load
new TrialPopupController();
