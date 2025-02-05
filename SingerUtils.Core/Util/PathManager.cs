﻿using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using SingerUtils.Core.Util;
using Serilog;

namespace SingerUtils.Core
{

    public class PathManager : SingletonBase<PathManager>
    {
        public PathManager()
        {
            RootPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            if (OS.IsMacOS())
            {
                //TODO
                string userHome = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                DataPath = Path.Combine(userHome, "Library", "OpenUtau");
                CachePath = Path.Combine(userHome, "Library", "Caches", "OpenUtau");
                HomePathIsAscii = true;
                try
                {
                    // Deletes old cache.
                    string oldCache = Path.Combine(DataPath, "Cache");
                    if (Directory.Exists(oldCache))
                    {
                        Directory.Delete(oldCache, true);
                    }
                }
                finally { }
            }
            else if (OS.IsLinux())
            {
                //TODO
                string userHome = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                string dataHome = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
                if (string.IsNullOrEmpty(dataHome))
                {
                    dataHome = Path.Combine(userHome, ".local", "share");
                }
                DataPath = Path.Combine(dataHome, "OpenUtau");
                string cacheHome = Environment.GetEnvironmentVariable("XDG_CACHE_HOME");
                if (string.IsNullOrEmpty(cacheHome))
                {
                    cacheHome = Path.Combine(userHome, ".cache");
                }
                CachePath = Path.Combine(cacheHome, "OpenUtau");
                HomePathIsAscii = true;
            }
            else
            {
                var args = Environment.GetCommandLineArgs();
                if (args.Count() > 1 && File.Exists(Path.Join(args[1],"../../prefs.json"))) //(File.Exists("../../prefs.json"))
                {
                    //If Launched as OpenUTAU Plugin, DataPath is where OpenUTAU is located
                    DataPath = Path.GetFullPath(Path.Join(args[1], "../.."));
                }
                else if(File.Exists(Path.Join(Process.GetCurrentProcess().MainModule.FileName,"../../prefs.json")))
                {
                    //If installed as OpenUTAU Plugin but not launched in OpenUTAU, DataPath is where OpenUTAU is located
                    DataPath = Path.GetFullPath(Path.Join(Process.GetCurrentProcess().MainModule.FileName, "../.."));
                }
                else
                {
                    //If not installed as OpenUTAU Plugin, DataPath is where SingerUtils is located
                    DataPath = Path.GetFullPath(".");
                }
                CachePath = Path.Combine(DataPath, "Cache");
                HomePathIsAscii = true;
                var etor = StringInfo.GetTextElementEnumerator(DataPath);
                while (etor.MoveNext())
                {
                    string s = etor.GetTextElement();
                    if (s.Length != 1 || s[0] >= 128)
                    {
                        HomePathIsAscii = false;
                        break;
                    }
                }
            }
            Log.Logger.Information($"Data path = {DataPath}");
            Log.Logger.Information($"Cache path = {CachePath}");
        }

        public string RootPath { get; private set; }
        public string DataPath { get; private set; }
        public string CachePath { get; private set; }
        public bool HomePathIsAscii { get; private set; }
        public string PrefsFilePath => Path.Combine(DataPath, "prefs.json");

        readonly static string[] sizes = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };

    }
}