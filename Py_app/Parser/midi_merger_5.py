#midi merger to combine midi exported with fl and midi parsed

import mido
import os
from mido import Message, MidiFile, MidiTrack

def midi_merger(path):
  for song in os.listdir(path):
    song_path = os.path.join(path,song)
    total_mid = MidiFile()
    total_mid.ticks_per_beat = 96
    if os.path.isdir(song_path) is True:
      for file in os.listdir(song_path):
        if file[-18:] == '_sample_parser.mid':
         rename_flp_path = path #'C:\\Users\\fcala\\Desktop\\Son\\FLP - Renamed'
         sample_mid_path = os.path.join(song_path,file)
         pattern_mid_path = os.path.join(rename_flp_path,file[0:-18] + '.mid')
         #pattern_mid_path = os.path.join(song_path,file[0:-18] + '.mid')
         sample_mid = MidiFile(sample_mid_path)
         pattern_mid = MidiFile(pattern_mid_path)
         sample_mid.ticks_per_beat  = 96
         pattern_mid.ticks_per_beat = 96
         mid_files = [sample_mid, pattern_mid]

         for mid_file in mid_files:
             for track in mid_file.tracks:
                 total_mid.tracks.append(track)
      filename = os.path.basename(os.path.splitext(pattern_mid_path)[0] )
  
      total_mid.save(os.path.join(song_path, filename+ '_total.mid'))
         
         
