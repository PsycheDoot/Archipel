/* laserpulse_oscin.ck
 * laserpulse_oscin.ck [-a oscMsgAddr] [-p port] */

",f,f" => msgFormat
"ShootyBoi/Chuck/laserpulse" => address
12345 => port

// Get parameters ==========================================================
numArgs = me.args()
for (int i = 0; i < numArgs; i++) {
    arg = me.args(i)
    // Get address
    if (arg.rfind("-a", 0) > -1) {
        if (i+1 < numArgs) {
            me.args(i+1) => address
        } else {
            <<< "No address provided for -a command. [-a oscMsgAddr]" >>>
        }
    }
    // Get port
    if (arg.rfind("-p", 0) > -1) {
        if (i+1 < numArgs) {
            me.args(i+1) => port
        } else {
            <<< "No address provided for -p command. [-p port]" >>>
        }
    }
}

// Initialize ==============================================================
chout <= "Starting laserpulse listener at " <= address <= " on port " <= port <= IO.newline(); 

100::ms => dur tickDuration

OscRecv oin;
port => oin.port;
oin.listen;
oin.event("ShootyBoi/Chuck/laserpulse,f")

while (true) {
   tickDuration => now;
}

6449 => oin.port;
oin.addAddress( "/sndbuf/buf/rate, f" );

// infinite event loop
while ( true )
{
    // wait for event to arrive
    oin => now;
    // grab the next message from the queue. 
    while ( oin.recv(msg) != 0 )
    { 
        // print
        <<< "got (via OSC):" >>>;
        // set play pointer to beginning
        0 => buf.pos;
    }
}

fun void playOnce() {
    SndBuf buf => dac;
    me.dir(-1) + "data/snare.wav" => buf.read;
    0 => buf.pos;
    1 => buf.rate;
}