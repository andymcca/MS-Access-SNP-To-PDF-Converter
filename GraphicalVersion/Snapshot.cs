using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;

namespace GraphicalVersion
{

    /// <summary>
    /// Original Author: Jervis Muindi
    /// Class that deals with conversion of snapshot files to PDFs. 
    /// Additional code to workaround PDF conversion security issue and fix broken SNP files by Andrew McCarthy
    /// </summary>    
    public static class Snapshot
    {
        /// <summary>
        ///  Converts the SNP file in the given directory
        /// </summary>
        /// <param name="directory">The given directory. It should have a "\" a the end or otherwise directory won't be found</param>
        /// <param name="compressedSnapshotName"></param>
        /// <returns></returns>
        public static bool convertToPDF(string directory, string compressedSnapshotName)
        {
            //Check that directory exists
            if (!Directory.Exists(directory))
            {
                Console.WriteLine("Directory does not exist - " + directory);
                return false;

            }
            else // the directory exists 
            {
                Directory.SetCurrentDirectory(directory);
            }

            // Ensure that the file exists
            if (!File.Exists(Path.Combine(directory, compressedSnapshotName)))
            {

                Console.WriteLine("The following file does not exist : " + Path.Combine(directory, compressedSnapshotName));
                return false;
            }

            string compressedFilepath = Path.Combine(directory, compressedSnapshotName);

            string uncompressedSnapshot = getUncompressedSnapshot(compressedFilepath);  // get the full path of the uncompressed snapshot file

            // Check and repair uncompressed Snapshot if necessary prior to conversion
            bool result = RepairSnapshot.ProcessFile(uncompressedSnapshot);

            // COULD DO - check previous result before moving on to conversion

            // Convert uncompressed Snapshot to PDF
            result = convertToPDF(uncompressedSnapshot);

            // remove previous temp file if it exists already. 
            if (File.Exists(uncompressedSnapshot))
            {
                FileInfo fi = new FileInfo(uncompressedSnapshot);
                fi.Delete();
                //Console.WriteLine("Temp File was deleted successfully"); // debuggin
            }

            return result;
        }


        public static string getUncompressedSnapshot(string compressedSnapshot)
        {

            if (!File.Exists(compressedSnapshot))
            {
                return null;
            }

            string dir = Path.GetDirectoryName(compressedSnapshot);
            string tempName = Path.GetFileNameWithoutExtension(compressedSnapshot) + ".tmp";
            string tempPath = Path.Combine(dir, tempName);


            SetupDecompressOrCopyFile(compressedSnapshot, tempPath, 0);

            //Console.WriteLine("temp path is " + tempPath); // debugging

            return tempPath;

        }

        public static bool convertToPDF(string uncompressedSnapshotName)
        {
            string outputPDF = Path.GetFileNameWithoutExtension(uncompressedSnapshotName) + ".pdf";

            /*
             * The following string contains a path to a PDF file which doesn't exist
             * This used in the MergePDFDocuments function below
             */
            string dummyPDF = Path.GetFileNameWithoutExtension(uncompressedSnapshotName) + "_dummy.pdf";

            bool r = false;
            try
            {
                r = ConvertUncompressedSnapshot(uncompressedSnapshotName, outputPDF, 0, "", "", 0, 0, 0);
                /*
                 * ConvertUncompressedSnapshot function will produce a Secured PDF with random restrictions
                 * 
                 * This is a known issue.  A workaround is to call the MergePDFDocuments function using a
                 * PDF that doesn't exist, as this will remove the security from the created PDF
                 * 
                 */
                r = MergePDFDocuments(outputPDF, dummyPDF);
            }
            catch (DllNotFoundException)
            {
                Console.WriteLine("\nA required DLL was not found. Please make sure that the" +
                                  "dynapdf.dll and StrStorage.dll both exist and are in the same folder as this executable and try again\n");
                Environment.Exit(-1);
            }

            return r;
        }



        [DllImport("setupapi.dll")]
        public static extern int SetupDecompressOrCopyFile(string SourFileName, string TargetFileName, uint CompressionType);


        /*
         * In .Net 4.0 Calling the external library causes a stack imbalance error even though
         * the specified parameters are correct for the unmanaged code function that we're calling. 
         * 
         * Use .Net 3.5 to get around this issue since in that runtime version, the framework will 
         * auto-correct and resolve this imbalance error. 
         * 
         * See: http://codenition.blogspot.com/2010/05/pinvokestackimbalance-in-net-40i-beg.html
         * 
         */
        //[DllImport("StrStorage.dll", CallingConvention = CallingConvention.Cdecl)]
        [DllImport("StrStorage.dll")]
        public static extern bool ConvertUncompressedSnapshot(String uncompressedSnapshotName,
                                                               String outputPDF,
                                                               long compressionLevel,
                                                               String passwordOpen,
                                                               String passwordOwner,
                                                               long passwordRestrictions,
                                                               long PDFNoFontEmbedding,
                                                               long PDFUnicodeFlags);
        
        [DllImport("StrStorage.dll")]
        public static extern bool MergePDFDocuments(String firstPDF, String secondPDF);

    }
}
