// Since iOS requires a user-initiated gesture to begin listening to audio, we have to
// create a button that actually calls the method which starts the audio

_dotNetAudio2 = {
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

// callback for button
function startAudio2Listen() {
    if (!_dotNetAudio2.hasInitialized()) { return; }

    navigator.mediaDevices.getUserMedia({ audio: true })
        .then((e) =>
        {
            _dotNetAudio2.mediaStream = _dotNetAudio2.audioContext.createMediaStreamSource(e);

            if (_dotNetAudio2.audioContext.createScriptProcessor) {
                _dotNetAudio2.recorder = _dotNetAudio2.audioContext.createScriptProcessor(
                    _dotNetAudio2.bufferSize,
                    _dotNetAudio2.numberOfInputChannels,
                    _dotNetAudio2.numberOfOutputChannels);
            } else {
                _dotNetAudio2.recorder = _dotNetAudio2.audioContext.createJavaScriptNode(
                    _dotNetAudio2.bufferSize,
                    _dotNetAudio2.numberOfInputChannels,
                    _dotNetAudio2.numberOfOutputChannels);
            }

            _dotNetAudio2.recorder.onaudioprocess = function (e) {
                _dotNetAudio2.sendAudioBuffer(e.inputBuffer.getChannelData(0));
            }

            _dotNetAudio2.mediaStream.connect(_dotNetAudio2.recorder);
            _dotNetAudio2.recorder.connect(_dotNetAudio2.audioContext.destination);

            _dotNetAudio2.sendSuccessMessage(e.id);
        })
        .catch((e) =>
        {
            _dotNetAudio2.sendErrorMessage(e.message);
        });
}

window.initializeAudio2Listen = (obj, numberOfInputChannels, sampleRate, bufferSize) => {

    if (_dotNetAudio2.hasInitialized()) { return; }
    
    window.AudioContext = window.AudioContext || window.webkitAudioContext;

    _dotNetAudio2.dotNetRef = obj;
    _dotNetAudio2.numberOfInputChannels = numberOfInputChannels;
    _dotNetAudio2.bufferSize = bufferSize;
    _dotNetAudio2.audioContext = new AudioContext({
        sampleRate: sampleRate
    });
}

window.hasAudio2ListenStarted = () => {
    return _dotNetAudio2.hasAudioStarted();
}

window.stopAudio2Listen = () => {
    if (_dotNetAudio2.hasAudioStarted()) {
        _dotNetAudio2.recorder.disconnect(_dotNetAudio2.audioContext.destination);
        _dotNetAudio2.mediaStream.disconnect(_dotNetAudio2.recorder);
    }
    _dotNetAudio2.resetValues();
}