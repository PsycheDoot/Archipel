0.0 => float listenerX;
0.0 => float listenerY;
0.0 => float listenerZ;
0.0 => float listenerRot;

fun float lerp(float a, float b, float t) {
    return (b-a)*t + a;
}

fun float mod2(float a, float b) {
    return (Math.fabs(a*b) + a) % b;
}

fun string getSamplePath(string sampleName) {
    return me.dir(0) + sampleName;
}
// Class Definitions ==============================================================
/*
class PanNSurround {
    UGen input;
    
    dac.channels() => int numChan;
    0 => int indexa;
    int indexb;
    
    fun void setListenerPos(float x, float y, float z) {
        
    }
    
    fun void Pan(float n) {
        Std.floor(n % numChan) => indexa;
        Std.ceil(n % numChan) => indexb;
        indexb - n => intensitya;
        n - indexa => intensityb;
    }  
}*/


dac.channels() => int numChan;

// Abstract class Voice
class Voice {
    Gain out => dac;
    
    0.0 => float curX;
    0.0 => float curY;
    0.0 => float curZ;
    
    0 => int indexa;
    0 => int indexb;
    0.0 => float intensitya; 
    0.0 => float intensityb;
    
    fun void setPosition(float x, float y, float z) {
        x => curX;
        y => curY;
        z => curZ;
    }
    
    fun void update() {
        // Remove from dac
        out !=> dac;
        // Calculate the angle between the listener and the audio source
        mod2(Math.atan2(curZ - listenerZ, curX - listenerX) + listenerRot, Math.TWO_PI) => float theta;
        <<< "Theta", theta>>>;
        // Calculate the channel index from the angle
        mod2((numChan * theta) / Math.TWO_PI, numChan) => float n;
        // Calculate the two dac channels to play from
        Math.floor(n) $ int => indexa;
        Math.ceil(n) $ int => indexb;
        // Intensity of each channel
        indexb - n => intensitya;
        n - indexa => intensityb;
        // Set buffer output channels in dac
        if (indexa >= 0 && indexa < numChan) {
            out => dac.chan(indexa);
            intensitya => dac.chan(indexa).gain;
        }
        if (indexb >= 0 && indexb < numChan) {
            out => dac.chan(indexb);
            intensityb => dac.chan(indexb).gain;
        }
        //<<<intensitya, intensityb>>>;
    }
    
    fun void playOnce()             {cherr <= "playOnce() not implemented";}
    fun void playOnce(float volume) {cherr <= "playOnce(float volume) not implemented";}
    fun void stop()                 {cherr <= "stop() not implemented";}
    fun void pause()                {cherr <= "pause() not implemented";}
    fun void resume()               {cherr <= "resume() not implemented";}
}

class Sample extends Voice {
    // Constructor
    SndBuf buf => out;
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
        while (buf.pos() < buf.samples()) {
            update();
            10::ms => now;
        }
        1::second => now;
    }
    
    fun void playOnce(float volume) {
        <<<intensitya, intensityb>>>;
        stop();
        resume();
        while (buf.pos() < buf.samples()) {
            update();
            10::ms => now;
        }
        1::second => now;
    }
}

class Wind extends Voice {
    Noise n => BiQuad f => dac;
    .99 => f.prad;
    .05 => f.gain;
    1 => f.eqzs;
    0.0 => float t;
    
    fun void play() {
        while (true) {
            100.0 + Std.fabs(Math.sin(t)) * 15000.0 => f.pfreq;
            t + .01 => t;
            5::ms => now;
        }
    }
}

class AudioSource { 
    OscRecv cmdIn;
    1234 => cmdIn.port;
    cmdIn.listen();
    
    string instanceID;
    
    Voice @ voice;
    Shred @ shred;
    
    50.0 => float minDist;
    500.0 => float maxDist;
    0 => int playing;
    
    0 => int spatialize;
    
    0 => float curX;
    0 => float curY;
    0 => float curZ;
    
    fun float curDist() {
        return Math.sqrt(Math.pow(curX-listenerX, 2) + Math.pow(curY-listenerY, 2) + Math.pow(curZ-listenerZ, 2));
    }
    
    fun void playOnce(){
        if (shred != null && shred.running()) shred.yield();
        spork ~ voice.playOnce() @=> shred;
    }
    
    fun void playOnce(float volume){
        if (shred != null && shred.running()) shred.yield();
        spork ~ voice.playOnce(volume) @=> shred;
    }
    
    fun void setPosition(float x, float y, float z) {
        x => curX;
        y => curY;
        z => curZ;
        if (voice != null) {
            voice.setPosition(x, y, z);
        }
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
    
}
*/

// Initialize Main ========================================================

OscRecv cmdIn;
1234 => cmdIn.port;
cmdIn.listen();
cmdIn.event("Archipel/AudioEngine/AddSampleAudioSource,s,s") @=> OscEvent addEvent;
cmdIn.event("Archipel/AudioEngine/PlayAudioSource,s,f") @=> OscEvent playEvent;
cmdIn.event("Archipel/AudioEngine/SetPosAudioListener,f,f,f") @=> OscEvent listenerPosEvent;
cmdIn.event("Archipel/AudioEngine/SetRotAudioListener,f") @=> OscEvent listenerRotEvent;
cmdIn.event("Archipel/AudioEngine/SetPosAudioSource,s,f,f,f") @=> OscEvent asPosEvent;

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
        while (listenerPosEvent.nextMsg() != 0) {
            listenerPosEvent.getFloat() => float x;
            listenerPosEvent.getFloat() => float y;
            listenerPosEvent.getFloat() => float z;
            x => listenerX;
            y => listenerY;
            z => listenerZ;
            <<< "Position set Listener", x,y,z >>>;
        }
        while (listenerRotEvent.nextMsg() != 0) {
            listenerRotEvent.getFloat() => float x;
            x => listenerRot;
            <<< "Rotation set Listener", x >>>;
        }
        while (asPosEvent.nextMsg() != 0) {
            asPosEvent.getString() => string name;
            asPosEvent.getFloat() => float x;
            asPosEvent.getFloat() => float y;
            asPosEvent.getFloat() => float z;
            if (audioSources[name] == null) {
                <<< name, "does not exist." >>>;
            } else {
                audioSources[name].setPosition(x, y, z);
                <<< "Position set", name >>>;
            }
        }
    }
}
