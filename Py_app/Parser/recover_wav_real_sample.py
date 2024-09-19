import os
import shutil
def recover_wav_real_samples(path):
    for song in os.listdir(path):
        song_path = os.path.join(path,song)
        f = open(os.path.join(song_path, "recovered_samples.txt"), "a+", encoding = 'utf-8')
        if os.path.isdir(song_path) is True:
            channel_name_list = open(os.path.join(song_path,"channel_name.txt"), encoding = 'utf-8')
            channel_dict = {}
            for line in channel_name_list:
                line_list  = line.split(":") 
                channel_dict[line_list[0]] = line_list[1]
                
            for file in os.listdir(song_path):
                if "_sample_mapper.txt" in file:
                    sample_list = open(os.path.join(song_path,file), encoding='utf-8').readlines()
                    sample_dict = {}
                    for line in sample_list:
                        line_list  = line.split(" , ") 
                        sample_dict[line_list[0]] = line_list[1]
            for channel in channel_dict.keys():
                if os.path.isfile(os.path.join(song_path, str(channel) + ".wav")) is False:
                    if channel in sample_dict.keys(): 
                        filepath = os.path.basename(os.path.normpath(sample_dict[channel]))
                        if ".wav" in os.path.splitext(filepath)[1]:
                            shutil.copy(os.path.join(song_path, filepath.replace("\n","")),
                                        os.path.join(song_path, channel + ".wav"))
                            f.write(channel+ ":" +filepath + "\n")
                            
        f.close()
                
