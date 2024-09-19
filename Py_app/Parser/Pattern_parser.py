
#Pattern Parseer

import os
from mido import Message, MidiFile, MidiTrack, MetaMessage

def pattern_parser(path):
  
  prefix_list = ["M1","M2","M3","M4","M5","B1","B2","B3","B4","K1","K2","K3","K4",
                 "S1","S2","S3","S4","H1","H2","H3","H4","L1","L2","L3","L4","C1",
                 "C2","C3","C4","R1","R2","T1","T2","Y1","Y2","SH"]


  for song in os.listdir(path):
    song_path = os.path.join(path,song)
    print(song_path)
    if os.path.isdir(song_path) is True:
      for file in os.listdir(song_path):
         if file[-18:] == '_sample_parser.mid':
           
             rename_flp_path = 'C:\\Users\\fcala\\Desktop\\Son\\FLP - Renamed'
             pattern_mid_path = os.path.join(rename_flp_path,file[0:-18] + '.mid')
             txt_file_path = os.path.join(rename_flp_path,file[0:-18] + '.txt')

             pattern_mid = MidiFile()   
             pattern_mid.ticks_per_beat = 96

             print(song_path)
             pattern_data = open(txt_file_path).readlines()
             
             # ARRNGE NOTES IN A DICT (CHANNEL, note data)
             pattern_track_dict = {}
             for index, item in enumerate(pattern_data):
                 
                 if item[0:19] == 'note_channel_name :':
                     if item[19:21] in prefix_list:
                        track_name = item[19:21]
                     item_pos = pattern_data[index + 1][10:]
                     item_offset = pattern_data[index + 2][13:]
                     note_key = pattern_data[index + 3][10:]
                     note_length = pattern_data[index + 4][13:]
                     note_pos = pattern_data[index + 5][10:]
         
                     #int conv
                     item_pos = int(item_pos.replace('\n',''))
                     note_pos = int(note_pos.replace('\n',''))
                     note_key = int(note_key.replace('\n',''))
                     note_length = int(note_length.replace('\n',''))
                     
                     # a verif item offset not used
                     valid_note_pos  = item_pos + note_pos


                     data = [valid_note_pos, note_key,  'note_on']
                     data2 = [valid_note_pos + note_length, note_key, 'note_off']

                     
                     if track_name in pattern_track_dict:
                       pattern_pos_list = pattern_track_dict[track_name]                        
                       pattern_pos_list.append(data)
                       pattern_pos_list.append(data2)
                     else:
                       pattern_pos_list = []
                       pattern_pos_list.append(data)
                       pattern_pos_list.append(data2)
                       
                     pattern_track_dict[track_name] = pattern_pos_list

                     

            # Write the midi file
             for key in pattern_track_dict.keys():

                  track_name = key
                  pattern_pos_list = pattern_track_dict[key]

                  # Sort patern pos list by the first item of each sublist(vlid_note_pos)
                  pattern_pos_list = sorted(pattern_pos_list, key=lambda x: x[0])

                  track = MidiTrack()

                  pattern_mid.tracks.append(track)
                  track.append(Message('program_change', program=12, time=0))
                  track.append(MetaMessage('track_name', name=track_name))
                  old_sample_pos = 0

                  for note_data in pattern_pos_list:
                    sample_pos = int(note_data[0])
                       
                    track.append(Message(note_data[2], note=int(note_data[1]), velocity=127, time=  sample_pos - old_sample_pos))
                    old_sample_pos = sample_pos
             print(pattern_mid_path)
             pattern_mid.save(pattern_mid_path)  
                  
             

