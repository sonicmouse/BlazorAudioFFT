
_dotNetAudio = {
    // settings
    bufferSize: null,
    numberOfInputChannels: null,
    numberOfOutputChannels: 1,
    // initialized
    dotNetRef: null,
    audioContext: null,
    // instantiated
    mediaStream: null,
    recorder: null,
    sendErrorMessage: function (msg) {
        if (this.dotNetRef) {
            this.dotNetRef.invokeMethodAsync(
                'OnStartAudioListenError', msg);
        }
    },
    sendSuccessMessage: function (msg) {
        if (this.dotNetRef) {
            this.dotNetRef.invokeMethodAsync(
                'OnStartAudioListenSuccess', msg);
        }
    },
    sendAudioBuffer: function (buf) {
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
    },
    hasInitialized: function () {
        return this.audioContext ? true : false;
    },
    hasAudioStarted: function () {
        return this.mediaStream ? true : false;
	}
}

window.initializeAudioListen = (obj, numberOfInputChannels, sampleRate, bufferSize) => {

    if (_dotNetAudio.hasInitialized()) { return; }

    window.AudioContext = window.AudioContext || window.webkitAudioContext;

    _dotNetAudio.dotNetRef = obj;
    _dotNetAudio.numberOfInputChannels = numberOfInputChannels;
    _dotNetAudio.bufferSize = bufferSize;
    _dotNetAudio.audioContext = new AudioContext({
        sampleRate: sampleRate
    });
}

window.startAudioListen = () => {
    if (!_dotNetAudio.hasInitialized()) { return; }

    navigator.mediaDevices.getUserMedia({ audio: true })
        .then((mic) =>
        {
            _dotNetAudio.mediaStream = _dotNetAudio.audioContext.createMediaStreamSource(mic);

            if (_dotNetAudio.audioContext.createScriptProcessor) {
                _dotNetAudio.recorder = _dotNetAudio.audioContext.createScriptProcessor(
                    _dotNetAudio.bufferSize,
                    _dotNetAudio.numberOfInputChannels,
                    _dotNetAudio.numberOfOutputChannels);
            } else {
                _dotNetAudio.recorder = _dotNetAudio.audioContext.createJavaScriptNode(
                    _dotNetAudio.bufferSize,
                    _dotNetAudio.numberOfInputChannels,
                    _dotNetAudio.numberOfOutputChannels);
            }

            _dotNetAudio.recorder.onaudioprocess = function (e) {
                _dotNetAudio.sendAudioBuffer(e.inputBuffer.getChannelData(0));
            }

            _dotNetAudio.mediaStream.connect(_dotNetAudio.recorder);
            _dotNetAudio.recorder.connect(_dotNetAudio.audioContext.destination);

            _dotNetAudio.sendSuccessMessage(mic.id);
        })
        .catch((e) =>
        {
            _dotNetAudio.sendErrorMessage(e.message);
        });
}

window.hasAudioListenStarted = () => {
    return _dotNetAudio.hasAudioStarted();
}

window.stopAudioListen = () => {
    if (_dotNetAudio.hasAudioStarted()) {
        _dotNetAudio.recorder.disconnect(_dotNetAudio.audioContext.destination);
        _dotNetAudio.mediaStream.disconnect(_dotNetAudio.recorder);
    }
    _dotNetAudio.resetValues();
}