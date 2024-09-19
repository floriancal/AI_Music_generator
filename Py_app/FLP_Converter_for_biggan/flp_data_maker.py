#FLP data conveerter
#take a .txt file made with .net tool for parsing flp files
# return .npy data representing 1 model input
# occasionaly parse a folder and do it for all files

import os
import numpy as np
import matplotlib.pyplot as plt
import math
import cv2

#inputs
path = 'C:/Users/Usuario/Desktop/son/FLP/txt_to_be_parsed/'
output_path = 'C:/Users/Usuario/Desktop/son/FLP/FLP_parsed/'
augmented_output_path = 'C:/Users/Usuario/Desktop/son/FLP/FLP_parsed_augmented/'

# Params
track_and_length_dimensions = (8,256) #first number is the number of channels !! all drums count for 1 channel)
track_method = 'double_octave' #binary or simple octave
len_method = 'mesure' # free or mesure
nombre_de_mesure = 4# useful only for measure method
ppq_compression = 6# in most of cases /96 revient a une note par temps
mesure_de_depart = 32
mesure_de_fin = 38

pitch_augmentation = 12

Fixed_size = (256,256)

# Path handling
fs = os.listdir(path)
fps = []
for f in fs : 
    fps.append(os.path.join(path, f))


##-----------------------------------------------------------------------------------------------------------------##   

#Reading and constructing loop
for fp in fps:
    
    if os.path.splitext(fp)[1] == '.txt':
        
        for pitch_aug in range(pitch_augmentation):
            
            #Data array initializer
            if track_method == 'binary':
                if len_method == 'free':
                    Frame = np.zeros((track_and_length_dimensions[0] * 108 ,track_and_length_dimensions[1]))
                elif len_method == 'mesure':
                    Frame = np.zeros((track_and_length_dimensions[0] * 108 , track_and_length_dimensions[1]))
                else:
                    print('unimplemented method')
                    raise(BaseException)
            elif track_method == 'simple_octave':
                if len_method == 'free':
                    Frame = np.zeros((track_and_length_dimensions[0] * 12 ,track_and_length_dimensions[1]))
                elif len_method == 'mesure':
                    Frame = np.zeros((track_and_length_dimensions[0] * 12 , track_and_length_dimensions[1]))
                else:
                    print('unimplemented method')
                    raise(BaseException)
                
            elif track_method == 'double_octave':
                if len_method == 'free':
                    Frame = np.zeros((track_and_length_dimensions[0] * 24 - 12 ,track_and_length_dimensions[1]))
                elif len_method == 'mesure':
                    Frame = np.zeros((track_and_length_dimensions[0] * 24 - 12 , track_and_length_dimensions[1]))
                else:
                    print('unimplemented method')
                    raise(BaseException)
                
            elif track_method == 'ten_fingers':
                if len_method == 'free':
                    Frame = np.zeros((track_and_length_dimensions[0] * 10 + 2 ,track_and_length_dimensions[1]))
                elif len_method == 'mesure':
                    Frame = np.zeros((track_and_length_dimensions[0] * 10 + 2 , track_and_length_dimensions[1]))
                else:
                    print('unimplemented method')
                    raise(BaseException)
                    
            else:
                print('unimplemented method')
                raise(BaseException)

            if Fixed_size is not None:
                Frame = np.zeros(Fixed_size)
                print('beware fxed_sie is used')

        ##-----------------------------------------------------##   

                # reading and construction
                f = open(fp, "r")

                line = "heellooo"
                while line !="":
                    line = f.readline()
                    line = line[0:-1]
                    
                    # General level
                    if line [0: 28] == 'this file has ppq value of :' :
                        ppq = int(line[28:])
                        stop_looking = mesure_de_fin * 4 * ppq # 1 mesure = 4 temps 
                        start_looking = mesure_de_depart * 4 * ppq 
                    # Item level
                    if line [0: 40] == 'this item has been classified on track :' :
                        track = int(line[40:])
                        
                    if line [0:20] == 'this item start at :':
                        item_pos = int(int(line[20:]) )
                        
                    if line [0:22]== 'this item has length :':
                        item_length = int(int (line[22:]) )

                    if line [0:28]== 'this item has start_offset :':
                        item_start_offset = int(int (line[28:]) )
                        if item_start_offset <= 0:
                            item_start_offset = 0
                            
                    if line [0:26]== 'this item has end_offset :':
                        item_end_offset = int(int (line[26:]) )
                        if item_end_offset <= 0:
                            item_end_offset = 0
                        elif item_end_offset != item_length :
                            item_length = item_end_offset

                    # Note level
                    if line [0:29] == 'this item has note position :':
                        note_position = int(int(line[29:]))

                    if line [0:27] == 'this item has note length :':
                        note_length = math.ceil(int(line[27:]))
                       
                    if line [0:24] == 'this item has note key :':
                        note_key = int(line[24:]) + pitch_aug

                    if line [0:29] == 'this item has note Velocity :':
                        note_velocity = int(line[29:])            

        ##-----------------------------------------------------------------------------------------------------------------##   
                        
                        # this part goes into velocity condition beacuase entering this if means parsing one note enitrely 
                        #Frame adding
                       
                        for i in range(note_length):
                            

                            # this avoid going outside pattern with offset to be checked
                            if item_pos + item_start_offset + note_position + i < item_pos + item_length:

                                # track 20 is reserved for trash items
                                if track !=20:
                                    if item_pos < stop_looking and item_pos >= start_looking:
                                        
                                        if len_method == 'free':
                                            
                                            if track_method == 'binary':
                                                
                                                #drums line are treated differently
                                                if track <= 6: #outside drum tracks
                                                    Frame[track * note_key][int((item_pos + item_start_offset + note_position + i) / ppq_compression)] = 255
                                                else: # inside drum track
                                                    #not implemented
                                                    print('not implemented')
                                                    
                                            if track_method == 'simple_octave':
                                                #drums line are treated differently
                                                if track <= 6: #outside drum tracks
                                                    Frame[track*(note_key%12) ][int((item_pos + item_start_offset + note_position + i) / ppq_compression)] = math.floor(note_key/12) + 1 

                                                else:
                                                    Frame[(6 * 12) + (track - 6) ][int((item_pos + item_start_offset + note_position + i) /ppq_compression) ] =  5
                                                    
                                        if len_method == 'mesure':
                                            
                                            if track_method == 'binary':
                                                #drums line are treated differently
                                                if track <= 6: #outside drum tracks
                                                    Frame[track * note_key][int(((item_pos%(ppq * 4 * nombre_de_mesure)) + item_start_offset + note_position + i) /ppq_compression) ] = 255
                                                else:
                                                    #not implemented
                                                    print('not implemented')
                                                
                                            if track_method == 'simple_octave':
                                                #drums line are treated differently

                                                if track <= 6 : #outside drum tracks 
                                                    #Frame[track * 12 + (note_key%12) ][int(((item_pos%(ppq * 4 * nombre_de_mesure)) + item_start_offset + note_position + i) /ppq_compression) ] =  math.floor(note_key/12) + 1 
                                                    Frame[track * 12 + (note_key%12) ][int(((item_pos + item_start_offset + note_position + i) %(ppq * 4 * nombre_de_mesure)) / ppq_compression) ] =  math.floor(note_key/12) + 1 
                                              
                                                else:
                                                    #Frame[(6 * 12) + (track - 6) ][int(((item_pos%(ppq * 4 * nombre_de_mesure)) + item_start_offset + note_position + i) /ppq_compression) ] = 5
                                                    Frame[(6 * 12) + (track - 6) ][int(((item_pos + item_start_offset + note_position + i) %(ppq * 4 * nombre_de_mesure)) / ppq_compression)] = 5

                                            if track_method == 'double_octave':
                                                #drums line are treated differently

                                                if track <= 6 : #outside drum tracks 
                                                    #Frame[track * 12 + (note_key%12) ][int(((item_pos%(ppq * 4 * nombre_de_mesure)) + item_start_offset + note_position + i) /ppq_compression) ] =  math.floor(note_key/24) + 1 
                                                    Frame[track * 24 + (note_key%24) ][int(((item_pos + item_start_offset + note_position + i) %(ppq * 4 * nombre_de_mesure)) / ppq_compression) ] =  math.floor(note_key/24) + 1 
                                                    
                                                else:
                                                    #Frame[(6 * 12) + (track - 6) ][int(((item_pos%(ppq * 4 * nombre_de_mesure)) + item_start_offset + note_position + i) /ppq_compression) ] = 5
                                                    Frame[((track_and_length_dimensions[0]-1) * 24) + (track - 6) ][int(((item_pos + item_start_offset + note_position + i) %(ppq * 4 * nombre_de_mesure)) / ppq_compression)] = 5

            if pitch_aug ==0:
                np.save( os.path.join(output_path, os.path.basename(os.path.splitext(fp)[0])), Frame)
                cv2.imwrite( os.path.join(output_path, os.path.basename(os.path.splitext(fp)[0])+ '.jpg' ), Frame * 30)
            else:
                np.save( os.path.join(augmented_output_path, os.path.basename(os.path.splitext(fp)[0]) + str(pitch_aug)), Frame)
                cv2.imwrite( os.path.join(augmented_output_path, os.path.basename(os.path.splitext(fp)[0])+ str(pitch_aug) +  '.jpg' ), Frame * 30)


                       
plt.imshow(Frame)
plt.show()
                            
                    
            
                


            

        
