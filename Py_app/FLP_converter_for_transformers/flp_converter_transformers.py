#@title Process MIDI to TXT (MIDI-TXT-MIDI v.3.5)
encoding_type = "score-karaoke-one-byte-encoding" #@param ["score-karaoke-one-byte-encoding", "score-special-one-byte-encoding"]
enable_sampling = False #@param {type:"boolean"}
sample_length_in_MIDI_events = 1681 #@param {type:"slider", min:0, max:10000, step:1}
advanced_events = True #@param {type:"boolean"}
allow_tempo_changes = True #@param {type:"boolean"}
allow_control_change = True #@param {type:"boolean"}
karaoke = True #@param {type:"boolean"}
debug = False #@param {type:"boolean"}
score2mil = True #@param {type:"boolean"}

%cd /content/

# MIDI Dataset to txt dataset converter 
import MIDI
import os
import numpy as np
import tqdm.auto

if os.path.exists("Dataset.txt"):
  os.remove("Dataset.txt")
  print('Removing old Dataset...')
else:
  print("Creating new Dataset file...")



def write_notes(file_address):
      u = 0
      score = []
      melody = []
      result = []
      midi_file = open(file_address, 'rb')
      if debug: print('Processing File:', file_address)

      if encoding_type == 'score-karaoke-one-byte-encoding':
        score = MIDI.midi2score(midi_file.read())
        midi_file.close()
        score1 = MIDI.grep(score, [0, 2, 4, 6, 8, 10, 12, 14])
        score2 = MIDI.grep(score, [1, 3, 5, 7, 9, 11, 13, 15])
        #score1 = MIDI.grep(score, [0, 2])
        #score2 = MIDI.grep(score, [1, 3])
        #print(score)

        def twolists(list1, list2):
            newlist = []
            a1 = len(list1)
            a2 = len(list2)

            for i in range(max(a1, a2)):
                if i < a1:
                    newlist.append(list1[i])
                if i < a2:
                    newlist.append(list2[i])

            return newlist

        
        #melody = [score[0], score[1]]
        melody = [score1[0], twolists(score1[1], score2[1])]
        #print(melody)

        itrack = 1
        file = open('Dataset.txt', 'a')
        file.write('H d0 tMIDI-TXT-MIDI-Textual-Music-Dataset ')
        while itrack < len(melody):
            for event in melody[itrack]:
                if event[0] == 'note':
                  file.write('N' + ' d' + str(event[1]) + ' D' + str(event[2]) + ' C' + str(event[3]) + ' n' + str(event[4]) + ' V' + str(event[5]) + ' ')
                if event[0] == 'lyric' or event[0] == 'text_event':
                  file.write('L' + ' d' + str(event[1]) + ' t' + str(event[2]) + ' ')


            itrack += 1
          

        file.close()
        if debug:
          print('File:', midi_file, 'Number of skipped events: ', u)


      if encoding_type == 'score-special-one-byte-encoding':
        score = MIDI.midi2score(midi_file.read())
        if debug: print(score)
        midi_file.close()
        score1 = MIDI.grep(score, [0, 2, 4, 6, 8, 10, 12, 14])
        score2 = MIDI.grep(score, [1, 3, 5, 7, 9, 11, 13, 15])
        #print(score)

        result = []

        def twolists(list1, list2):
            newlist = []
            a1 = len(list1)
            a2 = len(list2)

            for i in range(max(a1, a2)):
                if i < a1:
                    newlist.append(list1[i])
                if i < a2:
                    newlist.append(list2[i])

            return newlist

        result = [score1[0], twolists(score1[1], score2[1])]
        score = result
        # ['note', start_time, duration, channel, note, velocity]

        itrack = 1
        


        notes = []

        tokens = []

        this_channel_has_note = False

        file = open('Dataset.txt', 'a')
        file.write('H d0 tMIDI-TXT-MIDI-Textual-Music-Dataset ')
        while itrack < len(score):
            for event in score[itrack]:

                if event[0] == 'note':
                    this_channel_has_note = True
                    notes.append(event[4])
                    
                    tokens.append([event[5], event[3], event[2], event[1]])
                    file.write('N' + ' d' + str(event[1]) + ' D' + str(event[2]) + ' C' + str(event[3]) + ' n' + str(event[4]) + ' V' + str(event[5]) + ' ')
                
            itrack += 1
            if not this_channel_has_note:
              u+=1
              if debug: 
                print('Uknown Event: ', event[0])

            if this_channel_has_note and len(notes) > sample_length_in_MIDI_events:
              if enable_sampling:
                break
          

        file.close()
        if debug:
          print('File:', midi_file, 'Number of skipped events: ', u)


dataset_addr = "Dataset"
files = os.listdir(dataset_addr)
for file in tqdm.auto.tqdm(files):
    path = os.path.join(dataset_addr, file)
    write_notes(path)
#print('Done!')
#print('Number of skipped events: ', u)
