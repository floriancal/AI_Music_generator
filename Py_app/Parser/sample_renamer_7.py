#Sample renamer

#miss a file that write text sample file name associated to channel number
import os
import shutil
Fl_path = 'C://Program Files//Image-Line//FL Studio 20'

def sample_renamer(path):
  for song in os.listdir(path):
    song_path = os.path.join(path,song)
    if os.path.isdir(song_path) is True:
        for file in os.listdir(song_path):
            if file[-17:] == 'sample_mapper.txt':
               sample_mapping_data = open(os.path.join(song_path,file)).readlines()
               for data in sample_mapping_data:
                   data_splitted = data.split(',')
                   channel = int(data_splitted[0])
                   sample_path = data_splitted[1].split('\n')[0][1:]
                   
                   """if sample_path[0:9] == '%FLStudio':
                     pt_list = os.path.normpath(sample_path).split(os.path.sep)
                     build_pt = 'yy'
                     for item in pt_list[1: ]:
                       build_pt  = os.path.join(build_pt, item)
                     relative_path = build_pt
                     sample_path = os.path.join(Fl_path, relative_path, 'refusecopy')"""
                   
                   try:
                     #pass
                     # deactivate that line means not using real sample and only generated with flp samples
                     local_path = os.path.join(song_path, os.path.basename(sample_path))
                     if os.path.isdir(local_path) is False:
                       shutil.copy(local_path, os.path.join(song_path, str(channel) + '.wav'))
                     
                   except:
                     print( 'path : ', sample_path, 'is not leading to any sample')
                     
                
