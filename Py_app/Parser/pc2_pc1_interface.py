
# INTERFACE PC1 PC2  parsed folder to channel name apply
import os
import shutil

def interface(Sampling_flp_files_folder, output_folder):
    for song in os.listdir(Sampling_flp_files_folder):
        song_full = True
        song_path = os.path.join(Sampling_flp_files_folder,song)
        """
        for file in os.listdir(song_path):
             if os.path.splitext(file)[1] == ".flp" and  "_degraded.flp" not in file:
                 parsed_song_path = os.path.join(Parsed_folder,song)

                 #ne gere pas le split master track qui peut s'activer
                 if (os.path.isfile(os.path.join(Parsed_folder, song, os.path.splitext(file)[0] +".wav")) is False) and   (os.path.isfile(os.path.join(Parsed_folder, song, os.path.splitext(file)[0] +"_Master.wav")) is False):
                     song_full = False
                 else:
                    if file not in os.listdir(parsed_song_path):
                        song_full = False"""
        # Systematic copy even is song not full
        song_full == True
        if song_full is True:
            try:
                for file in os.listdir(song_path):
                    if os.path.isdir(os.path.join(song_path,file)):
                        shutil.copytree(os.path.join(Sampling_flp_files_folder,song,file), os.path.join(output_folder,song,file))
                    else:
                        shutil.copy(os.path.join(Sampling_flp_files_folder,song,file), os.path.join(output_folder,song,file))
            except:
                print("error copying song")
                pass
                        
                                    
                


    
    
         
         
         
                             
