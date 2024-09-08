using System.IO;

namespace BotSharp.Core.Plugins;

public class NupkgService
{
    /// <summary>
    /// Unzip xxx.nupkg into the corresponding plugin directory structure according to the current environment
    /// Plug-in installation package in xxx.zip form: Just unzip it
    /// </summary>
    /// <param name="sourceFile"></param>
    /// <param name="destinationDirectory"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public static bool DecomparessFile(string sourceFile, string destinationDirectory = null)
    {
        bool isDecomparessSuccess = false;

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

            destinationDirectory = destinationDirectory.Replace(".zip", "");
            isDecomparessSuccess = ZipHelper.FastDecomparessFile(sourceFile, destinationDirectory);
            if (!isDecomparessSuccess)
            {
                return isDecomparessSuccess;
            }
  
            var netVersion = Environment.Version;

            // netcoreapp3.1/net6.0/net8.0/net9.0 
            string libDirPath = Path.Combine(destinationDirectory, "lib");
            string netFolderName = string.Empty;
            if (netVersion >= new Version("3.1") && Directory.Exists(Path.Combine(libDirPath, $"netcoreapp{netVersion.Major}.{netVersion.Minor}")))
            {
                netFolderName = $"netcoreapp{netVersion.Major}.{netVersion.Minor}";
            }
            else if (netVersion.Major >= 5 && Directory.Exists(Path.Combine(libDirPath, $"net{netVersion.Major}.{netVersion.Minor}")))
            {
                netFolderName = $"net{netVersion.Major}.{netVersion.Minor}";
            }
            else if (Directory.Exists(Path.Combine(libDirPath, "netstandard2.0")))
            {
                netFolderName = $"netstandard2.0";
            }
            else if (Directory.Exists(Path.Combine(libDirPath, "netstandard2.1")))
            {
                netFolderName = $"netstandard2.1";
            }
            else
            {
                throw new Exception("Versions below .NET Core 3.1 are not supported, nor is the .NET Framework supported ");
            }
            // 1. ./lib/netcoreapp3.1
            string libNetDirPath = Path.Combine(libDirPath, netFolderName);
            FileHelper.CopyFolder(libNetDirPath, destinationDirectory);
            Directory.Delete(libDirPath, true);
            // 2. Normal files: e.g. wwwroot
            // 2.1 ./content
            string contentDirPath = Path.Combine(destinationDirectory, "content");
            bool isExistContentDir = Directory.Exists(contentDirPath);
            bool isFinishedContent = false;
            if (isExistContentDir)
            {
                DirectoryInfo dir = new DirectoryInfo(contentDirPath);
                bool isExistFile = dir.GetFiles()?.Length >= 1 || dir.GetDirectories()?.Length >= 1;
                if (isExistFile)
                {
                    FileHelper.CopyFolder(contentDirPath, destinationDirectory);
                    isFinishedContent = true; 
                }
            }
            else
            {
                // 2.2 ./contentFiles/any/netFolderName
                if (!isFinishedContent)
                {
                    string contentFilesDirPath = Path.Combine(destinationDirectory, "contentFiles");
                    DirectoryInfo dir = new DirectoryInfo(contentFilesDirPath);
                    int? childDirLength = dir.GetDirectories()?.Length;
                    if (childDirLength >= 1)
                    {
                        DirectoryInfo anyDir = dir.GetDirectories()[0];
                        DirectoryInfo netFolderDir = anyDir.GetDirectories(netFolderName)?.FirstOrDefault();
                        if (netFolderDir != null)
                        {
                            FileHelper.CopyFolder(netFolderDir.FullName, destinationDirectory);
                        }
                    }
                }
            }

        }
        catch (Exception ex)
        { 
            throw ex;
        }

        return isDecomparessSuccess;
    }
}