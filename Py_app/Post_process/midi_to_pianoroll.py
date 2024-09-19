import mido
from mido import MidiFile, tempo2bpm
import os
import json


post_process_dir = 'C:/Users/fcala/Desktop/Son/Post_Process'


for dir in os.listdir(post_process_dir):
    inpath = os.path.join(post_process_dir, dir, 'content', dir)
    for file in os.listdir(inpath):
        if os.path.splitext(file)[1] == '.mid':
             pattern_dict = {}
             mid = MidiFile(os.path.join(inpath, file))
             print(mid.ticks_per_beat)
            
             for track in mid.tracks:
                 track_name = track.name
                 
                 if track_name == 'M2':
                     for index, msg in enumerate(track):
                        # print(msg)
                        pass
                     
                 note_list = []
                 timeline = 0
                 for index, msg in enumerate(track):
                    timeline += msg.time
                    if msg.type =='set_tempo':
                        tempo = msg.tempo
                        bpm = round(tempo2bpm(tempo))
                    if msg.type =='note_on':
                        Position = timeline
                        Key = msg.note
                        FinePitch = 0
                        Pan = 64
                        Vel= msg.velocity

                        Length = 0
                        Found_end = False
                        for msg2 in track[index+1:]:
                            Length += msg2.time
                            if msg2.type == 'note_off' and msg2.note == Key:
                                Found_end = True
                                break         
                        if Found_end is False:
                            Length = 96
                        
                        Release = round(Length / 2)
                            
                        note = [Position, Length, Key, FinePitch, Release, Pan, Vel]
                        note_list.append(note)
                 pattern_dict[track_name] = note_list
             with open(os.path.join(inpath,'pattern_dict.txt'), 'w') as f1:
                 f1.write(json.dumps(pattern_dict))
             with open(os.path.join(inpath,'tempo.txt'), 'w') as f1:
                try:
                 f1.write(str(bpm))
                except:
                 f1.write(str(120))
                          
            
            
