import os
import shutil

def apply_channel_names_to_wav(path):
    count_name_linker = {}
    for song in os.listdir(path):
        song_path = os.path.join(path,song)
        
        if os.path.isdir(song_path) is True:
            for file in os.listdir(song_path):
                if file[-16:] == 'channel_name.txt':
                    my_file = open(os.path.join(song_path,file), encoding='utf-8')
                    track_names = my_file.readlines()
                    for line in track_names:     
                        count_name_linker[int(line.split(':')[0])] = line.split(':')[1]

                    for file2 in os.listdir(song_path):
                        fp = os.path.join(song_path,file2)
                        if os.path.splitext(file2)[1] == ".wav":
                            try:
                                int(os.path.splitext(file2)[0])
                                is_int = True
                            except:
                                is_int = False
                            try:
                                chan_name = count_name_linker[int(os.path.splitext(file2)[0])].split('\n')[0]
                            except:
                                is_int = False
                                    
                            if is_int is True:
                                chan_name = count_name_linker[int(os.path.splitext(file2)[0])].split('\n')[0]
                                chrn_name = chan_name.replace("\t","").replace(" ","")
                                new_fp  = os.path.join(song_path, chan_name + '.wav')
                                shutil.copyfile(fp, new_fp)
