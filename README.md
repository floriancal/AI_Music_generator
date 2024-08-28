# AI_Music_generator

This repository is more a collection of code than a proper project. It is not usable in its actual state. It is badly commented (some comments are in french) and not clean code.

However this can be used as a base for a similar project.
For anyone that would like some support at using some of the features please contact me.


The base concept is the following :
Instead of generating waveform directly in the time domain (like https://github.com/ibab/tensorflow-wavenet) this AI music generator uses a more guided approach for the AI.

The database (not provided) is a list of FLP projects (FL studio software from Image-Line). After a pre-process done with a C# app, the data are extrected from the FLP projects.
This application is based on https://github.com/monadgroup/FLParser.

Then a Python script transforms the data into a training database for the AI. 
The data types of the database are the following :

  - Midi files with a fixed number of channels for each song, they are always a sequence of midi files for each song (Intro, Verses, Choruses, Outro).
  - Samples of instruments from different FLP projects and a small encoding associated (based on frequency peaks).
  - A file linking which sample have been used with which channel on the project.

To train the AI at generating some new songs a sequence of model are trained. Originally there was some google collab notebooks with how to train model but those files have been lost.
However the training process is not very complicated.

  - First a small small-GPT3 model is trained using the INTRO midi files to propagate a sequence of note providing the first bar of the INTRO midi file. At generation the first bar is initialized randomly.  
    
  - Then a series of seq2seq model are used from https://github.com/google/seq2seq. There is one Verse model, one Choruses, one Outro. This straegy has been used to ensure some coherence through the song. The models uses as input the output of the previous model in the sequence like this : INTRO --> Verse --> Chorus --> Outro.
    At generation time one can define a sequence with more than one Verses and Chorus for the song.

  - Now we associate a sample for each channel. To do this we train another seq2seq model at : giving the full sequence of note for all channels, predict the best peak encoding of samples to associate with each channels. At prediction time, we pick in the database the sample with the closest peak encoding result.

  - We then uses another seq2seq model to predict the pan, the volume, and the stereo of each note. All the channels are passed sequentially with the associated encoding of the chosen sample.

- The obtained files can be used with the Python and the C# app to be converted to brand new FLP projects and can be exported to any format using FL Studio.
  
Again, this project is a complete draft at disposal of anyone who might have an interest in it, enjoy.


