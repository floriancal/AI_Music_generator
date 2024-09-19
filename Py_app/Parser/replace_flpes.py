#SWITCH FLPIES
import os
import shutil

def replace_flpees(path_in, path_out):

    for song in os.listdir(path_out):
        song_path = os.path.join(path_out,song) 
        if os.path.isdir(song_path) is True:

            song_unicoded = song.replace("'", "").replace("&","").replace("-","").replace('_',"")
            found = False
            for dir in os.listdir(path_in):
               if dir.replace("'", "").replace("&","").replace("-","").replace('_',"") == song_unicoded:
                    asssociated_in_folder = os.path.join(path_in,dir)
                    found = True
          
            
            if found:
                for file in os.listdir(asssociated_in_folder):
                    if os.path.splitext(file)[1] == ".flp":
                    #if "track_channel_sampling.txt" in file:                
                        fp_in = os.path.join(asssociated_in_folder,file)
                        file_name = file
                       
                for file2 in os.listdir(song_path):
                    if os.path.splitext(file2)[1] == ".flp":
                    #if "track_channel_sampling.txt" in file2: 
                       fp_out = os.path.join(song_path, file2)
                       os.remove(fp_out)
                    
                shutil.copy(fp_in, os.path.join(song_path,file_name))

                    
                

