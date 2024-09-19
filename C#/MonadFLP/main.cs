using Monad.FLParser
using System
namespace MyFLP Parser
{

    class main
    {
        static void Main(string[] args)
        {
            // Display the number of command line arguments.
            Project project1 = Project.Load("C:\Users\fcala\Desktop\Son\FLP\myflp.flp");
	        fp = _sample_parser.txt"
            insert_fp = 
            Track[] tracks = project1.Tracks;
            Insert[] Inserts = project1.Inserts;

	    //Variable initialisation
	    System.Collections.Generic.IDictionary<string, int> track_insert_mapper = new Dictionary<string, int>
	    System.Collections.Generic.IDictionary<string, string> track_sample_mapper = new Dictionary<string, string>

        using (StreamWriter file = new StreamWriter(fp))
	        {
                //Parse by tracks to get each items
                foreach (Track track in tracks)
                {
                    String name = track.Name
	                file.Writeline("track_name :"+ name)
                    System.Collections.Generic.List<IPlaylistItem> Items = track.Items;
                    foreach(IPlaylistItem item in Items)
                    {
                        if (item is ChannelPlaylistItem)
		                {       
			                int item_pos = item.Position;
			                int item_offset = item.StartOffset;
			                int item_f_pos = item_pos + item_offset;
			                file.Writeline("sample_pos :"+ item_f_pos);
						
					ChannelPlaylistItem channel = (ChannelPlaylistItem) item;
					IChannelData Data = channel.Data;
					if (Data is GeneratorData) 
					{
						track_insert_mapper.Add(track_name, Data.Insert)
						track_sample_mapper.Add(track_name, Data.SampleFileName)

			if (item is PatternPlaylistItem)
			{
				Dictionary<Channel, List<Note>> Notes = Pattern.Notes;
				foreach(System.Collections.Generic.KeyValuePair<Channel, System.Collections.Generic.List<Note>> kvp in Notes)
				{
					channel = kvp.Key;
				}
				IChannelData Data = channel.Data;
					if (Data is GeneratorData) 
					{
						track_insert_mapper.Add(track_name, Data.Insert)

						
					
                        }


                }

        using (StreamWriter file = new StreamWriter(insert_fp))
                {

		foreach(System.Collections.Generic.KeyValuePair<string, int> kvp in track_insert_mapper)
		{
			
			file.Writeline("track_name :"+ kvp.Key);
			insert_id = kvp.Value
			foreach( Insert insert in Inserts)
			{
 				int insert_ID = insert.Id
				if (insert_ID  == insert_id)
				{
					file.Writeline("Volume :"+ insert.Volume);
					file.Writeline("Pan :"+ insert.Pan);
					file.Writeline("StereoSep :"+ insert.StereoSep);
					file.Writeline("LowLevel :"+ insert.LowLevel);
					file.Writeline("BandLevel :"+ insert.BandLevel);
					file.Writeline("HighLevel :"+ insert.HighLevel);
					file.Writeline("LowFreq :"+ insert.LowFreq);
					file.Writeline("BandFreq :"+ insert.BandFreq);
					file.Writeline("HighFreq :"+ insert.HighFreq);
					file.Writeline("LowWidth :"+ insert.LowWidth);
					file.Writeline("BandWidth :"+ insert.BandWidth);
					file.Writeline("HighWidth :"+ insert.HighWidth);
				}

		 using (StreamWriter file = new StreamWriter(track_sample_fp))
	         {
			foreach(System.Collections.Generic.KeyValuePair<string, string> kvp in track_sample_mapper)
			{
				file.Writeline(kvp.key +':' + kvp.Value);
			}


                }

    }
} 