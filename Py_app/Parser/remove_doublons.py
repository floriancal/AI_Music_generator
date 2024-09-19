import os
def remove_txt_doublons(path):
    for song in os.listdir(path):
        song_path = os.path.join(path,song)
        for file in os.listdir(song_path):
            if "_channel_parser" in file or "_channel_pattern_linker" in file or "insert_parser" in file or "_pre_name_channel" in file or "_sample_mapper" in file or "_sample_parser" in file:

                 for file2 in os.listdir(song_path):
                     if os.path.splitext(file2)[1] == ".flp":
                         if os.path.splitext(file2)[0] not in file:
                             print(os.path.join(song_path,file))
                             os.remove(os.path.join(song_path,file))
                     
                         
            
path = 'C://Users//fcala//Desktop//Son//Channel_name_apply//'
remove_txt_doublons(path)                          
