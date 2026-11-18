// Tiny localStorage wrapper used from C# via IJSRuntime.
window.animeCalc = {
    load: function (key) {
        try { return window.localStorage.getItem(key); } catch { return null; }
    },
    save: function (key, value) {
        try { window.localStorage.setItem(key, value); } catch { /* ignore */ }
    },
    remove: function (key) {
        try { window.localStorage.removeItem(key); } catch { /* ignore */ }
    }
};
