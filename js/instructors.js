/* ============================================
   INSTRUCTORS TAB SWITCHER
   Version: 1.0.0 - TaskKarate v4
   
   Handles the file-drawer style tab switching
   for instructor bios on the About page.
   ============================================ */

(function() {
  'use strict';

  // Wait for DOM to be ready
  document.addEventListener('DOMContentLoaded', initInstructorTabs);

  function initInstructorTabs() {
    const tabs = document.querySelectorAll('.instructor-tab');
    const panels = document.querySelectorAll('.instructor-panel');

    if (tabs.length === 0 || panels.length === 0) return;

    tabs.forEach(tab => {
      tab.addEventListener('click', () => {
        const instructorId = tab.dataset.instructor;

        // Update tabs
        tabs.forEach(t => {
          t.classList.remove('active');
          t.setAttribute('aria-selected', 'false');
        });
        tab.classList.add('active');
        tab.setAttribute('aria-selected', 'true');

        // Update panels
        panels.forEach(panel => {
          if (panel.dataset.instructor === instructorId) {
            panel.classList.add('active');
          } else {
            panel.classList.remove('active');
          }
        });
      });

      // Keyboard navigation
      tab.addEventListener('keydown', (e) => {
        const tabsArray = Array.from(tabs);
        const currentIndex = tabsArray.indexOf(tab);
        let newIndex;

        switch (e.key) {
          case 'ArrowRight':
          case 'ArrowDown':
            e.preventDefault();
            newIndex = (currentIndex + 1) % tabsArray.length;
            tabsArray[newIndex].focus();
            tabsArray[newIndex].click();
            break;
          case 'ArrowLeft':
          case 'ArrowUp':
            e.preventDefault();
            newIndex = (currentIndex - 1 + tabsArray.length) % tabsArray.length;
            tabsArray[newIndex].focus();
            tabsArray[newIndex].click();
            break;
          case 'Home':
            e.preventDefault();
            tabsArray[0].focus();
            tabsArray[0].click();
            break;
          case 'End':
            e.preventDefault();
            tabsArray[tabsArray.length - 1].focus();
            tabsArray[tabsArray.length - 1].click();
            break;
        }
      });
    });
  }

})();
