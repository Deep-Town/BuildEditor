using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

namespace DeepTown.Utils
{
    public class FileUtils
    {
        /// <summary>
        /// Check if file exists
        /// </summary>
        /// <param name="path">File Path</param>
        /// <returns></returns>
        public static bool FileExists(string path)
        {
            return File.Exists(path);
        }

        /// <summary>
        /// Get File Extension from file uri
        /// </summary>
        /// <param name="path">File Path</param>
        /// <returns></returns>
        public static string GetFileExtension(string path)
        {
            return Path.GetExtension(path);
        }

        /// <summary>
        /// Get File name with or without extension
        /// </summary>
        /// <param name="path">File Path</param>
        /// <param name="withoutExtension">If get file name without extension</param>
        /// <returns></returns>
        public static string GetFileName(string path, bool withoutExtension = false)
        {
            if (withoutExtension) return Path.GetFileNameWithoutExtension(path);
            return Path.GetFileName(path);
        }

        /// <summary>
        /// Check if Path is valid
        /// </summary>
        /// <param name="path">Path</param>
        /// <returns></returns>
        public static bool IsValidPath(string path)
        {
            bool isValid = true;

            try
            {
                string fullPath = Path.GetFullPath(path);

                string root = Path.GetPathRoot(path);
                isValid = string.IsNullOrEmpty(root.Trim(new char[] { '\\', '/' })) == false;
            }
            catch (Exception ex)
            {
                isValid = false;
                Debug.LogError(ex.Message);
            }

            return isValid;
        }

        /// <summary>
        /// Get Streaming Asset path of any given file name (Platform dependent)
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <returns>Full Path with file name and streaming asset path</returns>
        public static string GetStreammingAssetsPath(string fileName)
        {
            #if UNITY_EDITOR_OSX
            return "file://" + Application.streamingAssetsPath + "/" + fileName;
            #elif UNITY_EDITOR
            return Application.streamingAssetsPath + "/" + fileName;
            #elif UNITY_ANDROID
            return Path.Combine("jar:file://" + Application.dataPath + "!/assets" , fileName);
            #elif UNITY_IOS
            return Path.Combine(Application.dataPath + "/Raw" , fileName);
            #else
            return Application.streamingAssetsPath + "/" + fileName;
            #endif
        }

        /// <summary>
        /// Create File
        /// </summary>
        /// <param name="path"></param>
        /// <param name="filename"></param>
        /// <param name="extension"></param>
        public static void CreateFile(string path, string filename, string extension)
        {
            string filePath = $"{path}/{filename}.{extension}";

            if (File.Exists(filePath))
            {
                Debug.LogError("File already exists!");
                return;
            }

            File.Create(filePath).Dispose();
        }                                       

        /// <summary>
        /// Create File and write lines to it
        /// </summary>
        /// <param name="path"></param>
        /// <param name="filename"></param>
        /// <param name="extension"></param>
        /// <param name="lines"></param>
        public static void CreateFile(string path, string filename, string extension, string[] lines)
        {
            string filePath = $"{path}/{filename}.{extension}";

            if (File.Exists(filePath))
            {
                Debug.LogError("File already exists!");
                return;
            }
            File.Create(filePath).Dispose();

            using (TextWriter writer = new StreamWriter(filePath, false))
            {
                string s = "";
                for (int i = 0; i < lines.Length; i++)
                {
                    s += lines[i] + "\n";
                }
                writer.WriteLine(s);
                writer.Close();
            }
        }

        /// <summary>
        /// Create File and write lines to it
        /// </summary>
        /// <param name="filePath"></param>
        public static void CreateFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                Debug.LogError("File already exists!");
                return;
            }

            File.Create(filePath).Dispose();
        }

        /// <summary>
        /// Create File and write lines to it
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="lines"></param>
        public static void CreateFile(string filePath, string[] lines)
        {
            if (File.Exists(filePath))
            {
                Debug.LogError("File already exists!");
                return;
            }

            string directoryPath = Path.GetDirectoryName(filePath);

            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            File.Create(filePath).Dispose();

            using (TextWriter writer = new StreamWriter(filePath, false))
            {
                string s = "";
                for (int i = 0; i < lines.Length; i++)
                {
                    s += lines[i] + "\n";
                }
                writer.WriteLine(s);
                writer.Close();
            }
        }

        /// <summary>
        /// Write lines to existing text file
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="lines"></param>
        public static void WriteLinesToFile(string filePath, string[] lines)
        {
            if (!File.Exists(filePath))
            {
                Debug.LogError("File doesn't exist!");
                return;
            }

            using (TextWriter writer = new StreamWriter(filePath, false))
            {
                string s = "";
                for (int i = 0; i < lines.Length; i++)
                {
                    s += lines[i] + "\n";
                }
                writer.WriteLine(s);
                writer.Close();
            }
        }
    }
}
