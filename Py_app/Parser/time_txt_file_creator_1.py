#time txt creator

import os

def time_txt_creator(path):
  for song in os.listdir(path):
    song_path = os.path.join(path,song)
    
    if os.path.isdir(song_path) is True:
        with open(os.path.join(song_path, 'time_cut.txt'),'w') as file:
            file.write('INTRO :\n')
            file.write('COUPLET :\n')
            file.write('PRE_REF :\n')
            file.write('REFRAIN :\n')
            file.write('PONT :\n')
            file.write('OUTRO :\n')
          
          
          
          
