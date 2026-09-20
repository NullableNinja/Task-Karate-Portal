/* ============================================
   NEWSLETTER CONTROLLER
   Version: 1.0.0 - TaskKarate v4
   
   ╔════════════════════════════════════════════╗
   ║  NEWSLETTER PAGE FUNCTIONALITY             ║
   ╠════════════════════════════════════════════╣
   ║  Features:                                 ║
   ║  • Newsletter signup form handling         ║
   ║  • Archive year/month selection            ║
   ║  • Download newsletter PDFs                ║
   ╚════════════════════════════════════════════╝
   
   📁 Related HTML: news.html
   📁 Related CSS: css/news.css
   ============================================ */


/**
 * NewsletterController
 * ─────────────────────────────────────────────
 * Manages newsletter signup and archive functionality.
 */
class NewsletterController {
  
  constructor() {
    // Archive configuration
    this.archiveStartYear = 2003;
    this.currentYear = new Date().getFullYear();
    
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
   * Get DOM elements and bind events.
   */
  init() {
    // Signup form elements
    this.signupForm = document.getElementById('newsletter-signup-form');
    this.signupMessage = document.getElementById('signup-message');
    
    // Archive elements
    this.yearSelect = document.getElementById('year-select');
    this.monthSelect = document.getElementById('month-select');
    this.downloadBtn = document.getElementById('download-newsletter-btn');
    this.archiveMessage = document.getElementById('archive-message');
    
    // Initialize components
    this.populateYears();
    this.bindEvents();
    
    console.log('Newsletter: Initialized');
  }
  
  
  /**
   * populateYears
   * ─────────────────────────────────────────────
   * Fill the year dropdown with available years.
   */
  populateYears() {
    if (!this.yearSelect) return;
    
    // Clear existing options except placeholder
    while (this.yearSelect.options.length > 1) {
      this.yearSelect.remove(1);
    }
    
    // Add years from current year back to archive start
    for (let year = this.currentYear; year >= this.archiveStartYear; year--) {
      const option = document.createElement('option');
      option.value = year;
      option.textContent = year;
      this.yearSelect.appendChild(option);
    }
  }
  
  
  /**
   * bindEvents
   * ─────────────────────────────────────────────
   * Attach event listeners.
   */
  bindEvents() {
    // Signup form submission
    if (this.signupForm) {
      this.signupForm.addEventListener('submit', (e) => this.handleSignup(e));
    }
    
    // Archive selection changes
    if (this.yearSelect) {
      this.yearSelect.addEventListener('change', () => this.updateDownloadButton());
    }
    if (this.monthSelect) {
      this.monthSelect.addEventListener('change', () => this.updateDownloadButton());
    }
    
    // Download button click
    if (this.downloadBtn) {
      this.downloadBtn.addEventListener('click', () => this.handleDownload());
    }
  }
  
  
  /**
   * handleSignup
   * ─────────────────────────────────────────────
   * Handle newsletter signup form submission.
   * 
   * @param {Event} e - Form submit event
   */
  handleSignup(e) {
    e.preventDefault();
    
    const name = document.getElementById('signup-name').value.trim();
    const email = document.getElementById('signup-email').value.trim();
    
    // Basic validation
    if (!name || !email) {
      this.showMessage(this.signupMessage, 'Please fill in all fields.', 'error');
      return;
    }
    
    // Email validation
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(email)) {
      this.showMessage(this.signupMessage, 'Please enter a valid email address.', 'error');
      return;
    }
    
    // TODO: Replace with actual API call
    // For now, simulate a successful signup
    console.log('Newsletter signup:', { name, email });
    
    // Show success message
    this.showMessage(
      this.signupMessage, 
      `Thanks, ${name}! You've been subscribed to the Task Karate newsletter.`, 
      'success'
    );
    
    // Reset form
    this.signupForm.reset();
  }
  
  
  /**
   * updateDownloadButton
   * ─────────────────────────────────────────────
   * Enable/disable download button based on selection.
   */
  updateDownloadButton() {
    if (!this.downloadBtn) return;
    
    const yearSelected = this.yearSelect.value !== '';
    const monthSelected = this.monthSelect.value !== '';
    
    this.downloadBtn.disabled = !(yearSelected && monthSelected);
    
    // Hide any previous message
    if (this.archiveMessage) {
      this.archiveMessage.style.display = 'none';
    }
  }
  
  
  /**
   * handleDownload
   * ─────────────────────────────────────────────
   * Handle newsletter download button click.
   */
  handleDownload() {
    const year = this.yearSelect.value;
    const month = this.monthSelect.value;
    
    if (!year || !month) return;
    
    // Get month name for display
    const monthNames = [
      'January', 'February', 'March', 'April', 'May', 'June',
      'July', 'August', 'September', 'October', 'November', 'December'
    ];
    const monthName = monthNames[parseInt(month) - 1];
    
    // Construct PDF filename
    // Expected format: newsletter/2024-01.pdf
    const pdfPath = `newsletter/${year}-${month}.pdf`;
    
    // TODO: Check if file exists before attempting download
    // For now, show a message that the feature is coming soon
    
    this.showMessage(
      this.archiveMessage,
      `Downloading ${monthName} ${year} newsletter... (Feature coming soon!)`,
      'success'
    );
    
    // When PDFs are ready, use this:
    // window.open(pdfPath, '_blank');
  }
  
  
  /**
   * showMessage
   * ─────────────────────────────────────────────
   * Display a feedback message.
   * 
   * @param {HTMLElement} element - Message container element
   * @param {string} message - Message text
   * @param {string} type - 'success' or 'error'
   */
  showMessage(element, message, type) {
    if (!element) return;
    
    element.textContent = message;
    element.className = `form-message form-message--${type}`;
    element.style.display = 'block';
    
    // Auto-hide after 5 seconds
    setTimeout(() => {
      element.style.display = 'none';
    }, 5000);
  }
}


// Create instance
new NewsletterController();


/* ============================================
   END OF NEWSLETTER CONTROLLER
   ============================================ */
