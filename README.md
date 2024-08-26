# AI_Music_generaor

Instead of generating songs directly in the time domain this AI music generator uses a more guided approach for the AI.

This repository is more a collection of code than a proper project. It is not usable in its actual state. it is badly commented (some comments are in french) and not clean code.

However this can be used as a base for a similar project.
For anyone that would like some support at using some of the features please contact me.



The base concept is the following :
The base data are FLP projects (FL studio software from Image-Line). After a pre-process done with a C# app the data are extrected from the FLP project.
Then a Python script transforms the data into a training database for the AI. 
The data types of the database are the following :
  - midi files with a fixed number of channels for each song, they are always a sequence of midi files for each song like Intro Couplet Refrain etc..
  - Samples from different FLP projects
  - A file linking which sample have been used with which channel on the project.

To train the AI at generating some new songs a sequence of model are trained. Originally there was some google collab notebooks with how to train model but those files have been lost.
However the training process is not very complicated.

  - First is trained using the midi files to propagate a sequence of note from a given first bar of a midi file. The model is trained for each type of midi files (Intro, Refrain,Couplet). At the generating process the first bar is initialized randomly for the intro model and the folowwing model uses as a first bar the last bar of the previous. The model used are classic small-GPT3 models.
    
  - Then a simple linear


