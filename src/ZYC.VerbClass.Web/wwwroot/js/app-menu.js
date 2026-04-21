class AppMenu extends HTMLElement {
  #items = [];

  set items(value) {
    this.#items = Array.isArray(value) ? value : [];
    this.render();
  }

  get items() {
    return this.#items;
  }

  constructor() {
    super();
    this.attachShadow({ mode: 'open' });
    this.handleDocumentClick = this.handleDocumentClick.bind(this);
  }

  connectedCallback() {
    document.addEventListener('click', this.handleDocumentClick);
    this.render();
  }

  disconnectedCallback() {
    document.removeEventListener('click', this.handleDocumentClick);
  }

  handleDocumentClick(event) {
    if (event.composedPath().includes(this)) {
      return;
    }

    this.closeAll();
  }

  closeAll() {
    this.shadowRoot?.querySelectorAll('.menu-item.open').forEach(item => {
      item.classList.remove('open');
    });
  }

  toggleItem(itemElement) {
    const isOpen = itemElement.classList.contains('open');
    const parentMenu = itemElement.parentElement;
    if (parentMenu) {
      Array.from(parentMenu.children).forEach(sibling => {
        if (sibling !== itemElement && sibling instanceof HTMLElement) {
          sibling.classList.remove('open');
        }
      });
    }

    itemElement.classList.toggle('open', !isOpen);
  }

  createMenu(items, isRoot = false) {
    const menu = document.createElement('ul');
    menu.className = isRoot ? 'menu-root' : 'submenu';

    for (const item of items) {
      const menuItem = document.createElement('li');
      menuItem.className = 'menu-item';

      const button = document.createElement('button');
      button.type = 'button';
      button.textContent = item.label ?? '';

      const children = Array.isArray(item.children) ? item.children : [];
      if (children.length > 0) {
        menuItem.classList.add('has-submenu');
        button.addEventListener('click', event => {
          event.stopPropagation();
          this.toggleItem(menuItem);
        });

        menuItem.appendChild(button);
        menuItem.appendChild(this.createMenu(children));
      } else {
        button.addEventListener('click', event => {
          event.stopPropagation();
          this.closeAll();
          this.dispatchEvent(new CustomEvent('menu-select', {
            detail: item,
            bubbles: true,
            composed: true
          }));
        });

        menuItem.appendChild(button);
      }

      menu.appendChild(menuItem);
    }

    return menu;
  }

  render() {
    if (!this.shadowRoot) {
      return;
    }

    this.shadowRoot.innerHTML = `
      <style>
        :host {
          all: initial;
          display: block;
          color-scheme: light;
          color: #1f2937;
          font-family: "Segoe UI", "Microsoft YaHei UI", sans-serif;
          font-size: 14px;
          font-weight: 400;
          line-height: 1.4;
          isolation: isolate;
          text-align: left;
          user-select: none;
        }

        :host,
        :host *,
        :host *::before,
        :host *::after {
          box-sizing: border-box;
        }

        .menu-bar,
        .menu-root,
        .submenu {
          margin: 0;
          padding: 0;
        }

        .menu-root,
        .submenu {
          list-style: none;
        }

        .menu-root {
          display: flex;
          align-items: center;
        }

        .menu-item {
          position: relative;
        }

        .menu-item > button {
          appearance: none;
          -webkit-appearance: none;
          width: 100%;
          min-width: 12rem;
          border: 0;
          border-radius: 0;
          background: transparent;
          cursor: pointer;
          color: inherit;
          font: inherit;
          line-height: inherit;
          margin: 0;
          padding: 0.7rem 0.9rem;
          text-align: left;
          text-decoration: none;
          text-transform: none;
          white-space: nowrap;
        }

        .menu-root > .menu-item > button {
          min-width: auto;
        }

        .menu-item > button:hover,
        .menu-item > button:focus-visible {
          background: rgba(15, 23, 42, 0.08);
          outline: none;
        }

        .submenu {
          display: none;
          position: absolute;
          top: 100%;
          left: 0;
          min-width: 12rem;
          background: #ffffff;
          border: 1px solid rgba(15, 23, 42, 0.12);
          border-radius: 0;
          box-shadow: none;
          z-index: 1000;
        }

        .submenu .submenu {
          top: 0;
          left: calc(100% - 0.25rem);
        }

        .menu-item.open > .submenu {
          display: block;
        }

        .menu-root > .has-submenu > button::after {
          content: " ▼";
          font-size: 0.72em;
        }

        .submenu .has-submenu > button::after {
          content: " ▶";
          float: right;
          font-size: 0.72em;
        }
      </style>
      <div class="menu-bar"></div>
    `;

    const menuBar = this.shadowRoot.querySelector('.menu-bar');
    menuBar?.appendChild(this.createMenu(this.#items, true));
  }
}

if (!customElements.get('app-menu')) {
  customElements.define('app-menu', AppMenu);
}
