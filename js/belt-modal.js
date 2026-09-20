/**
 * Belt Testing Requirements Modal
 * ──────────────────────────────────────────────
 * Opens a Paper-Fu modal with an iframe showing
 * the testing requirements for each belt level.
 * 
 * @version 1.0.0
 * @file js/belt-modal.js
 */

(function() {
  'use strict';
  
  // Belt color map for the colored bar
  const beltColors = {
    white: '#f8fafc',
    gold: '#eab308',
    orange: '#f97316',
    green: '#22c55e',
    purple: '#a855f7',
    blue: '#3b82f6',
    red: '#ef4444',
    brown: '#92400e',
    black: '#171717'
  };
  
  // Belt name map for the title
  const beltNames = {
    white: 'White Belt',
    gold: 'Gold Belt',
    orange: 'Orange Belt',
    green: 'Green Belt',
    purple: 'Purple Belt',
    blue: 'Blue Belt',
    red: 'Red Belt',
    brown: 'Brown Belt',
    black: 'Black Belt'
  };
  
  // Cache DOM elements
  const modal = document.getElementById('belt-modal');
  const modalBar = document.getElementById('belt-modal-bar');
  const modalTitle = document.getElementById('belt-modal-title');
  const modalFrame = document.getElementById('belt-modal-frame');
  const closeBtn = document.getElementById('belt-modal-close');
  const backdrop = modal?.querySelector('.belt-modal__backdrop');
  
  if (!modal || !modalFrame) return;
  
  // Get all belt cards
  const beltCards = document.querySelectorAll('.belt-card[data-belt]');
  
  /**
   * Open the modal with a specific belt's testing sheet
   * @param {string} belt - Belt color key
   * @param {string} src - URL to the testing sheet HTML
   */
  function openModal(belt, src) {
    // Set belt color bar
    if (modalBar) {
      modalBar.style.setProperty('--belt-color', beltColors[belt] || beltColors.blue);
      modalBar.style.background = beltColors[belt] || beltColors.blue;
    }
    
    // Set title
    if (modalTitle) {
      modalTitle.textContent = `${beltNames[belt] || 'Belt'} Testing Requirements`;
    }
    
    // Load the iframe
    modalFrame.src = src;
    
    // Show modal
    modal.setAttribute('aria-hidden', 'false');
    document.body.style.overflow = 'hidden';
    
    // Focus the close button for accessibility
    setTimeout(() => closeBtn?.focus(), 100);
  }
  
  /**
   * Close the modal
   */
  function closeModal() {
    modal.setAttribute('aria-hidden', 'true');
    document.body.style.overflow = '';
    
    // Clear iframe after transition
    setTimeout(() => {
      modalFrame.src = 'about:blank';
    }, 300);
  }
  
  // Belt card click handlers
  beltCards.forEach(card => {
    card.addEventListener('click', () => {
      const belt = card.dataset.belt;
      const src = card.dataset.src;
      if (belt && src) {
        openModal(belt, src);
      }
    });
  });
  
  // Close button handler
  closeBtn?.addEventListener('click', closeModal);
  
  // Backdrop click handler
  backdrop?.addEventListener('click', closeModal);
  
  // Escape key handler
  document.addEventListener('keydown', (e) => {
    if (e.key === 'Escape' && modal.getAttribute('aria-hidden') === 'false') {
      closeModal();
    }
  });
  
})();
