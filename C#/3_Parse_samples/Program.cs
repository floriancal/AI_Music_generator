using System;
using Monad.FLParser;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Linq;

namespace Sample_parser
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            string infolder = "C:/Users/fcala/Desktop/Son/Unitary_parsing_run/";
            string outfolder = "C:/Users/fcala/Desktop/Son/Sampling_flp_files";

            string[] fileEntries = Directory.GetFiles(infolder, "*.flp", SearchOption.AllDirectories);

            foreach (string file in fileEntries)
            {   
                //Load
                Project project1 = Monad.FLParser.Project.Load(file, false);
                
                Track[] Tracks = project1.Tracks;
                List<Pattern> Patterns = project1.Patterns;
                   
                // Replace notes in each pattern to one C5
                foreach(Pattern pattern in Patterns)
                {
                    Dictionary<Channel, List<Note>> note_dict = pattern.Notes;
                    Note replace_note = new Note();
                    replace_note.Position = 0;      
                    replace_note.Length = 96;        
                    replace_note.Key = 60;          
                    replace_note.FinePitch = 120;  
                    replace_note.Release = 48 ;     
                    replace_note.Pan = 64;           
                    replace_note.Velocity = 100;
                    List<Note> replace_note_list = new List<Note>();
                    replace_note_list.Add(replace_note);

                    foreach (Channel channel in note_dict.Keys.ToList())
                    {        
                        note_dict[channel] = replace_note_list;
                    }
                    pattern.Notes = note_dict;
                }

                //replace item in track to 1 pattern at pos 0
                int counter = 0;
                foreach (Track track in Tracks)
                {
                    List<IPlaylistItem> Items = track.Items;
                    List<IPlaylistItem> new_Items = new List<IPlaylistItem>();

                    
                    if (counter < Patterns.Count)
                    {
                        PatternPlaylistItem pattern_play = new PatternPlaylistItem();

                        pattern_play.Position = 0;
                        pattern_play.Length = 96;
                        pattern_play.StartOffset = 0;
                        pattern_play.EndOffset = 0;

                        pattern_play.Pattern = Patterns[counter];

                        IPlaylistItem Iplaylistpat = (IPlaylistItem)pattern_play;

                        new_Items.Add(Iplaylistpat);
                    }
                   
                  
                    track.Items = new_Items;

                    counter += 1;
                }


                //Write file to generatefolder location
                string outpath = Path.Join(outfolder, Path.GetFileName(file));
                bool sampling_mode = true;
                ProjectWriter project_writer = new ProjectWriter(file, outpath, project1, sampling_mode);


            }
        }
    }
}
