using System;
using System.Collections.Generic;
using Monad.FLParser;
using System.IO;
using System.Linq;
using System.IO.Compression;
using NUnrar.Archive;
using static Microsoft.VisualBasic.FileIO.FileSystem;

namespace ConsoleApp1
{
    class Program
    {   
        // this main will parse a flp file gather some needed data in txt files, in the second part it will create flp files containing only one note or one sample of each channel to be used by a sample parser those file will be created in the folder sammpling_flp_files
        static void Main(string[] args)
        {
            //Needed classes
            static string GetRootFolder(string path)
            {
                string path_out = Path.GetFileName(path);
                return path_out;
            }
            static string NormalizePath(string path)
            {
                return Path.GetFullPath(new Uri(path).LocalPath)
                           .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                           .ToUpperInvariant();
            }
            
            //////////////////////////////////////////////////////////////////////////////////////////////////////////   MY FLP PARSER ////////////////////////////////////////////////////////////
            Console.WriteLine("Starting Parser !");

            // Declare some paths
            string targetDirectory = "C:/Users/fcala/Desktop/Son/FLP_TEST/";
          
            string fail_song_dir = "C:/Users/fcala/Desktop/Son/need_att/";
            string raw_flp_dir = "C:/Users/fcala/Desktop/Son/FLP_RAW/";
            
            // Move raw prepared file to flp folder if they are ready
            foreach(string dir in Directory.GetDirectories(raw_flp_dir))
            {
                string[] files = Directory.GetFiles(dir);
                bool valid_log = false;
                bool valid_version = false;

                // First Test is does the previous code log file exist + is version 20 ?
                foreach( string file in files)
                { 
                    if (file.Contains("log.txt")) { valid_log = true; }

                    if (file.Contains(".flp"))
                    {
                        try
                        {
                            Project project1 = Monad.FLParser.Project.Load(file, false);
                            string V_ = project1.VersionString;
                            if (V_.Contains("20")) { valid_version = true; }
                        }
                        catch
                        {
                            string dirName = new DirectoryInfo(dir).Name;
                           Directory.Move(dir, Path.Join(fail_song_dir, dirName));
                        }
                    }
                }
               
                if (valid_version & valid_log)
                {
                    string dirName = new DirectoryInfo(dir).Name;
                    Directory.Move(dir, Path.Join(targetDirectory, dirName));
                }
            }
            
            // PRE Clean Folder
            string infolder = targetDirectory;
            string[] fileEntries = Directory.GetFiles(targetDirectory, "*.flp", SearchOption.AllDirectories);
            foreach (string dir in Directory.GetDirectories(targetDirectory))
            {
                int count_files = Directory.GetFiles(dir).Count();
                int count_dir = Directory.GetDirectories(dir).Count();
                if (count_files == 0 & count_dir == 0) { Directory.Delete(dir); }
            }

            // Redef in case of movement
            fileEntries = Directory.GetFiles(targetDirectory, "*.flp", SearchOption.AllDirectories);
            
            //#######################################################################################################################################################################
            // start parsing songs 
            //In case of failure we just move the song to a specific folder 'need_att'
            
            // Parse all files in the folder
            foreach (string file in fileEntries)
            {
                //try
                //{
                    Console.WriteLine("FLP Parsing File..." + file);
                    var dirname = Path.GetDirectoryName(file);
                    if (Path.GetDirectoryName(file) == targetDirectory)
                    {
                        dirname = Path.Join(targetDirectory, Path.GetFileNameWithoutExtension(file));
                        Directory.CreateDirectory(dirname);
                        File.Move(file, Path.Join(dirname, Path.GetFileName(file)));
                    }
                    bool is_sample = false;

                    // load file
                    Project project1 = Monad.FLParser.Project.Load(file, false);

                    // Path initialization
                    var filename = Path.GetFileNameWithoutExtension(file);
                    string fp = Path.Join(dirname, filename + "_sample_parser.txt");
                    string insert_fp = Path.Join(dirname, filename + "_insert_parser.txt");
                    string channel_fp = Path.Join(dirname, filename + "_channel_parser.txt");
                    string track_sample_fp = Path.Join(dirname, filename + "_sample_mapper.txt");
                    string channel_pattern_fp = Path.Join(dirname, filename + "_channel_pattern_linker.txt");
                    string pre_name_channel = Path.Join(dirname, filename + "_pre_name_channel.txt");

                    Track[] tracks = project1.Tracks;
                    Insert[] Inserts = project1.Inserts;
                    System.Collections.Generic.List<Channel> Channels = project1.Channels;
                    System.Collections.Generic.List<Pattern> Patterns = project1.Patterns;

                    //Variable initialisation
                    System.Collections.Generic.IDictionary<int, int> track_insert_mapper = new Dictionary<int, int>();
                    System.Collections.Generic.IDictionary<int, string> track_sample_mapper = new Dictionary<int, string>();
                    System.Collections.Generic.IDictionary<int, double> channel_volume = new Dictionary<int, double>();
                    System.Collections.Generic.IDictionary<int, double> channel_pan = new Dictionary<int, double>();
                    System.Collections.Generic.IDictionary<int, int> sample_amp = new Dictionary<int, int>();
                    System.Collections.Generic.IDictionary<string, System.Collections.Generic.List<int>> channel_pattern_mapper = new Dictionary<string, System.Collections.Generic.List<int>>();
                    System.Collections.Generic.List<(int, int)> pattern_list = new List<(int, int)>();

                    int count = 0;
                    foreach (Channel channel in Channels)
                    {
                        if (channel.Data is GeneratorData)
                        {
                            GeneratorData generator = (GeneratorData)channel.Data;
                            track_sample_mapper.Add(count, generator.SampleFileName);
                        }
                        count++;
                    }
                    using (StreamWriter sample_file = new StreamWriter(fp))
                    {
                        foreach (Pattern pattern in Patterns)
                        {
                            Dictionary<Channel, List<Note>> Notes = pattern.Notes;
                            foreach (Channel channel in Notes.Keys)
                            {
                                int channel_id = channel.Id;
                                int pattern_id = pattern.Id;
                                (int, int) tuple = (channel_id, pattern_id);
                                pattern_list.Add(tuple);
                            }
                        }

                        //Parse by tracks to get each items
                        foreach (Track track in tracks)
                        {
                            int possible_sample_count = 0;
                            String track_name = track.Name;

                            System.Collections.Generic.List<IPlaylistItem> Items = track.Items;

                            foreach (IPlaylistItem item in Items)
                            {

                                if (item is ChannelPlaylistItem)
                                {
                                    int item_pos = item.Position;
                                    int item_offset = item.StartOffset;
                                    int item_f_pos = item_pos;//+ item_offset;


                                    ChannelPlaylistItem channel_playlist = (ChannelPlaylistItem)item;
                                    Channel channel = channel_playlist.Channel;
                                    int channel_id = channel.Id;

                                    sample_file.WriteLine("channel_name :" + channel_id);
                                    sample_file.WriteLine("sample_pos :" + item_f_pos);

                                    IChannelData Data = channel.Data;
                                    if (Data is GeneratorData)
                                    {
                                        GeneratorData generator = (GeneratorData)Data;

                                        bool keyExists = track_insert_mapper.ContainsKey(channel.Id);
                                        if (keyExists == false)
                                        {
                                            if (generator.SampleFileName.Contains("%FLStudioData%"))
                                            {
                                                string filename2 = Path.GetFileName(generator.SampleFileName);
                                                generator.SampleFileName = Path.Join(dirname, filename2);
                                            }

                                            track_insert_mapper.Add(channel_id, generator.Insert);
                                            
                                            channel_volume.Add(channel_id, generator.Volume);
                                            channel_pan.Add(channel_id, generator.Panning);
                                            sample_amp.Add(channel_id, generator.SampleAmp);
                                            if (generator.GeneratorName == "Sampler")
                                            {
                                                if (item.Length > project1.Ppq * 4 * 12) { is_sample = true; }
                                                if (item.Length > project1.Ppq * 4 * 3) { possible_sample_count += 1; }
                                                if (possible_sample_count > 2) { is_sample = true; }
                                                if (is_sample) { channel.Name = "sample"; }
                                            }
                                        }
                                    }
                                }

                                if (item is PatternPlaylistItem)
                                {
                                    PatternPlaylistItem pattern_playlist = (PatternPlaylistItem)item;
                                    Pattern pattern = pattern_playlist.Pattern;
                                    Dictionary<Channel, List<Note>> Notes = pattern.Notes;
                                    Channel channel;

                                    foreach (System.Collections.Generic.KeyValuePair<Channel, System.Collections.Generic.List<Note>> kvp in Notes)
                                    {
                                        channel = kvp.Key;
                                        IChannelData Data = channel.Data;
                                        if (Data is GeneratorData)
                                        {
                                            bool keyExists = track_insert_mapper.ContainsKey(channel.Id);
                                            if (keyExists == false)
                                            {
                                                GeneratorData generator = (GeneratorData)Data;
                                                track_insert_mapper.Add(channel.Id, generator.Insert);
                                       
                                                channel_volume.Add(channel.Id, generator.Volume);
                                                channel_pan.Add(channel.Id, generator.Panning);
                                                sample_amp.Add(channel.Id, generator.SampleAmp);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    //Writing files using pre-parsed data above
                    using (StreamWriter insert_file = new StreamWriter(insert_fp))
                    {
                        foreach (System.Collections.Generic.KeyValuePair<int, int> kvp in track_insert_mapper)
                        {
                            insert_file.WriteLine("channel_name :" + kvp.Key);
                            int insert_id = kvp.Value;
                            foreach (Insert insert in Inserts)
                            {
                                int insert_ID = insert.Id;
                                if (insert_ID == insert_id)
                                {
                                    insert_file.WriteLine("Volume :" + insert.Volume);
                                    insert_file.WriteLine("Pan :" + insert.Pan);
                                    insert_file.WriteLine("StereoSep :" + insert.StereoSep);
                                    insert_file.WriteLine("LowLevel :" + insert.LowLevel);
                                    insert_file.WriteLine("BandLevel :" + insert.BandLevel);
                                    insert_file.WriteLine("HighLevel :" + insert.HighLevel);
                                    insert_file.WriteLine("LowFreq :" + insert.LowFreq);
                                    insert_file.WriteLine("BandFreq :" + insert.BandFreq);
                                    insert_file.WriteLine("HighFreq :" + insert.HighFreq);
                                    insert_file.WriteLine("LowWidth :" + insert.LowWidth);
                                    insert_file.WriteLine("BandWidth :" + insert.BandWidth);
                                    insert_file.WriteLine("HighWidth :" + insert.HighWidth);
                                }
                            }
                        }
                    }

                    using (StreamWriter tr_sp_file = new StreamWriter(track_sample_fp))
                    {
                        foreach (System.Collections.Generic.KeyValuePair<int, string> kvp in track_sample_mapper)
                        {
                            tr_sp_file.WriteLine(kvp.Key + " , " + kvp.Value);
                        }
                    }

                    using (StreamWriter channel_pattern_file = new StreamWriter(channel_pattern_fp))
                    {
                        foreach ((int, int) item in pattern_list)
                        {
                            channel_pattern_file.WriteLine(item);
                        }
                    }

                    using (StreamWriter channel_file = new StreamWriter(channel_fp))
                    {
                        foreach (System.Collections.Generic.KeyValuePair<int, double> kvp in channel_volume)
                        {
                            channel_file.WriteLine("volume : " + kvp.Key + " , " + kvp.Value);
                        }
                        foreach (System.Collections.Generic.KeyValuePair<int, double> kvp in channel_pan)
                        {
                            channel_file.WriteLine("pan : " + kvp.Key + " , " + kvp.Value);
                        }
                        foreach (System.Collections.Generic.KeyValuePair<int, double> kvp in channel_volume)
                        {
                            channel_file.WriteLine("sample_volume : " + kvp.Key + " , " + kvp.Value);
                        }
                    }

                    foreach (Channel channel in project1.Channels)
                    {
                        string chan_name = channel.Name;

                        IChannelData Data = channel.Data;
                        if (Data is GeneratorData) { GeneratorData generator = (GeneratorData)Data; if (generator.GeneratorName != "Sampler") { channel.Name = chan_name +  " plugin"; } }
                        else
                        { channel.Name = "auto"; }
                        if (chan_name.ToLower().Contains("sample")) { channel.Name = "X"; }
                        if (chan_name.ToLower().Contains("loop")) { channel.Name = "X"; }

                        // start by type naming if we cant do better
                        if (chan_name.ToLower().Contains("drum")) { channel.Name = "drums"; }
                        if (chan_name.ToLower().Contains("perc")) { channel.Name = "drums"; }

                        //plugin names  for types
                        if (chan_name.ToLower().Contains("sakura")) { channel.Name = "melo"; }
                        if (chan_name.ToLower().Contains("synth")) { channel.Name = "melo"; } // NOT ALWAYS TRUE
                        if (chan_name.ToLower().Contains("helix")) { channel.Name = "melo"; }
                        if (chan_name.ToLower().Contains("tunefish")) { channel.Name = "melo"; }
                        if (chan_name.ToLower().Contains("sytrus")) { channel.Name = "melo"; }
                        if (chan_name.ToLower().Contains("osc")) { channel.Name = "melo"; }
                        if (chan_name.ToLower().Contains("omnisphere")) { channel.Name = "melo"; }
                        if (chan_name.ToLower().Contains("kontakt")) { channel.Name = "melo"; }
                        if (chan_name.ToLower().Contains("keyscape")) { channel.Name = "melo"; }

                        //name by user or sample names stored as plugin name
                        if (chan_name.ToLower().Contains("close") & chan_name.ToLower().Contains("h")) { channel.Name = "H"; }
                        if (chan_name.ToLower().Contains("closed") & chan_name.ToLower().Contains("h")) { channel.Name = "H"; }
                        if (chan_name.ToLower().Contains("open") & chan_name.ToLower().Contains("h")) { channel.Name = "L"; }
                        if (chan_name.ToLower().Contains("hh")) { channel.Name = "H"; }
                        if (chan_name.ToLower().Contains("low")) { channel.Name = "L"; }
                        if (chan_name.ToLower().Contains("crash")) { channel.Name = "Y"; }
                        if (chan_name.ToLower().Contains("ride")) { channel.Name = "L"; }
                        if (chan_name.ToLower().Contains("shak")) { channel.Name = "SH"; }
                        if (chan_name.ToLower().Contains("tom")) { channel.Name = "T"; }
                        if (chan_name.ToLower().Contains("fx")) { channel.Name = "fx"; }

                        if (chan_name.ToLower().Contains("bass")) { channel.Name = "B"; }
                        if (chan_name.ToLower().Contains("808")) { channel.Name = "B"; }

                        if (chan_name.ToLower().Contains("kick")) { channel.Name = "K"; }
                        if (chan_name.ToLower().Contains("kck")) { channel.Name = "K"; }
                        if (chan_name.ToLower().Contains("clap")) { channel.Name = "C"; }
                        if (chan_name.ToLower().Contains("snare")) { channel.Name = "S"; }
                        if (chan_name.ToLower().Contains("rim")) { channel.Name = "S"; }
                        if (chan_name.ToLower().Contains("k ")) { channel.Name = "K"; }
                        if (chan_name.ToLower().Contains("k_")) { channel.Name = "K"; }
                        if (chan_name.ToLower().Contains(" k")) { channel.Name = "K"; }
                        if (chan_name.ToLower().Contains("_k")) { channel.Name = "K"; }

                        if (chan_name.ToLower().Contains("15_traphats_sp_09")) { channel.Name = "L"; }
                        if (chan_name.ToLower().Contains("15_traphats_sp_05")) { channel.Name = "H"; }
                        if (chan_name.ToLower().Contains("15_traphats_sp_06")) { channel.Name = "H"; }
                        if (chan_name.ToLower().Contains("15_traphats_sp_07")) { channel.Name = "H"; }
                        if (chan_name.ToLower().Contains("15_traphats_sp_04")) { channel.Name = "H"; }
                        if (chan_name.ToLower().Contains("hat 6")) { channel.Name = "H"; }
                        if (chan_name.ToLower().Contains("pierre hat")) { channel.Name = "H"; }
                        if (chan_name.ToLower().Contains("riser")) { channel.Name = "fx riser"; }
                        if (chan_name.ToLower().Contains("chimes")) { channel.Name = "fx chimes"; }
                        if (chan_name.ToLower().Contains("snr")) { channel.Name = "S"; }


                        if (chan_name.ToLower().Contains("bongo hit")) { channel.Name = "T"; }
                        if (chan_name.ToLower().Contains("00220")) { channel.Name = "fx"; }
                        if (chan_name.ToLower().Contains("funky electric")) { channel.Name = "B"; }
                        if (chan_name.ToLower().Contains("reverse")) { channel.Name = "fx cymbal REVERSE"; }
                        if (chan_name.ToLower().Contains("ns-k")) { channel.Name = "K"; }
                        if (chan_name.ToLower().Contains("kik")) { channel.Name = "K"; }
                        if (chan_name.ToLower().Contains("hat 6")) { channel.Name = "H"; }
                        if (chan_name.ToLower().Contains("bad guy hit")) { channel.Name = "H"; }

                        if (chan_name.ToLower().Contains("wu_1p (76)")) { channel.Name = "H"; }
                        if (chan_name.ToLower().Contains("wu_1p (44)")) { channel.Name = "H"; }
                        if (chan_name.ToLower().Contains("wu_1p (67)")) { channel.Name = "H"; }
                        if (chan_name.ToLower().Contains("wu_1p (45)")) { channel.Name = "H"; }
                        if (chan_name.ToLower().Contains("wu_1p (40)")) { channel.Name = "K"; }
                        if (chan_name.ToLower().Contains("wu_1s")) { channel.Name = "S"; }



                            //name with real plugin name 
                            if (chan_name.ToLower().Contains("addictive") & chan_name.ToLower().Contains("keys")) { channel.Name = "M"; }
                            if (chan_name.ToLower().Contains("real") & chan_name.ToLower().Contains("lpc")) { channel.Name = "M"; }
                            if (chan_name.ToLower().Contains("pianissimo")) { channel.Name = "M"; }

                            //equalities
                            if (chan_name.ToLower() == "wet") { channel.Name = "fx"; }

                            // OR WITH USER CLASSICAL NAMES
                            // we have to be careful on classic name to not classify an entire sample as an instrument so classic names are only appliable to plugins elems
                            if (channel.Data is GeneratorData)
                            {
                                GeneratorData gen_data = (GeneratorData)channel.Data;
                                if (gen_data.GeneratorName != "Sampler")
                                {
                                    if (chan_name.ToLower().Contains("mel")) { channel.Name = "M"; }
                                    if (chan_name.ToLower().Contains("piano")) { channel.Name = "M"; }
                                    if (chan_name.ToLower().Contains("guitar")) { channel.Name = "M"; }
                                    if (chan_name.ToLower().Contains("flute")) { channel.Name = "M"; }
                                    if (chan_name.ToLower().Contains("violin")) { channel.Name = "M"; }
                                    if (chan_name.ToLower().Contains("violon")) { channel.Name = "M"; }
                                }
                            }
    
                            if (chan_name.ToLower().Contains(" - ") & (chan_name.ToLower().Contains("insert")
                            || chan_name.ToLower().Contains("volume") || chan_name.ToLower().Contains("param.") 
                            ||  chan_name.ToLower().Contains("pitch") || chan_name.ToLower().Contains("mix level") || chan_name.ToLower().Contains("reverb")
                            || chan_name.ToLower().Contains("freq") || chan_name.ToLower().Contains("pre amp") || chan_name.ToLower().Contains("size") || chan_name.ToLower().Contains("freq"))) 
                            { channel.Name = "auto"; }
                            if (chan_name.ToLower().Contains("master")) { channel.Name = "auto"; }
                    }
                    
                    using (StreamWriter pre_name_channel_file = new StreamWriter(pre_name_channel))
                    {
                        foreach (Channel channel in project1.Channels)
                        {
                            pre_name_channel_file.WriteLine(channel.Name);
                        }
                    }
                    
               // }
               //catch
                //{
                  //  Console.WriteLine("fail file..Moving that file to need att folder for further attention");
                    //DirectoryInfo song_dir = Directory.GetParent(file);
                    //CopyDirectory(song_dir.FullName, Path.Join(fail_song_dir, song_dir.Name), true);
                //}
            }
            
            ////////////////////////////////////////////////////////////////////////////////////////////////////////// SAMPLE PARSER ///////////////////////////////////////////////////////////////////////////////
            ///This second part will create flp files needed for sampling sounds in a further step, those file we place in the output folder in separated folders containing all datas relative to the song
            Console.WriteLine("Hello World!");

            infolder = targetDirectory;
            string outfolder = "C:/Users/fcala/Desktop/Son/Export_sampling";
            string[] fileEntries2 = Directory.GetFiles(infolder, "*.flp", SearchOption.AllDirectories);

            // This call create a flp file with data provieded
            string template_path = "C:\\Users\\fcala\\Desktop\\Son\\Gen_template\\Template_parsing\\sampling_template_plugin.flp";

            // Parse all files
            foreach (string file in fileEntries2)
            {
                // if a file fail it is just ignored 
                //try
                //{
                    Console.WriteLine("Parsing sampler  File..." + file);
                    
                    //Load
                    Project project1 = Monad.FLParser.Project.Load(file, false);
                    Track[] Tracks = project1.Tracks;
                    System.Collections.Generic.List<Channel> Channels = project1.Channels;
                    List<Pattern> Patterns = project1.Patterns;

                    //Now we will create a flp file for each different sound to record (there will be doublons and they are handled by other scripts see python projects)
                    int project_count = 0;
                    int number_of_files = Channels.Count ;
                    string parent_dir = Path.GetFileName((string)Path.GetDirectoryName(file));

                    while (project_count < number_of_files)
                    {
                        //Write file to generatefolder location
                        bool flp_alone = true;
                        string in_folder_parent = Path.GetFileName(infolder);
                        string outpath = "";

                        if (parent_dir != in_folder_parent)
                        {
                            outpath = Path.Join(outfolder, parent_dir);
                            flp_alone = false;
                        }
                        else
                        {
                            flp_alone = true;
                            outpath = Path.Join(outfolder, Path.GetFileNameWithoutExtension(file));
                        }
                        //creates dir if not exist 
                        if (!Directory.Exists(outpath))
                        {
                            Directory.CreateDirectory(outpath);
                        }

                        Directory.CreateDirectory(Path.Join(outpath, project_count.ToString()));
                        string filepath = Path.Join(outpath, project_count.ToString(), Path.GetFileNameWithoutExtension(file) + "_" + project_count + ".flp");

                        Project_writer_sampler p1 = new Project_writer_sampler(file, template_path,  filepath,  project1, project_count);
                        
                        project_count += 1;
                        
                        //number_of_files = project_writer.number_of_files;
                        string in_directory = Path.GetDirectoryName(file);
                        string[] filePaths = Directory.GetFiles(in_directory);
                        if (flp_alone is false)
                        {
                            foreach (var filename in filePaths)
                            {
                                string filename_str = filename.ToString();
                                //Do your job with "file"  
                                string filename_out = Path.GetFileName(filename_str);
                                if (Path.GetExtension(filename_out) != ".flp")
                                {
                                    string outpath_file = Path.Join(outpath, filename_out);
                                    if (!File.Exists(outpath_file))
                                    {
                                        File.Copy(filename_str, outpath_file);
                                    }
                                }
                            }
                        }
                    }
                   
                //}
                //catch
                //{
                //Console.WriteLine("fail sampling file");
                //}
            }

            //Clean folder
            foreach (string dir in Directory.GetDirectories(targetDirectory))
            {
                int count_files = Directory.GetFiles(dir).Count();
                int count_dir = Directory.GetDirectories(dir).Count();
                if (count_files == 0 & count_dir == 0)
                {
                    Directory.Delete(dir);
                }
            }
           
        }
    }
}

// Replace notes in each pattern to one C5 create one file for each 
/*List<Pattern> new_pat_list = new List<Pattern>();

// Pattern modifier and also do a split by channels on pattern this result in new patterns creation
foreach (Pattern pattern in Patterns)
{   
    Dictionary<Channel, List<Note>> note_dict = pattern.Notes;
    Note replace_note = new Note();
    replace_note.Position = 0;
    replace_note.Length = 96;
    replace_note.Key = 60;
    replace_note.FinePitch = 120;
    replace_note.Release = 48;
    replace_note.Pan = 64;
    replace_note.Velocity = 100;
    List<Note> replace_note_list = new List<Note>();
    replace_note_list.Add(replace_note);

    int chan_count = 0;
    foreach (Channel channel in note_dict.Keys.ToList())
    {
        chan_count += 1;
        if (chan_count == 1)
        {
            note_dict[channel] = replace_note_list;
        }
        else
        {
            List<Note> replace_note_list_empty = new List<Note>();
            note_dict[channel] = replace_note_list_empty;
        }
    }
    pattern.Notes = note_dict;

    chan_count = 0;
    foreach (Channel channel in note_dict.Keys.ToList())
    {
        chan_count += 1;
        if (chan_count != 1)
        {
            Pattern new_pat = new Pattern();
            Dictionary<Channel, List<Note>> note_dict2 = new Dictionary<Channel, List<Note>>();
            note_dict2[channel] = replace_note_list;
            new_pat.Notes = note_dict2;
            new_pat_list.Add(new_pat);
        }
    }
}
foreach (Pattern pat in new_pat_list) { Patterns.Add(pat); }
int pat_added = new_pat_list.Count; // number of pattern added from original file 
*/

//
/*CLEAN DOUBLONS FLP SAVES COMPUTATION TIME FOR PC2
List<string> allLinesText = File.ReadAllLines(Path.Join(Path.GetDirectoryName(file), "track_channel_sampling.txt" )).ToList();
List<string> channel_passed = new List<string>();
string outpath_sample = Path.Join(outfolder, parent_dir);
foreach (string text in allLinesText)
{
    string[] subs = text.Split(" : ");
    string track = subs[0];
    string channel = subs[1];
    if (channel_passed.Contains(channel))
    {
        string[] fileEntries_in_folder_song = Directory.GetFiles(outpath_sample, ("*_" + track + ".flp"), SearchOption.TopDirectoryOnly);
        File.Delete(fileEntries_in_folder_song[0]);
    }
    if (channel.Contains("ignore"))
    {
        string[] fileEntries_in_folder_song = Directory.GetFiles(outpath_sample, "*_" + track + ".flp");
        if (fileEntries_in_folder_song.Count() > 0) { File.Delete(fileEntries_in_folder_song[0]); }
    }
    channel_passed.Add(channel);
}*/

