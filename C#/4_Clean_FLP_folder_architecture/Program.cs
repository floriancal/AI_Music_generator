using System;
using System.Collections.Generic;
using Monad.FLParser;
using System.IO;
using System.Linq;
using System.IO.Compression;
using static Microsoft.VisualBasic.FileIO.FileSystem;
namespace RECOVER_FOLDER_ARCH
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            // Declare some paths
            string targetDirectory = "C:/Users/fcala/Desktop/Son/FLPzz/";

            String[] fileEntries = Directory.GetFiles(targetDirectory, "*.flp", SearchOption.AllDirectories);

            foreach (string file in fileEntries)
            {
                // load file
                Console.WriteLine("Parsing File..." + file);
                Project project1 = Monad.FLParser.Project.Load(file, false);

                var filename = Path.GetFileNameWithoutExtension(file);
                var filename_we = Path.GetFileName(file);
                var dirname = Path.GetDirectoryName(file);
               

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
                        String track_name = track.Name;
                        System.Collections.Generic.List<IPlaylistItem> Items = track.Items;
                        foreach (IPlaylistItem item in Items)
                        {
                            if (item is ChannelPlaylistItem)
                            {
                                ChannelPlaylistItem channel_playlist = (ChannelPlaylistItem)item;
                                Channel channel = channel_playlist.Channel;
                                int channel_id = channel.Id;

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
                                        track_sample_mapper.Add(channel_id, generator.SampleFileName);
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
                                            track_sample_mapper.Add(channel.Id, generator.SampleFileName);
                                    
                                        }
                                    }
                                }
                            }
                        }
                    }
                        Directory.CreateDirectory(Path.Join(targetDirectory, filename));
                        File.Move(file, Path.Join(targetDirectory, filename, filename_we));
                        foreach (System.Collections.Generic.KeyValuePair<int, string> kvp in track_sample_mapper)
                        {
                              File.Copy(Path.Join(targetDirectory, Path.GetFileName(kvp.Value)),
                                        Path.Join(targetDirectory, filename, Path.GetFileName(kvp.Value)));
                        }
            }
        }
    }
}
