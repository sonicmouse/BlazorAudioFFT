// andy was here
//

var _numberOfOutputChannels = 1; // constant

var _audioContext = null;
var _mediaStream = null;
var _recorder = null;
var _dotNetRef = null;

var _dotNetCb = {
    sendErrorMessage: function(msg) {
        if (_dotNetRef) {
            _dotNetRef.invokeMethodAsync(
                'OnStartAudioListenError', msg);
        }
    },
    sendSuccessMessage: function(msg) {
        if (_dotNetRef) {
            _dotNetRef.invokeMethodAsync(
                'OnStartAudioListenSuccess', msg);
        }
    },
    sendAudioBuffer: function(buf) {
        if (_dotNetRef) {
            _dotNetRef.invokeMethodAsync(
                'OnAudioBufferReceived', buf);
        }
	}
}

window.startAudioListen = (obj, numberOfInputChannels, sampleRate, bufferSize) => {

    if (_audioContext) { return; }

    _dotNetRef = obj;

    // create an audio context before calling getUserMedia
    window.AudioContext = window.AudioContext || window.webkitAudioContext;
    _audioContext = new AudioContext({
        sampleRate: sampleRate
    });

    // derive getUserMedia
    navigator.getUserMedia =
        navigator.getUserMedia ||
        navigator.webkitGetUserMedia ||
        navigator.mozGetUserMedia ||
        navigator.msGetUserMedia;

    // if it's not avaliable, just leave
    if (!navigator.getUserMedia) {
        _dotNetCb.sendErrorMessage('navigator.getUserMedia is null');
        _audioContext = null;
        return;
	}

    // call it.
    navigator.getUserMedia({
        audio: true
    },
        function (e) { // success
            _mediaStream = _audioContext.createMediaStreamSource(e);

            if (_audioContext.createScriptProcessor) {
                _recorder = _audioContext.createScriptProcessor(
                    bufferSize, numberOfInputChannels, _numberOfOutputChannels);
            } else {
                _recorder = _audioContext.createJavaScriptNode(
                    bufferSize, numberOfInputChannels, _numberOfOutputChannels);
            }

            _recorder.onaudioprocess = function (e) {
                _dotNetCb.sendAudioBuffer(e.inputBuffer.getChannelData(0));
            }

            _mediaStream.connect(_recorder);
            _recorder.connect(_audioContext.destination);

            _dotNetCb.sendSuccessMessage(e.id);
        },
        function (e) { // failed
            _audioContext = null;
            _dotNetCb.sendErrorMessage(e.message);
        });
}

window.hasAudioListenStarted = () => {
    return _audioContext ? true : false;
}

window.stopAudioListen = () => {
    if (_audioContext && _recorder && _mediaStream) {
        _recorder.disconnect(_audioContext.destination);
        _mediaStream.disconnect(_recorder);
    }
    _audioContext = null;
    _mediaStream = null;
    _recorder = null;
    _dotNetRef = null;
}
