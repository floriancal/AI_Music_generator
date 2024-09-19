import os
import shutil
from check_empty_samples import Check_empty_samples
# wav parser manager
Sampling_flp_files_folder = "D:/Sampling_flp_files"
to_be_parsed_folder = "D:/to_be_parsed"
Parsed_folder = "D:/Parsed"

Path_to_degraded_app = "C://Users//fcala//Desktop//VS_projects//VS_degraded//FLP_Parser_degraded.exe"


# if a song is note samplend or insampling it gets copied to tbd folder
for song in os.listdir(Sampling_flp_files_folder):
    if song not in os.listdir(to_be_parsed_folder) and  song not in os.listdir(Parsed_folder):
        shutil.copytree(os.path.join(Sampling_flp_files_folder,song), os.path.join(to_be_parsed_folder,song))

# remove parsed flp to avoid flp batcher to resample same files 
for song in os.listdir(to_be_parsed_folder):
    song_path = os.path.join(to_be_parsed_folder,song)    
    for file in os.listdir(song_path):
        fp = os.path.join(song_path, file)
        if os.path.splitext(file)[1] == ".flp":
            #ne gere pas le split master qui s'active parfois
            if os.path.isfile(os.path.join(song_path,os.path.splitext(file)[0] + ".wav")) or ( os.path.isfile(os.path.join(song_path,os.path.splitext(file)[0] + "Master.wav"))):
                if os.path.isdir(os.path.join(Parsed_folder, song)) is False:
                    os.mkdir(os.path.join(Parsed_folder, song))
                shutil.move(fp, os.path.join(Parsed_folder,song,file))


            
# complete the folder Parsed_folder with other song files
for song in os.listdir(Sampling_flp_files_folder):
    song_path = os.path.join(to_be_parsed_folder,song)
    if song in os.listdir(Parsed_folder):
        Parsed_folder_song_path = os.path.join(Parsed_folder, song)
        for file in os.listdir(song_path):
            if file not in os.listdir(Parsed_folder_song_path):
                shutil.copy(os.path.join(song_path,file), os.path.join(Parsed_folder_song_path,file))

# Call check empty sample to prepare for degraded mode to analyze empty waves
Check_empty_samples('dummy', 'dummy', "D:/Parsed_TEST")
# call degraded mode generator
os.system(Path_to_degraded_app)

# rename degraded wav
for song in os.listdir(Parsed_folder):
    song_path = os.path.join(Parsed_folder,song)
    for file in os.listdir(song_path):
        if "degraded.wav" in file:
            shutil.move(os.path.join(song_path, file),
                        os.path.join(song_path, file.replace("_degraded", "")))

# Delete an entirely parsed folder
for song in os.listdir(to_be_parsed_folder):
    song_path = os.path.join(to_be_parsed_folder,song)
    empty_flp = True
    for file in os.listdir(song_path):
        fp = os.path.join(song_path, file)
        if os.path.splitext(file)[1] == ".flp":
            empty_flp = False
    if empty_flp is True:
        shutil.rmtree(song_path)



