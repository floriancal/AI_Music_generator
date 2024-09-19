#exctract wav files from their subfolders to th song folder
import os
import shutil

def  exctract_wav_subfolders(path)
for song in os.listdir(path):
    song_path = os.path.join(path,song)
    if os.path.isdir(song_path) is True:
        for sub_dir in os.listdir(song_path):
            sound_path = os.path.join(song_path,sub_dir)
            if os.path.isdir(sound_path):
                for file in os.listdir(sound_path):
                    if os.path.splitext(file)[1] == ".wav":
                        shutil.move(os.path.join(sound_path,file),
                                    os.path.join(song_path,file))
                        
