using System;
using System.IO;
using System.IO.Compression;
using Newtonsoft.Json;
using System.Collections.Generic;
using Monad.FLParser;

namespace Post_processor_flp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            //Project test = Monad.FLParser.Project.Load("C:/Users/fcala/Desktop/untitled.flp", true);
            //Project test = Monad.FLParser.Project.Load("C:/Users/fcala/Desktop/Son/Post_Process/generate031620211910/content/generate031620211910/generate.flp", true);

            // Get all folders
            string targetDirectory = "C:/Users/fcala/Downloads";
            string post_process_dir = "C:/Users/fcala/Desktop/Son/Post_Process";
            string template_path = "C:/Users/fcala/Desktop/Son/Gen_Template/template1.flp";

            string[] dirEntries = Directory.GetFiles(targetDirectory, "generate*.zip", SearchOption.AllDirectories);

            // unzip and copy folders to post process dir
            foreach (string dir in dirEntries)
            {
                string dirname = Path.GetFileName(dir);
                string outname = Path.GetFileNameWithoutExtension(dir);
                string inpath = Path.Join(targetDirectory, dirname);
                string outpath = Path.Join(post_process_dir, outname);
                if (System.IO.Directory.Exists(outpath) is false)
                { ZipFile.ExtractToDirectory(inpath, outpath); }
            }

            // Parse all dirs and do job
            string[] dirEntries2 = Directory.GetDirectories(post_process_dir, "generate*", SearchOption.TopDirectoryOnly);

            foreach (string dir in dirEntries2)
            {
                //collect data
                string dirname = Path.GetFileNameWithoutExtension(dir);
                string inpath = Path.Join(post_process_dir, dirname, "content", dirname);

                string Pan_string = File.ReadAllText(Path.Join(inpath, "pan.txt"));
                var Pan_str_dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(Pan_string);
                var Pan_dict = new Dictionary<string, int>();

                string Stereo_string = File.ReadAllText(Path.Join(inpath, "stereo.txt"));
                var Stereo_str_dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(Stereo_string);
                var Stereo_dict = new Dictionary<string, int>();

                string Vol_string = File.ReadAllText(Path.Join(inpath, "insert_volume.txt"));
                var Vol_str_dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(Vol_string);
                var Vol_dict = new Dictionary<string, int>();

                string bpm_string = File.ReadAllText(Path.Join(inpath, "tempo.txt"));
                double bpm = Convert.ToDouble(bpm_string);

                // convert to integers
                foreach (string key in Stereo_str_dict.Keys)
                {
                    int value = (int)Math.Round(float.Parse(Pan_str_dict[key], System.Globalization.CultureInfo.InvariantCulture));
                    Pan_dict[key] = value;

                    int value2 = (int)Math.Round(float.Parse(Stereo_str_dict[key], System.Globalization.CultureInfo.InvariantCulture));
                    Stereo_dict[key] = value2;

                    //int value3 = (int)Math.Round(float.Parse(Vol_str_dict[key], System.Globalization.CultureInfo.InvariantCulture));
                    //Vol_dict[key] = value;
                }

                //assign data to FLPparser objects
                //load template file
                Project template = Monad.FLParser.Project.Load(template_path, false);

                // always work with ppq of 96
                template.Ppq = 96;

                Console.WriteLine(template.Tempo) ;
                // set tempo to value exctracted in the midi file dowloanded from generation_pipeline.ipynb
                template.Tempo = bpm;
                


                Insert[] Inserts = template.Inserts;
                List<Channel> Channels = template.Channels;
                Track[] Tracks = template.Tracks;

                foreach (Insert insert in Inserts)
                {
                    //Guarantee that this instr exist in this project
                    if (Pan_dict.ContainsKey(insert.Name))
                    {
                        insert.Pan = Pan_dict[insert.Name];
                        insert.StereoSep = Stereo_dict[insert.Name];
                        Console.WriteLine(insert.Name);
                        Console.WriteLine(insert.StereoSep);
                        //insert.Volume = Vol_dict[insert.Name];
                    }
                }

                //Modify sample path for each project
                foreach (Channel channel in Channels)
                {
                    //Guarantee that this instr exist in this project

                    IChannelData data = channel.Data;
                    GeneratorData gen_data = (GeneratorData)data;
                    string template_sample_path = gen_data.SampleFileName;
                    string instr = Path.GetFileNameWithoutExtension(template_sample_path);

                    if (Pan_dict.ContainsKey(instr))
                    {
                        gen_data.SampleFileName = Path.Join(inpath, instr + ".wav");
                    }
                }

                // Replace patterns wit generated patterns for each instr
                
                string Pattern_string = File.ReadAllText(Path.Join(inpath, "pattern_dict.txt"));
                var Pattern_dict = JsonConvert.DeserializeObject<Dictionary<string, List<List<int>>>>(Pattern_string);

                // for each track in template file
                foreach (Track track in Tracks)
                {
                    List<IPlaylistItem> Items = track.Items;

                    foreach (IPlaylistItem item in Items)
                    {
                        PatternPlaylistItem pattern_playlist = (PatternPlaylistItem)item;
                        Pattern pattern = pattern_playlist.Pattern;
                        string instr = pattern.Name;

                        // check if this generate this instrument exist
                        if (Pan_dict.ContainsKey(instr))
                        {
                            List<List<int>> note_list = Pattern_dict[instr];
                            List<Note> Real_note_list = new List<Note>();
                            Dictionary<Channel, List<Note>> Real_note_dict = new Dictionary<Channel, List<Note>>();

                            //Parse all the note in the pattern dict to create a pattern
                            foreach (List<int> note in note_list)
                            {
                                Note one_note = new Note();
                                one_note.Position = note[0];
                                one_note.Length = note[1];
                                one_note.Key = (byte)note[2];
                                one_note.FinePitch = (ushort)note[3];
                                one_note.Release = (ushort)note[4];
                                one_note.Pan = (byte)note[5];
                                one_note.Velocity = (byte)note[6];

                                Real_note_list.Add(one_note);
                            }

                            // get the associated channel object
                            foreach (Channel one_channel in pattern.Notes.Keys)
                            {
                                Channel real_channel = one_channel;
                                Real_note_dict[real_channel] = Real_note_list;
                                pattern.Notes = Real_note_dict;

                                break;
                            }


                        }
                        else
                        // remove all notes to create empty pattern because this track is not used in this song
                        {
                            List<List<int>> note_list = Pattern_dict[instr];
                            List<Note> Real_note_list = new List<Note>();
                            Dictionary<Channel, List<Note>> Real_note_dict = new Dictionary<Channel, List<Note>>();
                            foreach (Channel one_channel in pattern.Notes.Keys)
                            {
                                Channel real_channel = one_channel;
                                Real_note_dict[real_channel] = Real_note_list;
                                pattern.Notes = Real_note_dict;

                                break;
                            }
                        }
                    }
                }
            
                //Write file to generatefolder location
                string outpath = Path.Join(inpath, "template1.flp");
                bool sampling_mode = false;
                ProjectWriter project_writer = new ProjectWriter(template_path, outpath, template, sampling_mode);

                // DEBUG
                //ProjectWriter project_writer = new ProjectWriter("C:/Users/fcala/Desktop/untitled.flp", outpath, template);

            }
        }
       
    }

}