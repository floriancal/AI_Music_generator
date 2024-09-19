#track count/ track: name merger

import os

def sample_parser_renamer(path):
  for song in os.listdir(path):
    song_path = os.path.join(path,song)
    count_name_linker = {}
    
    if os.path.isdir(song_path) is True:
        with open(os.path.join(song_path, 'channel_name.txt'),'r', encoding='utf-8') as file:
            track_names = file.readlines()
            for line in track_names:     
                count_name_linker[int(line.split(':')[0])] = line.split(':')[1]
                
        for file in os.listdir(song_path):          
            if file[-17:] == 'sample_parser.txt':
                with open(os.path.join(song_path, file),'r', encoding='utf-8') as file:
                    track_counts = file.readlines()
                    track_re_write = []
                    for line in track_counts:
                      
                              
                            if line[0:14] == 'channel_name :':
                                track_count = line[14:]
                                
                                try:
                                  channel_name =  count_name_linker[int(track_count)]
                                  track_re_write.append('channel_name :'+ channel_name)
                                  
                                except :
                                  track_re_write.append(line)
                                  
                            else:
                                track_re_write.append(line)
                                
        for file in os.listdir(song_path):
              if file[-17:] == 'sample_parser.txt':
                with open(os.path.join(song_path, file),'w', encoding='utf-8') as file:
                  for line in track_re_write:
                      file.write(line)


        
