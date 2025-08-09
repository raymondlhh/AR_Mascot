mergeInto(LibraryManager.library, {
    ShowConfirmUrl: function(url)
    {
        window.ShowConfirmUrl(UTF8ToString(url));
    },
    ShowWebGLScreenshot: function(dataUrl)
    {
        window.ShowScreenshot(UTF8ToString(dataUrl));
    },
});
