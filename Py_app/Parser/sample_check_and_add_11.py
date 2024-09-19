#Missing sample solution
import os
import random
import shutil
import mido
from mido import MidiFile
sample_base = 'C:/Users/fcala/Desktop/Son/sample_base'

def sample_check_and_add(path):
    for song in os.listdir(path):
        song_path = os.path.join(path,song)
        if os.path.isdir(song_path) is True:
          for file in os.listdir(song_path):
              if file[-10:] == '_total.mid':
                total_mid_path = os.path.join(song_path, file)
                mid = MidiFile(total_mid_path)
                for track in mid.tracks:
                    track_name = track.name
                    sample_path = os.path.join(song_path, track_name + '.wav')
                    
                    if os.path.isfile(sample_path):
                        pass
                    else:
                        try:
                            base_sample_path = os.path.join(sample_base, track_name[0])
                            selected_file_path  = os.path.join(base_sample_path, random.choice(os.listdir(base_sample_path)))
                            shutil.copy(selected_file_path, sample_path)
                        except:
                            print('fail on track name :', track_name, 'is that okay??' )
