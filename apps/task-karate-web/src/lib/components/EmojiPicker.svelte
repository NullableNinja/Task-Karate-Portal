<script lang="ts">
  import { createEventDispatcher } from 'svelte';

  export let compact = false;
  export let label = 'Add emoji';
  const dispatch = createEventDispatcher<{ select: string }>();

  type EmojiItem = { emoji: string; category: string };
  const favorites = ['🥷', '🥋', '👊', '💪', '🔥', '⭐', '🌟', '🏆', '🎉', '👏', '🙌', '😊', '😄', '😂', '🤣', '😍', '🤩', '😎', '🤔', '💯', '✅', '💙', '🙏', '✨'];
  const categories = ['Smileys', 'People', 'Animals', 'Food', 'Activity', 'Travel', 'Objects', 'Symbols', 'Flags'];
  const expand = (category: string, value: string): EmojiItem[] => value.trim().split(/\s+/).map((emoji) => ({ emoji, category }));
  const catalog: EmojiItem[] = [
    ...expand('Smileys', '😀 😃 😄 😁 😆 😅 😂 🤣 😊 😇 🙂 🙃 😉 😌 😍 🥰 😘 😗 😙 😚 😋 😛 😝 😜 🤪 🤨 🧐 🤓 😎 🤩 🥳 😏 😒 😞 😔 😟 😕 🙁 ☹️ 😣 😖 😫 😩 🥺 😢 😭 😤 😠 😡 🤬 🤯 😳 🥵 🥶 😱 😨 😰 😥 😓 🤗 🤔 🫣 🤭 🫢 🤫 🤥 😶 🫠 😐 😑 😬 🙄 😯 😦 😧 😮 😲 🥱 😴 🤤 😪 😵 🤐 🥴 🤢 🤮 🤧 😷 🤒 🤕',),
    ...expand('People', '👋 🤚 🖐️ ✋ 🖖 👌 🤏 ✌️ 🤞 🤟 🤘 🤙 👈 👉 👆 🖕 👇 ☝️ 👍 👎 ✊ 👊 🤛 🤜 👏 🙌 👐 🤲 🙏 ✍️ 💅 🤳 💪 🦾 🦿 🦵 🦶 👂 👃 🧠 🫀 🫁 🦷 🦴 👀 👁️ 👅 👄 💋 🧑 👨 👩 🧒 👦 👧 🧓 👴 👵 🧔 👱 🧑‍🏫 🧑‍🎓 🧑‍🤝‍🧑 💃 🕺 🧘 🏃 🚶 🧎 🧍 🥷 🥋'),
    ...expand('Animals', '🐶 🐱 🐭 🐹 🐰 🦊 🐻 🐼 🐨 🐯 🦁 🐮 🐷 🐸 🐵 🙈 🙉 🙊 🐒 🐔 🐧 🐦 🐤 🦆 🦅 🦉 🦇 🐺 🐗 🐴 🦄 🐝 🪲 🐛 🦋 🐌 🐞 🐜 🕷️ 🦂 🐢 🐍 🦎 🦖 🦕 🐙 🦑 🦀 🦞 🐠 🐟 🐡 🐬 🐳 🐋 🦈 🐊 🐅 🐆 🦓 🦍 🐘 🦏 🦛 🐪 🐫 🦒 🦘 🦬 🐃 🐂 🐄 🐎 🐖 🐏 🐑 🦙 🐐 🦌 🐕 🐈'),
    ...expand('Food', '🍏 🍎 🍐 🍊 🍋 🍌 🍉 🍇 🍓 🫐 🍈 🍒 🍑 🥭 🍍 🥥 🥝 🍅 🥑 🍆 🥔 🥕 🌽 🌶️ 🫑 🥒 🥬 🥦 🧄 🧅 🍄 🥜 🌰 🍞 🥐 🥖 🫓 🥨 🧀 🥚 🍳 🧈 🥞 🧇 🥓 🥩 🍗 🍔 🍟 🍕 🌭 🥪 🌮 🌯 🥗 🍿 🧂 🍱 🍣 🍙 🍚 🍜 🍲 🍛 🍝 🍰 🎂 🧁 🍩 🍪 🍫 🍬 🍭 ☕ 🧋 🥤 🧃 🍺 🍻 🍷 🥂 🍹'),
    ...expand('Activity', '⚽ 🏀 🏈 ⚾ 🥎 🎾 🏐 🏉 🥏 🎱 🪀 🪁 🏓 🏸 🏒 🏑 🥍 🏏 ⛳ 🏹 🎣 🤿 🥊 🥋 🎽 🛹 🛷 ⛸️ 🥌 🎿 ⛷️ 🏂 🪂 🏋️ 🤼 🤸 ⛹️ 🤺 🤾 🏌️ 🧘 🏄 🏊 🚴 🚵 🧗 🧭 🏆 🥇 🥈 🥉 🏅 🎖️ 🎗️ 🎫 🎟️ 🎪 🎭 🎨 🎬 🎤 🎧 🎼 🎹 🥁 🎷 🎺 🎸 🎻'),
    ...expand('Travel', '🚗 🚕 🚙 🚌 🚎 🏎️ 🚓 🚑 🚒 🚐 🛻 🚚 🚛 🚜 🛵 🏍️ 🚲 🛴 🚨 🚔 🚍 🚘 🚖 ✈️ 🛫 🛬 🛩️ 🚀 🛸 🚁 🛶 ⛵ 🚤 🛥️ 🛳️ 🚢 ⚓ ⛽ 🚧 🗺️ 🗿 🗽 🗼 🏰 🏯 🏟️ 🎡 🎢 🎠 🏖️ 🏝️ 🏜️ 🏕️ ⛺ 🏠 🏫 🏥 🏯'),
    ...expand('Objects', '⌚ 📱 💻 ⌨️ 🖥️ 🖨️ 🖱️ 💽 💾 💿 📷 📸 📹 🎥 ☎️ 📞 📺 📻 🎙️ 🔋 🔌 💡 🔦 🕯️ 🧯 🛒 🚪 🪑 🛏️ 🛋️ 🧸 🪆 🎁 🎈 🎉 🎊 ✉️ 📧 📦 📚 📖 📝 ✏️ 🖊️ 📌 📍 📎 ✂️ 🔒 🔑 🔨 ⚒️ 🛠️ ⚙️ 🧰 🧲 🧪 🔬 🔭 📅 📆 ⏰ ⏱️ ⌛ 💰 💳 🧾 🏷️'),
    ...expand('Symbols', '❤️ 🧡 💛 💚 💙 💜 🖤 🤍 🤎 💔 ❣️ 💕 💞 💓 💗 💖 💘 💝 💟 ✨ ⭐ 🌟 💫 ⚡ 💥 💦 🔥 🎯 💬 💭 💤 ✅ ☑️ ❌ ❗ ❓ ⁉️ ‼️ ⁇ ⚠️ 🚫 ⛔ 🔴 🟠 🟡 🟢 🔵 🟣 ⚫ ⚪ 🟤 🔷 🔶 🔹 🔸 🔺 🔻 💠 🔘 ✔️ ➕ ➖ ✖️ ➗ ♻️ 🔱 ☮️ ☯️ ☢️ ☣️ ⚜️ ©️ ®️ ™️'),
    ...expand('Flags', '🏳️ 🏴 🏁 🚩 🏳️‍🌈 🏳️‍⚧️ 🇺🇸 🇨🇦 🇲🇽 🇬🇧 🇮🇪 🇫🇷 🇩🇪 🇮🇹 🇪🇸 🇧🇷 🇦🇺 🇯🇵 🇰🇷 🇨🇳 🇮🇳 🇵🇭 🇹🇭 🇻🇳 🇳🇿 🇿🇦 🇺🇦 🇮🇱 🇵🇷')
  ];
  let open = false;
  let tab = 'Favorites';
  let search = '';
  $: allEmojis = [...new Map(catalog.map((item) => [item.emoji, item])).values()];
  $: visibleEmojis = (tab === 'Favorites' ? favorites.map((emoji) => ({ emoji, category: 'Favorites' })) : allEmojis.filter((item) => item.category === tab || tab === 'All'))
    .filter((item, index, list) => list.findIndex((candidate) => candidate.emoji === item.emoji) === index)
    .filter((item) => !search.trim() || item.emoji.includes(search.trim()));
  function choose(emoji: string) { dispatch('select', emoji); open = false; search = ''; }
</script>

<span class="emoji-picker" class:compact>
  <button class="emoji-trigger" type="button" aria-label={label} aria-expanded={open} on:click={() => open = !open}>😊</button>
  {#if open}<div class="emoji-panel" role="dialog" tabindex="-1" aria-label="Emoji picker" on:click|stopPropagation on:keydown|stopPropagation={() => undefined}>
    <div class="emoji-panel-head"><strong>EMOJIS</strong><input bind:value={search} type="search" placeholder="Search or paste an emoji" aria-label="Search emojis" /></div>
    <div class="emoji-tabs" role="tablist" aria-label="Emoji categories"><button class:active={tab === 'Favorites'} type="button" on:click={() => tab = 'Favorites'}>★ Favorites</button><button class:active={tab === 'All'} type="button" on:click={() => tab = 'All'}>All</button>{#each categories as category}<button class:active={tab === category} type="button" on:click={() => tab = category}>{category}</button>{/each}</div>
    <div class="emoji-grid">{#each visibleEmojis as item}<button type="button" title={item.emoji} on:click={() => choose(item.emoji)}>{item.emoji}</button>{:else}<small>No emoji matched that search.</small>{/each}</div>
  </div>{/if}
</span>

<style>
  .emoji-picker { position: relative; display: inline-flex; flex: none; }
  .emoji-trigger { min-width: 38px; min-height: 38px; padding: 0 8px; border: 1px solid var(--social-border-strong, #4c9ab9); border-radius: 9px; color: inherit; background: var(--social-surface-soft, #0c2038); cursor: pointer; font-size: 1.15rem; }
  .emoji-trigger:hover, .emoji-trigger[aria-expanded="true"] { border-color: var(--social-accent, #55d3ff); background: var(--social-accent-soft, #173f57); }
  .emoji-panel { position: absolute; right: 0; bottom: calc(100% + 8px); z-index: 130; display: grid; gap: 9px; width: min(360px, calc(100vw - 28px)); padding: 12px; border: 1px solid var(--social-border-strong, #4c9ab9); border-radius: 12px; color: var(--social-text, #f3f7fc); background: var(--social-surface-strong, #153451); box-shadow: 0 18px 45px #0008; }
  .emoji-panel-head { display: grid; grid-template-columns: auto minmax(0, 1fr); align-items: center; gap: 8px; }
  .emoji-panel-head strong { color: var(--social-accent, #55d3ff); font-size: .68rem; letter-spacing: .12em; }
  .emoji-panel-head input { min-width: 0; min-height: 32px; padding: 0 8px; border: 1px solid var(--social-border, #4c9ab9); border-radius: 7px; color: inherit; background: var(--social-bg, #081a2f); }
  .emoji-tabs { display: flex; gap: 4px; overflow-x: auto; padding-bottom: 2px; }
  .emoji-tabs button { flex: none; padding: 5px 7px; border: 1px solid transparent; border-radius: 6px; color: var(--social-muted, #aebfd0); background: transparent; cursor: pointer; font: inherit; font-size: .66rem; font-weight: 800; white-space: nowrap; }
  .emoji-tabs button:hover, .emoji-tabs button.active { border-color: var(--social-border, #4c9ab9); color: var(--social-text, #f3f7fc); background: var(--social-accent-soft, #173f57); }
  .emoji-grid { display: grid; grid-template-columns: repeat(9, 1fr); gap: 3px; max-height: 220px; overflow-y: auto; }
  .emoji-grid button { display: grid; place-items: center; min-width: 30px; min-height: 30px; padding: 0; border: 1px solid transparent; border-radius: 6px; background: transparent; cursor: pointer; font-size: 1.2rem; }
  .emoji-grid button:hover, .emoji-grid button:focus-visible { border-color: var(--social-accent, #55d3ff); background: var(--social-accent-soft, #173f57); outline: none; }
  .emoji-grid small { grid-column: 1 / -1; padding: 12px; color: var(--social-muted, #aebfd0); text-align: center; }
  .compact .emoji-trigger { min-width: 34px; min-height: 34px; }
  @media (max-width: 600px) { .emoji-panel { position: fixed; right: 10px; bottom: 70px; } }
</style>
