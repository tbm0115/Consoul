/* Progressive enhancements: navigation and examples work without JavaScript. */
(() => {
  const nav = document.querySelector('.nav-panel');
  const compact = window.matchMedia('(max-width: 760px)');
  const setNav = () => { nav.open = !compact.matches; };
  setNav();
  compact.addEventListener('change', setNav);

  const toc = document.querySelector('.page-toc');
  const tocList = document.querySelector('#page-toc-list');
  document.querySelectorAll('.content h2[id]').forEach(heading => {
    const item = document.createElement('li');
    const link = document.createElement('a');
    link.href = `#${heading.id}`;
    link.textContent = heading.textContent;
    item.append(link);
    tocList.append(item);
  });
  toc.hidden = !tocList.children.length;

  if (navigator.clipboard && window.isSecureContext) {
    document.querySelectorAll('pre > code').forEach(code => {
      const button = document.createElement('button');
      button.type = 'button';
      button.className = 'copy-code';
      button.textContent = 'Copy';
      button.setAttribute('aria-label', 'Copy code example');
      button.setAttribute('aria-live', 'polite');
      button.addEventListener('click', async () => {
        try {
          await navigator.clipboard.writeText(code.textContent);
          button.textContent = 'Copied!';
        } catch {
          button.textContent = 'Select and copy';
        }
        window.setTimeout(() => { button.textContent = 'Copy'; }, 2000);
      });
      code.parentElement.append(button);
      code.parentElement.classList.add('has-copy');
    });
  }

  const input = document.querySelector('#docs-search');
  const results = document.querySelector('#search-results');
  const status = document.querySelector('#search-status');
  let loadIndex;
  document.querySelector('.search').hidden = false;
  input.addEventListener('input', async () => {
    const query = input.value.trim().toLowerCase();
    results.replaceChildren();
    results.hidden = true;
    status.textContent = '';
    if (!query) return;
    try {
      status.textContent = 'Searching…';
      if (!loadIndex) {
        loadIndex = fetch(input.dataset.index).then(response => {
          if (!response.ok) throw new Error('Search index unavailable');
          return response.json();
        });
      }
      const index = await loadIndex;
      if (input.value.trim().toLowerCase() !== query) return;
      const terms = query.split(/\s+/);
      const matches = index.filter(page => {
        const haystack = `${page.title} ${page.description} ${page.text}`.toLowerCase();
        return terms.every(term => haystack.includes(term));
      }).sort((a, b) => Number(b.title.toLowerCase().includes(query)) - Number(a.title.toLowerCase().includes(query)));
      results.replaceChildren();
      matches.forEach(page => {
        const item = document.createElement('li');
        const link = document.createElement('a');
        link.href = page.url;
        link.textContent = page.title;
        item.append(link);
        results.append(item);
      });
      results.hidden = !matches.length;
      status.textContent = matches.length ? `${matches.length} guide${matches.length === 1 ? '' : 's'} found.` : 'No guides found. Try “table”, “prompt”, or “view”.';
    } catch {
      loadIndex = undefined;
      if (input.value.trim().toLowerCase() === query) {
        status.textContent = 'Search is unavailable. Browse the guides below.';
      }
    }
  });
})();
