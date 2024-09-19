#midi splitter to cut part of the song by length

import mido
import os
from mido import Message, MidiFile, MidiTrack, MetaMessage


def midi_len_splitter(path):
  for song in os.listdir(path):
    song_path = os.path.join(path,song)
    
    if os.path.isdir(song_path) is True:
      
      for file in os.listdir(song_path):
        if file[-10:] == '_total.mid':
         time_file = open(os.path.join(song_path,'time_cut.txt')).readlines()
        
         ########################################################this 0 means takes first couplet..##########################HERE SAME##########
         try:
           intro_time = (int(time_file[0].split(':')[1].split(',')[0].split('-')[0]), int(time_file[0].split(':')[1].split(',')[0].split('-')[1]))
         except:
           intro_time =(0,0)

         try:
           couplet_time = (int(time_file[1].split(':')[1].split(',')[0].split('-')[0]), int(time_file[1].split(':')[1].split(',')[0].split('-')[1]))
         except:
           couplet_time = (0,0)

         try:
           pre_ref_time = (int(time_file[2].split(':')[1].split(',')[0].split('-')[0]), int(time_file[2].split(':')[1].split(',')[0].split('-')[1]))
         except:
           pre_ref_time = (0,0)

         try:
           refrain_time = (int(time_file[3].split(':')[1].split(',')[0].split('-')[0]), int(time_file[3].split(':')[1].split(',')[0].split('-')[1]))
         except:
           refrain_time = (0,0)

         try:
           pont_time = (int(time_file[4].split(':')[1].split(',')[0].split('-')[0]), int(time_file[4].split(':')[1].split(',')[0].split('-')[1]))
         except:
           pont_time = (0,0)

         try:
           outro_time = (int(time_file[5].split(':')[1].split(',')[0].split('-')[0]), int(time_file[5].split(':')[1].split(',')[0].split('-')[1]))
         except:
           outro_time = (0,0)

         intro_mid = MidiFile()
         couplet_mid = MidiFile()
         pre_ref_mid = MidiFile()
         refrain_mid = MidiFile()
         pont_mid = MidiFile()
         outro_mid = MidiFile()
                          
         total_mid_path = os.path.join(song_path,file)
         total_mid = MidiFile(total_mid_path)
         
         # set every midi to total mid tick per beat
         intro_mid.ticks_per_beat = total_mid.ticks_per_beat
         couplet_mid.ticks_per_beat = total_mid.ticks_per_beat
         pre_ref_mid.ticks_per_beat = total_mid.ticks_per_beat
         refrain_mid.ticks_per_beat = total_mid.ticks_per_beat
         pont_mid.ticks_per_beat = total_mid.ticks_per_beat
         outro_mid.ticks_per_beat = total_mid.ticks_per_beat


         for track in total_mid.tracks:
           timeline = 0
                          
           intro_track = MidiTrack()
           couplet_track = MidiTrack()
           pre_ref_track = MidiTrack()
           refrain_track = MidiTrack()
           pont_track = MidiTrack()
           outro_track = MidiTrack()

           track_name = track.name
           
           intro_mid.tracks.append(intro_track)
           intro_track.append(Message('program_change', program=12, time=0))
           intro_track.append(MetaMessage('track_name', name=track_name))
           
           couplet_mid.tracks.append(couplet_track)
           couplet_track.append(Message('program_change', program=12, time=0))
           couplet_track.append(MetaMessage('track_name', name=track_name))
           
           pre_ref_mid.tracks.append(pre_ref_track)
           pre_ref_track.append(Message('program_change', program=12, time=0))
           pre_ref_track.append(MetaMessage('track_name', name=track_name))
                
           refrain_mid.tracks.append(refrain_track)
           refrain_track.append(Message('program_change', program=12, time=0))
           refrain_track.append(MetaMessage('track_name', name=track_name))
           
           pont_mid.tracks.append(pont_track)
           pont_track.append(Message('program_change', program=12, time=0))
           pont_track.append(MetaMessage('track_name', name=track_name))
                
           outro_mid.tracks.append(outro_track)
           outro_track.append(Message('program_change', program=12, time=0))
           outro_track.append(MetaMessage('track_name', name=track_name))

           first_pass_intro = True
           first_pass_couplet = True
           first_pass_pre_refrain = True
           first_pass_refrain = True
           first_pass_pont = True
           first_pass_outro = True
           
           for msg in track:
               
                timeline += msg.time / (total_mid.ticks_per_beat * 4)
                
                # REMOVE 1 BECAUSE FL COUNTS FROM 1 BUT MIDI TIME START AT 0
                if timeline < intro_time[1]  - 1:
                  # apply a correction on the firt note of each segment the delta is the delta from the mesure where begin the segment not the last note
                  if first_pass_intro:
                    msg.time = int((timeline - intro_time[0]+1) * total_mid.ticks_per_beat * 4)
                    first_pass_intro = False
                  intro_track.append(msg)
                 
                if timeline > couplet_time[0] - 1 and timeline < couplet_time[1] - 1:
                  if first_pass_couplet:
                    print(msg.time)
                    msg.time = int((timeline - couplet_time[0]+1) * total_mid.ticks_per_beat * 4)
                    print(msg.time)
                    first_pass_couplet = False
                  couplet_track.append(msg)
                          
                if timeline > pre_ref_time[0] - 1 and timeline < pre_ref_time[1] - 1:
                  if first_pass_pre_refrain:
                    msg.time =  int((timeline - pre_ref_time[0]+1) * total_mid.ticks_per_beat * 4)
                    first_pass_pre_refrain = False
                  pre_ref_track.append(msg)
              
                if timeline > refrain_time[0] - 1 and timeline < refrain_time[1] - 1:
                   if first_pass_refrain:
                     msg.time =  int((timeline - refrain_time[0]+1) * total_mid.ticks_per_beat * 4)
                     first_pass_refrain = False
                   refrain_track.append(msg)
                          
                if timeline > pont_time[0] - 1 and timeline < pont_time[1] - 1:
                   if first_pass_pont:
                     msg.time =  int((timeline - pont_time[0]+1) * total_mid.ticks_per_beat * 4)
                     first_pass_pont = False
                   pont_track.append(msg)
              
                if timeline > outro_time[0] - 1 and timeline < outro_time[1] - 1:
                   if first_pass_outro:
                     msg.time =  int((timeline - outro_time[0]+1) * total_mid.ticks_per_beat * 4)
                     first_pass_outro = False
                   outro_track.append(msg)
    
         intro_mid.save(os.path.join(song_path, file[:-10] + '_INTRO.mid'))
         couplet_mid.save(os.path.join(song_path, file[:-10] + '_COUPLET.mid'))
         pre_ref_mid.save(os.path.join(song_path, file[:-10] + '_PRE_REF.mid'))
         print("PPQ")
         print(couplet_mid.ticks_per_beat)
         refrain_mid.save(os.path.join(song_path, file[:-10] + '_REFRAIN.mid'))
         pont_mid.save(os.path.join(song_path, file[:-10] + '_PONT.mid'))
         outro_mid.save(os.path.join(song_path, file[:-10] + '_OUTRO.mid'))
                  
                  
                  
                          
                  
                
