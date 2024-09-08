using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Emit;
using System.Xml.Linq;

namespace BotSharp.Abstraction.Utilities;

public class ZipHelper
{
    /// <summary>
    /// Number of cached bytes
    /// </summary>
    private const int BufferSize = 4096;

    /// <summary>
    /// Compress the minimum grade
    /// </summary>
    public const int CompressionLevelMin = 0;

    /// <summary>
    /// Compression maximum grade
    /// </summary>
    public const int CompressionLevelMax = 9;

    /// <summary>
    /// Gets all file system objects
    /// </summary>
    /// <param name = "source" > source path</param>
    /// <param name = "topDirectory" > the top-level folder</param>
    /// <returns>In the dictionary, the Key is the full path, and the Value is the name of the file (folder).</returns>
    private static Dictionary<string, string> GetAllFileSystemEntities(string source, string topDirectory)
    {
        Dictionary<string, string> entitiesDictionary = new Dictionary<string, string>();
        entitiesDictionary.Add(source, source.Replace(topDirectory, ""));

        if (Directory.Exists(source))
        {
            //一次性获取下级所有目录，避免递归
            string[] directories = Directory.GetDirectories(source, "*.*", SearchOption.AllDirectories);
            foreach (string directory in directories)
            {
                entitiesDictionary.Add(directory, directory.Replace(topDirectory, ""));
            }

            string[] files = Directory.GetFiles(source, "*.*", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                entitiesDictionary.Add(file, file.Replace(topDirectory, ""));
            }
        }

        return entitiesDictionary;
    }

    /// <summary>
    /// Check the compression level
    /// </summary>
    /// <param name="compressionLevel"></param>
    /// <returns></returns>
    private static int CheckCompressionLevel(int compressionLevel)
    {
        compressionLevel = compressionLevel < CompressionLevelMin ? CompressionLevelMin : compressionLevel;
        compressionLevel = compressionLevel > CompressionLevelMax ? CompressionLevelMax : compressionLevel;
        return compressionLevel;
    }
 
    /// <summary>
    /// Compress byte arrays
    /// </summary>
    /// <param name="sourceBytes" > array of source bytes</param>
    /// <param name = "compressionLevel" > compression level</param>
    /// <param name = "password" > password </ param >
    /// < returns > A compressed byte array</returns>
    public static byte[] CompressBytes(byte[] sourceBytes, string password = null, int compressionLevel = 6)
    {
        byte[] result = new byte[] { };

        if (sourceBytes.Length > 0)
        {
            try
            {
                using (MemoryStream tempStream = new MemoryStream())
                {
                    using (MemoryStream readStream = new MemoryStream(sourceBytes))
                    {
                        using (ZipOutputStream zipStream = new ZipOutputStream(tempStream))
                        {
                            zipStream.Password = password;
                            zipStream.SetLevel(CheckCompressionLevel(compressionLevel)); 

                            ZipEntry zipEntry = new ZipEntry("ZipBytes");
                            zipEntry.DateTime = DateTime.Now;
                            zipEntry.Size = sourceBytes.Length;
                            zipStream.PutNextEntry(zipEntry);
                            int readLength = 0;
                            byte[] buffer = new byte[BufferSize];

                            do
                            {
                                readLength = readStream.Read(buffer, 0, BufferSize);
                                zipStream.Write(buffer, 0, readLength);
                            } while (readLength == BufferSize);

                            readStream.Close();
                            zipStream.Flush();
                            zipStream.Finish();
                            result = tempStream.ToArray();
                            zipStream.Close();
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                throw new Exception("An error occurred with the compression of the byte array", ex);
            }
        }

        return result;
    }

    /// <summary>
    /// Unzip the byte array
    /// </summary>
    /// <param name="sourceBytes" > array of source bytes</param>
    /// <param name = "password" > password </ param >
    /// < returns > An array of bytes after extraction</returns>
    public static byte[] DecompressBytes(byte[] sourceBytes, string password = null)
    {
        byte[] result = new byte[] { };

        if (sourceBytes.Length > 0)
        {
            try
            {
                using (MemoryStream tempStream = new MemoryStream(sourceBytes))
                {
                    using (MemoryStream writeStream = new MemoryStream())
                    {
                        using (ZipInputStream zipStream = new ZipInputStream(tempStream))
                        {
                            zipStream.Password = password;
                            ZipEntry zipEntry = zipStream.GetNextEntry();

                            if (zipEntry != null)
                            {
                                byte[] buffer = new byte[BufferSize];
                                int readLength = 0;

                                do
                                {
                                    readLength = zipStream.Read(buffer, 0, BufferSize);
                                    writeStream.Write(buffer, 0, readLength);
                                } while (readLength == BufferSize);

                                writeStream.Flush();
                                result = writeStream.ToArray();
                                writeStream.Close();
                            }
                            zipStream.Close();
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                throw new Exception("An error occurred in decompressing the byte array", ex);
            }
        }
        return result;
    }


    /// <summary>
    /// Prepare file system objects for compression
    /// </summary>
    /// <param name="sourceFileEntityPathList"></param>
    /// <returns></returns>
    private static Dictionary<string, string> PrepareFileSystementities(IEnumerable<string> sourceFileEntityPathList)
    {
        Dictionary<string, string> fileEntityDictionary = new Dictionary<string, string>();//文件字典
        string parentDirectoryPath = "";
        foreach (string fileEntityPath in sourceFileEntityPathList)
        {
            string path = fileEntityPath;
          
            if (path.EndsWith(@"\"))
            {
                path = path.Remove(path.LastIndexOf(@"\"));
            }

            parentDirectoryPath = Path.GetDirectoryName(path) + @"\";

            if (parentDirectoryPath.EndsWith(@":\\")) 
            {
                parentDirectoryPath = parentDirectoryPath.Replace(@"\\", @"\");
            }
 
            Dictionary<string, string> subDictionary = GetAllFileSystemEntities(path, parentDirectoryPath);
 
            foreach (string key in subDictionary.Keys)
            {
                if (!fileEntityDictionary.ContainsKey(key)) 
                {
                    fileEntityDictionary.Add(key, subDictionary[key]);
                }
            }
        }
        return fileEntityDictionary;
    }

    /// <summary>
    /// Compress a single file/folder
    /// </summary>
    /// <param name = "sourceList" > list of source file/folder paths</param>
    /// <param name = "zipFilePath" > the path to the compressed file</param>
    /// <param name = "comment" > comment information</param>
    /// <param name = "password" > compressed password</param>
    /// <param name = "compressionLevel" > compression level, ranging from 0 to 9, optional, default is 6</param>
    /// <returns></returns>
    public static bool CompressFile(string path, string zipFilePath,
        string comment = null, string password = null, int compressionLevel = 6)
    {
        return CompressFile(new string[] { path }, zipFilePath, comment, password, compressionLevel);
    }

    /// <summary>
    /// Compress a single file/folder
    /// </summary>
    /// <param name = "sourceList" > list of source file/folder paths</param>
    /// <param name = "zipFilePath" > the path to the compressed file</param>
    /// <param name = "comment" > comment information</param>
    /// <param name = "password" > compressed password</param>
    /// <param name = "compressionLevel" > compression level, ranging from 0 to 9, optional, default is 6</param>
    /// <returns></returns>
    public static bool CompressFile(IEnumerable<string> sourceList, string zipFilePath,
         string comment = null, string password = null, int compressionLevel = 6)
    {
        bool result = false;

        try
        {
            string zipFileDirectory = Path.GetDirectoryName(zipFilePath);
            if (!Directory.Exists(zipFileDirectory))
            {
                Directory.CreateDirectory(zipFileDirectory);
            }

            Dictionary<string, string> dictionaryList = PrepareFileSystementities(sourceList);

            using (ZipOutputStream zipStream = new ZipOutputStream(File.Create(zipFilePath)))
            {
                zipStream.Password = password; 
                zipStream.SetComment(comment); 
                zipStream.SetLevel(CheckCompressionLevel(compressionLevel)); 

                foreach (string key in dictionaryList.Keys) 
                {
                    if (File.Exists(key)) 
                    {
                        FileInfo fileItem = new FileInfo(key);

                        using (FileStream readStream = fileItem.Open(FileMode.Open,
                            FileAccess.Read, FileShare.Read))
                        {
                            ZipEntry zipEntry = new ZipEntry(dictionaryList[key]);
                            zipEntry.DateTime = fileItem.LastWriteTime;
                            zipEntry.Size = readStream.Length;
                            zipStream.PutNextEntry(zipEntry);
                            int readLength = 0;
                            byte[] buffer = new byte[BufferSize];

                            do
                            {
                                readLength = readStream.Read(buffer, 0, BufferSize);
                                zipStream.Write(buffer, 0, readLength);
                            } while (readLength == BufferSize);

                            readStream.Close();
                        }
                    }
                    else
                    {
                        ZipEntry zipEntry = new ZipEntry(dictionaryList[key] + "/");
                        zipStream.PutNextEntry(zipEntry);
                    }
                }

                zipStream.Flush();
                zipStream.Finish();
                zipStream.Close();
            }

            result = true;
        }
        catch (System.Exception ex)
        {
            throw new Exception("Failed to compress the file", ex);
        }

        return result;
    }

    /// <summary>
    /// Extract the file to the specified folder
    /// </summary>
    /// <param name="sourceFile" > compressed file</param>
    /// <param name = "destinationDirectory" > the destination folder, if empty, extract it to the current folder</param>
    /// <param name = "password" > password </ param >
    /// <returns></returns>
    public static bool DecomparessFile(string sourceFile, string destinationDirectory = null, string password = null)
    {
        bool result = false;

        if (!File.Exists(sourceFile))
        {
            throw new FileNotFoundException("The file to be extracted does not exist", sourceFile);
        }

        if (string.IsNullOrWhiteSpace(destinationDirectory))
        {
            destinationDirectory = Path.GetDirectoryName(sourceFile);
        }

        try
        {
            if (!Directory.Exists(destinationDirectory))
            {
                Directory.CreateDirectory(destinationDirectory);
            }

            using (ZipInputStream zipStream = new ZipInputStream(File.Open(sourceFile, FileMode.Open,
                FileAccess.Read, FileShare.Read)))
            {
                zipStream.Password = password;
                ZipEntry zipEntry = zipStream.GetNextEntry();

                while (zipEntry != null)
                {
                    if (zipEntry.IsDirectory) 
                    {
                        Directory.CreateDirectory(Path.Combine(destinationDirectory,
                            Path.GetDirectoryName(zipEntry.Name)));
                    }
                    else
                    {
                        string fileName = Path.GetFileName(zipEntry.Name);
                        if (!string.IsNullOrEmpty(fileName) && fileName.Trim().Length > 0)
                        {
                            FileInfo fileItem = new FileInfo(Path.Combine(destinationDirectory, zipEntry.Name));
                            if (fileItem.Directory is null)
                                continue;
                            if (!Directory.Exists(fileItem.Directory.FullName))
                                Directory.CreateDirectory(fileItem.Directory.FullName);
                            using (FileStream writeStream = fileItem.Create())
                            {
                                byte[] buffer = new byte[BufferSize];
                                int readLength = 0;

                                do
                                {
                                    readLength = zipStream.Read(buffer, 0, BufferSize);
                                    writeStream.Write(buffer, 0, readLength);
                                } while (readLength == BufferSize);

                                writeStream.Flush();
                                writeStream.Close();
                            }
                            fileItem.LastWriteTime = zipEntry.DateTime;
                        }
                    }
                    zipEntry = zipStream.GetNextEntry(); 
                }

                zipStream.Close();
            }
            result = true;
        }
        catch (System.Exception ex)
        {
            throw new Exception("An error occurred in the extraction of the file", ex);
        }

        return result;
    }

    public static bool FastDecomparessFile(string sourceFile, string destinationDirectory = null)
    {
        bool result = false;

        if (!File.Exists(sourceFile))
        {
            throw new FileNotFoundException("An error occurred in the extraction of the file", sourceFile);
        }

        if (string.IsNullOrWhiteSpace(destinationDirectory))
        {
            destinationDirectory = Path.GetDirectoryName(sourceFile);
        }

        try
        {
            if (!Directory.Exists(destinationDirectory))
            {
                Directory.CreateDirectory(destinationDirectory);
            }

            FastZip fastZip = new FastZip();
            string fileFilter = null;

            fastZip.ExtractZip(sourceFile, destinationDirectory, fileFilter);

            result = true;
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred in the extraction of the file", ex);
        }

        return result;
    } 
}
