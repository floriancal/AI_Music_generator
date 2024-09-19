#NAME EMPTY
import os
import shutil
import soundfile

prefix = ["H", "L", "C", "M", "S", "T", "K", "B", "Y1", "Y2"]
ignore = ["sample","fx","bell", "chimes", "wet"]
path =  "C://Users//fcala//Desktop//Son//Channel_name_apply"
for song in os.listdir(path):
    song_path = os.path.join(path,song)

    with open(os.path.join(song_path,"pre_name_with_audio.txt"),encoding='utf-8') as f1:
        list1 = f1.readlines()

    for file in os.listdir(song_path):
        try :
            dummy = int(os.path.splitext(file)[0])
            
            if list1[dummy].split(":")[1].replace("\n","") not in prefix:
                
                if os.path.splitext(file)[1] == ".wav":
                        wav_file = os.path.join(song_path,file)
                    try:
                        y, s = soundfile.read(wav_file)
                        vol_rms = y.max() - y.min()
                    except :
                        vol_rms =0

                        #No sound song folder will go to uncomplete
                        if vol_rms <= 6.103515625e-05:
                            empty_sample = True
                            #os.remove(wav_file)
                            list1[dummy]  = str(dummy) + ":EMPTY\n"
        except:
            pass
    f1 = open(os.path.join(song_path,"pre_name_with_audio.txt"), "w", encoding='utf-8')
    for i in list1:
        f1.write(i)
    f1.close()
        
