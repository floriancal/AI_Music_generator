#MAIN
from time_txt_file_creator_1 import time_txt_creator
from track_name_file_creator_2 import track_name_creator
from sample_parser_renamer_3 import sample_parser_renamer
from sample_parser_4 import sample_parser
from midi_merger_5 import midi_merger
from midi_len_splitter_6 import midi_len_splitter
from sample_renamer_7 import sample_renamer
from sample_extracter_and_renamer_8 import sample_extracter_and_renamer
from sample_check_and_delete_9 import sample_check_and_delete
from add_to_sample_base_10 import add_to_sample_base
from sample_check_and_add_11 import sample_check_and_add
from insert_parser_renamer_12 import insert_parser_renamer
from channel_file_renamer_13 import  channel_file_renamer
from dataset_collector_14 import dataset_collector
from apply_channel_name_to_wav import apply_channel_names_to_wav
from channel_name import channel_name
from Pattern_parser import pattern_parser
from recover_wav_real_sample import recover_wav_real_samples
from wav_cleaner import folder_clean
from exctract_wav_subfolders import exctract_wav_subfolders
import os

path = 'C://Users//fcala//Desktop//Son//FLP//'

VS_apply_channel_names_path = "C://Users//fcala//Desktop//VS_projects//Apply_channel_names//ConsoleApp1//bin//Debug//netcoreapp3.1//ConsoleApp1.exe"


#DONE IN APPLY CHANNEL NAME VS
# Insert channel names into the flp file (create a new flp sideways, plus de flp-renamed folder)
os.system(VS_apply_channel_names_path)

# remove channel number file with 0 sound or corrupted (updated to subfolders)
folder_clean(path)

# extract from the flp the pattern structure
pattern_parser(path)

#Extract wav sampled from their subfolders to song folder
exctract_wav_subfolders(path)

# If a wav channel number is missing replace with the real sample (pas utilisé si on ne génère pas les sample réels)
#recover_wav_real_samples(path)

# Copy pre name channel to channel name (unused)
#channel_name(path)

# Switch name in sample parser file froma number to the channel name 
sample_parser_renamer(path)

# Create a midi file from the data in sampel parser file
sample_parser(path)

# Create a total midi file from sample_parser midi file and vs apply channel name_midi file
midi_merger(path)

# create splitted midi files in (couplet,refrain,intro...)
midi_len_splitter(path)

#convert_number chanel wav files to their prefix names
apply_channel_names_to_wav(path)

#Remove all prefix wav files that are empty
sample_check_and_delete(path)

#Collect all sound to common sample base (prefix_list heeere)
add_to_sample_base(path)

#replace sound that are not empty with a random sound from the sample base
sample_check_and_add(path)

# Aplly channel name to insert parsing
insert_parser_renamer(path)

# Renamer for channel parser (volume and pan)
channel_file_renamer(path)

# Gather data for export to google drive and google colab 
dataset_collector(path)
