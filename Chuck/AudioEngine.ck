fun string getSamplePath(string sampleName) {
    return me.dir(0) + sampleName;
}
// Class Definitions ==============================================================
// Abstract class Voice
class Voice {
    fun void playOnce() {}
    fun void playOnce(float volume) {}
}

class Sample extends Voice {
    // Constructor
    SndBuf buf => dac;
    0 => buf.pos;
    0 => buf.rate;
    
    
    fun void setFile(string sampleName) {
        getSamplePath(sampleName) => buf.read;
    }
    
    fun void stop() {
        0 => buf.pos;
        0 => buf.rate;
    }
    
    fun void pause() {
        0 => buf.rate;
    }
    
    fun void resume() {
        1 => buf.rate;
    }
    
    fun void playOnce() {
        stop();
        resume();
        1::second => now;
    }
    
    fun void playOnce(float volume) {
        stop();
        resume();
        1::second => now;
    }
}

class AudioSource { 
    Voice @ voice;
    Shred @ shred;
    
    fun void playOnce(){
        spork ~ voice.playOnce() @=> shred;
    }
    
    fun void playOnce(float volume){
        spork ~ voice.playOnce(volume) @=> shred;
    }
}

/*
class OscAudioSource extends AudioSource {
    public string name;
    private string msgFormat;
    private string address;
    private int port;
    private OscRecv oin;
    
    fun void listen(int p) {
        p => port => oin.port;
        oin.listen();
    }
    
    fun void event(string ev) {
        return oin.event(ev);
    }
    
    fun void tick() {
    
    }
    
    port => oin.port;
    oin.listen;
    oin.event("ShootyBoi/Chuck/laserpulse,f")
    
}*/

// Initialize Main =========================================================
OscRecv cmdIn;
1234 => cmdIn.port;
cmdIn.listen();
cmdIn.event("Archipel/AudioEngine/AddSampleAudioSource,s,s") @=> OscEvent addEvent;
cmdIn.event("Archipel/AudioEngine/PlayAudioSource,s,f") @=> OscEvent playEvent;

AudioSource @ audioSources[0];

fun void AddSampleAudioSource(string name, string sampleName) {
    if (audioSources[name] == null) {
        new AudioSource @=> audioSources[name];
        new Sample @=> Sample @ samplr;
        samplr.setFile(sampleName);
        samplr @=> audioSources[name].voice;
        <<< "Audio Source", name, "Added!" >>>;
    }
    else
        <<< "Audio Source", name, "already exists" >>>;
}



10::ms => dur tickTime;
false => int pause;
while (true) {
    tickTime => now;
    if (!pause) {
        while (addEvent.nextMsg() != 0) {
            addEvent.getString() => string name;
            addEvent.getString() => string sampleName;
            AddSampleAudioSource(name, sampleName);
        }
        while (playEvent.nextMsg() != 0) {
            playEvent.getString() => string name;
            playEvent.getFloat() => float volume;
            if (audioSources[name] == null) {
                <<< name, "does not exist." >>>;
            } else {
                audioSources[name].playOnce(volume);
                <<< "Playing", name >>>;
            }
        }
    }
}