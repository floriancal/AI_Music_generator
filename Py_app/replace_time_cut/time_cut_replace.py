import os
import shutil
path_receiving = "C:/Users/fcala/Desktop/Son/Channel_name_apply"
path_emitting = "C:/Users/fcala/Desktop/Son/DONE_SONGS"

for song_rec in os.listdir(path_receiving) :
    if song_rec in os.listdir(path_emitting) :
        print(song_rec)
        emitting_song_path = os.path.join(path_emitting, song_rec)
        emitting_file = os.path.join(emitting_song_path, "pre_name_with_audio.txt")
        receiving_file = os.path.join(path_receiving, song_rec, "pre_name_with_audio.txt")
        shutil.copy(emitting_file, receiving_file)

    
