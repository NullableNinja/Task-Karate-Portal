/* ============================================
   SCHEDULE CONTROLLER
   Version: 1.0.0 - TaskKarate v4
   
   ╔════════════════════════════════════════════╗
   ║  SCHEDULE PAGE FUNCTIONALITY               ║
   ╠════════════════════════════════════════════╣
   ║  Features:                                 ║
   ║  • Schedule selector dropdown              ║
   ║  • Show/hide schedule panels               ║
   ║  • Download schedule button                ║
   ╚════════════════════════════════════════════╝
   
   📁 Related HTML: schedule.html
   📁 Related CSS: css/schedule.css
   ============================================ */


/**
 * ScheduleController
 * ─────────────────────────────────────────────
 * Manages schedule selection and display.
 */
class ScheduleController {
  
  constructor() {
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
    // Get DOM elements
    this.select = document.getElementById('schedule-select');
    this.downloadBtn = document.getElementById('download-schedule-btn');
    this.panels = document.querySelectorAll('.schedule-panel');
    
    if (!this.select || this.panels.length === 0) {
      console.warn('Schedule: Required elements not found');
      return;
    }
    
    // Bind events
    this.select.addEventListener('change', () => this.handleSelectChange());
    
    if (this.downloadBtn) {
      this.downloadBtn.addEventListener('click', () => this.handleDownload());
    }
    
    // Show initial schedule (kids by default)
    this.showSchedule('kids');
    
    console.log('Schedule: Initialized');
  }
  
  
  /**
   * handleSelectChange
   * ─────────────────────────────────────────────
   * Called when user selects a different schedule.
   */
  handleSelectChange() {
    const selectedValue = this.select.value;
    this.showSchedule(selectedValue);
  }
  
  
  /**
   * showSchedule
   * ─────────────────────────────────────────────
   * Show the selected schedule panel, hide others.
   * 
   * @param {string} scheduleId - The schedule identifier
   */
  showSchedule(scheduleId) {
    this.panels.forEach(panel => {
      const isActive = panel.dataset.schedule === scheduleId;
      
      if (isActive) {
        panel.removeAttribute('hidden');
        panel.classList.add('active');
        
        // Scroll to panel (with offset for sticky nav)
        setTimeout(() => {
          const offset = 100; // Account for sticky navbar
          const top = panel.getBoundingClientRect().top + window.scrollY - offset;
          window.scrollTo({ top, behavior: 'smooth' });
        }, 100);
      } else {
        panel.setAttribute('hidden', 'true');
        panel.classList.remove('active');
      }
    });
  }
  
  
  /**
   * handleDownload
   * ─────────────────────────────────────────────
   * Handle schedule download button click.
   * Opens the appropriate PDF for the selected schedule.
   */
  handleDownload() {
    const selectedValue = this.select.value;
    
    // Map schedule IDs to PDF file names
    const pdfMap = {
      'kids': 'kids-schedule.pdf',
      'teens-adults': 'teens-adults-schedule.pdf',
      'sparring': 'sparring-schedule.pdf',
      'weapons': 'weapons-schedule.pdf',
      'special': 'special-schedule.pdf',
      'eskrima': 'eskrima-schedule.pdf'
    };
    
    const pdfFile = pdfMap[selectedValue];
    
    if (pdfFile) {
      // TODO: Update this path when PDFs are available
      const pdfPath = `files/${pdfFile}`;
      
      // For now, show an alert since PDFs don't exist yet
      alert(`Download coming soon!\n\nSchedule: ${this.select.options[this.select.selectedIndex].text}\nFile: ${pdfFile}`);
      
      // When PDFs are ready, use this:
      // window.open(pdfPath, '_blank');
    }
  }
}


// Create instance
new ScheduleController();


/* ============================================
   END OF SCHEDULE CONTROLLER
   ============================================ */
