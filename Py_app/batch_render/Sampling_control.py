import pyautogui
import time
import subprocess
import os
import shlex

sampling_path ="C://Users//fcala//Desktop//Son//Export_sampling"
auto_pilot_time = 60

pyautogui.FAILSAFE = False
def auto_pilot(duration):
  start_time = time.time()
  while time.time() - start_time < duration:

    # clean avast intempestive startups
    try:
        subprocess.Popen("taskkill /IM AvastBrowser.exe")
    except:
        pass

    # regularly press enter and escape except on rendering
    if (pyautogui.locateCenterOnScreen("C://Users//Usuario//Desktop//batch_render_pilot/sore/rendering.PNG") is False):
        #pyautogui.press("esc")
        pyautogui.press("enter")

    # Click where prictures have been made
    path_batch = "C://Users//Usuario//Desktop//batch_render_pilot"
    for file in os.listdir(path_batch):
        if os.path.splitext(file)[1] == ".PNG":
               pyautogui.click(pyautogui.locateCenterOnScreen(os.path.join(path_batch,file)))
              


fl_command = 'C://Program Files//Image-Line//FL Studio 20//FL64.exe /R /Ewav /F')
for song in os.listdir(sampling_path):
    song_path = os.path.join(sampling_path,song)
    for sub_folder in os.listdir(song_path):
        sub_folder_path = os.path.join(song_path, sub_folder)
        if os.path.isdir(sub_folder_path):    
            found_flp = False
            runned  = False
            sample = False
            for file in os.listdir(sub_folder_path):               
                if os.path.splitext(file)[1] == ".flp":
                    found_flp = True
                if file == "runned.txt":
                    runned = True
                if file == "sample.txt":
                    sample = True 
            if not runned and found_flp and not sample:
                
                #FLSTUDIO COMMAND HERE  #wait for a certain amount of time or kill
                subprocess.Popen(shlex.split(fl_command + sub_folder_path )
               
                #Auto pilot flstudio for 60s
                auto_pilot(auto_pilot_time)

                #kill process if not finished
                try:
                    subprocess.run("taskkill /F /IM FL64.exe",timeout=30)
                except:
                    print("Failed to kill fl_studio EXITING")
                    break
               
                #Create the output file to avoid reruns
                open(os.path.join(sub_folder_path, "runned.txt"), "w")
                
               
            
                    
            
