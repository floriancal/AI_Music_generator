import os
path = "C://Users//fcala//Desktop//Son//FLP_RAW//ADDENDUM2"

for song in os.listdir(path):
    song_path = os.path.join(path,song)
    
    for file in os.listdir(song_path):
        if "untitled" in file and ".flp" in file:
            os.rename(os.path.join(song_path,file),
                      os.path.join(song_path,"untitled.flp"))
        
