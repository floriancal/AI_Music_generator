
# check that sample is not empty thus not a missing plugin

import soundfile
import shutil
import os

def sample_check_and_delete(path):
    prefix_list = ["M1","M2","M3","M4","M5","B1","B2","B3","B4","K1","K2","K3","K4",
                 "S1","S2","S3","S4","H1","H2","H3","H4","L1","L2","L3","L4","C1",
                 "C2","C3","C4","R1","R2","T1","T2","Y1","Y2","SH"]
    empty_song_path = 'C:/Users/fcala/Desktop/Son/empty_sample_base' 
    SR = 44100

    for song in os.listdir(path):
        song_path = os.path.join(path,song)
        filewriter = open(os.path.join(song_path,"empty_sample_tracker.txt"), "w+")
        if os.path.isdir(song_path) is True:
          for file in os.listdir(song_path):
              
              if os.path.splitext(file)[0] in prefix_list:
                  print(file)
                  fp = os.path.join(song_path, file)
                  try:
                      y, s = soundfile.read(fp)
                  except:
                      os.remove(fp)
                  try:
                      vol_rms =  y.max() - y.min()
                  except:
                      vol_rms = 0
                  if vol_rms <= 6.103515625e-05:
                      print('found empty file', fp)
                      new_fp = os.path.join(empty_song_path, file)
                      shutil.move(fp, new_fp)
                      filewriter.write("empty : " + fp)                                  
