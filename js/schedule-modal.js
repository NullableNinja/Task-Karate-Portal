/**
 * Schedule Modal - Time pill click interactions
 * TaskKarate v4 - Based on v2 implementation
 */

(function () {
  'use strict';

  const CLASS_DESCRIPTIONS = {
    // Kids Karate
    'White • Gold': {
      title: 'White & Gold – Kids Karate',
      body: [
        '<strong>Beginner karate for kids.</strong>',
        'Foundations of stance, basic strikes, blocks, and dojo manners in a fun, low-pressure environment.'
      ]
    },
    'Orange': {
      title: 'Orange Belt – Kids Karate',
      body: [
        'Builds on beginner skills with stronger basics, light combinations, and more focus on self-control and confidence.'
      ]
    },
    'Green • Purple': {
      title: 'Green & Purple – Kids Karate',
      body: [
        'Intermediate level. Students start linking techniques together, practicing combinations, and building cardio and discipline.'
      ]
    },
    'Blue • Red': {
      title: 'Blue & Red – Kids Karate',
      body: [
        'Higher-intensity drills with emphasis on precision, balance, and strong basics under light fatigue.'
      ]
    },
    'Brown': {
      title: 'Brown Belt – Kids Karate',
      body: [
        'Pre-black-belt training. Students refine techniques, mentor younger belts, and focus on leadership and responsibility.'
      ]
    },
    'Black': {
      title: 'Black Belt – Kids Karate',
      body: [
        "Black belts continue to refine fundamentals, assist in classes, and model the dojo's core values for younger students."
      ]
    },

    // Teens & Adults
    'White • Gold • Orange': {
      title: 'White • Gold • Orange – Teens & Adults',
      body: [
        'Beginner-level class for new and early-rank teens and adults.',
        'Builds strong basics, self-defense concepts, and confidence.'
      ]
    },
    'Green Belt & Up': {
      title: 'Green Belt & Up – Teens & Adults',
      body: [
        'Intermediate-level class. Stronger combinations, partner drills, and technique refinement.'
      ]
    },
    'Brown (Teens)': {
      title: 'Brown Belt – Teens & Adults',
      body: [
        'Pre–black belt work for older students. Stronger combinations, leadership, and controlled contact drills.'
      ]
    },
    'Black (Teens)': {
      title: 'Black Belt – Teens & Adults',
      body: [
        'Advanced-level material, high-intensity drills, and black-belt prep work.'
      ]
    },

    // Sparring
    'Kids Sparring': {
      title: 'Kids Sparring',
      body: [
        '<strong>Controlled contact sparring for kids.</strong>',
        'Students apply techniques safely with appropriate gear.',
        '<strong>Required gear:</strong> helmet, gloves, boots, chest protector, mouth guard.'
      ]
    },
    'Teens & Adults Sparring': {
      title: 'Teens & Adults Sparring',
      body: [
        'Higher-intensity sparring for older students with full control and strict safety.',
        '<strong>Required gear:</strong> full sparring gear.'
      ]
    },

    // Weapons
    'Kids Weapons (Green & up)': {
      title: 'Kids Weapons – Green & Up',
      body: [
        'Safe, structured weapons training for kids Green and above.',
        'Basic bo, nunchaku, and striking patterns.'
      ]
    },
    'Weapons (Green & up — Teens & Adults)': {
      title: 'Weapons – Teens & Adults',
      body: [
        'Traditional weapons training with bo, escrima, and drills connecting weapons to empty-hand skills.'
      ]
    },

    // Eskrima
    'Kids Eskrima': {
      title: 'Kids IS3 Eskrima',
      body: [
        'Filipino martial arts for kids with padded sticks and footwork drills.'
      ]
    },
    'IS3 Eskrima Program': {
      title: 'IS3 Eskrima Program',
      body: [
        'Authentic Inayan System III Eskrima training for teens and adults.',
        'Covers the seven IS3 styles passed down from Mangisursuro Mike Inay and Suro Emanuel Hart.'
      ]
    },

    // Mat Class & Special
    'Mat Class (Mixed Kids & Adults)': {
      title: 'Mat Class',
      body: [
        'Groundwork and grappling with emphasis on safety, position, and control.',
        'Mixed class for kids and adults.'
      ]
    },
    'Promotion Testing (Teens & Adults)': {
      title: 'Promotion Testing',
      body: [
        'Belt testing for teens and adults.',
        'Testing is by invitation only and typically occurs on the last Saturday of the month.'
      ]
    }
  };

  function buildDescription(group, program, isHour) {
    const base = CLASS_DESCRIPTIONS[group];
    const lines = [];

    if (base && Array.isArray(base.body)) {
      lines.push(...base.body);
    } else {
      // Generic fallback
      lines.push(
        `<strong>${group}</strong>`,
        'Regularly scheduled class at this day and time. Talk to Mr. Thomson if you have questions about which classes are right for you.'
      );
    }

    if (isHour) {
      lines.push('<em>This is a 60-minute class.</em>');
    }

    if (program) {
      lines.push(`<span class="muted">Program: ${program}</span>`);
    }

    return lines.map(p => `<p>${p}</p>`).join('');
  }

  function getTitle(group, day, time) {
    const def = CLASS_DESCRIPTIONS[group];
    const baseTitle = def && def.title ? def.title : group;
    if (!day || !time) {
      return baseTitle;
    }
    return `${baseTitle} – ${day} at ${time}`;
  }

  function initModal() {
    const modal = document.getElementById('classModal');
    if (!modal) return;

    const titleEl = modal.querySelector('#modalTitle');
    const bodyEl = modal.querySelector('#modalBody');
    const beltBar = modal.querySelector('#modalBeltBar');
    const closeBtn = modal.querySelector('[data-modal-close]');
    const backdrop = modal.querySelector('.pf-modal-backdrop');

    // Belt color mapping
    const BELT_COLORS = {
      'white': '#f8fafc',
      'gold': '#eab308',
      'orange': '#f97316',
      'green': '#22c55e',
      'purple': '#a855f7',
      'blue': '#3b82f6',
      'red': '#ef4444',
      'brown': '#92400e',
      'black': '#171717'
    };

    function getBeltColor(group) {
      const groupLower = group.toLowerCase();
      for (const [belt, color] of Object.entries(BELT_COLORS)) {
        if (groupLower.includes(belt)) return color;
      }
      return BELT_COLORS.blue; // Default
    }

    function openModal(group, program, day, time, isHour) {
      titleEl.textContent = getTitle(group, day, time);
      bodyEl.innerHTML = buildDescription(group, program, isHour);
      
      // Set belt bar color based on group
      if (beltBar) {
        beltBar.style.background = getBeltColor(group);
      }

      modal.classList.add('is-open');
      modal.setAttribute('aria-hidden', 'false');
    }

    function closeModal() {
      modal.classList.remove('is-open');
      modal.setAttribute('aria-hidden', 'true');
    }

    // Delegate clicks from any .time-pill
    document.body.addEventListener('click', (evt) => {
      const pill = evt.target.closest('.time-pill');
      if (!pill) return;

      const group = pill.dataset.group || 'Class';
      const program = pill.dataset.program || '';
      const day = pill.dataset.day || '';
      const time = pill.dataset.time || '';
      const isHour =
        pill.dataset.hour === 'true' ||
        pill.classList.contains('time-pill--hour');

      openModal(group, program, day, time, isHour);
    });

    // Close button
    if (closeBtn) {
      closeBtn.addEventListener('click', closeModal);
    }

    // Click on backdrop
    if (backdrop) {
      backdrop.addEventListener('click', closeModal);
    }
    
    // Also close if clicking modal container (outside stack)
    modal.addEventListener('click', (evt) => {
      if (evt.target === modal) {
        closeModal();
      }
    });

    // ESC key
    document.addEventListener('keydown', (evt) => {
      if (evt.key === 'Escape' && modal.classList.contains('is-open')) {
        closeModal();
      }
    });
  }

  document.addEventListener('DOMContentLoaded', initModal);
})();
