# track name creator

import os

def track_name_creator(path):
  for song in os.listdir(path):
    song_path = os.path.join(path,song)
    
    if os.path.isdir(song_path) is True:
        with open(os.path.join(song_path, 'channel_name.txt'),'w') as file:
            for i in range(1,36):
                file.write(str(i) + ',\n')
            
          
          
          

