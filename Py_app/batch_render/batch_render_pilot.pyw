import time
#automater sample parser for pc2
import pyautogui
import time
import subprocess
import os
import shlex




t0 = time.clock()
dt = 1000
#cd "C:\Users\Usuario\Desktop\batch_render_pilot"
#py batch_render_pilot.pyw
path = "C://Users//Usuario//Desktop//batch_render_pilot"
pyautogui.FAILSAFE = False

print("running batch_sampling_manager")
subprocess.run("py C://Users//Usuario//Desktop//PYTHON_pack_11_10_2021//Batch_sampling_manager.py")
print("launching FLStudio")

while True:
    print("initiate")
    # remove problem loading the project screen with 2 scenario and 2 redundancy
    """for file in os.listdir(path):
        if os.path.splitext(file)[1] == ".PNG":
              
            #pyautogui.click(pyautogui.locateCenterOnScreen(file))"""
    #       



    if (pyautogui.locateCenterOnScreen("C://Users//Usuario//Desktop//batch_render_pilot/sore/rendering.PNG") is False):
        pyautogui.press("esc")
        pyautogui.press("enter")
    print("running")
    time.sleep(6)
    #pyautogui.press("esc")

    #execute every 2min
    ti = time.clock()
    if dt >120 :
        with pyautogui.hold("alt"):
            pyautogui.press(["tab","tab","tab"])
        t_run = time.clock()
        print("runinig 2min checks")
        """subprocess.Popen('tasklist /fi "cputime gt 00:10:00" >C://Users//Usuario//Desktop//batch_render_pilot//tasklist.txt')
        fi = open("C://Users//Usuario//Desktop//batch_render_pilot//tasklist.txt")
        res =fi.read()
        fi.close()
        if "ILBridge.exe" in res:
            try:
                subprocess.Popen("taskkill /IM ILBridge.exe")
            except:
                pass"""
       
            
    dt = ti - t_run

    

    #click where ILINE put itself
    pyautogui.click(x=803, y=750)
    try:
        subprocess.run("py annex.pyw", timeout = 10)
    except:
            pass
        

    #Kill FLStudio every 1h
    if time.clock()-t0 > 3600:
        print("running hour checks")
        try:
            subprocess.run("taskkill /F /IM FL64.exe",timeout=20)
        except:
            print("failed to kill fl_studio")
        try:
           subprocess.run("py C://Users//Usuario//Desktop//PYTHON_pack_11_10_2021//Batch_sampling_manager.py")
        except:
            print("failed to run batch sampling manager")
        try:
           subprocess.Popen('"C://Program Files//Image-Line//FL Studio 20//FL64.exe" /R /Ewav /FL://to_be_parsed')
        except:
            print("failed to run batch sampling manager")
        
        t0 = time.clock()
    #with pyautogui.hold("win"):
       # pyautogui.press("m")
    #time.sleep(6)
   # with pyautogui.hold("alt"):
       # pyautogui.press(["tab","tab","tab"])
    #with pyautogui.hold("alt"):
     #   pyautogui.press(["tab","tab"])

    #c=0
    #while (c < 2 ):
       # with pyautogui.hold("alt"):

        #    pyautogui.keyDown("tab")
         #   time.sleep(1)
          #  pyautogui.keyUp("tab")
      
    #c += 1 
    
    #Pause X second
    #time.sleep(6)
    #with pyautogui.hold("ctrl"):
      #  with pyautogui.hold("alt"):
          #  pyautogui.press(["tab","left","left"])
    #time.sleep(6)
    #with pyautogui.hold("ctrl"):
       # with pyautogui.hold("alt"):
       #     pyautogui.press(["tab","","left"])

        #Point(x=803, y=750)
