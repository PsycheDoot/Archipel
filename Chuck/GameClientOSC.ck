OscSend xmit;
xmit.setHost("localhost", 1234);

fun void AddSampleAudioSource(string name, string sampleName) {
    if (name == "" || sampleName == "") return;
    xmit.startMsg("Archipel/AudioEngine/AddSampleAudioSource,s,s");
    xmit.addString(name);
    xmit.addString(sampleName);
}

fun void PlayAudioSource(string name, float volume) {
    if (name == "") return;
    xmit.startMsg("Archipel/AudioEngine/PlayAudioSource,s,f");
    xmit.addString(name);
    xmit.addFloat(volume);
}

AddSampleAudioSource("BassDrum", "Sounds/808/BassDrum.wav");
AddSampleAudioSource("BassKick", "Sounds/808/BassKick.wav");
AddSampleAudioSource("Clap", "Sounds/808/Clap.wav");
AddSampleAudioSource("CowBell", "Sounds/808/CowBell.wav");
AddSampleAudioSource("Cymbal", "Sounds/808/Cymbal.wav");
AddSampleAudioSource("HighHat", "Sounds/808/HighHat.wav");
AddSampleAudioSource("HighHat2", "Sounds/808/HighHat2.wav");
AddSampleAudioSource("Snare", "Sounds/808/Snare.wav");
AddSampleAudioSource("WoodBlock", "Sounds/808/WoodBlock.wav");

["BassKick", "", "Cymbal", "Snare", 
"BassKick", "Snare", "Cymbal", "", 
"BassKick", "", "Cymbal", "", 
"BassKick", "WoodBlock", "WoodBlock", ""] @=> string track1[];

0 => int step;
(500/4)::ms => dur timeStep; 
while (true) {
    PlayAudioSource(track1[step], 1);
    (step+1)%16 => step;
    timeStep => now;
}