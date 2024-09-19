import pyautogui
import os
path = "C://Users//Usuario//Desktop//batch_render_pilot"
pyautogui.FAILSAFE = False


for file in os.listdir(path):
    if os.path.splitext(file)[1] == ".PNG":
           pyautogui.click(pyautogui.locateCenterOnScreen(file))
            
