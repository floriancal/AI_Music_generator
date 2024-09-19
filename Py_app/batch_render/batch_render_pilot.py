#automater sample parser for pc2
import pyautogui
import time



while True:

    # remove problem loading the project screen with 2 scenario and 2 redundancy
    pyautogui.click(pyautogui.locateCenterOnScreen("Problem_loading_project_ok_button_dark.PNG"))
    pyautogui.click(pyautogui.locateCenterOnScreen("Problem_loading_project_ok_button_dark.PNG"))

    pyautogui.click(pyautogui.locateCenterOnScreen("Problem_loading_project_cross_button.PNG"))
    pyautogui.click(pyautogui.locateCenterOnScreen("Problem_loading_project_cross_button_grey.PNG"))

    #Pause X second
    time.sleep(10)
    
