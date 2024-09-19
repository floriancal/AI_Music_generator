## check if a song has a complete parsing or not if not runs command
## FOR PARSING DEGRADED MODE
import os
import shutil
import soundfile


def Check_empty_samples(path_complete, path_uncomplete, path_to_test):
    
    # parse entire folder
    for song in os.listdir(path_to_test):
        song_path = os.path.join(path_to_test,song)
        f1 = open(os.path.join(song_path, "sample_state.txt"), "w", encoding='utf-8')
        empty_sample = False
        
        # check for flp file that has an associated wav so a sample
        for file in os.listdir(song_path):
            if file[-12:] == 'degraded.flp':

                
                wav_file = os.path.join(song_path, os.path.splitext(file)[0] + ".wav")
                if os.path.isfile(wav_file):
                    # test if file has sound              
                    y, s = soundfile.read(wav_file)
                    vol_rms = y.max() - y.min()

                    #No sound song folder will go to uncomplete
                    if vol_rms <= 6.103515625e-05:
                        empty_sample = True
                    # sound remove associated flp, this way second batch render will ignore this sample  
                    else:
                        #os.remove(os.path.join(song_path, file)
                        pass
                    f1.write(wav_file + " : " + str(empty_sample) + " resampled ")

                    
            elif os.path.splitext(file)[1] == ".flp":

                wav_file = os.path.join(song_path, os.path.splitext(file)[0] + ".wav")
                if os.path.isfile(wav_file):
                    
                    # test if file has sound              
                    y, s = soundfile.read(wav_file)
                    vol_rms = y.max() - y.min()

                    #No sound song folder will go to uncomplete
                    if vol_rms <= 6.103515625e-05:
                        empty_sample = True
                    # sound remove associated flp, this way second batch render will ignore this sample  
                    else:
                        os.remove(wav_file)
                    f1.write(wav_file + " : " + str(empty_sample))
                
            

        """# depending or empty song or not goes to one or another folder
        if empty_sample = False:
            shutil.move(song_path, os.path.join(path_complete, song))
        else:
            shutil.move(song_path, os.path.join(path_uncomplete, song))"""
        
        
