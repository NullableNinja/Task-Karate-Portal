/**
 * Journal Navigation
 * ──────────────────────────────────────────────
 * Handles page flipping for the Student Handbook journal.
 * 
 * @version 1.0.0
 * @file js/journal.js
 */

(function() {
  'use strict';
  
  const journal = document.querySelector('.journal__book');
  if (!journal) return;
  
  const pages = journal.querySelectorAll('.journal__page');
  const prevBtn = journal.querySelector('.journal__nav--prev');
  const nextBtn = journal.querySelector('.journal__nav--next');
  
  if (!pages.length || !prevBtn || !nextBtn) return;
  
  let currentPage = 1;
  const totalPages = pages.length;
  
  /**
   * Show a specific page
   * @param {number} pageNum - Page number to show (1-indexed)
   */
  function showPage(pageNum) {
    // Clamp page number
    pageNum = Math.max(1, Math.min(totalPages, pageNum));
    
    // Update all pages
    pages.forEach(page => {
      const num = parseInt(page.dataset.page, 10);
      page.classList.toggle('journal__page--active', num === pageNum);
    });
    
    // Update current page
    currentPage = pageNum;
    
    // Update button states
    prevBtn.disabled = currentPage === 1;
    nextBtn.disabled = currentPage === totalPages;
  }
  
  // Navigation handlers
  prevBtn.addEventListener('click', () => {
    showPage(currentPage - 1);
  });
  
  nextBtn.addEventListener('click', () => {
    showPage(currentPage + 1);
  });
  
  // Keyboard navigation when journal is focused
  journal.addEventListener('keydown', (e) => {
    if (e.key === 'ArrowLeft') {
      e.preventDefault();
      showPage(currentPage - 1);
    } else if (e.key === 'ArrowRight') {
      e.preventDefault();
      showPage(currentPage + 1);
    }
  });
  
  // Make journal focusable for keyboard nav
  journal.setAttribute('tabindex', '0');
  
  // Initialize first page
  showPage(1);
  
})();
