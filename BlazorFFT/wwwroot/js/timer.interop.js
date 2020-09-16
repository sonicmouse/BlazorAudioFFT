var _timerDic = new Object();

window.setCallbackTimer = (obj, method, id, interval) => {

	if (_timerDic.hasOwnProperty(id)) {
		clearInterval(_timerDic[id]);
	}

	_timerDic[id] = setInterval(() =>
		obj.invokeMethodAsync(method, id), interval);
}

window.clearCallbackTimer = (id) => {
	if (_timerDic.hasOwnProperty(id)) {
		clearInterval(_timerDic[id]);
		delete _timerDic[id];
	}
}