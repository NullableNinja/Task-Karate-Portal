/* ============================================
   CONTACT FORM MODAL
   Version: 1.0.0 - TaskKarate v4
   
   ╔════════════════════════════════════════════╗
   ║  CONTACT FORM MODAL & FLOATING BUTTON      ║
   ╠════════════════════════════════════════════╣
   ║  Features:                                 ║
   ║  • Floating button appears while scrolling ║
   ║  • Opens contact form modal                ║
   ║  • Sends emails via FormSubmit API         ║
   ║  • Form validation & error handling        ║
   ║  • Smooth animations                       ║
   ╚════════════════════════════════════════════╝
   
   📁 Related CSS: css/index.css
   📁 Email Service: FormSubmit (https://formsubmit.co)
   ============================================ */


/**
 * ContactModalController
 * ─────────────────────────────────────────────
 * Manages contact form modal and floating button.
 */
class ContactModalController {
  
  constructor() {
    // Configuration
    this.showButtonThreshold = 300; // Pixels scrolled before showing button
    this.recipientEmail = 'taskkarateschool@gmail.com';
    this.apiEndpoint = 'https://formsubmit.co/ajax/' + this.recipientEmail;
    
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
    this.button = document.getElementById('contact-form-btn');
    this.modal = document.getElementById('contact-modal');
    this.closeBtn = document.getElementById('contact-modal-close');
    this.form = document.getElementById('contact-form');
    this.backdrop = this.modal?.querySelector('.contact-modal__backdrop');
    this.statusMsg = document.getElementById('contact-status');
    
    if (!this.button || !this.modal || !this.form) {
      console.warn('ContactModal: Required elements not found');
      return;
    }
    
    // Bind events
    this.bindEvents();
    
    // Initial button visibility check
    this.updateButtonVisibility();
    
    console.log('ContactModal: Initialized');
  }
  
  
  /**
   * bindEvents
   * ─────────────────────────────────────────────
   * Attach event listeners.
   */
  bindEvents() {
    // Button to open modal
    this.button.addEventListener('click', () => this.openModal());
    
    // Close button
    if (this.closeBtn) {
      this.closeBtn.addEventListener('click', () => this.closeModal());
    }
    
    // Close on backdrop click
    if (this.backdrop) {
      this.backdrop.addEventListener('click', () => this.closeModal());
    }
    
    // Keyboard: Escape to close
    document.addEventListener('keydown', (e) => {
      if (e.key === 'Escape' && this.modal.getAttribute('aria-hidden') === 'false') {
        this.closeModal();
      }
    });
    
    // Form submission
    this.form.addEventListener('submit', (e) => this.handleSubmit(e));
    
    // Real-time validation on inputs
    const nameInput = document.getElementById('contact-name');
    const emailInput = document.getElementById('contact-email');
    const messageInput = document.getElementById('contact-message');
    
    if (nameInput) {
      nameInput.addEventListener('blur', () => this.validateField(nameInput, 'name'));
      nameInput.addEventListener('input', () => this.clearFieldError(nameInput));
    }
    
    if (emailInput) {
      // Debounced email validation
      let emailTimeout;
      emailInput.addEventListener('input', () => {
        clearTimeout(emailTimeout);
        this.clearFieldError(emailInput);
        emailTimeout = setTimeout(() => {
          this.validateField(emailInput, 'email');
        }, 500);
      });
    }
    
    if (messageInput) {
      messageInput.addEventListener('blur', () => this.validateField(messageInput, 'message'));
      messageInput.addEventListener('input', () => {
        this.clearFieldError(messageInput);
        // Update character count
        const current = messageInput.value.length;
        const min = 10;
        if (current > 0 && current < min) {
          this.setFieldHint(messageInput, `${min - current} more characters needed`);
        } else {
          this.clearFieldHint(messageInput);
        }
      });
    }
    
    // Button visibility on scroll
    window.addEventListener('scroll', () => this.updateButtonVisibility());
  }
  
  
  /**
   * updateButtonVisibility
   * ─────────────────────────────────────────────
   * Show/hide button based on scroll position.
   */
  updateButtonVisibility() {
    if (window.scrollY > this.showButtonThreshold) {
      this.button.classList.add('show');
    } else {
      this.button.classList.remove('show');
    }
  }
  
  
  /**
   * openModal
   * ─────────────────────────────────────────────
   * Display the contact form modal.
   */
  openModal() {
    this.modal.setAttribute('aria-hidden', 'false');
    document.body.style.overflow = 'hidden';
    
    // Focus first input
    setTimeout(() => {
      document.getElementById('contact-name')?.focus();
    }, 100);
    
    console.log('ContactModal: Opened');
  }
  
  
  /**
   * closeModal
   * ─────────────────────────────────────────────
   * Hide the contact form modal.
   */
  closeModal() {
    this.modal.setAttribute('aria-hidden', 'true');
    document.body.style.overflow = '';
    
    // Clear status message
    if (this.statusMsg) {
      this.statusMsg.textContent = '';
      this.statusMsg.className = 'contact-modal__status';
    }
    
    console.log('ContactModal: Closed');
  }
  
  
  /**
   * handleSubmit
   * ─────────────────────────────────────────────
   * Process form submission and send email.
   */
  async handleSubmit(e) {
    e.preventDefault();
    
    // Get form data
    const formData = new FormData(this.form);
    const data = {
      name: formData.get('name')?.trim(),
      email: formData.get('email')?.trim(),
      message: formData.get('message')?.trim()
    };
    
    // Validate
    if (!this.validateForm(data)) {
      return;
    }
    
    // Disable submit button
    const submitBtn = this.form.querySelector('button[type="submit"]');
    const originalText = submitBtn.textContent;
    submitBtn.disabled = true;
    submitBtn.textContent = 'Sending...';
    
    try {
      // Send to FormSubmit API
      const response = await fetch(this.apiEndpoint, {
        method: 'POST',
        headers: {
          'Accept': 'application/json'
        },
        body: new FormData(this.form)
      });
      
      if (response.ok) {
        // Success
        this.showStatus('Message sent! We\'ll get back to you soon!', 'success');
        this.form.reset();
        
        // Close modal after delay
        setTimeout(() => {
          this.closeModal();
        }, 2000);
      } else {
        throw new Error('Failed to send message');
      }
    } catch (error) {
      console.error('ContactModal: Submission error', error);
      this.showStatus('Oops! Something went wrong. Please try again.', 'error');
    } finally {
      // Re-enable submit button
      submitBtn.disabled = false;
      submitBtn.textContent = originalText;
    }
  }
  
  
  /**
   * validateForm
   * ─────────────────────────────────────────────
   * Validate form data before sending.
   */
  validateForm(data) {
    // Check empty fields
    if (!data.name || !data.email || !data.message) {
      this.showStatus('Please fill out all fields', 'error');
      return false;
    }
    
    // Validate email format
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(data.email)) {
      this.showStatus('Please enter a valid email address', 'error');
      return false;
    }
    
    // Validate message length
    if (data.message.length < 10) {
      this.showStatus('Message must be at least 10 characters', 'error');
      return false;
    }
    
    // Clear any previous error messages
    this.statusMsg.textContent = '';
    this.statusMsg.className = 'contact-modal__status';
    
    return true;
  }
  
  
  /**
   * showStatus
   * ─────────────────────────────────────────────
   * Display status message to user.
   */
  showStatus(message, type = 'info') {
    if (!this.statusMsg) return;
    
    this.statusMsg.textContent = message;
    this.statusMsg.className = `contact-modal__status ${type}`;
    
    // Auto-clear non-success messages after 5 seconds
    if (type !== 'success') {
      setTimeout(() => {
        if (this.statusMsg.textContent === message) {
          this.statusMsg.textContent = '';
          this.statusMsg.className = 'contact-modal__status';
        }
      }, 5000);
    }
  }
  
  
  /**
   * validateField
   * ─────────────────────────────────────────────
   * Validate individual field and show inline error.
   */
  validateField(input, type) {
    const value = input.value.trim();
    let isValid = true;
    let errorMsg = '';
    
    switch(type) {
      case 'name':
        if (!value) {
          isValid = false;
          errorMsg = 'Name is required';
        } else if (value.length < 2) {
          isValid = false;
          errorMsg = 'Name must be at least 2 characters';
        }
        break;
        
      case 'email':
        if (!value) {
          isValid = false;
          errorMsg = 'Email is required';
        } else {
          const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
          if (!emailRegex.test(value)) {
            isValid = false;
            errorMsg = 'Please enter a valid email address';
          }
        }
        break;
        
      case 'message':
        if (!value) {
          isValid = false;
          errorMsg = 'Message is required';
        } else if (value.length < 10) {
          isValid = false;
          errorMsg = 'Message must be at least 10 characters';
        }
        break;
    }
    
    if (!isValid) {
      this.setFieldError(input, errorMsg);
    } else {
      this.setFieldSuccess(input);
    }
    
    return isValid;
  }
  
  
  /**
   * setFieldError
   * ─────────────────────────────────────────────
   * Mark field as invalid with error message.
   */
  setFieldError(input, message) {
    input.classList.remove('valid');
    input.classList.add('invalid');
    input.setAttribute('aria-invalid', 'true');
    
    // Find or create error message element
    let errorEl = input.parentElement.querySelector('.field-error');
    if (!errorEl) {
      errorEl = document.createElement('div');
      errorEl.className = 'field-error';
      errorEl.setAttribute('role', 'alert');
      input.parentElement.appendChild(errorEl);
    }
    errorEl.textContent = message;
  }
  
  
  /**
   * setFieldSuccess
   * ─────────────────────────────────────────────
   * Mark field as valid.
   */
  setFieldSuccess(input) {
    if (input.value.trim()) {
      input.classList.remove('invalid');
      input.classList.add('valid');
      input.setAttribute('aria-invalid', 'false');
    }
    this.clearFieldError(input);
  }
  
  
  /**
   * clearFieldError
   * ─────────────────────────────────────────────
   * Remove error state and message.
   */
  clearFieldError(input) {
    input.classList.remove('invalid', 'valid');
    input.removeAttribute('aria-invalid');
    
    const errorEl = input.parentElement.querySelector('.field-error');
    if (errorEl) {
      errorEl.remove();
    }
  }
  
  
  /**
   * setFieldHint
   * ─────────────────────────────────────────────
   * Show helper text for field.
   */
  setFieldHint(input, message) {
    let hintEl = input.parentElement.querySelector('.field-hint');
    if (!hintEl) {
      hintEl = document.createElement('div');
      hintEl.className = 'field-hint';
      input.parentElement.appendChild(hintEl);
    }
    hintEl.textContent = message;
  }
  
  
  /**
   * clearFieldHint
   * ─────────────────────────────────────────────
   * Remove helper text.
   */
  clearFieldHint(input) {
    const hintEl = input.parentElement.querySelector('.field-hint');
    if (hintEl) {
      hintEl.remove();
    }
  }
}


// Initialize on page load
new ContactModalController();
