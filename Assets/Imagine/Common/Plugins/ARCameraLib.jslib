mergeInto(LibraryManager.library, {
    SetWebGLARCameraSettings: function(settings)
	{
    	window.arCamera.setARCameraSettings(UTF8ToString(settings));
    },
    WebGLStartCamera: function()
    {
        window.StartWebcam();
    },
    WebGLIsCameraStarted: function()
    {
        if(!window.arCamera){
            console.error('%carCamera not found! Please make sure to use the correct WebGLTemplate in your ProjectSettings','font-size: 32px; font-weight: bold');
            throw new Error("arCamera not found! Please make sure to use the correct WebGLTemplate in your ProjectSettings");
            return;
        }
        return arCamera.isCameraStarted;
    },
    WebGLGetCameraFov: function()
    {
        return window.arCamera.FOV;
    },
    WebGLUnpauseCamera: function()
	{
    	window.arCamera.unpauseCamera();
    },
    WebGLPauseCamera: function()
	{
    	window.arCamera.pauseCamera();
    },
    WebGLFlipCamera: function(){
        window.FlipCam();
    },
    WebGLIsCameraFlipped: function(){
        return window.arCamera.videoCanvas.style.transform == "scaleX(-1)";
    },
    WebGLGetVideoDims: function()
    {
        var data = window.arCamera.getVideoDims();
        var bufferSize = lengthBytesUTF8(data) + 1;
        var buffer =  unityInstance.Module._malloc(bufferSize);
        stringToUTF8(data, buffer, bufferSize);
        return buffer;
    },
    WebGLSubscribeVideoTexturePtr: function(textureId){
        arCamera.updateUnityVideoTextureCallback = ()=>{
            var videoCanvas = window.arCamera.VIDEO//videoCapture;
            textureObj = GL.textures[textureId];

            if (videoCanvas == null || textureObj == null) return;      

            GLctx.bindTexture(GLctx.TEXTURE_2D, textureObj);
            GLctx.texParameteri(GLctx.TEXTURE_2D, GLctx.TEXTURE_WRAP_S, GLctx.CLAMP_TO_EDGE);
            GLctx.texParameteri(GLctx.TEXTURE_2D, GLctx.TEXTURE_WRAP_T, GLctx.CLAMP_TO_EDGE);
            GLctx.texParameteri(GLctx.TEXTURE_2D, GLctx.TEXTURE_MIN_FILTER, GLctx.LINEAR);
            GLctx.pixelStorei(GLctx.UNPACK_FLIP_Y_WEBGL, true); 
            GLctx.texSubImage2D(GLctx.TEXTURE_2D, 0, 0, 0, GLctx.RGBA, GLctx.UNSIGNED_BYTE, videoCanvas);
            GLctx.pixelStorei(GLctx.UNPACK_FLIP_Y_WEBGL, false);

            //console.log("updateUnityVideoTextureCallback - webcam texture updated " + textureId);
        }
    },
    WebGLGetCameraTexture: function(textureId){
        var videoCanvas = window.arCamera.VIDEO;
        textureObj = GL.textures[textureId];

        if (canvas == null || textureObj == null) return;      

        GLctx.bindTexture(GLctx.TEXTURE_2D, textureObj);
        GLctx.texParameteri(GLctx.TEXTURE_2D, GLctx.TEXTURE_WRAP_S, GLctx.CLAMP_TO_EDGE);
        GLctx.texParameteri(GLctx.TEXTURE_2D, GLctx.TEXTURE_WRAP_T, GLctx.CLAMP_TO_EDGE);
        GLctx.texParameteri(GLctx.TEXTURE_2D, GLctx.TEXTURE_MIN_FILTER, GLctx.LINEAR);
        GLctx.pixelStorei(GLctx.UNPACK_FLIP_Y_WEBGL, true); 
        GLctx.texSubImage2D(GLctx.TEXTURE_2D, 0, 0, 0, GLctx.RGBA, GLctx.UNSIGNED_BYTE, videoCanvas);
        GLctx.pixelStorei(GLctx.UNPACK_FLIP_Y_WEBGL, false);

        console.log("WebGLGetCameraTexture " + textureId, videoCanvas);

    },
    
    IsWebcamPermissionGranted: function()
    {
        return (window.webcamStream != null);
    },
}); 