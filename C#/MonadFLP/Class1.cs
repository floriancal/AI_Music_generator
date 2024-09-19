using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace Monad.FLParser
{
    public class Project_spy
    {

        public Project_spy(string template_path, string path, Project _project, bool sampling_mode, int pat_added, int master_counter)
        {
            // Create reader and writer
            var streamread = File.Open(template_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var streamwrite = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);

            var reader = new BinaryReader(streamread);
            var writer = new BinaryWriter(streamwrite);

            writer.Write("SPY VERSION");
            byte[] head_byte = reader.ReadBytes(4);
            writer.Write(head_byte);

            byte[] byte_header_length = reader.ReadBytes(4);
            writer.Write(byte_header_length);

            var type = reader.ReadInt16();
            writer.Write(type);


            var channelCount = reader.ReadInt16();
            writer.Write(channelCount);



            writer.Write(reader.ReadInt16());

            string id;
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
                                writer.Write(len);
                                break;
                            }
                        }
                    }
                }
            }

            //Header is parsed now the data block is parsed with differents method depending on the event readed
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                byte eventb = reader.ReadByte();
                var eventId = (Enums.Event)eventb;
                if (eventId != Enums.Event.DataPatternNotes)
                {
                    writer.Write(eventb);
                    writer.Write("THIS IS A FUCKING EVENT ID");
                    writer.Write(((int)eventId).ToString());
                }
                //Console.WriteLine("ID" + eventId);
                if (eventId < Enums.Event.Word) MyParseByteEvent(eventId, reader, writer, _project);
                else if (eventId < Enums.Event.Int) MyParseWordEvent(eventId, reader, writer, _project, sampling_mode, pat_added);
                else if (eventId < Enums.Event.Text) MyParseDwordEvent(eventId, reader, writer, _project);
                else if (eventId < Enums.Event.Data) MyParseTextEvent(eventId, reader, writer, _project);
                else MyParseDataEvent(eventId, reader, writer, _project, sampling_mode, pat_added,  eventb, master_counter);
            }

        }
        // Parse a byte event 
        private void MyParseByteEvent(Enums.Event eventId, BinaryReader reader, BinaryWriter writer, Project _project)
        {
            byte data = reader.ReadByte();
            writer.Write(data);

        }

        // Parse a word event 
        private void MyParseWordEvent(Enums.Event eventId, BinaryReader reader, BinaryWriter writer, Project _project, bool sampling_mode, int pat_added)
        {
            var data = reader.ReadUInt16();
            writer.Write(data);

        }

        // Parse a dword event
        private void MyParseDwordEvent(Enums.Event eventId, BinaryReader reader, BinaryWriter writer, Project _project)
        {
            var data = reader.ReadUInt32();

            writer.Write(data);

        }

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
            Write7BitEncodedInt(dataLen, writer);
            writer.Write(dataBytes);
        }

        //Parse a data event 
        private void MyParseDataEvent(Enums.Event eventId, BinaryReader reader, BinaryWriter writer, Project project, bool sampling_mode, int pat_added, byte eventb, int master_counter)
        {
            var dataLen = GetBufferLen(reader);
            var dataStart = reader.BaseStream.Position;
            var dataEnd = dataStart + dataLen;

            var dataBytes_def = reader.ReadBytes(dataLen);
            //Console.WriteLine(dataBytes_def.ToString());
            Write7BitEncodedInt(dataLen, writer);
            writer.Write(dataBytes_def);
        }
    }
}

