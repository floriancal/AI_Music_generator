# chanel parser file renamer

import os
def channel_file_renamer(path):
    for song in os.listdir(path):
        song_path = os.path.join(path,song)
        count_name_linker = {}
        
        if os.path.isdir(song_path) is True:
            with open(os.path.join(song_path, 'channel_name.txt'),'r', encoding='utf-8') as file:
                track_names = file.readlines()
                for line in track_names:     
                    count_name_linker[int(line.split(':')[0])] = line.split(':')[1]
                    
            for file in os.listdir(song_path):          
                if file[-19:] == '_channel_parser.txt':
                    with open(os.path.join(song_path, file),'r', encoding='utf-8') as file:
                        track_counts = file.readlines()
                        track_re_write = []
                        for line in track_counts:
                            try:
                                chan_number = int(line.split(' : ')[1].split(' , ')[0])
                                channel_name =  count_name_linker[chan_number]
                                my_line = line.split(' : ')[0] + ' : ' + channel_name.replace('\n','') + ' , ' + line.split(' : ')[1].split(' , ')[1]
                                track_re_write.append(my_line)
                            except:
                                track_re_write.append(line)
                            
            for file in os.listdir(song_path):
                  if file[-19:] == '_channel_parser.txt':
                    with open(os.path.join(song_path, file),'w', encoding='utf-8') as file:
                      for line in track_re_write:
                          file.write(line)
