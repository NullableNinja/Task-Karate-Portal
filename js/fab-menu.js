/* ============================================
   FLOATING ACTION BUTTON MENU
   Version: 1.0.0 - TaskKarate v4
   
   ╔════════════════════════════════════════════╗
   ║  EXPANDABLE FAB MENU                       ║
   ╠════════════════════════════════════════════╣
   ║  Floating button that expands to show      ║
   ║  multiple quick action buttons.            ║
   ║                                            ║
   ║  Features:                                 ║
   ║  • Appears after scrolling                 ║
   ║  • Expands radially on click               ║
   ║  • 4 quick actions: Contact, Trial,        ║
   ║    Schedule, Call                          ║
   ║  • Labels appear on hover                  ║
   ║  • Touch-friendly                          ║
   ╚════════════════════════════════════════════╝
   
   📁 Related CSS: css/index.css
   ============================================ */


/**
 * FABMenuController
 * ─────────────────────────────────────────────
 * Manages floating action button menu.
 */
class FABMenuController {
  
  constructor() {
    // Configuration
    this.showThreshold = 300; // Pixels scrolled before showing
    this.isExpanded = false;
    
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
   * Find elements and bind events.
   */
  init() {
    // Get elements
    this.menu = document.getElementById('fab-menu');
    this.mainBtn = document.getElementById('fab-main');
    this.actionsContainer = document.getElementById('fab-actions');
    this.actions = this.actionsContainer?.querySelectorAll('.fab-menu__action');
    
    // Contact modal elements
    this.contactModal = document.getElementById('contact-modal');
    this.contactCloseBtn = document.getElementById('contact-modal-close');
    
    if (!this.menu || !this.mainBtn) {
      console.warn('FABMenu: Required elements not found');
      return;
    }
    
    // Bind events
    this.bindEvents();
    
    // Initial visibility check
    this.updateVisibility();
    
    console.log('FABMenu: Initialized');
  }
  
  
  /**
   * bindEvents
   * ─────────────────────────────────────────────
   * Attach event listeners.
   */
  bindEvents() {
    // Main button toggle
    this.mainBtn.addEventListener('click', () => this.toggle());
    
    // Scroll visibility
    window.addEventListener('scroll', () => this.updateVisibility());
    
    // Action buttons
    this.actions.forEach(action => {
      action.addEventListener('click', (e) => this.handleAction(e, action));
    });
    
    // Close on outside click
    document.addEventListener('click', (e) => {
      if (this.isExpanded && !this.menu.contains(e.target)) {
        this.collapse();
      }
    });
    
    // Close on escape
    document.addEventListener('keydown', (e) => {
      if (e.key === 'Escape' && this.isExpanded) {
        this.collapse();
      }
    });
    
    // Contact modal close button
    if (this.contactCloseBtn) {
      this.contactCloseBtn.addEventListener('click', () => this.closeContactModal());
    }
    
    // Contact modal backdrop
    if (this.contactModal) {
      const backdrop = this.contactModal.querySelector('.contact-modal__backdrop');
      if (backdrop) {
        backdrop.addEventListener('click', () => this.closeContactModal());
      }
      
      // Escape key for modal
      document.addEventListener('keydown', (e) => {
        if (e.key === 'Escape' && this.contactModal.getAttribute('aria-hidden') === 'false') {
          this.closeContactModal();
        }
      });
    }
  }
  
  
  /**
   * updateVisibility
   * ─────────────────────────────────────────────
   * Show/hide menu based on scroll position.
   */
  updateVisibility() {
    if (window.scrollY > this.showThreshold) {
      this.menu.classList.add('show');
    } else {
      this.menu.classList.remove('show');
      if (this.isExpanded) {
        this.collapse();
      }
    }
  }
  
  
  /**
   * toggle
   * ─────────────────────────────────────────────
   * Expand or collapse the menu.
   */
  toggle() {
    if (this.isExpanded) {
      this.collapse();
    } else {
      this.expand();
    }
  }
  
  
  /**
   * expand
   * ─────────────────────────────────────────────
   * Show action buttons.
   */
  expand() {
    this.isExpanded = true;
    this.menu.classList.add('expanded');
    this.mainBtn.setAttribute('aria-expanded', 'true');
    
    console.log('FABMenu: Expanded');
  }
  
  
  /**
   * collapse
   * ─────────────────────────────────────────────
   * Hide action buttons.
   */
  collapse() {
    this.isExpanded = false;
    this.menu.classList.remove('expanded');
    this.mainBtn.setAttribute('aria-expanded', 'false');
    
    console.log('FABMenu: Collapsed');
  }
  
  
  /**
   * handleAction
   * ─────────────────────────────────────────────
   * Process action button clicks.
   */
  handleAction(e, action) {
    const actionType = action.getAttribute('data-action');
    
    switch(actionType) {
      case 'contact':
        e.preventDefault();
        this.openContactModal();
        this.collapse();
        break;
        
      case 'trial':
        e.preventDefault();
        this.openTrialPopup();
        this.collapse();
        break;
        
      // Schedule and Call are links, let them work naturally
    }
  }
  
  
  /**
   * openContactModal
   * ─────────────────────────────────────────────
   * Trigger contact modal (if exists).
   */
  openContactModal() {
    const modal = document.getElementById('contact-modal');
    if (modal) {
      modal.setAttribute('aria-hidden', 'false');
      document.body.style.overflow = 'hidden';
      
      // Focus first input
      setTimeout(() => {
        document.getElementById('contact-name')?.focus();
      }, 100);
      
      console.log('FABMenu: Opened contact modal');
    }
  }
  
  
  /**
   * openTrialPopup
   * ─────────────────────────────────────────────
   * Trigger trial popup (if exists).
   */
  openTrialPopup() {
    const popup = document.getElementById('trial-popup');
    if (popup) {
      popup.setAttribute('aria-hidden', 'false');
      document.body.style.overflow = 'hidden';
      
      console.log('FABMenu: Opened trial popup');
    }
  }
  
  
  /**
   * closeContactModal
   * ─────────────────────────────────────────────
   * Close contact modal.
   */
  closeContactModal() {
    if (this.contactModal) {
      this.contactModal.setAttribute('aria-hidden', 'true');
      document.body.style.overflow = '';
      
      console.log('FABMenu: Closed contact modal');
    }
  }
}


// Initialize FAB menu
new FABMenuController();
