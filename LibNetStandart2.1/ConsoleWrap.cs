#pragma warning disable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace System
{
    public class Logger
    {
        private static object Locker => ConsoleWrap.lockWrite;
        private static object lockerSave = new object();

        public readonly object target;
        public readonly Queue<string> messages = new Queue<string>(stringsCount);
        public const int stringsCount = 1024;
        private Func<bool> filter;
        private static readonly Func<bool> defaultFilterEnabled = () => true;
        private static readonly Func<bool> defaultFilterDisabled = () => false;
        private Func<bool> filterCached;
        private ConsoleColor defaultColor = Console.ForegroundColor;
        public Logger(Object target)
        {
            this.target = target;
            filter = defaultFilterEnabled;
        }


        public static void LogError(string msg)
        {
            ConsoleWrap.Red(msg);
        }
        public void Log(string msg)
        {
            LogSave(msg);
            WriteLine(msg);
        }
        public string LogSave(string msg)
        {
            lock (lockerSave)
            {
                messages.Enqueue(msg);

                if (messages.Count >= stringsCount)
                {
                    messages.Dequeue();
                }
                return msg;
            }
        }
        public void ClearScreenAndShow()
        {
            Console.Clear();

            foreach (var item in messages)
            {
                Console.WriteLine(item);
            }
        }

        public void WriteLine(string msg, ConsoleColor color = (ConsoleColor)16)
        {
            string timestamp = $"[{DateTime.Now.ToString("yyyy:MM:dd HH:mm:ss.FFF")}]   ";

            lock (Locker)
            {
                if ((int)color > 15)
                {
                    if (filter())
                    {
                        var col = Console.ForegroundColor;
                        Console.ForegroundColor = defaultColor;
                        Console.WriteLine(timestamp + msg);
                        Console.ForegroundColor = col;
                    }
                }
                else
                {
                    if (filter())
                    {
                        var col = Console.ForegroundColor;
                        Console.ForegroundColor = color;
                        Console.WriteLine(timestamp + msg);
                        Console.ForegroundColor = col;
                    }
                }
            }
        }

        public void SetColor(ConsoleColor color)
        {
            defaultColor = color;
        }
#if DEBUG
        public static void LogTrace(string msg = default)
        {
            StackTrace st = new StackTrace(0, true);
            StackFrame[] stFrames = st.GetFrames();

            var frame = stFrames[1];
            var methodBase = frame.GetMethod();
            Console.WriteLine($"TRACE:  {methodBase.DeclaringType}.{methodBase.Name}(). line:{frame.GetFileLineNumber()}. col:{frame.GetFileColumnNumber()}");
            //Console.WriteLine($"StackTrace: " + Environment.StackTrace.Split(Environment.NewLine).ElementAt(2));
        }

        public void Enable()
        {
            filter = defaultFilterEnabled;
        }
        public void Disable()
        {
            filter = defaultFilterDisabled;
        }
#endif
    }

    public class DevToolsForConsole
    {

    }

    public static class ConsoleWrap
    {
        public static readonly object lockWrite = new object();
        private static readonly object lockClear = new object();
        public static void Yellow(string message)
        {
            lock (lockWrite)
            {
                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(message);
                Console.ForegroundColor = color;
            }
        }
        public static void DarkRed(string message)
        {
            lock (lockWrite)
            {
                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine(message);
                Console.ForegroundColor = color;
            }
        }
        public static void Red(string message)
        {
            lock (lockWrite)
            {
                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(message);
                Console.ForegroundColor = color;
            }
        }
        public static void Green(string message)
        {
            lock (lockWrite)
            {
                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(message);
                Console.ForegroundColor = color;
            }
        }
        public static void Cyan(string message)
        {
            lock (lockWrite)
            {
                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(message);
                Console.ForegroundColor = color;
            }
        }
        public static void Magenta(string message)
        {
            lock (lockWrite)
            {
                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine(message);
                Console.ForegroundColor = color;
            }
        }

        public static void ClearLines(int countLines)
        {
            lock (lockClear)
            {
                string empty = new string(' ', Console.WindowWidth - 1);

                for (int i = 0; i < countLines; i++)
                {
                    Console.Write('\r');
                    Console.Write(empty);
                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                }
                Console.Write(empty);
                Console.Write('\r');
            }
        }
    }
}
