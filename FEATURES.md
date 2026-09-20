# TaskKarate Features

> Complete feature list for TaskKarate v4 with implementation details and usage

---

## 🎯 Homepage Features

### 1. **Free Trial Popup**
**File:** [js/trial-popup.js](js/trial-popup.js)

- Auto-appears 2.5 seconds after page load
- Hi-Yah! themed white card with 4px blue border
- Gold "Free Trial" badge
- Feature list with custom SVG checkmarks
- Tall CTA button with stacked shadow
- 30-day dismissal stored in localStorage
- Mobile responsive design

**How to customize:**
```javascript
// In js/trial-popup.js
this.showDelay = 2500;         // Delay in milliseconds
this.dismissalDuration = 30;   // Days before showing again
```

---

### 2. **Contact Modal System**
**Files:** [js/contact-modal.js](js/contact-modal.js), [css/index.css](css/index.css)

**Floating Button:**
- Appears after 300px scroll
- Fixed position bottom-right
- Purple-blue gradient with message icon
- Smooth fade-in animation

**Contact Form:**
- 3-field form (Name, Email, Message)
- Real-time validation
- Sends email via FormSubmit to taskkarateschool@gmail.com
- Success/error messaging
- White card design matching trial popup

**Email Setup (One-Time):**
1. Submit test form
2. Check email for FormSubmit confirmation
3. Click confirmation link
4. ✅ Done - all future emails auto-deliver

**How to customize:**
```javascript
// In js/contact-modal.js
this.showButtonThreshold = 300;  // Scroll pixels before button appears
this.recipientEmail = 'your-email@example.com';  // Change email recipient
```

---

### 3. **Hero Section**
**File:** [index.html](index.html), [css/index.css](css/index.css)

- Stacked paper effect with layered shadows
- Full-width cover image
- Separate title card overlay
- Paper-Fu stacked aesthetic

---

### 4. **Programs Tabbed Interface**
**File:** [js/tabs.js](js/tabs.js)

- 4 tabs: Kids, Teens/Adults, Eskrima, Weapons
- Smooth content switching
- Active state styling
- Mobile-friendly tap targets

---

### 5. **Photo Gallery**
**File:** [css/index.css](css/index.css)

- CSS Grid layout
- Responsive columns (4 → 2 → 1)
- Hover effects
- *Future: Lightbox planned (see ENHANCEMENT_PLAN.md #5)*

---

### 6. **Contact Information Tiles**
**File:** [index.html](index.html)

- Email, phone, address
- Icon + label design
- Paper-Fu card styling

---

### 7. **Google Maps Embed**
**File:** [index.html](index.html)

- Interactive map iframe
- Location marker for school

---

## 📅 Schedule Page Features

### 1. **Class Selector Dropdown**
**Files:** [js/schedule.js](js/schedule.js), [data/schedules.json](data/schedules.json)

- Dropdown to filter classes
- Loads from JSON data
- Dynamic schedule display
- Mobile responsive

### 2. **Schedule Detail Modal**
**File:** [js/schedule-modal.js](js/schedule-modal.js)

- Click class to see details
- Show time, instructor, level
- Paper-Fu modal styling

---

## 🎓 Students Page Features

### 1. **Belt Requirement Cards**
**File:** [js/belt-modal.js](js/belt-modal.js)

- 9 belt cards (White → Black)
- Click to open requirements modal
- Links to PDF testing sheets
- Hover lift animation

### 2. **FAQ Journal**
**File:** [js/journal.js](js/journal.js)

- Accordion-style FAQs
- Smooth expand/collapse
- Handwritten font for answers
- Paperclip decoration

### 3. **Dojo Code Modal**
**File:** [js/rules-modal.js](js/rules-modal.js)

- Student rules and code of conduct
- Opens from "Rules" button
- Scrollable content

### 4. **New Student Form CTA**
**File:** [students.html](students.html)

- Prominent call-to-action
- Links to new student form
- Gradient background

---

## 📰 News Page Features

### 1. **Newsletter Signup Form**
**File:** [js/newsletter.js](js/newsletter.js)

- Email signup field
- FormSubmit integration
- Success/error handling
- Form validation

### 2. **Newsletter Archive**
**File:** [news.html](news.html)

- PDF downloads by month/year
- Paper-Fu link styling
- Organized chronologically

---

## 👨‍🏫 About Page Features

### 1. **Instructor Bio Card**
**File:** [js/instructors.js](js/instructors.js)

- Photo + bio layout
- Paperclip decoration
- Credentials list
- Handwritten font accents

### 2. **Teaching Philosophy Section**
**File:** [about.html](about.html)

- Formatted text content
- Paper-Fu panel styling
- Readable typography

---

## 🧭 Global Features (All Pages)

### 1. **File Folder Navigation**
**File:** [js/navigation.js](js/navigation.js), [partials/navigation.html](partials/navigation.html)

- Unique tabbed navigation
- Active page highlighting
- Belt color underlines
- Mobile hamburger menu
- Logo spin animation on hover

**Navigation Items:**
- Home
- Schedule
- Students
- News
- About

### 2. **Belt-Colored Scroll Progress Bar**
**File:** [js/scroll-progress.js](js/scroll-progress.js)

- Fixed top progress bar
- Cycles through belt colors
- Updates as you scroll
- Smooth color transitions

**Belt Color Order:**
White → Gold → Orange → Green → Purple → Blue → Red → Brown → Black

### 3. **Back-to-Top Button**
**File:** [js/back-to-top.js](js/back-to-top.js)

- Appears after scrolling down
- Fixed bottom-left position
- Smooth scroll animation
- Paper-Fu button styling

### 4. **Partials System**
**File:** [js/partials.js](js/partials.js)

- Loads navigation.html and footer.html on all pages
- DRY (Don't Repeat Yourself) approach
- Consistent navigation across site
- Easy to update in one place

### 5. **Responsive Design**
**Files:** All CSS files

- Mobile-first approach
- Breakpoints:
  - `(max-width: 768px)` - Tablet/mobile
  - `(max-width: 480px)` - Small mobile
- Touch-friendly tap targets
- Stacked layouts on small screens

---

## 🎨 Paper-Fu Design Features

### Core Components

**Stacked Paper Effect (`.pf-stack`):**
```css
box-shadow: 
  0 2px 0 rgba(0,0,0,0.1),
  0 4px 0 rgba(0,0,0,0.08),
  0 6px 12px rgba(0,0,0,0.15);
```

**Panel (`.pf-panel`):**
- Background with subtle texture
- Border and shadow
- Padding for content

**Card (`.pf-card`):**
- Content container
- Rounded corners
- Shadow depth

**Button (`.pf-btn`):**
- Belt color variants
- Hover lift effect
- Active press state
- Stacked shadow

### Typography

**Headings:** Bebas Neue (900 weight)  
**Body:** Inter (400, 500, 600)  
**Accent:** Caveat (handwritten, 400-700)

**Scale:**
- `.pf-text-xs` - 0.75rem
- `.pf-text-sm` - 0.875rem
- `.pf-text-base` - 1rem
- `.pf-text-lg` - 1.125rem
- `.pf-text-xl` - 1.25rem
- `.pf-text-2xl` - 1.5rem
- `.pf-text-3xl` - 2rem
- `.pf-text-4xl` - 2.5rem

### Spacing System

**Variables:**
```css
--pf-space-xs: 8px;
--pf-space-sm: 16px;
--pf-space-md: 24px;
--pf-space-lg: 32px;
--pf-space-xl: 48px;
--pf-space-2xl: 64px;
```

---

## 📧 Email Integration

**Service:** FormSubmit (formsubmit.co)  
**Recipient:** taskkarateschool@gmail.com  
**Cost:** Free  
**Setup:** One-time confirmation required

**Forms Using Email:**
1. Contact Modal (homepage)
2. Newsletter Signup (news page)

**FormSubmit Features:**
- No backend required
- Works with static HTML
- Spam filtering
- Captcha option
- Custom "thank you" page redirects

---

## 🚀 Future Enhancements

See [ENHANCEMENT_PLAN.md](ENHANCEMENT_PLAN.md) for the complete 15-point improvement roadmap, including:

- Scroll-triggered animations
- Progressive image loading
- Photo gallery lightbox
- Belt progress tracker
- Dynamic class availability
- Testimonials carousel
- And more!

---

*Last Updated: 2025*
