/**
 * Schedule Loader - Dynamically populates schedule tables from JSON data
 * TaskKarate v4 - Based on v2 implementation
 */

(function() {
  'use strict';

  const DAYS_OF_WEEK = ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];

  /**
   * Load schedules from JSON and populate all tables
   */
  async function loadSchedules() {
    try {
      const response = await fetch('data/schedules.json');
      if (!response.ok) {
        throw new Error(`Failed to load schedules: ${response.status}`);
      }
      const data = await response.json();

      // Populate each schedule section
      populateKidsSchedule(data.kids);
      populateTeensAdultsSchedule(data.teensAdults);
      populateSparringSchedule(data.sparring);
      populateWeaponsSchedule(data.weapons);
      populateSpecialSchedules(data.special);
      populateEskrimaSchedule(data.eskrima);

    } catch (error) {
      console.error('Error loading schedules:', error);
      showScheduleError();
    }
  }

  /**
   * Populate Kids Karate schedule table
   */
  function populateKidsSchedule(data) {
    const tbody = document.querySelector('#kids-schedule-table tbody');
    if (!tbody) return;

    tbody.innerHTML = '';

    data.groups.forEach(group => {
      const row = createScheduleRow(group, 'Kids Karate');
      tbody.appendChild(row);
    });
  }

  /**
   * Populate Teens & Adults Karate schedule table
   */
  function populateTeensAdultsSchedule(data) {
    const tbody = document.querySelector('#teens-adults-schedule-table tbody');
    if (!tbody) return;

    tbody.innerHTML = '';

    data.groups.forEach(group => {
      const row = createScheduleRow(group, 'Teens & Adults');
      tbody.appendChild(row);
    });
  }

  /**
   * Populate Sparring schedule table
   */
  function populateSparringSchedule(data) {
    const tbody = document.querySelector('#sparring-schedule-table tbody');
    if (!tbody) return;

    tbody.innerHTML = '';

    data.groups.forEach(group => {
      const row = createScheduleRow(group, 'Sparring');
      tbody.appendChild(row);
    });
  }

  /**
   * Populate Weapons schedule table
   */
  function populateWeaponsSchedule(data) {
    const tbody = document.querySelector('#weapons-schedule-table tbody');
    if (!tbody) return;

    tbody.innerHTML = '';

    data.groups.forEach(group => {
      const row = createScheduleRow(group, 'Weapons');
      tbody.appendChild(row);
    });
  }

  /**
   * Populate Special Schedules table
   */
  function populateSpecialSchedules(data) {
    const tbody = document.querySelector('#mat-class-schedule-table tbody');
    if (!tbody) return;

    tbody.innerHTML = '';

    data.groups.forEach(group => {
      const row = createScheduleRow(group, 'Special Schedules');
      tbody.appendChild(row);
    });
  }

  /**
   * Populate IS3 Eskrima schedule table
   */
  function populateEskrimaSchedule(data) {
    const tbody = document.querySelector('#eskrima-schedule-table tbody');
    if (!tbody) return;

    tbody.innerHTML = '';

    data.groups.forEach(group => {
      const row = createScheduleRow(group, 'IS3 Eskrima', true);
      tbody.appendChild(row);
    });
  }

  /**
   * Create a schedule table row for a group
   */
  function createScheduleRow(group, program, allowSpecialNote = false) {
    const tr = document.createElement('tr');

    // Belt cell
    const beltCell = document.createElement('td');
    beltCell.className = 'col-belts';
    group.belts.forEach(belt => {
      const beltPip = document.createElement('span');
      beltPip.className = `belt-pip belt-${belt}`;
      beltPip.setAttribute('aria-label', `${capitalize(belt)} Belt`);
      beltCell.appendChild(beltPip);
    });
    tr.appendChild(beltCell);

    // Group name cell
    const nameCell = document.createElement('td');
    nameCell.innerHTML = `<strong>${group.name}</strong>`;
    tr.appendChild(nameCell);

    // Day cells
    if (allowSpecialNote && group.specialNote) {
      // Special case: Eskrima with colspan message
      const specialCell = document.createElement('td');
      specialCell.className = 'muted empty-cell';
      specialCell.setAttribute('colspan', '6');
      specialCell.textContent = group.specialNote;
      tr.appendChild(specialCell);
    } else {
      // Regular schedule cells
      DAYS_OF_WEEK.forEach(day => {
        const dayCell = document.createElement('td');
        
        if (group.schedule[day]) {
          const times = group.schedule[day];
          
          if (times.length > 1) {
            // Multiple times - use time-stack
            const timeStack = document.createElement('div');
            timeStack.className = 'time-stack';
            
            times.forEach(timeData => {
              const pill = createTimePill(timeData, group.name, program, day);
              timeStack.appendChild(pill);
            });
            
            dayCell.appendChild(timeStack);
          } else if (times.length === 1) {
            // Single time
            const pill = createTimePill(times[0], group.name, program, day);
            dayCell.appendChild(pill);
          }
        } else {
          // Empty cell
          dayCell.className = 'empty-cell';
          dayCell.textContent = '—';
        }
        
        tr.appendChild(dayCell);
      });
    }

    return tr;
  }

  /**
   * Create a time pill element
   */
  function createTimePill(timeData, groupName, program, day) {
    const pill = document.createElement('span');
    pill.className = 'time-pill';
    
    if (timeData.isHour) {
      pill.classList.add('time-pill--hour');
      pill.textContent = timeData.time + '*';
      pill.setAttribute('data-hour', 'true');
    } else {
      pill.textContent = timeData.time;
    }
    
    // Set data attributes for modal functionality
    pill.setAttribute('data-group', groupName);
    pill.setAttribute('data-program', program);
    pill.setAttribute('data-day', day);
    pill.setAttribute('data-time', timeData.time);
    
    return pill;
  }

  /**
   * Capitalize first letter
   */
  function capitalize(str) {
    return str.charAt(0).toUpperCase() + str.slice(1);
  }

  /**
   * Show error message if schedules fail to load
   */
  function showScheduleError() {
    const tables = document.querySelectorAll('.schedule-table tbody');
    tables.forEach(tbody => {
      tbody.innerHTML = '<tr><td colspan="8" class="text-center muted">Unable to load schedule data. Please refresh the page or contact us for assistance.</td></tr>';
    });
  }

  // Initialize when DOM is ready
  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', loadSchedules);
  } else {
    loadSchedules();
  }
})();
