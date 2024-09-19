from sample_extracter_and_renamer_8 import sample_extracter_and_renamer
from sample_renamer_7 import sample_renamer
from auto_namer import auto_namer
from pc2_pc1_interface import interface
from replace_flpes import replace_flpees
from recover_wav_real_sample import recover_wav_real_samples
from clean_folder import folder_clean
path = 'C://Users//fcala//Desktop//Son//FLP_TEST'
Sampling_flp_files_folder = "C://Users//fcala//Desktop//Son//Export_sampling_TEST"


# collect samples and rename them to channel number A MODIFIER (creer un fichier sur vs)
#sample_renamer(Sampling_flp_files_folder) #NOT USED IF ALL SOUND COMMES FROM SAMPLING

# Rename to channel number (les sons sortent des sous dossier) 
#sample_extracter_and_renamer(Sampling_flp_files_folder, "V2")

# Remove parasite wav coming from split insert track on sampling (also clean subfolders)
#folder_clean(Sampling_flp_files_folder)

# Apply channel_names so we have prefix_list format
auto_namer(Sampling_flp_files_folder)

# collect from pc2 folder samples back to flp folder A MODIFIER 
interface(Sampling_flp_files_folder, path)


