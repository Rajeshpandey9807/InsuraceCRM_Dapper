// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

(() => {
    const initSideNavToggle = () => {
        const toggleButton = document.getElementById('sideNavToggle');
        const body = document.body;
        const overlay = document.getElementById('sideNavOverlay');
        const breakpoint = window.matchMedia('(max-width: 991.98px)');

        if (!toggleButton) {
            return;
        }

        const updateAriaState = () => {
            const isMobile = breakpoint.matches;
            const navVisible = isMobile
                ? body.classList.contains('side-nav-open')
                : !body.classList.contains('side-nav-collapsed');
            toggleButton.setAttribute('aria-expanded', navVisible ? 'true' : 'false');
        };

        const closeNav = () => {
            if (breakpoint.matches) {
                body.classList.remove('side-nav-open');
            } else {
                body.classList.add('side-nav-collapsed');
            }
            updateAriaState();
        };

        const handleToggle = () => {
            if (breakpoint.matches) {
                const willOpen = !body.classList.contains('side-nav-open');
                body.classList.toggle('side-nav-open', willOpen);
                body.classList.remove('side-nav-collapsed');
            } else {
                body.classList.toggle('side-nav-collapsed');
                body.classList.remove('side-nav-open');
            }
            updateAriaState();
        };

        const handleBreakpointChange = (event) => {
            body.classList.remove('side-nav-open');
            if (!event.matches) {
                body.classList.remove('side-nav-collapsed');
            }
            updateAriaState();
        };

        toggleButton.addEventListener('click', handleToggle);
        overlay?.addEventListener('click', closeNav);
        document.addEventListener('keydown', (event) => {
            if (event.key === 'Escape' && body.classList.contains('side-nav-open')) {
                closeNav();
            }
        });
        breakpoint.addEventListener('change', handleBreakpointChange);
        updateAriaState();
    };

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initSideNavToggle);
    } else {
        initSideNavToggle();
    }
})();
