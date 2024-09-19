using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Monad.FLParser
{
    public class ProjectWriter
    {
        // Needed methods for the following
        private static int GetBufferLen(BinaryReader reader)
        {
            var data = reader.ReadByte();
            var dataLen = data & 0x7F;
            var shift = 0;
            while ((data & 0x80) != 0)
            {
                data = reader.ReadByte();
                dataLen = dataLen | ((data & 0x7F) << (shift += 7));
            }
            return dataLen;
        }

        // write a flp file with data passed to the function, this used a reference or template flp file and modify data passed in it so this is a reading copying and modifying only necessary stuff function 

        //Needed stuffs
        private Pattern _curPattern;
        private Channel _curChannel;
        private Boolean chan_on = false;
        private Insert _curInsert;
        private InsertSlot _curSlot;
        private readonly bool _verbose;
        private int _versionMajor;
        public int number_of_files;
        public string active_slot_name;
        public byte[] byte_last;

        public ProjectWriter(bool verbose)
        {
            _verbose = verbose;
        }
        static bool ByteArrayCompare(byte[] a1, byte[] a2)
        {
            for (int i = 0; i < a1.Length; i++)
            {
                if (a1[i] != a2[i]) { Console.WriteLine("diff at pos :" + i); }
            }
            return true;
        }

        // This counter is used all over the file to compute the size difference between the readed and the writted file
        public int diff_counter = 0;

        public ProjectWriter(string template_path, string path, Project _project, bool sampling_mode, int pat_added, int master_counter, bool degraded_mode)
        {
        
            List<Pattern> Patterns = _project.Patterns;
            List<Channel> chan = Patterns[master_counter].Notes.Keys.ToList();
            Channel act_channel = chan[0];

                Enums.Event[] Cancel_list = {Enums.Event.WordNewChan, Enums.Event.ByteChanType, Enums.Event.GeneratorName, Enums.Event.DataNewPlugin, Enums.Event.TextPluginName, Enums.Event.ForChan6, Enums.Event.DWordColor, Enums.Event.Byte,
                Enums.Event.TextDelay, Enums.Event.DWordDelayReso, Enums.Event.DWordReverb, Enums.Event.WordShiftDelay, Enums.Event.ForChan2, Enums.Event.WordFx, Enums.Event.WordFx3, Enums.Event.WordCutOff,Enums.Event.WordResonance, Enums.Event.WordPreAmp,
                Enums.Event.WordDecay, Enums.Event.WordAttack, Enums.Event.WordStDel, Enums.Event.DWordFxSine, Enums.Event.WordFadeStereo, Enums.Event.ByteMixSliceNum, Enums.Event.DataBasicChanParams, Enums.Event.ForChan9, Enums.Event.ForChan7,
                Enums.Event.DataChanParams, Enums.Event.DWordCutCutBy, Enums.Event.ForChan4, Enums.Event.ForChan5, Enums.Event.DataAutomationData, Enums.Event.ForChan1, Enums.Event.ForChan8,
                Enums.Event.DataEnvLfoParams, Enums.Event.ForChan3, Enums.Event.ByteLoopType, Enums.Event.TextSampleFileName, Enums.Event.DataPluginParams, Enums.Event.ForChan11, Enums.Event.ForChan12 };

            //
            // We will do the parsing two times, one for evaluating the difference in files and the second to create the new file with modif applied
            bool second_pass = false;
            while (true)
            {
                // create a file for further exploit
                string track_channel_fp = Path.Combine(Path.GetDirectoryName(template_path), "track_channel_sampling.txt");

                // Create reader and writer
                var streamread = File.Open(template_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                var streamwrite = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);

                var reader = new BinaryReader(streamread);
                var writer = new BinaryWriter(streamwrite);

                // Parse head and modify if necessary 
                byte[] head_byte = reader.ReadBytes(4);
                if (Encoding.ASCII.GetString(head_byte) != "FLhd")
                    throw new FlParseException("Invalid magic number", reader.BaseStream.Position);
                writer.Write(head_byte);

                // header + type
                byte[] byte_header_length = reader.ReadBytes(4);
                var headerLength = BitConverter.ToInt32(byte_header_length, 0);
                if (headerLength != 6)
                    throw new FlParseException($"Expected header length 6, not {headerLength}", reader.BaseStream.Position);
                byte[] byte_write_header_length = BitConverter.GetBytes((UInt32)headerLength);

                writer.Write(byte_write_header_length);

                var type = reader.ReadInt16();
                if (type != 0) throw new FlParseException($"Type {type} is not supported", reader.BaseStream.Position);
                writer.Write(type);

                // channels
                var channelCount = reader.ReadInt16();
                if (channelCount < 1 || channelCount > 1000)
                    throw new FlParseException($"Invalid number of channels: {channelCount}", reader.BaseStream.Position);
                writer.Write(channelCount);

                // ppq
                UInt16 Ppq = (UInt16)reader.ReadInt16();
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
                    writer.Write(one_byte);
                    if (Encoding.ASCII.GetString(one_byte) == "F")
                    {
                        byte[] sec_byte = reader.ReadBytes(1);
                        writer.Write(sec_byte);
                        if (Encoding.ASCII.GetString(sec_byte) == "L")
                        {
                            byte[] third_byte = reader.ReadBytes(1);
                            writer.Write(third_byte);
                            if (Encoding.ASCII.GetString(third_byte) == "d")
                            {
                                byte[] fourth_byte = reader.ReadBytes(1);
                                writer.Write(fourth_byte);
                                if (Encoding.ASCII.GetString(fourth_byte) == "t")
                                {
                                    len = reader.ReadInt32();
                                    writer.Write(len + diff_counter);
                                    break;
                                }
                            }
                        }
                    }
                }
                Console.WriteLine("START");
                //Header is parsed now the data block is parsed with differents method depending on the event readed
                Boolean clean = false;
                
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    Boolean go_for_write = true;
                    byte eventb = reader.ReadByte();
                    var eventId = (Enums.Event)eventb;
                    Console.WriteLine(eventId);
                    
                    if (eventId != Enums.Event.DataPatternNotes && !sampling_mode) { writer.Write(eventb); }                 
                    if (eventId == Enums.Event.DataPlayListItems) { clean = false; }
                    if (eventId == Enums.Event.WordNewPat) { clean = false; }
                    if (eventId == Enums.Event.WordNewChan) { clean = true; }

                    if (chan_on) { Console.WriteLine(chan_on); }
                    if (sampling_mode && eventId == Enums.Event.WordNewChan)
                    {
                        var data = reader.ReadUInt16();
                        reader.BaseStream.Position = reader.BaseStream.Position - 2;
                        _curChannel = _project.Channels[data];
                        if (_curChannel == act_channel) { chan_on = true; }
                        else { chan_on = false; }
                    }

                    if (clean)
                    {// Active or unactive channel if unactive data will not be written
                        if (sampling_mode && chan_on == true) { if (eventId != Enums.Event.DataPatternNotes) { writer.Write(eventb); } }
                        else if (sampling_mode)
                        {
                            if (Cancel_list.Contains(eventId))
                            {
                                if (eventId < Enums.Event.Word) { byte data = reader.ReadByte(); diff_counter -= 1; }
                                else if (eventId < Enums.Event.Int) { var data = reader.ReadUInt16(); diff_counter -= 2; }
                                else if (eventId < Enums.Event.Text) { var data = reader.ReadUInt32(); diff_counter -= 4; }
                                else
                                {
                                    var dataLen = GetBufferLen(reader);
                                    var dataBytes = reader.ReadBytes(dataLen);
                                    diff_counter -= dataLen;
                                    if (dataLen < 128) { diff_counter -= 1; }
                                    if (dataLen > 128 && dataLen < 16384) { diff_counter -= 2; }
                                    if (dataLen > 16384 && dataLen < 2097152) { diff_counter -= 3; }
                                }
                                go_for_write = false;
                            }
                        }
                    }
                    if (go_for_write)
                    {
                        //Console.WriteLine(eventId + "WRITTEN");
                        if (eventId < Enums.Event.Word) MyParseByteEvent(eventId, reader, writer, _project);
                        else if (eventId < Enums.Event.Int) MyParseWordEvent(eventId, reader, writer, _project, sampling_mode, pat_added);
                        else if (eventId < Enums.Event.Text) MyParseDwordEvent(eventId, reader, writer, _project);
                        else if (eventId < Enums.Event.Data) MyParseTextEvent(eventId, reader, writer, _project);
                        else MyParseDataEvent(eventId, reader, writer, _project, sampling_mode, pat_added, track_channel_fp, eventb, master_counter, degraded_mode);
                    }
                }
                Console.WriteLine("FINITO");
                writer.Close();
                reader.Close();
                streamread.Close();
                streamwrite.Close();

                // Create reader and writer
                var readtemp = File.Open(template_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                var readnew = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                var reader2 = new BinaryReader(readtemp);
                var writer2 = new BinaryReader(readnew);
                byte[] allDatatemp = reader2.ReadBytes((int)reader2.BaseStream.Length);
                byte[] allDatanew = writer2.ReadBytes((int)writer2.BaseStream.Length);

                readtemp.Close();
                readnew.Close();
                reader2.Close();
                writer2.Close();

                // this is the two pass control
                if (second_pass is true) { break; }
                second_pass = true;
            }
        }

        // Parse a byte event 
        private void MyParseByteEvent(Enums.Event eventId, BinaryReader reader, BinaryWriter writer,  Project _project)
        {
            byte data = reader.ReadByte();
            //Console.WriteLine(data.ToString());
            writer.Write(data);
            var genData = _curChannel?.Data as GeneratorData;
        }

        // Parse a word event 
        private void MyParseWordEvent(Enums.Event eventId, BinaryReader reader, BinaryWriter writer, Project _project, bool sampling_mode, int pat_added)
        {
            var data = reader.ReadUInt16();
            var genData = _curChannel?.Data as GeneratorData;
            //Console.WriteLine(data.ToString());
            switch (eventId)
            {
                case Enums.Event.WordNewChan:
                    _curChannel = _project.Channels[data];
                    writer.Write(data);
                    break;
                case Enums.Event.WordNewPat:
                    _curPattern = _project.Patterns[data - 1];
                    writer.Write(data);
                    break;
                case Enums.Event.WordTempo:
                    if (_project.Tempo != data)
                    {
                        double new_tempo = _project.Tempo;
                        writer.Write((UInt16)new_tempo);
                    }
                    else { writer.Write(data); }
                    break;
                case Enums.Event.WordFadeStereo:
                    writer.Write(data);
                    break;
                case Enums.Event.WordPreAmp:
                    if (genData == null) { writer.Write(data); break; }
                    if (genData.SampleAmp != data)
                    {
                        UInt16 new_SampleAmp = data;
                        writer.Write(data);
                    }
                    else { writer.Write(data); }
                    break;
                case Enums.Event.WordMainPitch:
                    if (_project.MainPitch != data)
                    {
                        UInt16 new_MainPitch = data;
                        writer.Write(data);
                    }
                    else { writer.Write(data); }
                    break;
                case Enums.Event.WordInsertIcon:
                    writer.Write(data);
                    break;
                case Enums.Event.WordCurrentSlotNum:
                    if (_curSlot != null) // Current slot after plugin event, now re-arranged.
                    {
                        _curInsert.Slots[data] = _curSlot;
                        _curSlot = new InsertSlot();
                    }
                    _curChannel = null;
                    writer.Write(data);
                    break;
                default:
                    writer.Write(data);
                    break;
            }
        }

        // Parse a dword event
        private void MyParseDwordEvent(Enums.Event eventId, BinaryReader reader, BinaryWriter writer, Project _project)
        {
            var data = reader.ReadUInt32();
            switch (eventId)
            {
                case Enums.Event.DWordColor:
                    writer.Write(data);
                    break;
                case Enums.Event.DWordMiddleNote:
                    writer.Write(data);
                    break;
                case Enums.Event.DWordInsertColor:
                    writer.Write(data);
                    break;
                case Enums.Event.DWordFineTempo:
                    if (_project.Tempo != data)
                    {
                        double new_tempo = _project.Tempo;
                        writer.Write((UInt32)new_tempo * 1000);
                    }
                    else { writer.Write(data); }
                    break;
                default:
                    writer.Write(data);
                    break;
            }
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

        // Parse a text event
        private void MyParseTextEvent(Enums.Event eventId, BinaryReader reader, BinaryWriter writer, Project project)
        {
            long base_pos_before_size = reader.BaseStream.Position;
            var dataLen = GetBufferLen(reader);

            var dataBytes = reader.ReadBytes(dataLen);

            var unicodeString = Encoding.Unicode.GetString(dataBytes);
            if (unicodeString.EndsWith("\0")) unicodeString = unicodeString.Substring(0, unicodeString.Length - 1);
            //Console.WriteLine(unicodeString);
            var genData = _curChannel?.Data as GeneratorData;

            switch (eventId)
            {
                case Enums.Event.GeneratorName:
                    Write7BitEncodedInt(dataLen, writer);
                    writer.Write(dataBytes);
                    active_slot_name = unicodeString;
                    break;
                case Enums.Event.TextChanName:
                    Write7BitEncodedInt(dataLen, writer);
                    writer.Write(dataBytes);
                    break;
                case Enums.Event.TextPatName:
                    Write7BitEncodedInt(dataLen, writer);
                    writer.Write(dataBytes);
                    break;
                case Enums.Event.TextTitle:
                    Write7BitEncodedInt(dataLen, writer);
                    writer.Write(dataBytes);
                    break;
                case Enums.Event.TextAuthor:
                    Write7BitEncodedInt(dataLen, writer);
                    writer.Write(dataBytes);
                    break;
                case Enums.Event.TextComment:
                    Write7BitEncodedInt(dataLen, writer);
                    writer.Write(dataBytes);
                    break;
                case Enums.Event.TextGenre:
                    Write7BitEncodedInt(dataLen, writer);
                    writer.Write(dataBytes);
                    break;
                case Enums.Event.TextSampleFileName:
                    if (genData == null)
                    {
                        Write7BitEncodedInt(dataLen, writer);
                        writer.Write(dataBytes);
                        break;
                    }
                    if (genData.SampleFileName != unicodeString)
                    {
                        string new_sample_path = genData.SampleFileName;
                        new_sample_path = new_sample_path + "\0";
                        byte[] bytes_sample_path = Encoding.Unicode.GetBytes(new_sample_path);
                        int data_len = bytes_sample_path.Count();
                        Write7BitEncodedInt(data_len, writer);
                        writer.Write(bytes_sample_path);
                        diff_counter += dataLen - data_len;
                    }
                    else
                    {
                        Write7BitEncodedInt(dataLen, writer);
                        writer.Write(dataBytes);
                    }
                    break;
                case Enums.Event.TextPluginName:
                    if (_curChannel != null)
                    {
                        string chan_name = _curChannel.Name;
                        byte[] byte_chan_name = Encoding.Unicode.GetBytes(chan_name);
                        int data_len = byte_chan_name.Count();
                        Write7BitEncodedInt(data_len, writer);
                        writer.Write(byte_chan_name);
                        diff_counter += dataLen - data_len;
                    }
                    else
                    {
                        Write7BitEncodedInt(dataLen, writer);
                        writer.Write(dataBytes);
                    }
                    break;
                case Enums.Event.TextVersion:
                    string version = Encoding.UTF8.GetString(dataBytes);
                    string version_to_write = version;
                    if (version.EndsWith("\0")) version = version.Substring(0, version.Length - 1);
                    var numbers = version.Split('.');
                    _versionMajor = int.Parse(numbers[0]);

                    // Bruteforce version 
                    byte[] version_byte = Encoding.UTF8.GetBytes(version + "\0");
                    int data_len2 = version_byte.Count();
                    Write7BitEncodedInt(data_len2, writer);
                    writer.Write(version_byte);
                    break;
                default:
                    Write7BitEncodedInt(dataLen, writer);
                    writer.Write(dataBytes);
                    break;
            }
        }

        //Parse a data event 
        private void MyParseDataEvent(Enums.Event eventId, BinaryReader reader, BinaryWriter writer, Project project, bool sampling_mode, int pat_added, string track_channel_fp, byte eventb, int master_counter, bool degraded_mode)
        {
            var dataLen = GetBufferLen(reader);
            var dataStart = reader.BaseStream.Position;
            var dataEnd = dataStart + dataLen;
            var genData = _curChannel?.Data as GeneratorData;
            var autData = _curChannel?.Data as AutomationData;
            var slotData = _curSlot;

            switch (eventId)
            {
                // data containing notes of given patterns 
                case Enums.Event.DataPatternNotes:
                    if (sampling_mode)
                    {
                        Int16 Unknown1 = 0;
                        byte Unknown2 = 0;
                        Int16 Unknown3 = 0;
                        byte Unknown4 = 0;
                        byte X1 = 0;
                        byte X2 = 0;

                        // read all the notes of a pattern
                        while (reader.BaseStream.Position < dataEnd)
                        {
                            var pos = reader.ReadInt32();
                            var unknown1 = reader.ReadInt16();
                            Unknown1 = unknown1;
                            var ch = reader.ReadByte();
                            var unknown2 = reader.ReadByte();
                            Unknown2 = unknown2;
                            var length = reader.ReadInt32();
                            var key = reader.ReadByte();
                            var unknown3 = reader.ReadInt16();
                            Unknown3 = unknown3;
                            var unknown4 = reader.ReadByte();
                            Unknown4 = unknown4;
                            var finePitch = reader.ReadUInt16();
                            var release = reader.ReadUInt16();
                            var pan = reader.ReadByte();
                            var velocity = reader.ReadByte();
                            var x1 = reader.ReadByte();
                            X1 = x1;
                            var x2 = reader.ReadByte();
                            X2 = x2;
                        }

                        //counting how much notes we have in our file 
                        int note_number = 0;
                        foreach (Channel channel in _curPattern.Notes.Keys)
                        {
                            List<Note> list_of_note = _curPattern.Notes[channel];
                            foreach (Note note in list_of_note) { note_number += 1; }
                        }

                        // if there is no note we won't writ anything
                        if (note_number > 0) { writer.Write(eventb); }

                        //this number give the 7bit encoding to write 
                        int byte_size = note_number * (4 + 2 + 1 + 1 + 4 + 1 + 2 + 1 + 2 + 2 + 1 + 1 + 1 + 1);

                        // If there is no note we wont write anything 
                        if (note_number > 0) { Write7BitEncodedInt(byte_size, writer); }

                        // this writes the notes 
                        foreach (Channel channel in _curPattern.Notes.Keys)
                        {
                            List<Note> list_of_note = _curPattern.Notes[channel];
                            foreach (Note note in list_of_note)
                            {
                                writer.Write((Int32)note.Position);
                                writer.Write((Int16)Unknown1);
                                writer.Write((byte)project.Channels.IndexOf(channel));
                                writer.Write((byte)Unknown2);
                                writer.Write((Int32)note.Length);
                                writer.Write((byte)note.Key);
                                writer.Write((Int16)Unknown3);
                                writer.Write((byte)Unknown4);
                                writer.Write((Int16)note.FinePitch);
                                writer.Write((Int16)note.Release);
                                writer.Write((byte)note.Pan);
                                writer.Write((byte)note.Velocity);
                                writer.Write((byte)X1);
                                writer.Write((byte)X2);
                            }
                        }

                        // Now in case of a sampling mode we will add patterns that corresponds to the split by channel equivalence 
                        // this if is valid when reaching the last normal pattern 
                        if ((_curPattern == project.Patterns[project.Patterns.Count - pat_added - 1]) & (sampling_mode is true))
                        {
                            // we do it only if there are some split by channel
                            if (pat_added > 0)
                            {
                                int number_int = project.Patterns.Count - pat_added;
                                UInt16 number = (UInt16)number_int;

                                // This is the same repeated process but for added patterns
                                foreach (Pattern pat in project.Patterns.GetRange(project.Patterns.Count - pat_added, pat_added))
                                {
                                    int note_number_add = 0;

                                    foreach (Channel channel in pat.Notes.Keys)
                                    {
                                        List<Note> list_of_note = pat.Notes[channel];
                                        foreach (Note note in list_of_note) { note_number_add += 1; }
                                    }
                                    if (note_number_add > 0)
                                    {
                                        _curPattern = project.Patterns[number];
                                        number += 1;
                                        //Pattern ID
                                        writer.Write((byte)65);
                                        // Pattern Number
                                        writer.Write(number);
                                        //Data pattern notes id
                                        writer.Write((byte)224);
                                        //len 7bit encoding
                                        byte_size = 1 * (4 + 2 + 1 + 1 + 4 + 1 + 2 + 1 + 2 + 2 + 1 + 1 + 1 + 1);
                                        Write7BitEncodedInt(byte_size, writer);
                                        foreach (Channel channel in _curPattern.Notes.Keys)
                                        {
                                            List<Note> list_of_note = _curPattern.Notes[channel];
                                            foreach (Note note in list_of_note)
                                            {
                                                //note data
                                                writer.Write((Int32)note.Position);
                                                writer.Write((Int16)Unknown1);
                                                writer.Write((byte)project.Channels.IndexOf(channel));
                                                writer.Write((byte)Unknown2);
                                                writer.Write((Int32)note.Length);
                                                writer.Write((byte)note.Key);
                                                writer.Write((Int16)Unknown3);
                                                writer.Write((byte)Unknown4);
                                                writer.Write((Int16)note.FinePitch);
                                                writer.Write((Int16)note.Release);
                                                writer.Write((byte)note.Pan);
                                                writer.Write((byte)note.Velocity);
                                                writer.Write((byte)X1);
                                                writer.Write((byte)X2);
                                            }
                                        }
                                        //byte event 65 + Uint 16 pat number + Byte event 224 + 7bit encoding + byte size
                                        diff_counter += 1 + 2 + 1 + byte_size;
                                    }
                                }
                            }
                            break;
                        }
                        diff_counter += dataLen - byte_size;
                    }
                    else
                    {
                        var dataBytes = reader.ReadBytes(dataLen);
                        writer.Write(eventb);
                        Write7BitEncodedInt(dataLen, writer);
                        writer.Write(dataBytes);
                        break;
                    }
                    break;

                case Enums.Event.DataInsertParams:
                    Write7BitEncodedInt(dataLen, writer);
                    while (reader.BaseStream.Position < dataEnd)
                    {
                        var startPos = reader.BaseStream.Position;
                        var unknown1 = reader.ReadInt32();
                        writer.Write((Int32)unknown1);
                        byte message_id_byte = reader.ReadByte();
                        var messageId = (Enums.InsertParam)message_id_byte;
                        writer.Write((byte)message_id_byte);
                        var unknown2 = reader.ReadByte();
                        writer.Write((byte)unknown2);
                        var channelData = reader.ReadUInt16();
                        writer.Write((UInt16)channelData);
                        var messageData = reader.ReadInt32();
                        //writer.Write(messageData);
                        var slotId = channelData & 0x3F;
                        var insertId = (channelData >> 6) & 0x7F;
                        var insertType = channelData >> 13;
                        var insert = project.Inserts[insertId];

                        switch (messageId)
                        {
                            case Enums.InsertParam.SlotState:
                                if (degraded_mode is true)
                                {
                                    if (active_slot_name == "Fruity Wrapper") { writer.Write((Int32)0); }
                                    else { writer.Write(messageData); }

                                }
                                else { writer.Write((Int32)messageData); }
                                break;
                            case Enums.InsertParam.Volume:
                                // if an insert volume is too low (also valid for master) we choose to set it at 100% this avoid an instr to be manually disabled
                                if ((Int32) messageData < 500) { writer.Write((Int32)(101 * 100 * 1.25)); }
                                else { writer.Write(messageData); }
                                break;
                            case Enums.InsertParam.Pan:
                                writer.Write((Int32)insert.Pan);
                                break;
                            case Enums.InsertParam.StereoSep:
                                writer.Write((Int32)insert.StereoSep);
                                break;
                            default:
                                writer.Write(messageData);
                                break;
                        }
                    }
            
                     break;

                case Enums.Event.DataPlayListItems:

                    // Goal here is to replace items in pmaylist by only one itemp foreach channelss
                    if (sampling_mode is true)
                    {
                        UInt16 Unknown_1 = 0;
                        UInt16 ItemFlags = 0;
                        UInt32 Unknown_3 = 0;

                        // Reading the data first
                        List<UInt16> pattern_list = new List<UInt16>();
                        UInt16 Pat_base_sauve = 0;
                        while (reader.BaseStream.Position < dataEnd)
                        {
                            var startTime = reader.ReadInt32();
                            var patternBase = reader.ReadUInt16();
                            Pat_base_sauve = patternBase;
                            var patternId = reader.ReadUInt16();
                            pattern_list.Add(patternId);
                            var length = reader.ReadInt32();
                            var track = reader.ReadInt32();
                            var track_s = track;
                            if (_versionMajor == 20) { track = 501 - track; }
                            else { track = 198 - track; }
                            UInt16 unknown1 = reader.ReadUInt16();
                            Unknown_1 = unknown1;
                            var itemFlags = reader.ReadUInt16();
                            ItemFlags = itemFlags;
                            var unknown3 = reader.ReadUInt32();
                            Unknown_3 = unknown3;
                            bool muted = (itemFlags & 0x2000) > 0 ? true : false;   // flag determines if item is muted

                            if (patternId <= patternBase)
                            {
                                var startOffset = reader.ReadSingle();
                                var endOffset = reader.ReadSingle();
                            }
                            else
                            {
                                var startOffset = reader.ReadInt32();
                                var endOffset = reader.ReadInt32();
                            }
                        }

                        // Pattern to write is a list containing each items present in the playlist and once only 
                        var pattern_to_write = pattern_list.Distinct().ToList();

                        // We add to this list the added pattern that we have created in the split by channel
                        int count_int = project.Patterns.Count - pat_added + 1;
                        UInt16 count = (UInt16)count_int;
                        foreach (Pattern pat in project.Patterns.GetRange(project.Patterns.Count - pat_added, pat_added)) { pattern_to_write.Add((UInt16)(count + Pat_base_sauve)); count += 1; }
                        pattern_to_write.Sort();

                        // Number to compute the diff counter
                        int pattern_byte_size = 1 * (4 + 2 + 2 + 4 + 4 + 2 + 2 + 4 + 4 + 4);

                        // VERY IMPORTANT IS USED IN MY FLP PARSER TO KNOW HOW MANY FILES TO CREATE 
                        number_of_files = pattern_to_write.Count;

                        // We begin to write playlist item, one item by file on the track its supposed to be
                        using (StreamWriter track_channel_file = new StreamWriter(track_channel_fp))
                        {
                            // Pattern to write contains all different items present in playlist
                            int counter = 0;
                            foreach (UInt16 patternID in pattern_to_write)
                            {
                                counter += 1;
                                bool ignore = false;


                                // As we want to avoid doublons this part identify auto channel and set bool ignore to tue if its the case this way track_channel_samling.txt has the info 
                                if (patternID < 1000)
                                {
                                    IChannelData Data = project.Channels[patternID].Data;
                                    GeneratorData generator = (GeneratorData)Data;
                                    if (generator.GeneratorName != "Sampler") { ignore = true; }
                                }

                                // This ensure one item is written in each file
                                if (counter == master_counter)
                                {
                                    Write7BitEncodedInt(pattern_byte_size, writer);
                                   
                                    writer.Write((Int32)0);
                                    writer.Write((UInt16)Pat_base_sauve);

                                    // this is a classic pattern both need to be treated diff cause the end data is not in the same format
                                    if (patternID > 1000)
                                    {
                                        writer.Write((UInt16)patternID);
                                        writer.Write((Int32)96);
                                        if (_versionMajor == 20) { writer.Write((Int32)(501 - counter - 1)); }
                                        else { writer.Write((Int32)(198 - counter - 1)); }
                                        writer.Write((UInt16)Unknown_1);
                                        writer.Write((UInt16)ItemFlags); //ItemFlags In sampling mode no pattern shall be muted
                                        writer.Write((UInt32)Unknown_3);
                                        writer.Write((Int32)0);
                                        writer.Write((Int32)96);
                                    }
                                    // Its a sample or auto 
                                    else
                                    {
                                        IChannelData Data = project.Channels[patternID].Data;
                                        GeneratorData generator = (GeneratorData)Data;
                                        writer.Write((UInt16)patternID);
                                        if (generator.GeneratorName == "Sampler") { writer.Write((Int32)20000); }
                                        else { writer.Write((Int32)2);  }
                                        if (_versionMajor == 20) { writer.Write((Int32)(501 - counter - 1)); }
                                        else { writer.Write((Int32)(198 - counter - 1)); }
                                        writer.Write((UInt16)Unknown_1);
                                        writer.Write((UInt16)ItemFlags); //ItemFlags In sampling mode no pattern shall be muted
                                        writer.Write((UInt32)Unknown_3);
                                        writer.Write(Convert.ToSingle(0));
                                        if (generator.GeneratorName == "Sampler") { writer.Write(Convert.ToSingle(20000)); }
                                        else { writer.Write(Convert.ToSingle(2)); }
                                    }
                                }
                                // use for file writing txt
                                int Chan_id = 0;
                                if (patternID <= Pat_base_sauve) { Chan_id = project.Channels[patternID].Id; }
                                if (patternID > Pat_base_sauve)
                                {
                                    Pattern pat_to_find_chan = project.Patterns[patternID - Pat_base_sauve - 1];
                                    Dictionary<Channel, List<Note>> Notes = pat_to_find_chan.Notes;
                                    foreach (Channel channel2 in Notes.Keys) { Chan_id = channel2.Id; }
                                }
                                int track_number = counter;
                                if (ignore == false) { track_channel_file.WriteLine(track_number + " : " + Chan_id); }
                                else { track_channel_file.WriteLine(track_number + " : " + "ignore"); }
                                
                                diff_counter += dataLen - pattern_byte_size;
                            }
                        }
                    }
                    else
                    {
                        var dataBytes2 = reader.ReadBytes(dataLen);
                        Write7BitEncodedInt(dataLen, writer);
                        writer.Write(dataBytes2);
                    }
                    break;
                case Enums.Event.track_mute:
                    var dataBytes3 = reader.ReadBytes(dataLen);
                    Write7BitEncodedInt(dataLen, writer);
                    int by_count = 0;
                    // modify byte 12 in event track mute and replace it by a 1 meaning that the track is necessarly on 
                    foreach (byte by in dataBytes3)
                    {
                        if (by_count == 12) { writer.Write((byte)1); }
                        else { writer.Write(by); }
                        by_count += 1;
                    }
                    break;
                case Enums.Event.UN1:
                    var dataBytes4 = reader.ReadBytes(dataLen);
                    Write7BitEncodedInt(dataLen, writer);
                    writer.Write(dataBytes4);
                    break;

                default:
                    var dataBytes_def = reader.ReadBytes(dataLen);
                    Write7BitEncodedInt(dataLen, writer);
                    writer.Write(dataBytes_def);
                    break;
            }
        }
    }
}

