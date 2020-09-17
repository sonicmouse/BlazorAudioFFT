window.getWindowWidth = () => {
	return window.innerWidth;
}

window.getWindowHeight = () => {
	return window.innerHeight;
}

window.addResizeEventCallback = (namespace, method) => {
	window.addEventListener('resize', () => {
		DotNet.invokeMethodAsync(namespace, method, window.innerWidth, window.innerHeight);
	});
}

window.resizeComponentById = (id, width, height) => {
	var obj = document.getElementById(id);
	if (obj) {
		obj.width = width;
		obj.height = height;
	}
}