#SCRIPT TO LIMIT THE MESURES OF A MIDI FILE TO AVOID MEMORRY ERROR IN SEQ 2 SeQ
# midi_max_len_limiter("C://Users//fcala//Desktop//Son//test")
import mido
import os
from mido import Message, MidiFile, MidiTrack, MetaMessage



def midi_max_len_limiter(path):
    segment_to_limit = ['refrain', 'intro', 'pont','outro']
    split_time = 'half'
    for song in os.listdir(path):
        song_path = os.path.join(path,song)
    
        if os.path.isdir(song_path) is True:
          
          for segment in segment_to_limit:
              segment = segment.upper()
              segment_flnme = '_' + segment + '.mid'
              for file in os.listdir(song_path):
                  if file[-10:] == segment_flnme:
                      mid_path = os.path.join(song_path, file)
                      mid = MidiFile(mid_path)
                      prt1 = MidiFile()
                      prt2 = MidiFile()
                      
                      if split_time == 'half':

                          max_time = 0
                          for track in mid.tracks:
                              timeline_check  = 0
                              for msg in track:
                                   timeline_check += msg.time
                              if timeline_check > max_time:
                                  max_time = timeline_check
                          split_time = max_time / 2
                          
                      for track in mid.tracks:
                           timeline = 0
                           track_name = track.name
                           
                           prt_1_track = MidiTrack()
                           prt_2_track = MidiTrack()
                           
                           prt1.tracks.append(prt_1_track)
                           prt_1_track.append(Message('program_change', program=12, time=0))
                           prt_1_track.append(MetaMessage('track_name', name=track_name))
        
                           prt2.tracks.append(prt_2_track)
                           prt_2_track.append(Message('program_change', program=12, time=0))
                           prt_2_track.append(MetaMessage('track_name', name=track_name))

                           first_pass_pt2 = True
                           
                           for msg in track:
                               timeline += msg.time
                               if timeline < split_time:
                                   prt_1_track.append(msg)
                               else:
                                   if first_pass_pt2:
                                        msg.time = int(timeline - split_time +1)
                                        first_pass_pt2 = False
                                   prt_2_track.append(msg)
                      prt1.save(os.path.join(song_path, os.path.splitext(segment_flnme)[0] + '_part1.mid'))                 
                      prt2.save(os.path.join(song_path, os.path.splitext(segment_flnme)[0] + '_part2.mid'))               
                                   
                                
                               
                      
                    
