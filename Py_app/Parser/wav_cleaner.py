#FOLDER CLEAN
import os
import soundfile
def folder_clean(path):
    for song in os.listdir(path):
        song_path = os.path.join(path,song)
        for file in os.listdir(song_path):
            if os.path.isdir(file):
                sound_path = os.path.join(song_path,file)
                for file2 in os.listdir(sound_path):
                    
                    if os.path.splitext(file)[1] == ".wav":
                        fp = os.path.join(song_path, file)
                        try:
                          test = int(os.path.splitext(file)[0])           
                          try:
                              y, s = soundfile.read(fp)
                          except:
                              os.remove(fp)
                          try:
                              vol_rms =  y.max() - y.min()
                          except:
                              vol_rms = 0
                          
                          if vol_rms <= 6.103515625e-05:
                              os.remove(fp)                      
                        except:
                          pass

