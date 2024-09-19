#FOLDER CLEAN
import os
def folder_clean(path):
    for song in os.listdir(path):
        song_path = os.path.join(path,song)
        for file in os.listdir(song_path):
            if "_Current.wav" in file:
                os.remove(os.path.join(song_path,file))
            if "_Insert" in file and ".wav" in file:
                os.remove(os.path.join(song_path,file))
            if os.path.isdir(os.path.join(song_path,file)):
                sound_path = os.path.join(song_path,file)
                for file2 in os.listdir(sound_path):
                    if "_Current.wav" in file2:
                        os.remove(os.path.join(sound_path,file2))
                    if "_Insert" in file2 and ".wav" in file2:
                        os.remove(os.path.join(sound_path,file2))
                    
            

