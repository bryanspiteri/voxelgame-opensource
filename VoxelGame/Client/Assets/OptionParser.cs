using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VoxelGame.Client
{
    public class OptionParser
    {
        public OptionParser() { }

        public string[] file;
        public string path;

        /// <summary>
        /// Returns an OptionParser with a loaded option file
        /// </summary>
        /// <param name="path">The path of the option file</param>
        /// <returns>An OptionParser  with the file loaded</returns>
        public static OptionParser LoadOptionsFile(string path)
        {
            OptionParser opt = new OptionParser();
            opt.path = path;
            try
            {
                opt.file = File.ReadAllLines(path);
                for (int y = 0; y < opt.file.Length; y++) { opt.file[y] = opt.file[y].Trim(); }

                return opt;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                opt.file = new string[1];
                return opt;
            }
        }

        /// <summary>
        /// Returns the value of a specified key. Returns null if the key doesn't exist
        /// </summary>
        /// <param name="key">Name of the key</param>
        /// <returns>The value of the key.</returns>
        public string Get(string key)
        {
            if (file == null)
            {
                throw new FileNotFoundException();
            }
            for (int y = 0; y < file.Length; y++)
            {
                file[y] = file[y].Trim();
                if (file[y].Contains(key))
                {
                    return file[y].Substring(file[y].IndexOf('=') + 1);
                }
            }
            return null;
        }

        /// <summary>
        /// Sets the value of a specified key. Throws FileNotFoundException if the OptionParser wasn't initialised properly.
        /// </summary>
        /// <param name="key">The key of the option to set</param>
        /// <param name="value">The value of the option to set</param>
        public void Set(string key, string value)
        {
            if (file == null)
            {
                throw new FileNotFoundException();
            }
            //If the key already exists, overwrite it
            for (int y = 0; y < file.Length; y++)
            {
                file[y] = file[y].Trim();
                if (file[y].Contains(key))
                {
                    file[y] = key + "=" + value;
                    file[y] = file[y].Trim();
                    return;
                }
            }
            List<string> fl = file.ToList();
            fl.Add((key + "=" + value).Trim());
            file = fl.ToArray<string>();
        }

        /// <summary>
        /// Save the Options to a file
        /// </summary>
        public void Save() { Save(path); }

        /// <summary>
        /// Save the Options to a file
        /// </summary>
        /// <param name="path">Path to save the options to</param>
        public void Save(string path)
        {
            //trim the file
            for (int y = 0; y < file.Length; y++) { file[y] = file[y].Trim(); }
            //if the directory doesnt exist, create it
            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }
            //save it to a location
            File.WriteAllLines(path, file);
        }
    }
}
