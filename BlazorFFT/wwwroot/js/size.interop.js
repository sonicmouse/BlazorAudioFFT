window.getWindowWidth = () => {
    return window.innerWidth;
}

window.getWindowHeight = () => {
    return window.innerHeight - 18;
}

window.addResizeEventCallback = (namespace, method) => {
    window.addEventListener('resize', () => {
        DotNet.invokeMethodAsync(namespace, method, window.innerWidth, window.innerHeight - 18);
    });
}

window.resizeComponentById = (id, width, height) => {
    var obj = document.getElementById(id);
    if (obj) {
        obj.width = width;
        obj.height = height;
    }
}