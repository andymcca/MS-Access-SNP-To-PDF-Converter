using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace GraphicalVersion
{
    /// <summary>
    /// Author: Andrew McCarthy
    /// Class that attempts to repair broken Snapshot files prior to conversion, specifically 
    /// those created using Access 2007 onwards
    /// </summary>   
	public static class RepairSnapshot
    {

        public static bool ProcessFile(string ucSnapshot)
        {
            //  Specify expected dmSpecVersion here.
            byte[] dmSpecVersion = null;
            dmSpecVersion = new byte[2] { 1, 4 };

            // Define returnedVersion String
            byte[] returnedVersion = null;
            returnedVersion = new byte[2];

            returnedVersion = ReadHeader(ucSnapshot, 52, 2);

            //if (returnedVersion != dmSpecVersion)
            if (ByteArrayCompare(returnedVersion, dmSpecVersion))
            {
                //  File looks OK.  No action required.
                return true;
            }

            // Unexpected dmSpecVersion at offset 0x34
            // Check 'wrong' offset for dmSpecVersion
            returnedVersion = ReadHeader(ucSnapshot, 84, 2);

            if (!ByteArrayCompare(returnedVersion, dmSpecVersion))
            {
                //  Unexpected dmSpecVersion at offset 0x54!
                //  File might be corrupted or not an SNP.
                return false;
            }

            //  Found dmSpecVersion at offset 0x54 - we can attempt a repair
            //  Read in 200 bytes starting at offset 0x54
            byte[] byteHeader = ReadHeader(ucSnapshot, 84, 200);

            // Write the array back to the file at offset 0x34
            WriteHeader(ucSnapshot, 52, byteHeader);

            // Finished.  Return successfully.
            return true;
        }

        private static Byte[] ReadHeader(string tempfile, int startOffset, int readLength)
        {

            using (BinaryReader fs = new BinaryReader(File.Open(tempfile, FileMode.Open)))
            {
                //Console.Write("File Size: ");
                //Console.Write(fs.BaseStream.Length);
                //Console.WriteLine(" bytes");
                //Console.WriteLine("");

                // Counter to be used in loop and to assign bytes to array
                int b_arr = 0;

                // Set offset to be start of the HEADER stream
                int fileLength = (int)fs.BaseStream.Length;
                int headerOffset = fileLength - 512;

                // Set specified offset
                int specOffset = headerOffset + startOffset;

                // Set up reader
                Byte[] readValue = null;
                readValue = new Byte[readLength];

                fs.BaseStream.Seek(specOffset, SeekOrigin.Begin);
                while (readLength > b_arr)
                {
                    //Console.WriteLine("OK");
                    readValue[b_arr] = fs.ReadByte();
                    b_arr++;
                }

                fs.Close();
                return readValue;
            }

        }

        private static bool WriteHeader(string tempfile, int startOffset, byte[] writeArr)
        {

            using (BinaryReader fs = new BinaryReader(File.Open(tempfile, FileMode.Open)))
            {
                //Console.Write("File Size: ");
                //Console.Write(fs.BaseStream.Length);
                //Console.WriteLine(" bytes");
                //Console.WriteLine("");

                // Counter to be used in loop and to assign bytes to array
                int b_arr = 0;

                // Set offset to be start of the HEADER stream
                int fileLength = (int)fs.BaseStream.Length;
                int headerOffset = fileLength - 512;
                int writeLength = writeArr.Length;

                // Set specified offset
                int specOffset = headerOffset + startOffset;

                fs.BaseStream.Seek(specOffset, SeekOrigin.Begin);
                while (writeLength > b_arr)
                {
                    fs.BaseStream.WriteByte(writeArr[b_arr]);
                    b_arr++;
                }

                fs.Close();
                return true;
            }

        }

        private static bool ByteArrayCompare(byte[] a1, byte[] a2)
        {
            if (a1.Length != a2.Length)
            {
                // Console.WriteLine("Length Mismatch");
                return false;
            }
            for (int i = 0; i < a1.Length; i++)
                if (a1[i] != a2[i])
                {
                    // Console.WriteLine("Value Mismatch");
                    return false;
                }
            return true;
        }
    }
    
}
