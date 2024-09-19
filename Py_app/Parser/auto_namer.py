
#Parse all song look at channel_pre_namer_file and try to add some channel names in it
import os
from scipy.io import wavfile
import matplotlib
import matplotlib.pyplot as plt
import crepe
import numpy as np
import pickle
import librosa

KNN_hat = pickle.load(open("knn_hat.pickle","rb"))
KNN_drums = pickle.load(open("knn_hat.pickle","rb"))
KNN_cymbal = pickle.load(open("knn_cymbal.pickle","rb"))
KNN_melo_drums = pickle.load(open("knn_melo_drums.pickle","rb"))
KNN_sample_drums = pickle.load(open("knn_sample_drums.pickle","rb"))
def mean_mfccs(wave,sr):
    return [np.mean(feature) for feature in librosa.feature.mfcc(y=wave, sr=sr)]

def normalize(v):
    norm = np.linalg.norm(v)
    if norm == 0: 
       return v
    return v / norm

# Determine the closest drum sound
def drum_sort(MFCC):

    pred = KNN_drums.predict([MFCC])
    if pred == 0:
        channel_name = 'L'
    if pred == 1:
        channel_name = 'H'
    if pred == 2:
        channel_name = 'S'
    if pred == 3:
        channel_name = 'C'
    if pred == 4:
        channel_name = 'T'
    if pred == 5:
        channel_name = 'Y'
    if pred ==6:
        channel_name = 'SH' 
    return channel_name

#Main funtion 
def auto_namer(path):
    # PREFIX LIST in the future we need a cmmon file to define those parameters
    instr_list = ["M", "B", "K", "S", "H", "L", "C", "R", "T",  "Y", "SH", "X", "F"]

    prefix_list = ["M1","M2","M3","M4","M5","B1","B2","B3","B4","K1","K2","K3","K4",
                   "S1","S2","S3","S4","H1","H2","H3","H4","L1","L2","L3","L4","C1",
                   "C2","C3","C4","T1","T2","Y1","Y2","SH", "X1", "X2",]

    # classic all songs in folder loop
    for song in os.listdir(path):
        song_path = os.path.join(path, song)
        if os.path.isdir(song_path) is True:
            for file in os.listdir(song_path):
                
                #look at pre namer file
                if file[-20:] == 'pre_name_channel.txt':
                    fp = os.path.join(song_path,file)
                    fp2 = os.path.join(song_path,'pre_name_with_audio.txt')
                    channel_list = open(fp, encoding='utf-8').readlines()

                    if os.path.isfile(fp2):
                        channel_audio_list = open(fp2, encoding='utf-8').readlines()
                        for index, item in enumerate(channel_audio_list):
                            channel_audio_list[index] = item.split(':')[1].replace("\n","")
                            
                    # recover channel name in a a list    
                    for index, item in enumerate(channel_list):
                        channel_list[index] = item[:-1]
                    if os.path.isfile(fp2):
                        for index, item in enumerate(channel_list):
                            try:
                                channel_list[index] = channel_audio_list[index]
                            except:
                                pass #handle saut de ligne a la fin d'un fichier mais pas l'autre 
                    # iterate on that list                             
                    for index, channel_name in enumerate(channel_list):          
                        #consider only non determined samples
                        if channel_name not in prefix_list and channel_name not in instr_list and "fx" not in channel_name.lower():
                            # take real sample if exist
                            fp = os.path.join(song_path,str(index) + '.wav')
                            if os.path.isfile(fp):
                                    wav_path = fp
                                    print(fp)
                                    print(channel_name)
                                #detect the main frequency
                                #try:
                                    sr, y = wavfile.read(wav_path)
                                    y2,sr2 = librosa.load(wav_path)
                                    try:
                                        y = y.sum(axis=1) / 2
                                    except:
                                        pass
                                    try:
                                        y2 =  y2.sum(axis=1) / 2
                                    except:
                                        pass
                                    MFCC = mean_mfccs(y2,sr2)
                                    
                                    #limit to 3 sec the eval
                                    y = y [:sr*2]
                                    time, frequency, confidence, activation = crepe.predict(y, sr, viterbi=True)                              
                                        
                                    #keep data above 0.5 level of confidece
                                    keeped_freq = []
                                    for index2, conf in enumerate(confidence):
                                        if conf > 0.5:
                                            keeped_freq.append(frequency[index2])
                                    pitch = np.mean(np.array(keeped_freq))
                          
                                    # Handle drums
                                    if channel_name == 'drums':
                                        if pitch < 150:
                                            channel_name = 'K'
                                        elif pitch > 3000:
                                            channel_name = 'H'
                                        
                                        else:
                                            channel_name = drum_sort(MFCC)
                                                        
                    
                                    #Handle melo M or B 
                                    elif channel_name == 'melo':
                                        if pitch < 256:
                                            channel_name = 'B'
                                        else:
                                            channel_name = 'M'
                                            
                                    # Use cases
                                    # Hat case
                                    elif 'hat' in channel_name.lower():
                                        
                                      pred = KNN_hat.predict([MFCC])

                                      if  pred  == 0:
                                          channel_name = "H"
                                      elif pred == 1:
                                          channel_name = "L"      
                                    # Cymbal case
                                    elif 'cymbal' in channel_name.lower():
                                       pred = knn_cymbal.predict([MFCC])
                                       if pred == 0:
                                           channel_name = "L"
                                       elif pred == 1:
                                           channel_name = "Y"
            
                                    # Default mode full analysis   
                                    else:
                                        #Here we have no clue with the name so we proceed by eliminations
                                        #first we eliminate sample possibility if we come from a plugin

                                        #melo or drums
                                        if "plugin" in channel_name.lower():
                                            #crash and ride has sustain too
                                            #Detect sustain properties 
                                             sr2, y2 = wavfile.read(wav_path)
                                             try:
                                                y2 = y2.sum(axis=1) / 2
                                             except:
                                                pass
                                             y2 = normalize(y2)
                                             #check correlation between a drum type signal and a melo type signal 
                                             pred =  KNN_melo_drums.predict([MFCC])
                                             if pred == 0:
                                                if pitch < 256:
                                                    channel_name = 'B'
                                                else:
     
                                                    channel_name = 'M'
                                             elif pred == 1:
                                                if pitch < 150:
                                                        channel_name = 'K'
                                                elif pitch > 3000:
                                                        channel_name = 'H'
                                                else:
                                                        channel_name = drum_sort([MFCC])
                                                      
                                        #sample or drums
                                        else:
                                             #Detect sustain properties 
                                             sr2, y2 = wavfile.read(wav_path)
                                            
                                             y2 = normalize(y2)
                                             stop = 0
                                             for index2, i in enumerate(range(len(y2),0,-1)):
                                                 if i >0.25:
                                                     stop = index2
                                                     break
                                             y_stop  = y2[0: len(y2) - stop]
                                            #if no sustain not a sample
                                             if len(y_stop) < sr*2:
                                                pred =  KNN_sample_drums.predict([MFCC])
                                                if pred == 0:
                                                    channel_name = "X"
                                                else:
                                              
                                                    if pitch < 150:
                                                        channel_name = 'K'
                                                    elif pitch > 3000:
                                                        channel_name = 'H'
                                                    else:
                                                        channel_name = drum_sort(MFCC)
                                             else:
                                                channel_name = "X"
                                #except:
                                  #  pass
                            channel_list[index] = channel_name
            print(channel_list)
            # Finnaly write modified channel list        
            with open(fp2, 'w', encoding='utf8') as file:
                for index3, channel_name in enumerate(channel_list):
                    file.write(str(index3) +':'+ channel_name + '\n')
