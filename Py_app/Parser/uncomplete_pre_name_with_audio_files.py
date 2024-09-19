#print uncomplete pre_name_with audio files
import os
path = "C:/Users/fcala/Desktop/Son/Channel_name_apply"
instr_list = ["M", "B", "K", "S", "H", "L", "C", "R", "T",  "Y", "SH"]
prefix_list = ["M1","M2","M3","M4","M5","B1","B2","B3","B4","K1","K2","K3","K4",
               "S1","S2","S3","S4","H1","H2","H3","H4","L1","L2","L3","L4","C1",
               "C2","C3","C4","R1","R2","T1","T2","Y1","Y2","SH"]

def uncomplete_name_printer(path):
    for song in os.listdir(path):
        song_path = os.path.join(path,song)
        name_list = open(os.path.join(song_path, "pre_name_with_audio.txt"), encoding = "utf-8").readlines()
        for n in name_list:
            [channel,name] = n.split(":")
            name = name.replace("\n","")
            if os.path.isfile(os.path.join(song_path,channel + ".wav")):
                if name not in instr_list:
                    if name not in prefix_list:
                         if "fx" not in name.lower():
                             if "sample" not in name.lower():
                                 if "bell" not in name.lower():
                                     if "empty" not in name.lower():
                                         print("uncomplete : " + song_path + "on channel : " + channel)
                    
uncomplete_name_printer(path)
