import pickle
import os
import librosa
from sklearn.neighbors import KNeighborsClassifier
from sklearn.model_selection import cross_val_score
from scipy.io import wavfile
import numpy as np
import imblearn
from imblearn.over_sampling import RandomOverSampler
def mean_mfccs(wave,sr):
    return [np.mean(feature) for feature in librosa.feature.mfcc(y=wave, sr=sr)]


###TRAINER FOR LOW HAT HIHAT SELECT

#best param k = 1 leaf= 1 for all till now
#Compute MFCC coefs
"""X = []
y = []
paths = ["C://Users//fcala//Desktop//Son//Sort_training_base//H", "C://Users//fcala//Desktop//Son//Sort_training_base//L"]

for i,path in enumerate(paths):
    for sound in os.listdir(path):
        sound = os.path.join(path,sound)
        wave,sr = librosa.load(sound)
        try:
            wave =  wave.sum(axis=1) / 2
        except:
            pass
        MFCC = mean_mfccs(wave,sr)
        X.append(MFCC)  
        y.append(i)


KNN = KNeighborsClassifier(leaf_size=1, n_neighbors=1, p=1)
KNN.fit(X, y)
score = cross_val_score(KNN, X, y)
print(score)

with open('knn_hat.pickle', 'wb') as f:
    pickle.dump(KNN, f)
##
##
#TRAINER FOR full drum
X = []
y = []
paths = [ "C://Users//fcala//Desktop//Son//Sort_training_base//L",
          "C://Users//fcala//Desktop//Son//Sort_training_base//H",
          "C://Users//fcala//Desktop//Son//Sort_training_base//S",
          "C://Users//fcala//Desktop//Son//Sort_training_base//C",
          "C://Users//fcala//Desktop//Son//Sort_training_base//T",
          "C://Users//fcala//Desktop//Son//Sort_training_base//Y",
          "C://Users//fcala//Desktop//Son//Sort_training_base//SH"]

for i,path in enumerate(paths):
    for sound in os.listdir(path):
        sound = os.path.join(path,sound)
        wave,sr = librosa.load(sound)
        try:
            wave =  wave.sum(axis=1) / 2
        except:
            pass
        MFCC = mean_mfccs(wave,sr)
        X.append(MFCC)  
        y.append(i)


KNN = KNeighborsClassifier(leaf_size=1, n_neighbors=1, p=1)
KNN.fit(X, y)
score = cross_val_score(KNN, X, y)
print(score)
"""
##
##with open('knn_drums.pickle', 'wb') as f:
##    pickle.dump(KNN, f)
##
##
###TRAINER FOR CYMBALS
##X = []
##y = []
##paths = [ "C://Users//fcala//Desktop//Son//Sort_training_base//L",
##          "C://Users//fcala//Desktop//Son//Sort_training_base//Y"]
##
##for i,path in enumerate(paths):
##    for sound in os.listdir(path):
##        sound = os.path.join(path,sound)
##        wave,sr = librosa.load(sound)
##        try:
##            wave =  wave.sum(axis=1) / 2
##        except:
##            pass
##        MFCC = mean_mfccs(wave,sr)
##        X.append(MFCC)  
##        y.append(i)
##
##
##KNN = KNeighborsClassifier()
##KNN.fit(X, y)
##
##with open('knn_cymbal.pickle', 'wb') as f:
##    pickle.dump(KNN, f)


#TRAINER FOR melo/drums choice
#unbalanced model 
X = []
y = []
paths = [ "C://Users//fcala//Desktop//Son//Sort_training_base//M",
          "C://Users//fcala//Desktop//Son//Sort_training_base//B",
          "C://Users//fcala//Desktop//Son//Sort_training_base//Y",
          "C://Users//fcala//Desktop//Son//Sort_training_base//L",
          "C://Users//fcala//Desktop//Son//Sort_training_base//H",
          "C://Users//fcala//Desktop//Son//Sort_training_base//S",
          "C://Users//fcala//Desktop//Son//Sort_training_base//C",
          "C://Users//fcala//Desktop//Son//Sort_training_base//T",
          "C://Users//fcala//Desktop//Son//Sort_training_base//Y",
          "C://Users//fcala//Desktop//Son//Sort_training_base//SH",
          "C://Users//fcala//Desktop//Son//Sort_training_base//K"]

for i,path in enumerate(paths):
    for sound in os.listdir(path):
        sound = os.path.join(path,sound)
        wave,sr = librosa.load(sound)
        try:
            wave =  wave.sum(axis=1) / 2
        except:
            pass
        MFCC = mean_mfccs(wave,sr)
        X.append(MFCC)
        if i>1:
            y.append(1)
        else:
            y.append(0)
#ROS
# define oversampling strategy
oversample = RandomOverSampler(sampling_strategy=0.75)
# fit and apply the transform
X, y = oversample.fit_resample(X, y)

KNN = KNeighborsClassifier(leaf_size=1, n_neighbors=1, p=1)
KNN.fit(X, y)

with open('knn_melo_drums.pickle', 'wb') as f:
    pickle.dump(KNN, f)

"""from sklearn.neighbors import KNeighborsClassifier
from sklearn.metrics import classification_report
from sklearn.model_selection import train_test_split
from sklearn.metrics import roc_auc_score
from sklearn.model_selection import GridSearchCV
#List Hyperparameters that we want to tune.
leaf_size = list(range(1,50))
n_neighbors = list(range(1,30))
p=[1,2]
#Convert to dictionary
hyperparameters = dict(leaf_size=leaf_size, n_neighbors=n_neighbors, p=p)
#Create new KNN object
knn_2 = KNeighborsClassifier()
#Use GridSearch
clf = GridSearchCV(knn_2, hyperparameters, cv=4)
#Fit the model
best_model = clf.fit(X,y)
#Print The value of best Hyperparameters
print('Best leaf_size:', best_model.best_estimator_.get_params()['leaf_size'])
print('Best p:', best_model.best_estimator_.get_params()['p'])
print('Best n_neighbors:', best_model.best_estimator_.get_params()['n_neighbors'])
"""

#TRAINER FOR sample/drums choice
#unbalanced model 
X = []
y = []
paths = [ "C://Users//fcala//Desktop//Son//Sort_training_base//X",
          "C://Users//fcala//Desktop//Son//Sort_training_base//FX",
          "C://Users//fcala//Desktop//Son//Sort_training_base//Y",
          "C://Users//fcala//Desktop//Son//Sort_training_base//L",
          "C://Users//fcala//Desktop//Son//Sort_training_base//H",
          "C://Users//fcala//Desktop//Son//Sort_training_base//S",
          "C://Users//fcala//Desktop//Son//Sort_training_base//C",
          "C://Users//fcala//Desktop//Son//Sort_training_base//T",
          "C://Users//fcala//Desktop//Son//Sort_training_base//Y",
          "C://Users//fcala//Desktop//Son//Sort_training_base//SH",
          "C://Users//fcala//Desktop//Son//Sort_training_base//K"]

for i,path in enumerate(paths):
    for sound in os.listdir(path):
        sound = os.path.join(path,sound)
        wave,sr = librosa.load(sound)
        try:
            wave =  wave.sum(axis=1) / 2
        except:
            pass
        MFCC = mean_mfccs(wave,sr)
        X.append(MFCC)
        if i>1:
            y.append(1)
        else:
            y.append(0)
#ROS
# define oversampling strategy
oversample = RandomOverSampler(sampling_strategy=0.75)
# fit and apply the transform
X, y = oversample.fit_resample(X, y)

KNN = KNeighborsClassifier(leaf_size=1, n_neighbors=1, p=1)
KNN.fit(X, y)

with open('knn_sample_drums.pickle', 'wb') as f:
    pickle.dump(KNN, f)
