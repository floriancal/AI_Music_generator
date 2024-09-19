# add to sample base after removing the empty samples and before replacing them
import os
import shutil
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

## SAMPLE TRANSFER
def add_to_sample_base(path):
  sample_base_path = 'C:/Users/fcala/Desktop/Son/sample_base'
  for song in os.listdir(path):
    song_path = os.path.join(path, song)
    for file in os.listdir(song_path):
      if file[-27:] == '_channel_pattern_linker.txt':
        song_name = file[0:-27]
    for file in os.listdir(song_path):  
        if file[0:2] !='SH':
             if os.path.splitext(file)[0] in prefix_list:
                 fp = os.path.join(song_path,file)
                 new_fp =  os.path.join(sample_base_path, file[0], song_name + file[1] + '.wav')
                 shutil.copy(fp, new_fp)

                 
        if file[0:2] =='SH':
             if os.path.splitext(file)[0] in prefix_list:
                 fp = os.path.join(song_path,file)
                 new_fp =  os.path.join(sample_base_path, 'SH', song_name + file[1] + '.wav')
                 shutil.copy(fp, new_fp)
                 
