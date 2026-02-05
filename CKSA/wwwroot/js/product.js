function PrintMe() {
    window.print();
}

(function () {
    window.PinIt = window.PinIt || { loaded: false };
    if (window.PinIt.loaded) return;
    window.PinIt.loaded = true;
    function async_load() {
        var s = document.createElement("script");
        s.type = "text/javascript";
        s.async = true;
        s.src = "//assets.pinterest.com/js/pinit.js";
        var x = document.getElementsByTagName("script")[0];
        x.parentNode.insertBefore(s, x);
    }
    if (window.attachEvent)
        window.attachEvent("onload", async_load);
    else
        window.addEventListener("load", async_load, false);
})();

!function (d, s, id) { var js, fjs = d.getElementsByTagName(s)[0]; if (!d.getElementById(id)) { js = d.createElement(s); js.id = id; js.src = "https://platform.twitter.com/widgets.js"; fjs.parentNode.insertBefore(js, fjs); } } (document, "script", "twitter-wjs");