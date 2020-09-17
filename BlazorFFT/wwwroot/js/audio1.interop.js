// Audio processing interop layer for Blazor
// This version isn't iOS friendly as iOS requires a touch-instantiated audio stream

var _dotNetAudio1 = {
    audioContext: null,
    mediaStream: null,
    recorder: null,
    dotNetRef: null,
    numberOfOutputChannels: 1,
    sendErrorMessage: function(msg) {
        if (this.dotNetRef) {
            this.dotNetRef.invokeMethodAsync(
                'OnStartAudioListenError', msg);
        }
    },
    sendSuccessMessage: function(msg) {
        if (this.dotNetRef) {
            this.dotNetRef.invokeMethodAsync(
                'OnStartAudioListenSuccess', msg);
        }
    },
    sendAudioBuffer: function(buf) {
        if (this.dotNetRef) {
            this.dotNetRef.invokeMethodAsync(
                'OnAudioBufferReceived', buf);
        }
    },
    resetValues: function () {
        this.audioContext = null;
        this.mediaStream = null;
        this.recorder = null;
        this.dotNetRef = null;
	}
}

window.startAudioListen = (obj, numberOfInputChannels, sampleRate, bufferSize) => {

    if (_dotNetAudio1.audioContext) { return; }

    _dotNetAudio1.dotNetRef = obj;

    // derive getUserMedia
    navigator.getUserMedia =
        navigator.getUserMedia ||
        navigator.webkitGetUserMedia ||
        navigator.mozGetUserMedia ||
        navigator.msGetUserMedia;

    // if it's not avaliable, just leave
    if (!navigator.getUserMedia) {
        _dotNetAudio1.sendErrorMessage('navigator.getUserMedia is null');
        return;
	}

    // call it.
    navigator.getUserMedia({
        audio: true
    },
        function (e) { // success

            window.AudioContext = window.AudioContext || window.webkitAudioContext;
            _dotNetAudio1.audioContext = new AudioContext({
                sampleRate: sampleRate
            });

            _dotNetAudio1.mediaStream = _dotNetAudio1.audioContext.createMediaStreamSource(e);

            if (_dotNetAudio1.audioContext.createScriptProcessor) {
                _dotNetAudio1.recorder = _dotNetAudio1.audioContext.createScriptProcessor(
                    bufferSize, numberOfInputChannels, _dotNetAudio1.numberOfOutputChannels);
            } else {
                _dotNetAudio1.recorder = _dotNetAudio1.audioContext.createJavaScriptNode(
                    bufferSize, numberOfInputChannels, _dotNetAudio1.numberOfOutputChannels);
            }

            _dotNetAudio1.recorder.onaudioprocess = function (e) {
                _dotNetAudio1.sendAudioBuffer(e.inputBuffer.getChannelData(0));
            }

            _dotNetAudio1.mediaStream.connect(_dotNetAudio1.recorder);
            _dotNetAudio1.recorder.connect(_dotNetAudio1.audioContext.destination);

            _dotNetAudio1.sendSuccessMessage(e.id);
        },
        function (e) { // failed
            _dotNetAudio1.sendErrorMessage(e.message);
        });
}

window.hasAudioListenStarted = () => {
    return _dotNetAudio1.audioContext ? true : false;
}

window.stopAudioListen = () => {
    if (_dotNetAudio1.audioContext &&
        _dotNetAudio1.recorder &&
        _dotNetAudio1.mediaStream) {

        _dotNetAudio1.recorder.disconnect(_dotNetAudio1.audioContext.destination);
        _dotNetAudio1.mediaStream.disconnect(_dotNetAudio1.recorder);
    }
    _dotNetAudio1.resetValues();
}
