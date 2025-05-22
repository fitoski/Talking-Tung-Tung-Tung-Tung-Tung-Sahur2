#import <AVFoundation/AVFoundation.h>

extern "C" void _ForceSpeaker()
{
    AVAudioSession* s = [AVAudioSession sharedInstance];
    [s setCategory: AVAudioSessionCategoryPlayAndRecord
         mode:      AVAudioSessionModeDefault
         options:   AVAudioSessionCategoryOptionDefaultToSpeaker
         error:     nil];
    [s setActive:YES error:nil];
}
