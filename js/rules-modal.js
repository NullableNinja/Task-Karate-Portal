/**
 * Dojo Rules Modal
 * ──────────────────────────────────────────────
 * Opens a Paper-Fu modal with elaboration on each
 * of the dojo rules when clicked.
 * 
 * @version 1.0.0
 * @file js/rules-modal.js
 */

(function() {
  'use strict';
  
  // Rule elaborations - have fun with these!
  const ruleContent = {
    1: {
      number: '1',
      title: "Don't use it the wrong way!",
      content: `
        <p class="rules-modal__text">
          <strong>This is Rule #1 for a reason.</strong> Martial arts gives you 
          power—the power to defend yourself and others. But with great power 
          comes great responsibility! 🦸
        </p>
        <p class="rules-modal__text">
          <strong>What this means:</strong>
        </p>
        <ul class="rules-modal__list">
          <li>Never use karate to bully or intimidate</li>
          <li>Only use skills for self-defense as a last resort</li>
          <li>Protect those who cannot protect themselves</li>
          <li>Walk away from fights—real strength is self-control</li>
        </ul>
        <p class="rules-modal__quote">
          "The ultimate aim of karate lies not in victory or defeat, but in the 
          perfection of character." — Gichin Funakoshi
        </p>
      `,
      stamp: 'RULE #1'
    },
    2: {
      number: '2',
      title: 'Be respectful!',
      content: `
        <p class="rules-modal__text">
          <strong>Respect is the foundation of martial arts.</strong> We bow when 
          entering and leaving the dojo. We address instructors properly. We treat 
          our training partners with care. 🙏
        </p>
        <p class="rules-modal__text">
          <strong>Respect means:</strong>
        </p>
        <ul class="rules-modal__list">
          <li>Listen when others are speaking</li>
          <li>Follow instructions the first time</li>
          <li>Take care of your uniform and equipment</li>
          <li>Encourage your fellow students</li>
          <li>Respect yourself—you're worth the effort!</li>
        </ul>
        <p class="rules-modal__quote">
          "Respect is earned, honor is given, and loyalty is demonstrated."
        </p>
      `,
      stamp: 'OSS!'
    },
    3: {
      number: '3',
      title: 'Do your best!',
      content: `
        <p class="rules-modal__text">
          <strong>Your best is always good enough.</strong> We don't compare 
          ourselves to others—only to who we were yesterday. Progress over 
          perfection! 💪
        </p>
        <p class="rules-modal__text">
          <strong>Doing your best means:</strong>
        </p>
        <ul class="rules-modal__list">
          <li>Giving 100% effort in every class</li>
          <li>Practicing at home (even just 5 minutes helps!)</li>
          <li>Asking questions when you don't understand</li>
          <li>Getting back up when you fall</li>
          <li>Celebrating small victories</li>
        </ul>
        <p class="rules-modal__quote">
          "A black belt is just a white belt who never gave up."
        </p>
      `,
      stamp: 'KIAI!'
    },
    bonus: {
      number: '★',
      title: 'Have fun! 🎉',
      content: `
        <p class="rules-modal__text">
          <strong>This is the secret sauce!</strong> If you're not having fun, 
          you're doing it wrong. Karate should make you smile, laugh, and feel 
          amazing! 😄
        </p>
        <p class="rules-modal__text">
          <strong>Fun in karate looks like:</strong>
        </p>
        <ul class="rules-modal__list">
          <li>Making friends with your training partners</li>
          <li>Laughing when you mess up (everyone does!)</li>
          <li>Getting excited about new techniques</li>
          <li>Playing karate games and challenges</li>
          <li>Looking forward to the next class</li>
        </ul>
        <p class="rules-modal__quote">
          "The more you sweat in training, the less you bleed in combat... 
          but also, have a blast while you're sweating!" — TASK Philosophy
        </p>
      `,
      stamp: 'BONUS!'
    }
  };
  
  // Cache DOM elements
  const modal = document.getElementById('rules-modal');
  const modalNumber = document.getElementById('rules-modal-number');
  const modalTitle = document.getElementById('rules-modal-title');
  const modalContent = document.getElementById('rules-modal-content');
  const closeBtn = document.getElementById('rules-modal-close');
  const backdrop = modal?.querySelector('.rules-modal__backdrop');
  const stamp = modal?.querySelector('.rules-modal__stamp-text');
  
  if (!modal) return;
  
  // Get all rule buttons
  const ruleButtons = document.querySelectorAll('.dojo-rule[data-rule]');
  
  /**
   * Open the modal with a specific rule's content
   * @param {string} ruleId - Rule ID (1, 2, 3, or 'bonus')
   */
  function openModal(ruleId) {
    const rule = ruleContent[ruleId];
    if (!rule) return;
    
    // Set content
    if (modalNumber) modalNumber.textContent = rule.number;
    if (modalTitle) modalTitle.textContent = rule.title;
    if (modalContent) modalContent.innerHTML = rule.content;
    if (stamp) stamp.textContent = rule.stamp;
    
    // Add bonus class for special styling
    modal.classList.toggle('rules-modal--bonus', ruleId === 'bonus');
    
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
  }
  
  // Rule button click handlers
  ruleButtons.forEach(btn => {
    btn.addEventListener('click', () => {
      const ruleId = btn.dataset.rule;
      if (ruleId) openModal(ruleId);
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
