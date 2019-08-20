using System.IO;
using System;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Web;

namespace IonLib
{
    public static class Base
    {
        /// <summary>
        /// Writes in colored text.
        /// </summary>
        /// <param name="text">The text to display as WriteLine</param>
        /// <param name="color">The color to write in</param>
        public static void WriteInColor(string text, ConsoleColor color)
        {
            ConsoleColor preColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ForegroundColor = preColor;
        }
        /// <summary>
        /// Writes in colored text and a newline.
        /// </summary>
        /// <param name="text">The text to display as WriteLine</param>
        /// <param name="color">The color to write in</param>
        public static void WriteLineInColor(string text, ConsoleColor color)
        {
            WriteInColor(text, color);
            Console.WriteLine();
        }

        #region Error Functions
        public static void Error()
        {
            WriteLineInColor("An error has occurred", ConsoleColor.Red);
        }
        public static void Error(string message)
        {
            WriteLineInColor(message, ConsoleColor.Red);
        }
        #endregion
    }

    // Functions with ReadLine properties
    namespace ReadLine
    {
        public static class ReadLine
        {
            /// <summary>
            /// Convert a Console.ReadLine output into an integer directly.
            /// Anything which isn't an integer will be declined.
            /// </summary>
            /// <param name="failureMsg">
            /// An optional error message for when the user writes something that isn't an integer
            /// </param>
            /// <returns>An integer which has been converted from a Console.ReadLine</returns>
            public static int ToInt(string failureMsg = "")
            {
                int answer;
                while (!int.TryParse(Console.ReadLine(), out answer))
                {
                    if (failureMsg != "")
                    {
                        Console.WriteLine();
                    }
                }
                return answer;
            }
            /// <summary>
            /// Convert a Console.ReadLine output into an integer directly.
            /// Anything which isn't an integer will be declined.
            /// </summary>
            /// <param name="failureMsg">
            /// An error message for when the user writes something that isn't an integer
            /// </param>
            /// <param name="color">
            /// The color you want the error text to be written in.
            /// </param>
            /// <returns>An integer which has been converted from a Console.ReadLine</returns>
            public static int ToInt(string failureMsg, ConsoleColor color)
            {
                int answer;
                while (!int.TryParse(Console.ReadLine(), out answer))
                {
                    Base.WriteLineInColor(failureMsg, color);
                }
                return answer;
            }
        }
    }

    // Interacting with MySql using https://lucasion.tk/sql/getMySql.php
    namespace MySql
    {
        public static class Sql
        {
            public static Dictionary<int, Dictionary<string, object>> Query(string db, string query)
            {
                //Security code
                string sqlKey = "89ADNYE2CY7892E19YC78Y7YR7823089R2B78R30N9D827M2389R";
                //Encoder function using binary
                string EncodeTo64(string toEncode)
                {
                    byte[] toEncodeAsBytes = Encoding.ASCII.GetBytes(toEncode);
                    string returnValue = Convert.ToBase64String(toEncodeAsBytes);
                    return returnValue;
                }
                //Use the Encoder
                string queryString = EncodeTo64(query);
                //Do i actually need to tell you what this is?
                string url = $"https://lucasion.tk/sql/getMySql.php?key={sqlKey}&query={queryString}&db={db}";

                //Defining contents
                string contents;
                using (WebClient wc = new WebClient())
                {
                    contents = wc.DownloadString(url);
                }
                Dictionary<int, Dictionary<string, object>> queryResult =
                    new Dictionary<int, Dictionary<string, object>>();
                contents = contents.TrimEnd('\n');
                string[] lines = contents.Split("\n");
                for (int i = 0; i < lines.Length; i++)
                {
                    Dictionary<string, object> row = new Dictionary<string, object>();
                    //Console.WriteLine(lines[i]+"\n");
                    string[] args = lines[i].Split(":{", 2);
                    if (args.Length > 1 && args[1].Contains(","))
                    {
                        args[1] = args[1].TrimEnd(',', '}', ';');
                        string[] cols = args[1].Split("\",\"");
                        foreach (var item in cols)
                        {
                            string[] keyvalue;
                            if (item.Trim('"').Contains("\":\""))
                            {
                                keyvalue = item.Trim('"').Split("\":\"");
                            }
                            else
                            {
                                keyvalue = item.Trim('"').Split("\":");
                            }
                            string key = keyvalue[0];
                            object value;
                            if (keyvalue.Length > 1 && double.TryParse(keyvalue[1], out double num))
                            {
                                value = num;
                            }
                            else if (keyvalue.Length > 1)
                            {
                                value = keyvalue[1];
                            }
                            else
                            {
                                value = "";
                            }
                            row.Add(key, value);
                            try
                            {
                                if (key == "@ERROR" && (string)value != "false")
                                {
                                    Console.WriteLine("An error has occured: \"value\"");
                                }
                            }
                            catch (Exception)
                            {

                            }
                        }
                    }
                    queryResult.Add(Convert.ToInt32(args[0]), row);
                }
                return queryResult;
            }
        }
    }

    // A file downloader
    namespace Network
    {
        class DL
        {
            public enum BytePreference
            {
                Auto,
                Bytes,
                KiloBytes,
                MegaBytes,
                GigaBytes
            }
            static char[] illegalChars = "\\:*?\"<>|".ToCharArray(); //doesn't include /
            static string FilterIllegalChars(string _text, string replaceWith = "_")
            {
                for (int i = 0; i < illegalChars.Length; i++)
                {
                    _text = _text.Replace(illegalChars[i].ToString(), replaceWith);
                }
                return _text;
            }
            public static bool silentDownload = false;
            public static bool forceDownload = false;
            static string rootUrl = "";
            public static void RootUrl(string _rootUrl)
            {
                rootUrl = _rootUrl;
            }
            static string dlDirectory = "C:/";
            public static void DLDirectory(string directory)
            {
                dlDirectory = directory;
            }
            static BytePreference bytePreference = BytePreference.Auto;
            /// <summary>
            /// Set the Byte preference
            /// </summary>
            public static void BytePref(BytePreference bp)
            {
                bytePreference = bp;
            }
            public static int procent = -1;
            public static long curbytes, totalBytes;
            readonly static WebClient webClient = new WebClient();
            static Uri url;
            //public static List<string> downloadList = new List<string>();
            public static List<string[]> downloadList = new List<string[]>();
            //Add SetDlList Support for
            //  Giving each download their own custom name.
            //  Potentionally use List<string[]>
            //  a string array of 2, 1st being the URL, 2nd being the file name.

            //Also add support for
            //  Reading a local file with URL's
            //  and maybe also with custom names if i add support for that
            #region SetDlList() overloads without custom names
            public static void SetDlList(string url)
            {
                ResetDlList();
                AddDlList(url);
            }
            public static void SetDlList(string[] urls)
            {
                ResetDlList();
                AddDlList(urls);
            }

            public static void AddDlList(string url)
            {
                downloadList.Add(new string[] { url, RemoveRoot(url) });
            }
            public static void AddDlList(string[] urls)
            {
                string[][] toAdd = new string[urls.Length][];
                for (int i = 0; i < urls.Length; i++)
                {
                    toAdd[i] = new string[] { urls[i], RemoveRoot(urls[i]) };
                }
                downloadList.AddRange(toAdd);
            }

            public static void ResetDlList()
            {
                downloadList.Clear();
            }
            #endregion
            #region SetDlList() overloads with custom names
            public static void SetDlList(string[] url, bool customNames)
            {
                if (!customNames)
                {
                    SetDlList(url[0]);
                }
                else
                {
                    ResetDlList();
                    downloadList.Add(url);
                }
            }
            public static void SetDlList(string[][] urls)
            {
                ResetDlList();
                downloadList.AddRange(urls);
            }

            public static void AddDlList(string[] url, bool customNames)
            {
                if (!customNames)
                {
                    AddDlList(url[0]);
                }
                else
                {
                    downloadList.Add(url);
                }
            }
            public static void AddDlList(string[][] urls)
            {
                downloadList.AddRange(urls);
            }
            #endregion

            //Create directories and all sub directories if they don't exist
            public static void CreateDirectory(string directory)
            {
                directory = directory.Replace("\\", "/");
                string[] _dirs = directory.Split("/");
                List<string> dirs = new List<string>(_dirs);
                string curDir = dirs[0] + "/";
                dirs.RemoveAt(0);
                while (!Directory.Exists(directory))
                {
                    if (Directory.Exists(curDir))
                    {
                        curDir += dirs[0] + "/";
                        dirs.RemoveAt(0);
                    }
                    else
                    {
                        Directory.CreateDirectory(curDir);
                    }
                }
            }

            //Get a string from a page
            public static string GetString(string url)
            {
                return webClient.DownloadString(url);
            }
            //Get a string from a page as array
            public static string[] GetStringArray(string url)
            {
                List<string> list = new List<string>(webClient.DownloadString(url).Split("\n"));
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] == "")
                    {
                        list.RemoveAt(i);
                        i--;
                    }
                }
                return list.ToArray();
            }

            //Download function to start the downloading.
            //Used in StartDownload()
            static void DownloadFile(string url, string directory)
            {
                DL.url = new Uri(url);
                string fileName = GetFileName(url);
                fileName = FilterIllegalChars(fileName);
                if (!Directory.Exists(directory))
                {
                    CreateDirectory(directory);
                }
                webClient.DownloadFileAsync(DL.url, directory + "/" + fileName);
            }
            static void DownloadFile(string url, string directory, string fileName)
            {
                DL.url = new Uri(url);
                fileName = FilterIllegalChars(fileName);
                if (!Directory.Exists(directory))
                {
                    CreateDirectory(directory);
                }
                webClient.DownloadFileAsync(DL.url, directory + "/" + fileName);
            }

            static string GetFileName(string url)
            {
                string[] urlArgs = url.Split("/");
                return WebUtility.UrlDecode(urlArgs[urlArgs.Length - 1]);
            }
            static string RemoveRoot(string url)
            {
                return "/" + HttpUtility.UrlDecode(url.Substring(rootUrl.Length));
            }

            /// <summary>
            /// Start Download from downloadList
            /// </summary>
            public static Dictionary<string, int> StartDownload()
            {
                Dictionary<string, int> newFiles = new Dictionary<string, int>
                {
                    { "new", 0 },
                    { "updated", 0 },
                    { "exists", 0 },
                    { "failed", 0 }
                };
                webClient.DownloadDataCompleted += WebClient_DownloadDataCompleted;
                webClient.DownloadProgressChanged += WebClient_DownloadProgressChanged;
                int preProcent = -1;
                while (true)
                {
                    if (!webClient.IsBusy)
                    {
                        if (procent != -1)
                        {
                            ConsoleColor _c = Console.ForegroundColor;
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("\n | Completed!\n");
                            Console.ForegroundColor = _c;
                        }
                        if (downloadList.Count > 0)
                        {
                            string fileName = downloadList[0][1];
                            string fullFileName = dlDirectory + "/" + fileName;
                            FileInfo fileInfo = new FileInfo(fullFileName);
                            try
                            {
                                webClient.OpenRead(downloadList[0][0]);
                                if (!forceDownload && File.Exists(fullFileName) && fileInfo.Length == Convert.ToInt64(webClient.ResponseHeaders["Content-Length"]))
                                {
                                    if (!silentDownload)
                                    {
                                        ConsoleColor _c = Console.ForegroundColor;
                                        Console.ForegroundColor = ConsoleColor.Yellow;
                                        Console.WriteLine("File already in folder: " + fullFileName);
                                        Console.ForegroundColor = _c;
                                        newFiles["exists"]++;
                                        procent = -1;
                                    }
                                }
                                else
                                {
                                    if (!forceDownload && File.Exists(fullFileName))
                                    {
                                        newFiles["updated"]++;
                                        if (!silentDownload) Console.WriteLine("Updating \"" + fileName + "\" from " + downloadList[0][0]);
                                    }
                                    else
                                    {
                                        newFiles["new"]++;
                                        if (!silentDownload) Console.WriteLine("Downloading \"" + fileName + "\" from " + downloadList[0][0]);
                                    }
                                    string dlToDir =
                                        dlDirectory + "/" + fileName.Substring(0, fileName.Length - GetFileName(fileName).Length);
                                    string dlAsName = fileName.Substring(fileName.Length - GetFileName(fileName).Length);
                                    DownloadFile(downloadList[0][0], dlToDir, dlAsName);
                                }
                            }
                            catch (Exception e)
                            {
                                ConsoleColor _c = Console.ForegroundColor;
                                Base.Error(e.Message);
                                Base.Error(fullFileName + "\nFrom " + downloadList[0][0]);
                                newFiles["failed"]++;
                                preProcent = -1;
                                //Thread.Sleep(1000);
                            }
                            downloadList.RemoveAt(0);
                        }
                        else
                        {
                            Console.WriteLine("Downloads are complete.");
                            return newFiles;
                        }
                    }
                    if (!silentDownload && procent != -1 && preProcent != procent)
                    {
                        BytePreference bp = bytePreference;

                        if (bp == BytePreference.Auto)
                        {
                            if (totalBytes < 1000)
                                bp = BytePreference.Bytes;
                            else if (totalBytes < 1000000)
                                bp = BytePreference.KiloBytes;
                            else if (totalBytes < 1000000000)
                                bp = BytePreference.MegaBytes;
                            else if (totalBytes >= 1000000000)
                                bp = BytePreference.GigaBytes;
                        }

                        if (bp == BytePreference.Bytes)
                            Console.Write(procent + "% | " + curbytes + " B/" + totalBytes + " B ");
                        else if (bp == BytePreference.KiloBytes)
                            Console.Write(procent + "% | " + Math.Round(curbytes / 1000.0, 2) + " KB/" + Math.Round(totalBytes / 1000.0, 2) + " KB ");
                        else if (bp == BytePreference.MegaBytes)
                            Console.Write(procent + "% | " + Math.Round(curbytes / 1000000.0, 2) + " MB/" + Math.Round(totalBytes / 1000000.0, 2) + " MB ");
                        else if (bp == BytePreference.GigaBytes)
                            Console.Write(procent + "% | " + Math.Round(curbytes / 1000000000.0, 2) + " GB/" + Math.Round(totalBytes / 1000000000.0, 2) + " GB ");
                        Console.CursorLeft = 0;
                        preProcent = procent;
                    }
                }
            }
            #region StartDownload() overloads
            /// <summary>
            /// Start Download from downloadList
            /// </summary>
            /// <param name="bytePreference">Set how you want to show bytes, like Bytes, KiloBytes, MegaBytes and GigaBytes</param>
            public static Dictionary<string, int> StartDownload(BytePreference bytePreference)
            {
                DL.bytePreference = bytePreference;
                return StartDownload();
            }
            /// <summary>
            /// Start Download from URL
            /// </summary>
            /// <param name="url">The URL to download from</param>
            public static Dictionary<string, int> StartDownload(string url)
            {
                SetDlList(url);
                return StartDownload();
            }
            /// <summary>
            /// Start Download from an array of urls
            /// </summary>
            /// <param name="urls">The URL array to download from</param>
            public static Dictionary<string, int> StartDownload(string[] urls)
            {
                SetDlList(urls);
                return StartDownload();
            }
            #endregion

            private static void WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
            {
                procent = e.ProgressPercentage;
                curbytes = e.BytesReceived;
                totalBytes = e.TotalBytesToReceive;
            }

            private static void WebClient_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
            {
                if (!silentDownload)
                {
                    Console.WriteLine("Download is completed");
                    Console.WriteLine(e.Result);
                }
            }
        }
        class UL
        {
            public static long procent = -1;
            static WebClient webClient = new WebClient();
            public enum BytePreference
            {
                Auto,
                Bytes,
                KiloBytes,
                MegaBytes,
                GigaBytes
            }
            static BytePreference bytePreference = BytePreference.Auto;
            public static long curbytes, totalBytes;
            static char[] illegalChars = "\\/:*?\"<>|".ToCharArray();
            static string FilterIllegalChars(string _text, string replaceWith = "_")
            {
                for (int i = 0; i < illegalChars.Length; i++)
                {
                    _text = _text.Replace(illegalChars[i].ToString(), replaceWith);
                }
                return _text;
            }
            static string ulDirectory = "E:/temp",
                ulUrl = "",
                uploadTo = "default",
                rootUrl = "";
            public static void ULDirectory(string directory)
            {
                ulDirectory = directory;
            }
            public static void ULUrl(string url)
            {
                ulUrl = url;
            }
            /// <summary>
            /// (Works on https://lucasion.tk/projects/files)
            /// Set subfolder to upload to in the server
            /// </summary>
            /// <param name="uploadto">The folder name</param>
            public static void UploadTo(string uploadto)
            {
                uploadTo = uploadto;
            }
            /// <summary>
            /// The rootUrl if set, will be used for folder downloading into correct folders
            /// </summary>
            /// <param name="root"></param>
            public static void RootUrl(string root)
            {
                rootUrl = root;
            }
            public enum UType
            {
                CurrentFolder
            }

            static List<string> uploadList = new List<string>();

            #region SetDlList() overloads without custom names
            public static void SetUlList(string file)
            {
                ResetUlList();
                AddUlList(file);
            }
            public static void SetUlList(string[] files)
            {
                ResetUlList();
                AddUlList(files);
            }

            public static void AddUlList(string file)
            {
                uploadList.Add(file);
            }
            public static void AddUlList(string[] files)
            {
                uploadList.AddRange(files);
            }
            /// <summary>
            /// This addUlList REQUIRES rootUrl to be set
            /// </summary>
            /// <param name="uri">The uri to upload from</param>
            /// <param name="subfiles">Include subfolders and files?</param>
            public static void AddUlList(string uri, bool subfiles = false, string newRootUrl = null)
            {
                if (rootUrl == "")
                {
                    if (newRootUrl == null)
                    {
                        rootUrl = uri + "/";
                    }
                    else
                    {
                        rootUrl = newRootUrl + "/";
                    }
                }
                rootUrl = FixUri(rootUrl);
                var allFiles = Directory.GetFiles(uri);
                for (int i = 0; i < allFiles.Length; i++)
                {
                    allFiles[i] = FixUri(allFiles[i]);
                    allFiles[i] = "/" + allFiles[i].Substring(rootUrl.Length);
                }
                if (subfiles)
                {
                    var allFolders = Directory.GetDirectories(uri);
                    for (int i = 0; i < allFolders.Length; i++)
                    {
                        AddUlList(allFolders[i], true);
                        allFolders[i] = FixUri(allFolders[i]);
                        allFolders[i] = "/" + allFolders[i].Substring(rootUrl.Length);
                    }
                }
                uploadList.AddRange(allFiles);

            }

            public static void ResetUlList()
            {
                uploadList.Clear();
            }
            #endregion

            public static string FixUri(string uri)
            {
                uri = uri.Replace("\\", "/");
                while (uri.Contains("//"))
                {
                    uri = uri.Replace("//", "/");
                }
                return uri;
            }


            public static void Upload(string fileName, UType uType = UType.CurrentFolder)
            {
                /*byte[] response = webClient.UploadFile(
                    new Uri(ulUrl+"?uploadTo=" + uploadTo),
                    ulDirectory+"/"+fileName
                );

                Console.WriteLine(Encoding.ASCII.GetString(response));*/

                //Async
                totalBytes = new FileInfo(ulDirectory + "/" + fileName).Length;
                string curUrl = ulUrl + "?uploadTo=" + uploadTo;
                string[] pathArgs = fileName.Split("/");
                curUrl += "&folder=/" + fileName.Substring(0, fileName.Length - pathArgs[pathArgs.Length - 1].Length - 1);
                Console.WriteLine(curUrl);

                webClient.UploadFileAsync(
                    new Uri(curUrl),
                    ulDirectory + "/" + fileName
                );
                /*
                totalBytes = new FileInfo(ulDirectory + "/" + fileName).Length;
                ulUrl = FixUri(ulUrl);
                string curUrl = ulUrl + "?uploadTo=" + uploadTo;
                string[] pathArgs = fileName.Split("/");
                curUrl += "&folder=" + HttpUtility.UrlEncode(fileName.Substring(0, fileName.Length - pathArgs[pathArgs.Length - 1].Length - 1));
                webClient.UploadFileAsync(
                    new Uri(ulUrl),
                    ulDirectory + "/" + fileName
                );
                */

            }
            public static void StartUpload()
            {
                webClient.UploadProgressChanged += WebClient_UploadProgressChanged;
                webClient.UploadFileCompleted += WebClient_UploadFileCompleted;
                long preProcent = -1;
                while (true)
                {
                    if (!webClient.IsBusy)
                    {
                        if (procent != -1)
                        {
                            ConsoleColor _c = Console.ForegroundColor;
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("\n | Completed!\n");
                            Console.ForegroundColor = _c;
                        }
                        if (uploadList.Count > 0)
                        {
                            int index = uploadList.Count - 1;
                            Console.WriteLine("Uploading \"" + uploadList[index] + "\" to " + ulUrl);
                            Upload(uploadList[index]);
                            uploadList.RemoveAt(index);
                        }
                        else
                        {
                            Console.WriteLine("Uploads are complete.");
                            break;
                        }
                    }
                    if (procent != -1 && preProcent != procent)
                    {
                        BytePreference bp = bytePreference;

                        if (bp == BytePreference.Auto)
                        {
                            if (totalBytes < 1000)
                                bp = BytePreference.Bytes;
                            else if (totalBytes < 1000000)
                                bp = BytePreference.KiloBytes;
                            else if (totalBytes < 1000000000)
                                bp = BytePreference.MegaBytes;
                            else if (totalBytes >= 1000000000)
                                bp = BytePreference.GigaBytes;
                        }

                        if (bp == BytePreference.Bytes)
                            Console.Write(procent + "% | " + curbytes + " B/" + totalBytes + " B ");
                        else if (bp == BytePreference.KiloBytes)
                            Console.Write(procent + "% | " + Math.Round(curbytes / 1000.0, 2) + " KB/" + Math.Round(totalBytes / 1000.0, 2) + " KB ");
                        else if (bp == BytePreference.MegaBytes)
                            Console.Write(procent + "% | " + Math.Round(curbytes / 1000000.0, 2) + " MB/" + Math.Round(totalBytes / 1000000.0, 2) + " MB ");
                        else if (bp == BytePreference.GigaBytes)
                            Console.Write(procent + "% | " + Math.Round(curbytes / 1000000000.0, 2) + " GB/" + Math.Round(totalBytes / 1000000000.0, 2) + " GB ");
                        Console.CursorLeft = 0;
                        preProcent = procent;
                    }
                }
            }
            private static void WebClient_UploadProgressChanged(object sender, UploadProgressChangedEventArgs e)
            {
                curbytes = e.BytesSent;
                procent = 100 * curbytes / Math.Max(1, totalBytes);

            }

            private static void WebClient_UploadFileCompleted(object sender, UploadFileCompletedEventArgs e)
            {
                Console.WriteLine("\nAll completed");
            }
        }
    }

    // File manager
    namespace FileMangement
    {
        class FileManage
        {
            public static void CreateDirectory(string directory)
            {
                directory = directory.Replace("\\", "/");
                string[] _dirs = directory.Split("/");
                List<string> dirs = new List<string>(_dirs);
                string curDir = dirs[0] + "/";
                dirs.RemoveAt(0);
                while (!Directory.Exists(directory))
                {
                    if (Directory.Exists(curDir))
                    {
                        curDir += dirs[0] + "/";
                        dirs.RemoveAt(0);
                    }
                    else
                    {
                        Directory.CreateDirectory(curDir);
                    }
                }
            }
        }
    }

    // IonConfig Scriptfile Parser
    namespace IonParser
    {
        class Parser
        {
            public Dictionary<string, object> vars = new Dictionary<string, object>();
            string filePath;
            public enum Type
            {
                None,
                Variable,
                Comment,
            }
            public Parser(string filePath)
            {
                this.filePath = filePath;
                if (!File.Exists(filePath))
                {
                    string dirPath = filePath.Substring(0, filePath.Length - filePath.Split("/")[filePath.Split("/").Length - 1].Length);
                    FileMangement.FileManage.CreateDirectory(dirPath);
                    StreamWriter file = File.CreateText(filePath);
                    file.Write("$_SCRIPTTYPE=basic");
                    file.WriteLine("$dir=\"C:/IonLibFiles/files\"");

                    file.Close();
                }

                StreamReader sr = new StreamReader(filePath);
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    line = line.Trim();
                    Type type = Type.None;
                    //Type found = Type.None;
                    //Detect type
                    #region Detect Type
                    if (line.StartsWith("$"))
                    {
                        type = Type.Variable;
                    }
                    else if (line.StartsWith("#") || line.StartsWith("//"))
                    {
                        type = Type.Comment;
                    }
                    #endregion Detect Type
                    if (type == Type.Variable)
                    {
                        string[] args = line.Split("=", 2);

                        string varName = args[0].Substring(1).Trim();
                        string rest = args[1].Trim();
                        //Setting values, if anything goes wrong, default to num
                        try
                        {
                            if (rest.StartsWith("\"") && rest.EndsWith("\""))
                            {
                                vars.Add(varName, rest.Trim('"'));
                            }
                            else if (!rest.StartsWith("\"") && !rest.EndsWith("\""))
                            {
                                vars.Add(varName, double.Parse(rest));
                            }
                            else
                            {
                                vars.Add(varName, null);
                            }
                        }
                        catch (Exception)
                        {
                            //vars.Add(varName, null);
                            Base.Error("Couldn't create variable, error in script.");
                        }
                    }
                }
                sr.Close();
            }
        }
    }
}