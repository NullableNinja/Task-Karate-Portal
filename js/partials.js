/* ============================================
   PARTIALS LOADER
   Version: 1.0.0 - TaskKarate v4
   
   ╔════════════════════════════════════════════╗
   ║  LOADS HTML PARTIALS INTO PAGE             ║
   ╠════════════════════════════════════════════╣
   ║  This script loads reusable HTML snippets: ║
   ║  • Navigation bar → #site-header           ║
   ║  • Footer → #site-footer                   ║
   ╚════════════════════════════════════════════╝
   
   WHY USE PARTIALS?
   ─────────────────────────────────────────────
   Instead of copying the same navigation HTML
   into every page, we keep it in one file and
   load it with JavaScript. This means:
   
   1. Change navigation once, updates everywhere
   2. Smaller HTML files, easier to read
   3. Consistent structure across all pages
   
   HOW IT WORKS:
   ─────────────────────────────────────────────
   1. Page loads with empty div: <div id="site-header"></div>
   2. This script fetches partials/navigation.html
   3. HTML content is injected into the div
   4. Active navigation link is highlighted
   
   TO ADD A NEW PARTIAL:
   ─────────────────────────────────────────────
   1. Create the HTML file in /partials/
   2. Add target div in your HTML: <div id="new-partial"></div>
   3. Add to PARTIALS array below
   ============================================ */


/**
 * Array of partials to load
 * Each object has:
 * - id: The div ID where HTML will be injected
 * - file: Path to the HTML file
 */
const PARTIALS = [
  { id: 'site-header', file: 'partials/navigation.html' },
  { id: 'site-footer', file: 'partials/footer.html' }
];


/**
 * loadPartial
 * ─────────────────────────────────────────────
 * Fetches HTML content and injects it into a target element.
 * 
 * @param {string} elementId - The ID of the target div
 * @param {string} filePath - Path to the HTML file
 * @returns {Promise<void>}
 */
async function loadPartial(elementId, filePath) {
  const target = document.getElementById(elementId);
  
  // Exit if target element doesn't exist
  if (!target) {
    console.warn(`Partials: Target element #${elementId} not found`);
    return;
  }
  
  try {
    // Fetch the HTML file
    const response = await fetch(filePath);
    
    // Check if fetch was successful
    if (!response.ok) {
      throw new Error(`HTTP ${response.status}: ${response.statusText}`);
    }
    
    // Get the HTML content
    const html = await response.text();
    
    // Inject into the target element
    target.innerHTML = html;
    
    console.log(`Partials: Loaded ${filePath} into #${elementId}`);
    
  } catch (error) {
    console.error(`Partials: Failed to load ${filePath}`, error);
    // Don't break the page - leave the div empty
  }
}


/**
 * setActiveNavLink
 * ─────────────────────────────────────────────
 * Highlights the navigation link for the current page.
 * Uses the data-nav attribute to match the current URL.
 */
function setActiveNavLink() {
  // Get current page name from URL
  const path = window.location.pathname;
  const pageName = path.split('/').pop() || 'index.html';
  
  // Map page names to nav identifiers
  const pageMap = {
    'index.html': 'home',
    '': 'home', // Root URL
    'schedule.html': 'schedule',
    'students.html': 'students',
    'news.html': 'news',
    'about.html': 'about'
  };
  
  const activeNav = pageMap[pageName];
  
  if (!activeNav) {
    console.log(`Partials: No nav mapping for ${pageName}`);
    return;
  }
  
  // Find and activate the matching link
  const navLink = document.querySelector(`.nav-link[data-nav="${activeNav}"]`);
  
  if (navLink) {
    navLink.classList.add('active');
    console.log(`Partials: Set active nav to ${activeNav}`);
  }
}


/**
 * setFooterYear
 * ─────────────────────────────────────────────
 * Sets the current year in the footer copyright.
 */
function setFooterYear() {
  const yearSpan = document.getElementById('year');
  if (yearSpan) {
    yearSpan.textContent = new Date().getFullYear();
  }
}


/**
 * initPartials
 * ─────────────────────────────────────────────
 * Main initialization function.
 * Loads all partials and sets up the page.
 */
async function initPartials() {
  // Load all partials
  await Promise.all(
    PARTIALS.map(p => loadPartial(p.id, p.file))
  );
  
  // After partials are loaded, set up the page
  setActiveNavLink();
  setFooterYear();
  
  // Dispatch custom event for other scripts to know partials are ready
  document.dispatchEvent(new CustomEvent('partialsLoaded'));
}


// Run when DOM is ready
document.addEventListener('DOMContentLoaded', initPartials);


/* ============================================
   END OF PARTIALS LOADER
   ============================================ */
