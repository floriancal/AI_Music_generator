import os

Path_to_batch_render = "C://Users//fcala//Desktop//Son//Sampling_flp_files"
Fl_studio_root = "C://Program Files//Image-Line//FL Studio 20"
os.chdir(Fl_studio_root)
for folder in os.listdir(Path_to_batch_render):
    folder_path = os.path.join(Path_to_batch_render, folder)
    Command = "FL64.exe /R /Ewav /F'" + folder_path + "'"
    os.popen( Command)
