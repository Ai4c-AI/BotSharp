using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotSharp.Abstraction.Utilities;

public  class FileHelper
{
    /// <summary>
    /// Copy folders and files (does not include the root name of the source folder,
    /// just copy the contents to the destination folder)
    /// https://www.cnblogs.com/wangjianhui008/p/3234519.html
    /// </summary>
    /// <param name="sourceFolder">source file folder</param>
    /// <param name="destFolder">dest file folder</param>
    /// <returns></returns>
    public static bool CopyFolder(string sourceFolder, string destFolder)
    {
        try
        {
            if (!System.IO.Directory.Exists(destFolder))
            {
                System.IO.Directory.CreateDirectory(destFolder);
            }
            string[] files = System.IO.Directory.GetFiles(sourceFolder);
            foreach (string file in files)
            {
                string name = System.IO.Path.GetFileName(file);
                string dest = System.IO.Path.Combine(destFolder, name);
                System.IO.File.Copy(file, dest);
            }
            string[] folders = System.IO.Directory.GetDirectories(sourceFolder);
            foreach (string folder in folders)
            {
                string name = System.IO.Path.GetFileName(folder);
                string dest = System.IO.Path.Combine(destFolder, name);
                CopyFolder(folder, dest);
            }
            return true;
        }
        catch (Exception e)
        {

            return false;
        }

    }
}
