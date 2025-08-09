mergeInto(LibraryManager.library, {

	StartWebGLiTracker: function(ids, name)
	{
        if(!window.iTracker){
            console.error('%ciTracker not found! Please make sure to use the iTracker WebGLTemplate in your ProjectSettings','font-size: 32px; font-weight: bold');
            throw new Error("Tracker not found! Please make sure to use the iTracker WebGLTemplate in your ProjectSettings");
            return;
        }

    	window.iTracker.startTracker(UTF8ToString(ids), UTF8ToString(name));
    },
    StopWebGLiTracker: function()
	{
    	window.iTracker.stopTracker();
    },
    IsWebGLiTrackerReady: function()
    {
        return window.iTracker != null;
    },
    SetWebGLiTrackerSettings: function(settings)
	{
    	window.iTracker.setTrackerSettings(UTF8ToString(settings), "1.7.1.429689");
    },
    DebugImageTarget: function(id)
    {
        window.iTracker.debugImageTarget(UTF8ToString(id));
    },
    IsWebGLImageTracked: function(id)
    {
        return window.iTracker.isImageTracked(id);
    },
    GetWebGLWarpedTexture: function(targetId, textureId, resolution)
    {
        var canvasId = 'iTrackerWarpedTextureCanvas';
        var textureCanvas = window.iTracker.GetWebGLWarpedTexture(UTF8ToString(targetId), canvasId, resolution);

        //var textureCanvas = document.getElementById(canvasId);
        textureObj = GL.textures[textureId];

        if (textureCanvas == null || textureObj == null) return;      

        GLctx.bindTexture(GLctx.TEXTURE_2D, textureObj);
        GLctx.texParameteri(GLctx.TEXTURE_2D, GLctx.TEXTURE_WRAP_S, GLctx.CLAMP_TO_EDGE);
        GLctx.texParameteri(GLctx.TEXTURE_2D, GLctx.TEXTURE_WRAP_T, GLctx.CLAMP_TO_EDGE);
        GLctx.texParameteri(GLctx.TEXTURE_2D, GLctx.TEXTURE_MIN_FILTER, GLctx.LINEAR);
        GLctx.pixelStorei(gl.UNPACK_FLIP_Y_WEBGL, true); 
        GLctx.texSubImage2D(GLctx.TEXTURE_2D, 0, 0, 0, GLctx.RGBA, GLctx.UNSIGNED_BYTE, textureCanvas);
        GLctx.pixelStorei(GLctx.UNPACK_FLIP_Y_WEBGL, false);

        //console.log("WebGLGetWarpedImageTexture " + textureId, textureCanvas);
    },
});
