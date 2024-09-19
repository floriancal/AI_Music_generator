#Dataset collector
#parse all file for each song
# create song name folder
#put files in their correct dataset folder
#handle : flp_insert_data, sample_base, midi_data_transfer, sample_data
import os
import shutil


def dataset_collector(path):

  insert_path = 'C:/Users/fcala/Desktop/Son/flp_insert_data'
  sample_base_path = 'C:/Users/fcala/Desktop/Son/sample_base'
  sample_data_path = 'C:/Users/fcala/Desktop/Son/sample_data'
  midi_data_transfer_path = 'C:/Users/fcala/Desktop/Son/midi_data_transfer'

  prefix_list = ["M1","M2","M3","M4","M5","M6", "M7",
                 "B1","B2","B3","B4","B5","B6", "B7",
                 "K1","K2","K3","K4","K5","K6", "K7",
                 "S1","S2","S3","S4","S5","S6", "S7",
                 "H1","H2","H3","H4","H5","H6", "H7",
                 "L1","L2","L3","L4","L5","L6", "L7",
                 "C1","C2","C3","C4","C5","C6", "C7",
                 "R1","R2",
                 "T1","T2",
                 "Y1","Y2",
                 "SH"]




  ## INSERT PARSER TRANSFER
  for song in os.listdir(path):
    song_path = os.path.join(path,song)
    if os.path.isdir(song_path) is True:
      for file in os.listdir(song_path):
          if file[-18:] == '_insert_parser.txt':
              fp = os.path.join(song_path,file)
              song_name = file[:-18]
              
              new_song_folder_path_insert = os.path.join(insert_path, song_name)
              
              
              if not os.path.exists(new_song_folder_path_insert):
                  os.makedirs(new_song_folder_path_insert)
                  
             
                  
              new_fp = os.path.join(new_song_folder_path_insert, file)
              shutil.copy(fp, new_fp)


  #Channel Parser Transfer
  for song in os.listdir(path):
      song_path = os.path.join(path,song)
      if os.path.isdir(song_path) is True:
        for file in os.listdir(song_path):
            if file[-19:] == '_channel_parser.txt':
                fp = os.path.join(song_path,file)
                new_fp = os.path.join(new_song_folder_path_insert, file)
                shutil.copy(fp, new_fp) 
  


              

  # MIDI FILES TRANSFER        
      for file in os.listdir(song_path):   
          if file[-10:] == '_INTRO.mid':
               fp = os.path.join(song_path,file)
               new_fp = os.path.join(midi_data_transfer_path, 'intro', file)
               shutil.copy(fp, new_fp)
          if file[-12:] == '_COUPLET.mid':
               fp = os.path.join(song_path,file)
               new_fp = os.path.join(midi_data_transfer_path, 'couplet', file)
               shutil.copy(fp, new_fp)
          if file[-12:] == '_PRE_REF.mid':
               fp = os.path.join(song_path,file)
               new_fp = os.path.join(midi_data_transfer_path, 'pre_refrain', file)
               shutil.copy(fp, new_fp)
          if file[-12:] == '_REFRAIN.mid':
               fp = os.path.join(song_path,file)
               new_fp = os.path.join(midi_data_transfer_path, 'refrain', file)
               shutil.copy(fp, new_fp)
          if file[-9:] == '_PONT.mid':
               fp = os.path.join(song_path,file)
               new_fp = os.path.join(midi_data_transfer_path, 'pont', file)
               shutil.copy(fp, new_fp)
          if file[-10:] == '_OUTRO.mid':
               fp = os.path.join(song_path,file)
               new_fp = os.path.join(midi_data_transfer_path, 'outro', file)
               shutil.copy(fp, new_fp)
          if file[-10:] == '_part1.mid':
               fp = os.path.join(song_path,file)
               new_fp = os.path.join(midi_data_transfer_path, (file.split('_')[-2] + '_1').lower(), file)
               shutil.copy(fp, new_fp)
          if file[-10:] == '_part2.mid':
               fp = os.path.join(song_path,file)
               new_fp = os.path.join(midi_data_transfer_path, (file.split('_')[-2] + '_2').lower(), file)
               shutil.copy(fp, new_fp)

### ADD to song sample data folder
  for song in os.listdir(path):
    song_path = os.path.join(path, song)
    for file in os.listdir(song_path):
      if file[-27:] == '_channel_pattern_linker.txt':
        song_name = file[0:-27]
    for file in os.listdir(song_path):  
        if file[0:2] !='SH':
             if os.path.splitext(file)[0] in prefix_list:
                 fp = os.path.join(song_path,file)
                 
                 if not os.path.exists(os.path.join(sample_data_path, song_name)):
                   os.makedirs(os.path.join(sample_data_path, song_name))
                    
                 sample_data_fp = os.path.join(sample_data_path, song_name, file)
                 shutil.copy(fp, sample_data_fp)
        if file[0:2] =='SH':
             if os.path.splitext(file)[0] in prefix_list:
                 fp = os.path.join(song_path,file)

                 if not os.path.exists(os.path.join(sample_data_path, song_name)):
                   os.makedirs(os.path.join(sample_data_path, song_name))

                 sample_data_fp = os.path.join(sample_data_path, song_name, file)
                 shutil.copy(fp, sample_data_fp)
                   
