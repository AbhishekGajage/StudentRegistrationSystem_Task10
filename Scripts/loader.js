// Shared page-loader behavior. Include on every page:
//   <script src="Scripts/loader.js"></script>
// after the #pageLoader markup (see Loader.css for the markup snippet).
(function () {
    function getLoader() {
        return document.getElementById('pageLoader');
    }

    function showLoader() {
        var el = getLoader();
        if (el) { el.classList.remove('is-hidden'); }
    }

    function hideLoader() {
        var el = getLoader();
        if (el) { el.classList.add('is-hidden'); }
    }

    // Hide once the page (images, scripts, etc.) has fully loaded.
    window.addEventListener('load', hideLoader);

    // Show it again on normal full-page postbacks (button clicks that
    // submit the ASP.NET form), so it also covers server round-trips.
    document.addEventListener('DOMContentLoaded', function () {
        var form = document.getElementById('form1');
        if (form) {
            form.addEventListener('submit', showLoader);
        }
    });

    // If the page is restored from the browser's back/forward cache,
    // make sure it isn't stuck showing the loader.
    window.addEventListener('pageshow', function (e) {
        if (e.persisted) { hideLoader(); }
    });

    // If this page uses an ASP.NET ScriptManager with UpdatePanels,
    // show/hide the loader around async (AJAX) postbacks too.
    if (window.Sys && window.Sys.WebForms && window.Sys.WebForms.PageRequestManager) {
        var prm = window.Sys.WebForms.PageRequestManager.getInstance();
        prm.add_beginRequest(showLoader);
        prm.add_endRequest(hideLoader);
    }

    // Exposed for pages that navigate via plain JS (window.location.href)
    // instead of an ASP.NET form postback — e.g. Landing.aspx's buttons.
    // Usage: onclick="PageLoader.navigate('Login.aspx'); return false;"
    window.PageLoader = {
        show: showLoader,
        hide: hideLoader,
        navigate: function (url) {
            showLoader();
            window.location.href = url;
        }
    };
})();