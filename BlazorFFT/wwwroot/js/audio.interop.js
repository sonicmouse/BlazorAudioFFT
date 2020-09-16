// andy was here
//
// found example of recording from mic here:
// https://www.meziantou.net/record-audio-with-javascript.htm

var _numberOfOutputChannels = 1; // constant

var _audioContext = null;
var _mediaStream = null;
var _recorder = null;
var _dotNetRef = null;

window.startAudioListen = (obj, numberOfInputChannels, sampleRate, bufferSize) => {

    if (_audioContext) { return; }

    _dotNetRef = obj;

    navigator.getUserMedia =
        navigator.getUserMedia ||
        navigator.webkitGetUserMedia ||
        navigator.mozGetUserMedia ||
        navigator.msGetUserMedia;

    if (!navigator.getUserMedia) {
        _dotNetRef.invokeMethodAsync(
            'OnStartAudioListenError',
            'navigator.getUserMedia is null');
        return;
	}

    navigator.getUserMedia({
        audio: true
    },
        function (e) { // success
            window.AudioContext = window.AudioContext || window.webkitAudioContext;
            _audioContext = new AudioContext({
                sampleRate: sampleRate
            });

            _mediaStream = _audioContext.createMediaStreamSource(e);

            if (_audioContext.createScriptProcessor) {
                _recorder = _audioContext.createScriptProcessor(
                    bufferSize, numberOfInputChannels, _numberOfOutputChannels);
            } else {
                _recorder = _audioContext.createJavaScriptNode(
                    bufferSize, numberOfInputChannels, _numberOfOutputChannels);
            }

            _recorder.onaudioprocess = function (e) {
                if (_dotNetRef) {
                    _dotNetRef.invokeMethodAsync(
                        'OnAudioBufferReceived', e.inputBuffer.getChannelData(0));
                }
            }

            _mediaStream.connect(_recorder);
            _recorder.connect(_audioContext.destination);

            _dotNetRef.invokeMethodAsync(
                'OnStartAudioListenSuccess', e.id);
        },
        function (e) { // failed
            _dotNetRef.invokeMethodAsync(
                'OnStartAudioListenError', e.message);
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
