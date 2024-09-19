#copy chan name.txt
import os
import shutil 
def channel_name(path):
     for song in os.listdir(path):
        song_path = os.path.join(path,song)
        
        if os.path.isdir(song_path) is True:
            for file in os.listdir(song_path):
                if file == 'pre_name_with_audio.txt':
                    shutil.copy(os.path.join(song_path, 'pre_name_with_audio.txt'), os.path.join(song_path, 'channel_name.txt'))
                    
