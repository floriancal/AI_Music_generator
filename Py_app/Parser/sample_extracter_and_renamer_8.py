# Fl extracted sample mover and renamer
import os
#import mido
#from mido import MidiFile
import shutil

def sample_extracter_and_renamer(path, mode):
     for song in os.listdir(path):
      song_path =os.path.join(path,song)
      for item in os.listdir(song_path):
           sound_path = os.path.join(song_path,item)
           if os.path.isdir(sound_path):
               for file in os.listdir(sound_path):
                    if os.path.splitext(file)[1] == ".flp":
                        name = os.path.splitext(file)[0]
               for file in os.listdir(sound_path):
                   if os.path.splitext(file)[1] == ".wav":
                       if os.path.splitext(file)[0] == name or os.path.splitext(file)[0] == name + "_Master":
                           os.rename(os.path.join(sound_path,file), os.path.join(song_path, item +".wav"))
            

     """ sample_path = C:/Users/fcala/Documents/AAA_Sample_auto_saver
    #MOVER RETROUVER LE NOM DU fichier flp et les wav dans l'autre dossier 
    for sample in os.listdir(sample_path):
        if os.path.splitext(sample)[1] == '.wav':
            sample_fp = os.path.join(sample_path,sample)
            
            for song in os.listdir(path):
                  song_path = os.path.join(path,song)
                  
                  for file in os.listdir(song_path):
                      if os.path.splitext(file)[1] == ".flp":
                          name = os.path.splitext(sample)[0].split(' - ')[0]
                          if len(os.path.splitext(sample)[0].split(' - ')) > 2:
                              name = os.path.splitext(sample)[0].split(' - ')[0] + ' - ' +  os.path.splitext(sample)[0].split(' - ')[1]
                          if os.path.splitext(file)[0] == name:
                           
                            new_sample_fp = os.path.join(song_path,sample)
                            shutil.copyfile(sample_fp, new_sample_fp)
    
    #RENAMER TO CHANNEL NUMBER             
    for song in os.listdir(path):
      song_path =os.path.join(path,song)

      # get sample mapper file and song name 
      for file in os.listdir(song_path):
          if file[-18:] == '_sample_mapper.txt':
              song_name = file[:-19]
              
      for file in os.listdir(song_path):
          if os.path.isdir(song_path) is True:
            for file in os.listdir(song_path):
              if file == 'track_channel_sampling.txt':
                 
                with open(os.path.join(song_path,file)) as f1:
                    channel_track_link = f1.readlines()
               
                for line in channel_track_link:
                    
                    track = int(line.split(' : ')[0])
                    if line.split(' : ')[1] != 'ignore\n':
                        channel = int(line.split(' : ')[1]) 
                        new_fp2 = os.path.join(song_path, str(channel) + '.wav')
                        
                        for file2 in os.listdir(song_path):

                            if mode == 'V1':
                                fp = os.path.join(sample_path,file2)
                                if os.path.splitext(file2)[1] == '.wav':
                                 #MODE V1
                                   if ' - Track ' in file2:
                                       if os.path.splitext( file2.split(' - Track ')[1])[0]== str(track):
                                          
                                            new_fp = os.path.join(song_path,'Track_channel_' + str(channel) + '.wav')
                                            
                                            if os.path.isfile(new_fp) == True:
                                                os.remove(new_fp)
                                            shutil.copy(fp, new_fp)

                                            # if not already a real sample create a X.wav where x is the channel number 
                                            #if os.path.isfile(new_fp2) == False:
                                            shutil.copy(fp, new_fp2)
                                    

                            if mode == 'V2':

                                if file2[:len(song_name)] == song_name and os.path.splitext(file2.split('_')[-1])[0] == str(track) and file2[-4:] == '.wav':
                                    shutil.copy(os.path.join(song_path, file2), new_fp2)
                                try:
                                    if file2[:len(song_name)] == song_name and os.path.splitext(file2.split('_')[-2])[0] == str(track) and file2[-11:] == '_Master.wav':
                                        shutil.copy(os.path.join(song_path, file2 ), new_fp2)
                                except:
                                    pass
                                try:
                                    if file2[:len(song_name)] == song_name and file2[-13:] == '_degraded.wav':
                                        if os.path.splitext(file2.split('_')[-2])[0] == str(track):
                                            shutil.copy(os.path.join(song_path, file2), new_fp2)
                                except:
                                    pass
                                try:
                                    if file2[:len(song_name)] == song_name and file2[-20:] == '_degraded_Master.wav':
                                        if os.path.splitext(file2.split('_')[-3])[0] == str(track):
                                            shutil.copy(os.path.join(song_path, file2), new_fp2)
                                except:
                                    pass

                               #MODE V2
                               #if os.path.isfile(os.path.splitext(file2)[0] +'.flp'):
                                #   if os.path.splitext(file2.split('_')[-1])[0] == str(track):
                            
                        print(new_fp2)
                        if os.path.isfile(new_fp2) == False:
                            print('flp')
                            for i in lines:
                                if i.split(' , ') [0] == channel:
                                   filename_ = os.path.filename(i.split(' , ')[1])
                                   fp = os.path.join(song_path,filename)
                                   print(fp)
                                   shutil.copy(fp, new_fp2


        #RENAMER TO CHANNEL NAME STORED IN MIDI FILE
        count_name_linker = {}
        for song in os.listdir(path):
            song_path = os.path.join(path,song)
            
            if os.path.isdir(song_path) is True:
                for file in os.listdir(song_path):
                    if file[-16:] == 'channel_name.txt':
                        my_file = open(os.path.join(song_path,file))
                        track_names = my_file.readlines()
                        for line in track_names:     
                            count_name_linker[int(line.split(':')[0])] = line.split(':')[1]

              
                        for file2 in os.listdir(song_path):
                            fp = os.path.join(song_path,file2)
                            if 'Track_channel_' in file2:
                                    chan_name = os.path.splitext(file2.split('_channel_')[1])[0]
                                    
                                    output_name = count_name_linker[int(chan_name)].split('\n')[0]
                                    
                                    new_fp  = os.path.join(song_path, output_name + '.wav')
                                    if os.path.isfile(new_fp) == True:
                                        os.remove(new_fp)
                                    os.rename(fp, new_fp)
                        
                        # RENAME SAMPLE TO CHANNEL NAME AND OVERRIDE MANUALLY GENERATED SAMPLES IF THEY EXIST REAL SAMPLES HAVE PRIOTRITY
                        for file3 in os.listdir(song_path):
                            fp = os.path.join(song_path,file3)
                            try:
                                int(os.path.splitext(file3)[0])
                                is_int = True
                            except:
                                is_int = False
                            if is_int == True:
                                chan_name = count_name_linker[int(os.path.splitext(file3)[0])].split('\n')[0]
                                
                                new_fp  = os.path.join(song_path, chan_name + '.wav')
                                shutil.copyfile(fp, new_fp)"""
                    
