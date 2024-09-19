# Set volume of sample to volume_dict generated value
import os
import json
import soundfile

post_process_dir = 'C:/Users/fcala/Desktop/Son/Post_Process'

for dir in os.listdir(post_process_dir):
    inpath = os.path.join(post_process_dir, dir, 'content', dir)
    for file in os.listdir(inpath):
        if file == 'insert_volume.txt':
            vol_dict = json.loads(open(os.path.join(inpath,file)).read())
            for instr in vol_dict.keys():
                fp = os.path.join(inpath, instr + '.wav')
                y, s = soundfile.read(fp)
                amp = y.max()
                vol_factor =  float(vol_dict[instr]) / amp
                y =  y * vol_factor
                samplerate = 44100
                
                soundfile.write(fp, y, samplerate)

                
                
                
