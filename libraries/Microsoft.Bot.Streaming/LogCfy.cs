using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Streaming
{
    public static class LogCfy
    {
        private const bool AutoFinished = false;
        private static string logPath = "C:/Dev/Samples";
        private static string subPath = "NoSubPath";
        private static string index = "NoIndex";

        private static Queue<Tuple<string, int>> logs = new Queue<Tuple<string, int>>();

        private static Semaphore _endSemaphore = new Semaphore(0, 10);
        private static Semaphore _logSemaphore = new Semaphore(0, 10000);

        private static Dictionary<string, int[]> hash = new Dictionary<string, int[]>();
        private static int[] _valid = null;

        public static Task LogTask { get; private set; }

        public static string FileName => $"{logPath}/{subPath}/Log{index}.cfy";

        public static string IndexName => $"{logPath}/{subPath}/Index.cfy";

        public static void StartLogCfy(string subPath)
        {
            var dir = new DirectoryInfo(logPath);
            if (!dir.Exists)
            {
                logPath = "../..";
            }

            logPath += "/CfyLogs";

            var index = subPath.IndexOf(".");
            if (index > 0)
            {
                subPath = subPath.Substring(index + 1);
            }

            LogCfy.subPath = subPath;
            Init();
        }

        public static bool LogAddHash(Guid guid, int index)
        {
            Log($"LogAddHash--{guid}--{index}");
            var guidStr = guid.ToString();
            lock (hash)
            {
                if (!hash.ContainsKey(guidStr))
                {
                    return false;
                }

                hash[guidStr][index]++;

                return true;
            }
        }

        public static void LogInitHash(string guid, bool isStart)
        {
            lock (hash)
            {
                if (isStart)
                {
                    hash.Add(guid, new int[100]);
                }
                else
                {
                    var tmp = hash[guid];

                    if (_valid == null)
                    {
                        _valid = tmp;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(CompareArr(_valid, tmp)))
                        {
                            _ = Task.Run(() =>
                            {
                                Log($"GuidEndUpWithError:{guid}--{tmp}");
                            });
                        }
                    }

                    hash.Remove(guid);
                }
            }
        }

        public static void Log(string str)
        {
            if (str == "End")
            {
                _endSemaphore.Release();
            }

            lock (logs)
            {
                var threadId = Thread.CurrentThread.ManagedThreadId;
                logs.Enqueue(new Tuple<string, int>(str, threadId));
                _logSemaphore.Release();
            }
        }

        private static string CompareArr(int[] arr1, int[] arr2)
        {
            var ans = string.Empty;
            for (var i = 0; i < arr1.Length; i++)
            {
                if (arr1[i] == arr2[i])
                {
                    continue;
                }

                ans += $"\t{i}({arr2[i]}/{arr1[i]})";
            }

            return ans;
        }

        private static async Task SetFileIndex()
        {
            var index = -1;
            using (var fs = new FileStream(IndexName, FileMode.OpenOrCreate))
            {
                using (var sw = new StreamReader(fs))
                {
                    int.TryParse(await sw.ReadToEndAsync().ConfigureAwait(false), out index);
                }
            }

            index++;
            using (var fs = new FileStream(IndexName, FileMode.OpenOrCreate))
            {
                using (var sw = new StreamWriter(fs))
                {
                    await sw.WriteLineAsync(index.ToString()).ConfigureAwait(false);
                }
            }

            LogCfy.index = index.ToString();
        }

        private static void StartTmpLogTask()
        {
            while (true)
            {
                _logSemaphore.WaitOne();
                Tuple<string, int> item;
                lock (logs)
                {
                    if (logs.Count == 0)
                    {
                        return;
                    }

                    item = logs.Dequeue();
                }

                using (var fs = new FileStream(FileName, FileMode.Append))
                {
                    using (var sw = new StreamWriter(fs))
                    {
                        sw.WriteLine($"{item.Item2}\t{item.Item1}");
                    }
                }
            }
        }

        private static void Init()
        {
            LogTask = Task.Run(async () =>
            {
                var fileInfo = new FileInfo(IndexName);
                if (!fileInfo.Directory.Exists)
                {
                    fileInfo.Directory.Create();
                }

                await SetFileIndex();
                var tmpLogTask = Task.Run(StartTmpLogTask);

                if (AutoFinished)
                {
                    _endSemaphore.WaitOne(30000);
                }
                else
                {
                    _endSemaphore.WaitOne();
                }

                _logSemaphore.Release();
                await tmpLogTask.ConfigureAwait(false);

                lock (logs)
                {
                    try
                    {
                        using (var fs = new FileStream(FileName, FileMode.Append))
                        {
                            using (var sw = new StreamWriter(fs))
                            {
                                if (_valid != null)
                                {
                                    var tmp = new int[100];
                                    for (var i = 0; i < 100; i++)
                                    {
                                        tmp[i] = i;
                                    }

                                    sw.WriteLine(string.Join("\t", tmp.Take(25)));
                                    sw.WriteLine(string.Join("\t", _valid.Take(25)));
                                    sw.WriteLine();

                                    sw.WriteLine(string.Join("\t", tmp.Skip(25).Take(25)));
                                    sw.WriteLine(string.Join("\t", _valid.Skip(25).Take(25)));
                                    sw.WriteLine();

                                    sw.WriteLine(string.Join("\t", tmp.Skip(50).Take(25)));
                                    sw.WriteLine(string.Join("\t", _valid.Skip(50).Take(25)));
                                    sw.WriteLine();

                                    foreach (var h in hash)
                                    {
                                        sw.WriteLine($"NotCompletedRequest:{h.Key}-\r\n-{CompareArr(_valid, h.Value)}");
                                    }
                                }

                                sw.WriteLine("LogFinished!!!");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        using (var fs = new FileStream(FileName, FileMode.Append))
                        {
                            using (var sw = new StreamWriter(fs))
                            {
                                sw.WriteLine(e);
                            }
                        }
                    }
                }
            });
        }
    }
}
