using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Monad.FLParser
{
   public class Project_writer_sampler
   {
        // Needed methods for the following
        private static int GetBufferLen(BinaryReader readerO)
        {
            var data = readerO.ReadByte();
            var dataLen = data & 0x7F;
            var shift = 0;
            while ((data & 0x80) != 0)
            {
                data = readerO.ReadByte();
                dataLen = dataLen | ((data & 0x7F) << (shift += 7));
            }
            return dataLen;
        }
        protected void Write7BitEncodedInt(int value, BinaryWriter writer)
        {
            // Write out an int 7 bits at a time. The high bit of the byte,
            // when on, tells reader to continue reading more bytes.
            uint v = (uint)value; // support negative numbers
            while (v >= 0x80)
            {
                writer.Write((byte)(v | 0x80));
                v >>= 7;
            }
            writer.Write((byte)v);
        }

        private Boolean chan_on = false;
        public Project_writer_sampler(string input_flp_path, string template_path, string path, Project _project,  int master_counter)
	    {
            bool second_pass = false;
            long final_len = 0;
            while (true)
            {
                System.Collections.Generic.IDictionary<Enums.Event, List<Byte[]>> Copy_dict = new Dictionary<Enums.Event, List<Byte[]>>();
                Enums.Event[] Cancel_list = {Enums.Event.WordNewChan, Enums.Event.ByteChanType, Enums.Event.GeneratorName, Enums.Event.DataNewPlugin, Enums.Event.TextPluginName, Enums.Event.ForChan6, Enums.Event.DWordColor, 
                Enums.Event.TextDelay, Enums.Event.DWordDelayReso, Enums.Event.DWordReverb, Enums.Event.WordShiftDelay, Enums.Event.ForChan2, Enums.Event.WordFx, Enums.Event.WordFx3, Enums.Event.WordCutOff,Enums.Event.WordResonance, Enums.Event.WordPreAmp,
                Enums.Event.WordDecay, Enums.Event.WordAttack, Enums.Event.WordStDel, Enums.Event.DWordFxSine, Enums.Event.WordFadeStereo, Enums.Event.ByteMixSliceNum, Enums.Event.DataBasicChanParams, Enums.Event.ForChan9, Enums.Event.ForChan7,
                Enums.Event.DataChanParams, Enums.Event.DWordCutCutBy, Enums.Event.ForChan4, Enums.Event.ForChan5, Enums.Event.DataAutomationData, Enums.Event.ForChan1, Enums.Event.ForChan8,
                Enums.Event.DataEnvLfoParams, Enums.Event.ForChan3, Enums.Event.ByteLoopType, Enums.Event.TextSampleFileName,  Enums.Event.ForChan11, Enums.Event.ForChan12,
                Enums.Event.ForChan13,Enums.Event.ForChan14,Enums.Event.ForChan15, Enums.Event.DataPluginParams };
                //Enums.Event[] Cancel_list = { Enums.Event.Byte, };

                //Detect active channel
                List<Channel> Channels = _project.Channels;
                Channel act_channel = Channels[master_counter];

                // If this channel is a samping create a file that will be used as a flag by the parser system
                IChannelData Data = act_channel.Data;
                if (Data is GeneratorData)
                {
                    GeneratorData generator = (GeneratorData)Data;
                    if (generator.GeneratorName == "Sampler" || (generator.GeneratorName == "Sample_null"& !generator.GeneratorName.Contains("_plugin")))
                    {
                        string sample_flag = Path.Combine(Path.GetDirectoryName(path), "sample.txt");
                        using (StreamWriter sample_file = new StreamWriter(sample_flag)) { }
                    }
                    


                }
                // Automation so no writing file
                else { break; }
                string chan_name = act_channel.Name;

                if (chan_name.ToLower().Contains(" - ") & (chan_name.ToLower().Contains("insert")
                                           || chan_name.ToLower().Contains("volume") || chan_name.ToLower().Contains("param.")
                                           || chan_name.ToLower().Contains("pitch") || chan_name.ToLower().Contains("mix level") || chan_name.ToLower().Contains("reverb")
                                           || chan_name.ToLower().Contains("freq") || chan_name.ToLower().Contains("pre amp") || chan_name.ToLower().Contains("size") || chan_name.ToLower().Contains("freq")))
                { break; }
                if (chan_name.ToLower().Contains("master")) { break; }

                //Read the flp input file 
                // Create reader and writer
                var streamread = File.Open(template_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                var streamread2 = File.Open(input_flp_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                var streamwrite = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
               
                var reader = new BinaryReader(streamread);
                var writer = new BinaryWriter(streamwrite);
                var reader2 = new BinaryReader(streamread2);


                // Parse head and modify if necessary 
                byte[] head_byte = reader.ReadBytes(4);
                byte[] head_byte2 = reader2.ReadBytes(4);
                if (Encoding.ASCII.GetString(head_byte) != "FLhd")
                    throw new FlParseException("Invalid magic number", reader.BaseStream.Position);
                writer.Write(head_byte);

                // header + type
                byte[] byte_header_length = reader.ReadBytes(4);
                byte[] byte_header_length2 = reader2.ReadBytes(4);

                var headerLength = BitConverter.ToInt32(byte_header_length, 0);
                if (headerLength != 6)
                    throw new FlParseException($"Expected header length 6, not {headerLength}", reader.BaseStream.Position);
                byte[] byte_write_header_length = BitConverter.GetBytes((UInt32)headerLength);

                writer.Write(byte_write_header_length);

                var type = reader.ReadInt16();
                var type2 = reader2.ReadInt16();
                if (type != 0) throw new FlParseException($"Type {type} is not supported", reader.BaseStream.Position);
                writer.Write(type);

                // channels
                var channelCount = reader.ReadInt16();
                var channelCount2 = reader2.ReadInt16();
                if (channelCount < 1 || channelCount > 1000)
                    throw new FlParseException($"Invalid number of channels: {channelCount}", reader.BaseStream.Position);
                writer.Write(channelCount);

                // ppq
                UInt16 Ppq = (UInt16)reader.ReadInt16();
                UInt16 Ppq2 = (UInt16)reader2.ReadInt16();
                if (Ppq < 0) throw new Exception($"Invalid PPQ: {Ppq}");
                if (_project.Ppq != Ppq)
                {
                    UInt16 new_ppq = (UInt16)_project.Ppq;
                    //writer.BaseStream.Seek(reader.BaseStream.Position - 2, SeekOrigin.Begin);         
                    writer.Write(new_ppq);
                }
                else
                {
                    writer.Write(Ppq);
                }

                var len = 0;
                while (true)
                {
                    byte[] one_byte = reader.ReadBytes(1);
                    byte[] one_byte2 = reader2.ReadBytes(1);
                    writer.Write(one_byte);
                    if (Encoding.ASCII.GetString(one_byte) == "F")
                    {
                        byte[] sec_byte = reader.ReadBytes(1);
                        byte[] sec_byte2 = reader2.ReadBytes(1);
                        writer.Write(sec_byte);
                        if (Encoding.ASCII.GetString(sec_byte) == "L")
                        {
                            byte[] third_byte = reader.ReadBytes(1);
                            byte[] third_byte2 = reader2.ReadBytes(1);
                            writer.Write(third_byte);
                            if (Encoding.ASCII.GetString(third_byte) == "d")
                            {
                                byte[] fourth_byte = reader.ReadBytes(1);
                                byte[] fourth_byte2 = reader2.ReadBytes(1);
                                writer.Write(fourth_byte);
                                if (Encoding.ASCII.GetString(fourth_byte) == "t")
                                {
                                    len = reader.ReadInt32();
                                    var len2 = reader2.ReadInt32();
                                    writer.Write((Int32)(final_len - reader.BaseStream.Position));
                                    break;
                                }
                            }
                        }
                    }
                }
                // Parse input flp file to retreive channel on data 
                while (reader2.BaseStream.Position < reader2.BaseStream.Length)
                {
                    //Read event
                    byte eventb = reader2.ReadByte();
                    var eventId = (Enums.Event)eventb;
                    //Control the on channel
                    if (eventId == Enums.Event.WordNewChan)
                    {
                        var data3 = reader2.ReadUInt16();
                        //reader2.BaseStream.Position = reader2.BaseStream.Position - 2;
                        Channel _curChannel = _project.Channels[data3];
                        if (_curChannel == act_channel) { chan_on = true; }
                        else { chan_on = false; }
                       
                    }
                    else
                    {
                        if (eventId == Enums.Event.DataPlayListItems) { chan_on = false; }

                        // read data
                        Byte[] data;
                        if (eventId < Enums.Event.Word) { data = reader2.ReadBytes(1); }
                        else if (eventId < Enums.Event.Int) { data = reader2.ReadBytes(2); }
                        else if (eventId < Enums.Event.Text) { data = reader2.ReadBytes(4); }
                        else
                        {
                            var dataLen = GetBufferLen(reader2);
                            data = reader2.ReadBytes(dataLen);
                          
                        }
                       
                        if (chan_on && Cancel_list.Contains(eventId)) 
                        {
                            if (Copy_dict.ContainsKey(eventId))
                            {

                                System.Collections.Generic.List<byte[]> a_list = Copy_dict[eventId];
                                a_list.Add(data);
                                Copy_dict[eventId] = a_list;
                            }
                            else 
                            {
                                System.Collections.Generic.List<byte[]> a_list = new List<byte[]>();
                                a_list.Add(data);
                                Copy_dict.Add(eventId, a_list); 
                            }
                        } 
                    }
                }
                reader2.Close();
                streamread2.Close();
                var dataLen2 = 0;
                // Parse the template file and copy it with channel in imputed
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {   

                    //Read event
                    byte eventb = reader.ReadByte();
                    var eventId = (Enums.Event)eventb;
                    writer.Write(eventb);
                  
                    //writer.Write("ID  " + eventId);

                    // read
                    byte[] data;
                    if (eventId < Enums.Event.Word) { data = reader.ReadBytes(1); }
                    else if (eventId < Enums.Event.Int) { data = reader.ReadBytes(2); }
                    else if (eventId < Enums.Event.Text) { data = reader.ReadBytes(4); }
                    else
                    {
                        dataLen2 = GetBufferLen(reader);
                        data = reader.ReadBytes(dataLen2);
                        

                    }
                    if (eventId >= Enums.Event.Text && !Copy_dict.Keys.Contains(eventId)) { Write7BitEncodedInt(dataLen2, writer); }
                    if (Copy_dict.Keys.Contains(eventId)) 
                    {
                        System.Collections.Generic.List<byte[]> a_list = Copy_dict[eventId];
                        byte[] writing_data = a_list[0];
                        a_list.RemoveRange(0, 1);

                        if (eventId >= Enums.Event.Text) { Write7BitEncodedInt(writing_data.Length, writer); }
                        writer.Write(writing_data); 
                    }
                    else { writer.Write(data); }
                    
                }
                final_len = writer.BaseStream.Position;
                reader.Close();
                writer.Close();
                streamread.Close();
                streamwrite.Close();
                if (second_pass) { break; }
                second_pass = true;
            }
        }
    }
}
