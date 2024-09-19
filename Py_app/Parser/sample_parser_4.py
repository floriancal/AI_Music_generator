#sample parser convert a txt file to midi data

#import mido
import os
from mido import Message, MidiFile, MidiTrack, MetaMessage

def sample_parser(path):
  
  prefix_list = ["M1","M2","M3","M4","M5","B1","B2","B3","B4","K1","K2","K3","K4",
                 "S1","S2","S3","S4","H1","H2","H3","H4","L1","L2","L3","L4","C1",
                 "C2","C3","C4","R1","R2","T1","T2","Y1","Y2","SH"]


  for song in os.listdir(path):
    song_path = os.path.join(path,song)
    if os.path.isdir(song_path) is True:
      for file in os.listdir(song_path):
        if file[-17:] == 'sample_parser.txt':
            fp = os.path.join(song_path,file)
            sample_data = open(fp,encoding='utf-8').readlines()
            
            mid = MidiFile()
            activate = False

            sample_track_dict = {}
            for index, item in enumerate(sample_data):
                if item[0:14] == 'channel_name :':
                   
                    if item[14:16] in prefix_list:
                        activate = True
                        track_name = item[14:16]

                        if track_name in sample_track_dict:
                          sample_pos_list = sample_track_dict[track_name]
                          if sample_data[index + 1][0:12] == 'sample_pos :':
                             sample_pos = sample_data[index + 1][12:]                              
                             sample_pos_list.append(sample_pos)
                          sample_track_dict[track_name] = sample_pos_list
                        else:
                          sample_pos_list = []

                          if sample_data[index + 1][0:12] == 'sample_pos :':
                             sample_pos = sample_data[index + 1][12:]
                             sample_pos_list.append(sample_pos)
                          
                          sample_track_dict[track_name] = sample_pos_list

                         
            for key in sample_track_dict.keys():
               
                track_name = key
                sample_pos_list = sample_track_dict[key]

                sample_pos_list = [int(i) for i in sample_pos_list]
                sample_pos_list2 = []
                for i in sample_pos_list:
                  if i < 0:
                    sample_pos_list2.append(0)
                  else:
                    sample_pos_list2.append(i)
                sample_pos_list = sample_pos_list2
                    
                sample_pos_list.sort()
                
                track = MidiTrack()
                mid.tracks.append(track)
                track.append(Message('program_change', program=12, time=0))
                track.append(MetaMessage('track_name', name=track_name))
                old_sample_pos = 0

                for sample_pos in sample_pos_list:
                    track.append(Message('note_on', note=60, velocity=127, time= int(sample_pos) - old_sample_pos))
                    track.append(Message('note_off', note=60, velocity=127, time=32))
                    #print(int(sample_pos) - old_sample_pos)
                    old_sample_pos = int(sample_pos)
                    
                    
            mid.save(os.path.splitext(fp)[0]+'.mid')


