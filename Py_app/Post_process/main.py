#Parse all song look at channel_pre_namer_file and try to add some channel names in it
from frequency_detector import *
import os
instr_list = ["M", "B", "K", "S", "H", "L", "C", "R", "T",  "Y", "SH"]
prefix_list = ["M1","M2","M3","M4","M5","B1","B2","B3","B4","K1","K2","K3","K4",
               "S1","S2","S3","S4","H1","H2","H3","H4","L1","L2","L3","L4","C1",
               "C2","C3","C4","R1","R2","T1","T2","Y1","Y2","SH"]


path  = 'C:/Users/fcala/Desktop/Son/Unitary_run/'

# classic all songs in folder loop
for song in os.listdir(path):
    song_path = os.path.join(path, song)
    if os.path.isdir(song_path) is True:
        for file in os.listdir(song_path):
           
            #look at pre namer file
            if file[-20:] == 'pre_name_channel.txt':
               
                fp = os.path.join(song_path,file)
                channel_list = open(fp).readlines()
                count = 0
                for index, channel_name in enumerate(channel_list):
                    count += 1

                    #consider only non determined samples
                    if channel_name not in prefix_list and channel_name not in instr_list:

                        # take real sample if exist 
                        if os.path.isfile(str(count) + 'wav'):
                            wav_path = str(count) + 'wav'
                        else:
                            wav_path = os.path.join(song_path, 'Track_channel_' + str(count) + '.wav')

                        #detect the main frequency 
                        pitch = detect_pitch(wav_path)

                        if channel_name == 'drums':
                            if pitch < 350:
                                channel_name = 'K'
                                
                        #this icludes bass 
                        elif channel_name == 'melo':
                            if pitch < 400:
                                channel_name = 'B'
                            else:
                                channel_name = 'M'
                                

                        else:
                            channel_name = 'UNKNOWN'

                        channel_list[index] = channel_name
                with open(fp, 'w') as file:
                    for channel_name in channel_list:
                        file.write(channel_name)
                    
                        
                        
