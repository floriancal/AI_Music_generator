# Test auto namer perfo
import os
from scipy.io import wavfile
import matplotlib
import matplotlib.pyplot as plt
import crepe
import numpy as np
import pickle
import librosa
def normalize(v):
    norm = np.linalg.norm(v)
    if norm == 0: 
       return v
    return v / norm


#hyperparams
# treshold sustain off
# longeur du son sustain
# valeurs de pitch sur H / K etc
# confidence level



#then sustain classification pas ready 
                
wrong_counter = 0
right_counter =0
wrong_counterpt1=0
wrong_counterpt2=0
right_counterpt1=0
right_conterpt2=0
paths = [ "C://Users//fcala//Desktop//Son//Sort_training_base//X",
          "C://Users//fcala//Desktop//Son//Sort_training_base//FX",
          "C://Users//fcala//Desktop//Son//Sort_training_base//H"]

"""#ici on vise le max avec un wrong counter de 0 
for treshold in [0.15, 0.25, 0.5, 0.75]
    for sustain_defaut = [1.5, 2, 3, 4, 5]
        for ith,path in enumerate(paths):
            for sound in os.listdir(path):
                 sound = os.path.join(path,sound)
                 sr2, y2 = wavfile.read(sound)
                 try:
                    y2 = y2.sum(axis=1) / 2
                 except:
                    pass
                 y2 = y2/max(y2)
                 stop = 0
                 for index2, i in enumerate(range(len(y2),0,-1)):
                     
                     if y2[i-1] >0.25:
                         stop = index2
                         break
                 y_stop  = y2[0: len(y2) - stop]
                 
                #if no sustain not a sample
                 if ith == 0:
                    #if len(y_stop) < sr2:
                    if len(y_stop) < len(y2)*0.8 and len(y_stop) < sr2*1.5:
                         wrong_counter+=1
                         wrong_counterpt1+=1
                    else:
                        right_counter+=1
                        right_counterpt1+=1
                 else:
                    #if len(y_stop) < sr2:
                    if len(y_stop) < len(y2)*0.8:
                         
                         right_counter+=1
                         right_conterpt2+=1
                    else:
                        wrong_counter+=1
                        wrong_counterpt2+=1
        perfo = right_counter/(wrong_counter+right_counter)*100
        P1= right_counterpt1/(wrong_counterpt1+right_counterpt1)*100
        P2= right_conterpt2/(wrong_counterpt2+right_conterpt2)*100
        print(perfo,P1,P2)"""

"""
#K
paths = [ "C://Users//fcala//Desktop//Son//Sort_training_base//K",]
store_wrong= []
store_perfo =[]
for treshold in [30, 50, 100, 200,300,400]:
    wrong_counter = 0
    right_counter =0
    for ith,path in enumerate(paths):
        for sound in os.listdir(path):
            sound = os.path.join(path,sound)
            sr, y = wavfile.read(sound)
            try:
                y = y.sum(axis=1) / 2
            except:
                pass
            y = y [:sr*2]
            time, frequency, confidence, activation = crepe.predict(y, sr, viterbi=True)                              
                
            #keep data above 0.5 level of confidece
            keeped_freq = []
            for index2, conf in enumerate(confidence):
                if conf > 0.5:
                    keeped_freq.append(frequency[index2])
            pitch = np.mean(np.array(keeped_freq))
            if pitch < treshold:
                    right_counter+=1
            else:
                    wrong_counter+=1
    perfo = right_counter/(wrong_counter+right_counter)*100
    store_perfo.append(perfo)
    store_wrong.append(wrong_counter)
print(store_perfo)
print(store_wrong)


#H
paths = [ "C://Users//fcala//Desktop//Son//Sort_training_base//H",]
store_wrong= []
store_perfo =[]
for treshold in [2000, 3000, 100, 200,300,400]:
    wrong_counter = 0
    right_counter =0
    for ith,path in enumerate(paths):
        for sound in os.listdir(path):
            sound = os.path.join(path,sound)
            sr, y = wavfile.read(sound)
            try:
                y = y.sum(axis=1) / 2
            except:
                pass
            y = y [:sr*2]
            time, frequency, confidence, activation = crepe.predict(y, sr, viterbi=True)                              
                
            #keep data above 0.5 level of confidece
            keeped_freq = []
            for index2, conf in enumerate(confidence):
                if conf > 0.5:
                    keeped_freq.append(frequency[index2])
            pitch = np.mean(np.array(keeped_freq))
            if pitch > treshold:
                        right_counter+=1
            else:
                        wrong_counter+=1
    perfo = right_counter/(wrong_counter+right_counter)*100
    store_perfo.append(perfo)
    store_wrong.append(wrong_counter)
print(store_perfo)
print(store_wrong)




"""
# M B test ok ready 256 pr l'instant 
paths = [ "C://Users//fcala//Desktop//Son//Sort_training_base//M",
          "C://Users//fcala//Desktop//Son//Sort_training_base//B"]
for treshold = [100, 200, 256, 300,400,500,1000]
    wrong_counter = 0
    right_counter =0
    for ith,path in enumerate(paths):
        for sound in os.listdir(path):
             sound = os.path.join(path,sound)
             sr, y = wavfile.read(sound)
             try:
                y = y.sum(axis=1) / 2
             except:
                pass
            y = y [:sr*2]
            time, frequency, confidence, activation = crepe.predict(y, sr, viterbi=True)                              
                
            #keep data above 0.5 level of confidece
            keeped_freq = []
            for index2, conf in enumerate(confidence):
                if conf > 0.5:
                    keeped_freq.append(frequency[index2])
            pitch = np.mean(np.array(keeped_freq))
                if pitch > treshold :
                    if ith == 0:
                        right_counter+=1
                    else:
                        wrong_counter+=1
                else:
                    if ith == 0:
                        wrong_counter+=1     
                    else:  
                        right_counter+=1
    perfo = right_counter/(wrong_counter+right_counter)*100
    print(perfo)
            
      """  

             
