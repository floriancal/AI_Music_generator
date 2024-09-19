using System;
using System.Collections.Generic;
using Monad.FLParser;
using System.IO;
using System.Linq;

namespace ConsoleApp1
{
    class Program
    {
        static List<List<int>> Sorter(List<int> splitting_blocks, int group_number, int part_size, Project project1, string mode)
        {   
            // this is the list to return at the end cotaining the groups 
            List<List<int>> output_list = new List<List<int>>();


            // make the list of concerned blocks starts and ends
            int count = 0;
            List<int> block_list_start = new List<int>();
            List<int> block_list_end = new List<int>();
            foreach (int block in splitting_blocks)
            {
                if (count < splitting_blocks.Count - 1)
                {
                    if (part_size == 8)
                    {
                        if (block + 8 == splitting_blocks[count + 1])
                        {
                            block_list_start.Add(block);
                            block_list_end.Add(block + 8);
                        }
                    }
                    if (part_size == 16)
                    {
                        if (block + 16 == splitting_blocks[count + 1])
                        {
                            block_list_start.Add(block);
                            block_list_end.Add(block + 16);
                        }
                    }
                }
                count += 1;
            }

            //Make A  mesure size descriptive list this count  for each differents group how many items are present for each channel  ( an item is a pattern or a sample)
            count = 0;
            List<Dictionary<Channel, int>> part_list_descript = new List<Dictionary<Channel, int>>();
            foreach (int start in block_list_start)
            {
                int end = block_list_end[count];
                Dictionary<Channel, int> part = new Dictionary<Channel, int>();
                // Compute score of  mesures 
                foreach (Track track in project1.Tracks)
                {
                    foreach (IPlaylistItem item in track.Items)
                    {
                        float time = (float)(item.Position) / (project1.Ppq * 4);
                        int mesure = (int)Math.Floor(time);
                        if (mesure >= start & mesure < end)
                        {
                            if (item is ChannelPlaylistItem)
                            {
                                ChannelPlaylistItem chan_item = (ChannelPlaylistItem)item;
                                Channel _cur_channel = chan_item.Channel;

                                if (part.ContainsKey(_cur_channel))
                                {
                                    int part_list = part[_cur_channel];
                                    // number of elements in channel
                                    part_list += 1;
                                }
                                else { part[_cur_channel] = 1; }
                            }
                            if (item is PatternPlaylistItem)
                            {
                                PatternPlaylistItem pattern_item = (PatternPlaylistItem)item;
                                Pattern pat = pattern_item.Pattern;
                                Dictionary<Channel, List<Note>> pat_notes = pat.Notes;
                                foreach (Channel key_channel in pat_notes.Keys)
                                {
                                    foreach (Note note in pat_notes[key_channel])
                                    {
                                        if (part.ContainsKey(key_channel))
                                        {
                                            int part_list = part[key_channel];
                                            // number of elements in channel
                                            part_list += 1;
                                        }
                                        else { part[key_channel] = 1; }
                                    }
                                }
                            }
                        }
                    }
                }
                part_list_descript.Add(part);
            }

            // compute correlation of each part as just one int for each comparison
            List<List<int>> score_list = new List<List<int>>();

            // Parse each group 
            int part1_count = 0;
            foreach (Dictionary<Channel, int> part in part_list_descript)
            {       
                List<int> inter_list = new List<int>();
                // Parse again each groups we aim to compare them
                int part2_count = 0;
                foreach (Dictionary<Channel, int> part2 in part_list_descript)
                {
                    // Now go over the dictionary of channels/ number of items in it 
                    List<int> channel_score_list = new List<int>();
                    foreach (Channel channel in part.Keys)
                    {
                        int val1 = part[channel];
                        int val2 = 0;

                        // we have a score of zero if the channel is not present in the other group 
                        if (part2.ContainsKey(channel)) { val2 = part2[channel]; } else { val2 = 0; }

                        // if we have it we have a score representing the difference of item number in this channel
                        int score = Math.Abs(val2 - val1);

                        // Finally we sum all the channels meaning that the min score means max ressemblance 
                        channel_score_list.Add(score);
                    }

                
                    int final_score = channel_score_list.Sum();
                    // Dumb score to avoid comparing itself
                    if (part1_count == part2_count)
                    {
                        final_score = 10000;
                    }
                    inter_list.Add(final_score);
                    part2_count += 1;
                }
                score_list.Add(inter_list);
                part1_count += 1;
            }

            //group by part depending on their correlation score
            // we create the group list to join groups together 
            List <List<int>> group_list = new List<List<int>>();
            count = 0;
            foreach (int value in Enumerable.Range(0, block_list_start.Count)) { group_list.Add(new List<int>()); }
            // and automatically assign each group in a list at the beginning
            foreach (List<int> group in group_list) { group.Add(count); count += 1; }

      

            //Tant qu'on a trop de grp 
            if (group_list.Count > group_number)
            {
                while (group_list.Count != group_number)
                {
                    count = 0;
                    float total_min_value = 100000;
                    int list_number = 0;
                    int total_min_index = 0;

                    // Score list is a list of list comparing every group between them as a correlation matrix
                    // donc ici ca revient a faire pour cque grp
                    foreach (List<int> inter_list in score_list)
                    {
                        float min_value = inter_list.Min();
                        // GETTING the max value is actually getting the worst comparison and the best is probably zero comparing itself so we trick itself comparison with a big value and we now look for min
                        int min_index = inter_list.IndexOf(inter_list.Min());


                        if (min_value < total_min_value) { total_min_value = min_value; list_number = count; total_min_index = min_index; }
                        count += 1;
                    }
                    // Reset max value for next pass
                    List<int> reset_list = score_list[list_number];
                    reset_list[total_min_index] = 1000;
                    score_list[list_number] = reset_list;

                    List<int> group1 = new List<int>();
                    List<int> group2 = new List<int>();
                    int group1_index = 0;
                    int group2_index = 0;
                    count = 0;
                    foreach (List<int> group in group_list)
                    {
                        if (group.Contains(list_number)) { group1 = group; group1_index = count; }
                        if (group.Contains(total_min_index)) { group2 = group; group2_index = count; }
                        count += 1;
                    }

                    List<int> new_group = group1.Union(group2).ToList();
                    group_list.RemoveAt(group1_index);

                    group_list.RemoveAt(group2_index);

                    group_list.Add(new_group);
                }
            }

            // fill the part list           
            //group_list.Sort(); this seems non-logical since sorting lists is weird or we aim to sort by len ?
            // 
            List<int> refrain_start = new List<int>();
            List<int> refrain_end = new List<int>();
            List<int> bridge_start = new List<int>();
            List<int> bridge_end = new List<int>();
            if (group_list.Count == 2)
            {
                if (mode == "br")
                {
                    if (group_list[0].Count > group_list[1].Count)
                    {
                        // max number of occurence is refrain

                        foreach (int part in group_list[0])
                        {
                            refrain_start.Add(block_list_start[part]);
                            refrain_end.Add(block_list_end[part]);
                        }
                        foreach (int part in group_list[1])
                        {
                            bridge_start.Add(block_list_start[part]);
                            bridge_end.Add(block_list_end[part]);
                        }
                    }
                }
            }
            else if (group_list.Count == 1)
            {
                if (mode == "br")
                {
                    refrain_start.Add(block_list_start[0]);
                    refrain_end.Add(block_list_end[0]);
                }
            }
            else { Console.WriteLine("SIZE PROBLEM!!!!!!!!!!!"); }

            // compute the mean of active channels in each block
            List<double> group_total_chan_list = new List<double>();
            foreach (List<int> group in group_list)
            {
                List<int> active_channel_list = new List<int>();
                foreach (int part in group)
                {
                    Dictionary<Channel, int> data_dict = part_list_descript[part];
                    int active_channels = data_dict.Keys.ToList().Count;
                    active_channel_list.Add(active_channels);
                }
                double group_chan = active_channel_list.Average();
                group_total_chan_list.Add(group_chan);
            }

            List<int> couplet_start = new List<int>();
            List<int> couplet_end = new List<int>();
            // max number of channel is  refrain
            bool valid = true;

            if (mode == "bcr" & group_list.Count > 2)
            {
                if (group_list[2].Count < group_list[1].Count)
                {
                    valid = true;
                }
                else
                { valid = false; }        
            }


            if ((group_list.Count == 2 & mode == "cr") | (mode == "bcr" & group_list.Count >= 2))
            {
                if (valid)
                { 
                    // if we have same number of active channel its the biggest occurence that is set to refrain
                    if (group_total_chan_list[0] >= group_total_chan_list[1])
                    {
                        foreach (int part in group_list[0])
                        {
                            refrain_start.Add(block_list_start[part]);
                            refrain_end.Add(block_list_end[part]);
                        }
                        foreach (int part in group_list[1])
                        {
                            couplet_start.Add(block_list_start[part]);
                            couplet_end.Add(block_list_end[part]);
                        }
                    }
                    if (group_total_chan_list[1] > group_total_chan_list[0])
                    {
                        foreach (int part in group_list[0])
                        {
                            couplet_start.Add(block_list_start[part]);
                            couplet_end.Add(block_list_end[part]);
                        }
                        foreach (int part in group_list[1])
                        {
                            refrain_start.Add(block_list_start[part]);
                            refrain_end.Add(block_list_end[part]);
                        }
                    }
                }
            }

            // min number of occurence is bridge max number of channel is  refrain
            if (mode == "bcr")
            {
                if (valid & group_list.Count > 2)
                {
                    foreach (int part in group_list[2])
                    {
                        bridge_start.Add(block_list_start[part]);
                        bridge_end.Add(block_list_end[part]);
                    }
                }
                else
                {
                    if (group_list.Count > 2)
                    {
                        //block1
                        if (group_total_chan_list[0] >= group_total_chan_list[1] & group_total_chan_list[0] >= group_total_chan_list[2])
                        {
                            foreach (int part in group_list[0])
                            {
                                refrain_start.Add(block_list_start[part]);
                                refrain_end.Add(block_list_end[part]);
                            }
                        }
                        if (group_total_chan_list[1] >= group_total_chan_list[0] & group_total_chan_list[1] >= group_total_chan_list[2])
                        {
                            foreach (int part in group_list[1])
                            {
                                refrain_start.Add(block_list_start[part]);
                                refrain_end.Add(block_list_end[part]);
                            }
                        }

                        if (group_total_chan_list[2] >= group_total_chan_list[0] & group_total_chan_list[2] >= group_total_chan_list[1])
                        {
                            foreach (int part in group_list[1])
                            {
                                refrain_start.Add(block_list_start[part]);
                                refrain_end.Add(block_list_end[part]);
                            }
                        }

                        // block2
                        if (group_total_chan_list[0] >= group_total_chan_list[1] & group_total_chan_list[0] <= group_total_chan_list[2])
                        {
                            foreach (int part in group_list[0])
                            {
                                couplet_start.Add(block_list_start[part]);
                                couplet_end.Add(block_list_end[part]);
                            }
                        }
                        if (group_total_chan_list[1] >= group_total_chan_list[0] & group_total_chan_list[1] <= group_total_chan_list[2])
                        {
                            foreach (int part in group_list[1])
                            {
                                couplet_start.Add(block_list_start[part]);
                                couplet_end.Add(block_list_end[part]);
                            }
                        }

                        if (group_total_chan_list[2] >= group_total_chan_list[0] & group_total_chan_list[2] <= group_total_chan_list[1])
                        {
                            foreach (int part in group_list[1])
                            {
                                couplet_start.Add(block_list_start[part]);
                                couplet_end.Add(block_list_end[part]);
                            }
                        }

                        // block3
                        if (group_total_chan_list[0] <= group_total_chan_list[1] & group_total_chan_list[0] <= group_total_chan_list[2])
                        {
                            foreach (int part in group_list[0])
                            {
                                bridge_start.Add(block_list_start[part]);
                                bridge_end.Add(block_list_end[part]);
                            }
                        }
                        if (group_total_chan_list[1] <= group_total_chan_list[0] & group_total_chan_list[1] <= group_total_chan_list[2])
                        {
                            foreach (int part in group_list[1])
                            {
                                bridge_start.Add(block_list_start[part]);
                                bridge_end.Add(block_list_end[part]);
                            }
                        }
                        if (group_total_chan_list[2] <= group_total_chan_list[0] & group_total_chan_list[2] <= group_total_chan_list[1])
                        {
                            foreach (int part in group_list[1])
                            {
                                bridge_start.Add(block_list_start[part]);
                                bridge_end.Add(block_list_end[part]);
                            }
                        }
                    }
                    // if only two groups we end up here there will be no bridge in song 
                    else
                    {
                        if (group_total_chan_list[1] >= group_total_chan_list[0])
                        {
                            foreach (int part in group_list[1])
                            {
                                refrain_start.Add(block_list_start[part]);
                                refrain_end.Add(block_list_end[part]);
                            }
                            foreach (int part in group_list[0])
                            {
                                couplet_start.Add(block_list_start[part]);
                                couplet_end.Add(block_list_end[part]);
                            }

                        }
                        else
                        {
                            foreach (int part in group_list[0])
                            {
                                refrain_start.Add(block_list_start[part]);
                                refrain_end.Add(block_list_end[part]);
                            }
                            foreach (int part in group_list[1])
                            {
                                couplet_start.Add(block_list_start[part]);
                                couplet_end.Add(block_list_end[part]);
                            }

                        }
                    }
                }
            }
            output_list.Add(bridge_start); output_list.Add(bridge_end);
            output_list.Add(couplet_start); output_list.Add(couplet_end);
            output_list.Add(refrain_start); output_list.Add(refrain_end);
            return output_list; 
        }
   /// <summary>
   /// ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// START MAIN ///////////////////////
   /// </summary>
   /// <param name="args"></param>
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            // Path where to execute code 
            string targetDirectory = "C:/Users/fcala/Desktop/Son/FLP/";

            //top directory only to not parse flp created from sampling
            string[] fileEntries = Directory.GetFiles(targetDirectory, "*.flp", SearchOption.TopDirectoryOnly);
            foreach (string file in fileEntries)
            {
                Console.WriteLine(file);

                Project project1 = Monad.FLParser.Project.Load(file, false);
                Console.WriteLine("File Loaded");
                List<Channel> Channels = project1.Channels;

                var filename = Path.GetFileNameWithoutExtension(file);
                var dirname = Path.GetDirectoryName(file);

                string name_channel = Path.Join(dirname, "pre_name_with_audio.txt");
                string channel_name_file_fp = Path.Join(dirname, "channel_name.txt");

                string[] channel_name_list = File.ReadAllLines(name_channel);

                int count = 0;
                foreach (string channel_name_str in channel_name_list)
                {
                    string[] subs = channel_name_str.Split(':');
                    string channel_name = subs[1];

                    if (channel_name != "")
                    {
                        Channels[count].Name = channel_name;
                    }
                    count += 1;
                }

                bool sampling_mode = false;

                // Path where to create the flp that will have channel names applied 
                string outpath = Path.GetFileNameWithoutExtension(file) + "_chanels.flp";
                string outpath_txt = Path.GetFileNameWithoutExtension(file) + "_chanels.txt";

                // Define scores to classify channel places
                Dictionary<int, int> note_score = new Dictionary<int, int>();
                Dictionary<int, List<int>> mesure_score = new Dictionary<int, List<int>>();

                foreach (Track track in project1.Tracks)
                {
                    foreach (IPlaylistItem item in track.Items)
                    {   
                        float time_start = (float)(item.Position) / (project1.Ppq * 4);
                        float time_stop = (float)(item.Position + item.Length) / (project1.Ppq * 4);

                        int mesure_start = (int)Math.Floor(time_start);
                        int mesure_stop = (int)Math.Ceiling(time_stop);

                        if (item is ChannelPlaylistItem)
                        {
                            ChannelPlaylistItem chan_item = (ChannelPlaylistItem)item;
                            Channel _cur_channel = chan_item.Channel;

                            if (note_score.ContainsKey(_cur_channel.Id))
                            {
                                note_score[_cur_channel.Id] = note_score[_cur_channel.Id] + 1;
                                List<int> mesure_list = mesure_score[_cur_channel.Id];
                                foreach (int value in Enumerable.Range(mesure_start, mesure_stop  - mesure_start)) { mesure_list.Add(value); }
                                mesure_score[_cur_channel.Id] = mesure_list;
                            }
                            else
                            {
                                note_score[_cur_channel.Id] = 1;
                                List<int> mesure_list = new List<int>();
                                foreach (int value in Enumerable.Range(mesure_start, mesure_stop  - mesure_start)) { mesure_list.Add(value); }
                                mesure_score[_cur_channel.Id] = mesure_list;
                            }
                        }
                        if (item is PatternPlaylistItem)
                        {
                            PatternPlaylistItem pattern_item = (PatternPlaylistItem)item;
                            Pattern pat = pattern_item.Pattern;
                            Dictionary<Channel, List<Note>> pat_notes = pat.Notes;
                            foreach (Channel key_channel in pat_notes.Keys)
                            {
                                Channel _cur_channel = key_channel;
                                foreach (Note note in pat_notes[key_channel])
                                {
                                    if (note_score.ContainsKey(_cur_channel.Id))
                                    {
                                        note_score[_cur_channel.Id] = note_score[_cur_channel.Id] + 1;
                                        List<int> mesure_list = mesure_score[_cur_channel.Id];
                                        foreach (int value in Enumerable.Range(mesure_start, mesure_stop  - mesure_start)) { mesure_list.Add(value); }
                                        mesure_score[_cur_channel.Id] = mesure_list;
                                    }
                                    else
                                    {
                                        note_score[_cur_channel.Id] = 1;
                                        List<int> mesure_list = new List<int>();
                                        foreach (int value in Enumerable.Range(mesure_start, mesure_stop  - mesure_start)) { mesure_list.Add(value); }
                                        mesure_score[_cur_channel.Id] = mesure_list;
                                    }
                                }
                            }
                        }
                    }
                }

                //Define mesure score better
                Dictionary<int, int> mesure_score_final = new Dictionary<int, int>();
                foreach (int key in mesure_score.Keys)
                {
                    List<int> score_channel_list = mesure_score[key];
                    int distinctCount = score_channel_list.Distinct().Count();
                    mesure_score_final[key] = distinctCount;
                }

                // get max values to normalize
                Dictionary<int, double> score_final = new Dictionary<int, double>();
                var max_note_Value = note_score.Values.Max();
                var max_mesure_Value = mesure_score_final.Values.Max();

                // GET THE FINAL SCORE
                foreach (int key in mesure_score_final.Keys)
                {
                    score_final[key] = 0.5 * note_score[key] / max_note_Value + 0.5 * mesure_score_final[key] / max_mesure_Value;
                }

                var sortedDict = from entry in score_final orderby entry.Value ascending select entry;

                List<string> instr_list = new List<string>();


                // Give channels their attributed names
                foreach (KeyValuePair<int, double> pair in sortedDict)
                {
                    foreach (Channel channel in Channels)
                    {

                        Channel _cur_channel = channel;
                        if (_cur_channel.Id == pair.Key)
                        {
                            string channel_name = _cur_channel.Name;

                            if (channel_name != "SH" & channel_name != "Y1" & channel_name != "Y2")
                            {
                                instr_list.Add(channel_name);
                                int counting = instr_list.Where(x => x.Equals(channel_name)).Count();

                                _cur_channel.Name = channel_name + counting.ToString();
                                break;
                            }
                        }
                    }
                }

                // HERE WE WRITE THE FILE WITH THE CHANNEL NAMES APPLIED
                ProjectWriter project_writer = new ProjectWriter(file, outpath, project1, sampling_mode, 0, 0, false);


                using (StreamWriter pre_name_channel_file = new StreamWriter(channel_name_file_fp))
                {
                    // THEN WE WRITE A TXT CHANNEL NAME FILE FOR PYTHON
                    int counting = 0;
                    foreach (Channel chan in Channels)
                    {   
                        pre_name_channel_file.WriteLine(counting + ":" + chan.Name);
                        counting += 1;
                    }
                }

                // WE NOW USE THE CREATED FILE TO CREATE A MIDI FILE WITH MIDI NTOES EXPORTED 
                Project project2 = Monad.FLParser.Project.Load(outpath, false);

                using (StreamWriter midi_export_file = new StreamWriter(outpath_txt))
                {
                    //Parse by tracks to get each items
                    foreach (Track track in project2.Tracks)
                    {
                        String track_name = track.Name;
                        System.Collections.Generic.List<IPlaylistItem> Items = track.Items;
                        foreach (IPlaylistItem item in Items)
                        {
                            if (item is PatternPlaylistItem)
                            {
                                PatternPlaylistItem pattern_playlist = (PatternPlaylistItem)item;
                                Pattern pattern = pattern_playlist.Pattern;
                                Dictionary<Channel, List<Note>> Notes = pattern.Notes;

                                int item_pos = item.Position;
                                int item_offset = item.StartOffset;

                                foreach (System.Collections.Generic.KeyValuePair<Channel, System.Collections.Generic.List<Note>> kvp in Notes)
                                {
                                    string channel_name = kvp.Key.Name;
                                    foreach (Note note in kvp.Value)
                                    {
                                        int key = note.Key;
                                        int note_length = note.Length;
                                        int pan = note.Pan;
                                        int note_pos = note.Position;

                                        midi_export_file.WriteLine("note_channel_name :" + channel_name);
                                        midi_export_file.WriteLine("item_pos :" + item_pos);
                                        midi_export_file.WriteLine("item_offset :" + item_offset);
                                        midi_export_file.WriteLine("note_key :" + key);
                                        midi_export_file.WriteLine("note_length :" + note_length);
                                        midi_export_file.WriteLine("note_pos :" + note_pos);

                                    }
                                }



                            }
                        }
                    }
                }
                ///////////////////////////////////////////////////////////////////////////////////////Len cutter strat
                ///
                // last mesure definition 
                List<int> mesure_list_max = new List<int>();
                foreach (Track track in project1.Tracks)
                {
                    foreach (IPlaylistItem item in track.Items)
                    {

                        float time = (item.Position + item.Length) / (project1.Ppq * 4);
                        int mesure = (int)Math.Ceiling(time);
                        mesure_list_max.Add(mesure);
                    }
                }
                int max_mesure = mesure_list_max.Max();

                // compute delta mesures (difference of mesure betwenn the actual mesure and the one before
                List<int> delta_channel_list = new List<int>();

                Dictionary<int, int> old_mesure_active = new Dictionary<int, int>();
                foreach (int key in mesure_score.Keys) { old_mesure_active[key] = 0; }

                foreach (int value in Enumerable.Range(0, max_mesure))
                {
                    int delta_channel = 0;
                    foreach (int key in mesure_score.Keys)
                    {                
                        List<int> time_list = mesure_score[key];
                        int active_channels = 0;
                        if (time_list.Contains(value)) { active_channels = 1; } else { active_channels = 0; }
                        int old_active_channel = old_mesure_active[key];
                        delta_channel += Math.Abs(active_channels - old_active_channel);
                        old_mesure_active[key] = active_channels;

                    }
                    delta_channel_list.Add(delta_channel);
                }
              
                // DETERMINE INTRO end POSITION
                // get the first kick 
                bool found_kick = false;
                int first_kick_pos = 0;
                foreach (Track track in project1.Tracks)
                {
                    foreach (IPlaylistItem item in track.Items)
                    {
                        float time = (item.Position) / (project1.Ppq * 4);
                        int mesure = (int)Math.Floor(time);
                        if (item is ChannelPlaylistItem)
                        {
                            ChannelPlaylistItem chan_item = (ChannelPlaylistItem)item;
                            Channel _cur_channel = chan_item.Channel;
                            if (_cur_channel.Name[0] == 'K')
                            {
                                first_kick_pos = mesure;
                                found_kick = true;
                                break;
                            }
                        }
                        if (item is PatternPlaylistItem)
                        {
                            PatternPlaylistItem pattern_item = (PatternPlaylistItem)item;
                            Pattern pat = pattern_item.Pattern;
                            Dictionary<Channel, List<Note>> pat_notes = pat.Notes;
                            foreach (Channel key_channel in pat_notes.Keys)
                            {
                                if (key_channel.Name[0] == 'K')
                                {
                                    first_kick_pos = mesure;
                                    found_kick = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (found_kick is true)
                    {
                        break;
                    }
                }
                // SI ON A un kick entre 2 et 18 on prend comme fin d'intro le plus gros delta dans les 3 mesures d'avant et la mesure d'après 
                int intro_end = 0;
                if (first_kick_pos > 2 & first_kick_pos < 18)
                {
                    int mesure_big_gap = delta_channel_list.GetRange(first_kick_pos - 3, 4).IndexOf(delta_channel_list.GetRange(first_kick_pos - 3, 4).Max());
                    intro_end = first_kick_pos - 3 + mesure_big_gap;
                }
                // SI ON a pas de kick on prend le plus gros delta entre 2 et 18 
                else
                {
                    // plus two because we start counting at real position 2
                    int mesure_big_gap = delta_channel_list.GetRange(2, 16).IndexOf(delta_channel_list.GetRange(2, 16).Max());
                    intro_end = mesure_big_gap + 2;
                }

                // directly add intro end value
                List<int> splitting_blocks = new List<int>();
                splitting_blocks.Add(intro_end);

                // Get the last kick
                int last_kick_pos = 0;
                foreach (Track track in project1.Tracks)
                {
                    foreach (IPlaylistItem item in track.Items)
                    {
                        float time = (float)(item.Position) / (project1.Ppq * 4);
                        int mesure = (int)Math.Floor(time);
                        if (item is ChannelPlaylistItem)
                        {
                            ChannelPlaylistItem chan_item = (ChannelPlaylistItem)item;
                            Channel _cur_channel = chan_item.Channel;
                            if (_cur_channel.Name[0] == 'K') { last_kick_pos = mesure; }
                        }
                        if (item is PatternPlaylistItem)
                        {
                            PatternPlaylistItem pattern_item = (PatternPlaylistItem)item;
                            Pattern pat = pattern_item.Pattern;
                            Dictionary<Channel, List<Note>> pat_notes = pat.Notes;
                            foreach (Channel key_channel in pat_notes.Keys)
                            {
                                if (key_channel.Name[0] == 'K') { last_kick_pos = mesure; }
                            }
                        }
                    }
                }
                // pareil dans les 18 dernieres mesures 
                int outro_start = 0;
                if (last_kick_pos > max_mesure - 18) 
                {
                    int gap_range = 3;
                    if (delta_channel_list.Count < last_kick_pos +3 )
                    {
                        gap_range = delta_channel_list.Count - last_kick_pos; 
                    }
                    int mesure_big_gap = delta_channel_list.GetRange(last_kick_pos, gap_range).IndexOf(delta_channel_list.GetRange(last_kick_pos, gap_range).Max());
                    outro_start = last_kick_pos + mesure_big_gap; 
                }
                else
                {
                    int mesure_big_gap = delta_channel_list.GetRange(max_mesure - 18, 18).IndexOf(delta_channel_list.GetRange(max_mesure - 18, 18).Max());
                    outro_start = max_mesure - 18  + mesure_big_gap;
                    delta_channel_list[outro_start] = 0;
                }


                //directly add outro end value except if not concordant with intro cannot be pair and impair in the same song si c pas concourdant outro marchera sur le dernier bout qui s'arretera a la fin
                
                if((intro_end%2 == 0 & outro_start%2 == 0) | (intro_end % 2 != 0 & outro_start% 2 != 0)) { splitting_blocks.Add(outro_start); }    
                else { splitting_blocks.Add(max_mesure); }
                    

                //get the deltas of channel between each block of 2 mesures
                List<int> delta = new List<int>();
                count= 0;
                int impair_value = 0;
                foreach (int value in delta_channel_list.GetRange(intro_end, max_mesure - intro_end))
                {
                    if (count % 2 == 0)
                    {
                        delta.Add(Math.Abs(value + impair_value));
                    }
                    else { impair_value = value; }
                    count += 1;
                }

                //Split in blocks of size min of 2 and max of 32
                bool delta_zero = true;
                count = 0;
                while (true)
                {
                    splitting_blocks.Add(delta.IndexOf(delta.Max()) * 2 + intro_end);
                    delta[delta.IndexOf(delta.Max())] = 0;
                    splitting_blocks.Sort();

                    count = 0;
                    bool target = true;
                    foreach (int block in splitting_blocks)
                    {
                        if (count < splitting_blocks.Count -1)
                        {
                            if (block + 33 < splitting_blocks[count + 1])
                            {
                                target = false;
                            }
                        }
                        count += 1;
                    }
                    if (splitting_blocks.Count == 1) { target = false;  }
                    if (splitting_blocks.Max()  + 32 < max_mesure) { target = false;  }
                    delta_zero = true;
                    foreach(int delt in delta ) { if (delt != 0) { delta_zero = false;  } }
                    if(delta_zero == true) { break; }
                    if (target == true) { break; }
                }

                // WE Cant differentiate the last non splitted bocks by their channels for now we will cut them in two but we could use another metric
                if (delta_zero)
                {
                    bool repass = true;
                    List<int> Splitting_blocks_to_add = new List<int>();

                    while (repass)
                    {
                        repass = false;
                        int count_add = 0;
                        foreach (int block in splitting_blocks)
                        {
                            if (count_add < splitting_blocks.Count - 1)
                            {
                                if (block + 33 < splitting_blocks[count_add + 1])
                                {
                                    int middle = (splitting_blocks[count_add + 1] - block) / 2;
                                    if (block%2 == 0 & middle % 2 != 0)
                                    {
                                        middle += 1;
                                    }
                                    if (block % 2 != 0 & middle % 2 == 0)
                                    {
                                        middle += 1;
                                    }
                                    Splitting_blocks_to_add.Add(middle);
                                    repass = true;
                                }
                            }
                            count_add += 1;
                        }
                        foreach (int block_to_add in Splitting_blocks_to_add)
                        {
                            splitting_blocks.Add(block_to_add);
                        }
                        splitting_blocks.Sort();
                    }
                    
                }

                // block of size 32 are investigated  cut them in 2 if no special block separation else cut in the biggest sep
                count = 0;
                List<int> tm_list = new List<int>();
                foreach (int block in splitting_blocks)
                {
                    tm_list.Add(block);
                    if (count < splitting_blocks.Count - 1)
                    {
                        if (block + 32 == splitting_blocks[count + 1])
                        {
                            if (delta.IndexOf(delta.Max()) > block & delta.IndexOf(delta.Max()) < block + 32)
                                tm_list.Add(delta.IndexOf(delta.Max()));
                            else
                                tm_list.Add(block + 16);
                        }
                    }
                    count += 1;
                    tm_list.Sort();
                }
                splitting_blocks = tm_list.Distinct().ToList();

                int first_sixteen = splitting_blocks[0] + 16;
                Console.WriteLine(first_sixteen);
                List<int> to_remove = new List<int>();

                foreach (int block in splitting_blocks)
                {
                    if (block < first_sixteen && block != splitting_blocks[0])
                    {
                        to_remove.Add(block);
                    }
                }
                foreach (int block in to_remove)
                {
                    splitting_blocks.Remove(block);
                }
                splitting_blocks.Add(first_sixteen);
                splitting_blocks.Sort();


                // we need to authorize only elementry sizes of 2 4 6 8 16
                bool splitted = false;
                while (!splitted)
                {
                    count = 0;
                    splitted = true; 
                    List<int> final_spliting_blocks = new List<int>();
                    foreach (int block in splitting_blocks)
                    {
                        if (count < splitting_blocks.Count - 1)
                        {
                            int db_index = (block - intro_end) / 2;
                            if (block + 10 == splitting_blocks[count + 1]) { final_spliting_blocks.Add(delta.GetRange(db_index + 1, 4).IndexOf(delta.GetRange(db_index + 1, 4).Max()) * 2 + db_index * 2 + intro_end + 2); splitted = false;  }
                            if (block + 12 == splitting_blocks[count + 1]) { final_spliting_blocks.Add(delta.GetRange(db_index + 1, 5).IndexOf(delta.GetRange(db_index + 1, 5).Max()) * 2 + db_index * 2 + intro_end + 2); splitted = false;  }
                            if (block + 14 == splitting_blocks[count + 1]) { final_spliting_blocks.Add(delta.GetRange(db_index + 1, 6).IndexOf(delta.GetRange(db_index + 1, 6).Max()) * 2 + db_index * 2 + intro_end + 2); splitted = false;  }
                            if (block + 18 == splitting_blocks[count + 1]) { final_spliting_blocks.Add(delta.GetRange(db_index + 1, 8).IndexOf(delta.GetRange(db_index + 1, 8).Max()) * 2 + db_index * 2 + intro_end + 2); splitted = false;  }
                            if (block + 20 == splitting_blocks[count + 1]) { final_spliting_blocks.Add(delta.GetRange(db_index + 1, 9).IndexOf(delta.GetRange(db_index + 1, 9).Max()) * 2 + db_index * 2 + intro_end + 2); splitted = false;  }
                            if (block + 22 == splitting_blocks[count + 1]) { final_spliting_blocks.Add(delta.GetRange(db_index + 1, 10).IndexOf(delta.GetRange(db_index + 1, 10).Max()) * 2 + db_index * 2 + intro_end + 2); splitted = false; }
                            if (block + 24 == splitting_blocks[count + 1]) { final_spliting_blocks.Add(delta.GetRange(db_index + 1, 11).IndexOf(delta.GetRange(db_index + 1, 11).Max()) * 2 + db_index * 2 + intro_end + 2); splitted = false; }
                            if (block + 26 == splitting_blocks[count + 1]) { final_spliting_blocks.Add(delta.GetRange(db_index + 1, 12).IndexOf(delta.GetRange(db_index + 1, 12).Max()) * 2 + db_index * 2 + intro_end + 2); splitted = false; }
                            if (block + 28 == splitting_blocks[count + 1]) { final_spliting_blocks.Add(delta.GetRange(db_index + 1, 13).IndexOf(delta.GetRange(db_index + 1, 13).Max()) * 2 + db_index * 2 + intro_end + 2); splitted = false; }
                            if (block + 30 == splitting_blocks[count + 1]) { final_spliting_blocks.Add(delta.GetRange(db_index + 1, 14).IndexOf(delta.GetRange(db_index + 1, 14).Max()) * 2 + db_index * 2 + intro_end + 2); splitted = false; }
                        }
                        final_spliting_blocks.Add(block);
                        count += 1;
                    }
                    final_spliting_blocks.Sort();
                    splitting_blocks = final_spliting_blocks;
                }
              
                // block of size 2 next to each other are joined 
                List<int> to_delete = new List<int>();
                count = 0;
                foreach (int block in splitting_blocks)
                {   
                    if (count < splitting_blocks.Count - 2)
                    {
                        if (block + 2 == splitting_blocks[count + 1] & block + 4 == splitting_blocks[count + 2])
                        {
                            to_delete.Add(splitting_blocks[count + 1]);
                        }
                    }
                    count +=1;
                }
                foreach (int block in to_delete)
                {
                    splitting_blocks.Remove(block);
                }

               


                // Determine which sizes are present in soNG
                bool two = false;
                bool four = false;
                bool six = false;
                bool eight = false;
                bool sixteen = false;
                count = 0;
                foreach (int block in splitting_blocks)
                {
                    if (count < splitting_blocks.Count - 1)
                    {
                        if (block + 2 == splitting_blocks[count + 1]) { two = true; }
                        if (block + 4 == splitting_blocks[count + 1]) { four = true; }
                        if (block + 6 == splitting_blocks[count + 1]) { six = true; }
                        if (block + 8 == splitting_blocks[count + 1]) { eight = true; }
                        if (block + 16 == splitting_blocks[count + 1]) { sixteen = true; }
                    }
                    count += 1;
                }

                //next is couplet refrain ou pre ref
                List<int> refrain_end = new List<int>();
                List<int> couplet_end = new List<int>();
                List<int> bridge_end = new List<int>();
                List<int> pre_ref_end = new List<int>();
                
                List<int> pre_ref_start = new List<int>();
                List<int> refrain_start = new List<int>();
                List<int> couplet_start = new List<int>();
                List<int> bridge_start = new List<int>();

                // two blocks are necessarly pre_ref
                if (two)
                {
                    count = 0;
                    foreach (int block in splitting_blocks)
                    {
                        if (count < splitting_blocks.Count - 1)
                        {
                            if (block + 2 == splitting_blocks[count + 1])
                            {
                                pre_ref_start.Add(block);
                                pre_ref_end.Add(block + 2);
                            }
                        }
                        count += 1;
                    }
                }

                // if block of size 4 and other blocks exist we declare them as bridges
                if (four)
                {
                    count = 0;
                    foreach (int block in splitting_blocks)
                    {
                        if (count < splitting_blocks.Count - 1)
                        {
                            if (block + 4 == splitting_blocks[count + 1])
                            {
                                bridge_start.Add(block);
                                bridge_end.Add(block + 4);
                            }
                        }
                        count += 1;
                    }
                }
                

                // three types has different size so we can simplificate
                if (four & eight & sixteen)
                {
                    count = 0;
                    foreach (int block in splitting_blocks)
                    {
                        if (count < splitting_blocks.Count - 1)
                        {
                            if (block + 8 == splitting_blocks[count + 1])
                            {
                                refrain_start.Add(block);
                                refrain_end.Add(block + 8);
                            }
                            if (block + 16 == splitting_blocks[count + 1])
                            {
                                couplet_start.Add(block);
                                couplet_end.Add(block + 16);
                            }
                        }
                        count += 1;
                    }
                }

                // eight is balanced between bridges and refrain sixteen is couplet 
                if (!four & eight & sixteen)
                {
                    int part_size = 8;
                    int group_number = 2;
                    string mode = "br";

                   
                    List<List<int>> output_list = Sorter(splitting_blocks, group_number, part_size, project1, mode);
                    bridge_start = output_list[0];
                    bridge_end = output_list[1];
                    refrain_start = output_list[4];
                    refrain_end = output_list[5];

                    // fill couplet
                    count = 0;
                    foreach (int block in splitting_blocks)
                    {
                        if (count < splitting_blocks.Count - 1)
                        {
                            if (block + 16 == splitting_blocks[count + 1])
                            {
                                couplet_start.Add(block);
                                couplet_end.Add(block + 16);
                            } 
                        }
                        count += 1;
                    }
                }

                //Everything is eight sized so they can be the three (group the connected ones)
                if (!four & eight & !sixteen)
                {
                    int part_size = 8;
                    int group_number = 3;
                    string mode = "bcr";
                    List<List<int>> output_list = Sorter(splitting_blocks, group_number, part_size, project1, mode);
                    bridge_start = output_list[0];
                    bridge_end = output_list[1];
                    couplet_start = output_list[2];
                    couplet_end = output_list[3];
                    refrain_start = output_list[4];
                    refrain_end = output_list[5];
                }

                // eight are couplet or refrain (group the connected ones)
                if (four & eight & !sixteen)
                {
                    int part_size = 8;
                    int group_number = 2;
                    string mode = "cr";
                    List<List<int>> output_list = Sorter(splitting_blocks, group_number, part_size, project1, mode);
                    couplet_start = output_list[2];
                    couplet_end = output_list[3];
                    refrain_start = output_list[4];
                    refrain_end = output_list[5];
                }

                // sixteen are couplet or refrain
                if (!eight & sixteen)
                {
                    int part_size = 16;
                    int group_number = 2;
                    string mode = "cr";
                    List<List<int>> output_list = Sorter(splitting_blocks, group_number, part_size, project1, mode);
                    couplet_start = output_list[2];
                    couplet_end = output_list[3];
                    refrain_start = output_list[4];
                    refrain_end = output_list[5];   
                }

                // if block of size 6 and other blocks exist we declare them as bridges WILL ERASE OTHER SIZED BRIDGES..
                if (six)
                {
                    count = 0;
                    foreach (int block in splitting_blocks)
                    {
                        if (count < splitting_blocks.Count - 1)
                        {
                            if (block + 6 == splitting_blocks[count + 1])
                            {
                                Console.WriteLine("hello");
                                bridge_start.Add(block);
                                bridge_end.Add(block + 6);
                            }
                        }
                        count += 1;
                    }
                }


                // Finally write results
                string name_channel_fp = Path.Join(dirname, "time_cut.txt");
                using (StreamWriter name_channel_file = new StreamWriter(name_channel_fp))
                {
                    name_channel_file.WriteLine("INTRO : 0-" + intro_end + ',');

                    string str_to_write = "COUPLET :";
                    int counter2 = 0;
                    foreach (int start in couplet_start)
                    {
                        int end = couplet_end[counter2];
                        str_to_write = str_to_write + ' ' + start + '-' + end + ',';
                        counter2 += 1;
                    }
                    name_channel_file.WriteLine(str_to_write);

                    str_to_write = "PRE_REF :";
                    counter2 = 0;
                    foreach (int start in pre_ref_start)
                    {
                        int end = pre_ref_end[counter2];
                        str_to_write = str_to_write + ' ' + start + '-' + end + ',';
                        counter2 += 1;
                    }
                    name_channel_file.WriteLine(str_to_write);


                    str_to_write = "REFRAIN :";
                    counter2 = 0;
                    foreach (int start in refrain_start)
                    {
                        int end = refrain_end[counter2];
                        str_to_write = str_to_write + ' ' + start + '-' + end + ',';
                        counter2 += 1;
                    }
                    name_channel_file.WriteLine(str_to_write);

                    str_to_write = "PONT :";
                    counter2 = 0;
                    foreach (int start in bridge_start)
                    {
                        int end = bridge_end[counter2];
                        str_to_write = str_to_write + ' ' + start + '-' + end + ',';
                        counter2 += 1;
                    }
                    name_channel_file.WriteLine(str_to_write);

                    str_to_write = "OUTRO :";
                    counter2 = 0;
                    int ending = max_mesure;
                    str_to_write = str_to_write + ' ' + outro_start + '-' + ending + ',';
                    counter2 += 1;
                 
                    name_channel_file.WriteLine(str_to_write);
                }
            }
        }
    }
}
