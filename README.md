# AI_Music_generator

This repository is more a collection of code than a proper project. It is not usable in its actual state. it is badly commented (some comments are in french) and not clean code.

However this can be used as a base for a similar project.
For anyone that would like some support at using some of the features please contact me.



The base concept is the following :
Instead of generating songs directly in the time domain this AI music generator uses a more guided approach for the AI.

The base data are FLP projects (FL studio software from Image-Line). After a pre-process done with a C# app the data are extrected from the FLP project.
this application is based on https://github.com/monadgroup/FLParser.

Then a Python script transforms the data into a training database for the AI. 
The data types of the database are the following :
  - midi files with a fixed number of channels for each song, they are always a sequence of midi files for each song (Intro Verse Chorus, Outro).
  - Samples of instruments from different FLP projects and a small encoding associated (based on frequency peaks).
  - A file linking which sample have been used with which channel on the project.

To train the AI at generating some new songs a sequence of model are trained. Originally there was some google collab notebooks with how to train model but those files have been lost.
However the training process is not very complicated.

  - First a model is trained using the midi files to propagate a sequence of note from a given first bar of a midi file. The model is trained on INTRO midi files. At the generation the first bar is initialized randomly. The model used are classic small-GPT3 models.
    
  - Then some seq2seq model are used from https://github.com/google/seq2seq, there is one Verse model, one Choruse, one Outro. This straegy has been used to ensure some coherence through the song. The model uses as input the output of the previous model in the sequence like this : INTRO --> Verse --> Chorus --> Outro.
    At generation time one can define a sequence with more than one Verses and Chorus for the song.

  - We then uses another seq2seq model to predict the pan, the volume, and the stereo of each note for this we encode the midi file as text, with sequentially each channels and an associated encoding of the chosen sample using a simple encoder 

  - Now we finally need to associate a sample for each channel. To do this we train another seq2seq model at giving the full sequence of note for all channels predict the best peak  encoding of samples to associate each channels. We then pick in the database the sample with the closest peak encoding


the obtained files can be used with the Python 

