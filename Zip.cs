using System;
using System.Text;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Text.RegularExpressions;


[assembly: AssemblyTitle("Zip: Line-by-line merge of files")]
[assembly: AssemblyDescription("����������� ����� ������")]
[assembly: AssemblyFileVersion("1.0.0.3")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("")]
[assembly: AssemblyCopyright("Copyright � Garry Galler 2015")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]


namespace Sharp
{
    
    class Program
    {
        
        internal static Encoding encodingIn;
        internal static Encoding encodingOut;
        internal static string exePath = Assembly.GetExecutingAssembly().Location;
        [STAThread]
        static int Main(string[] args)
        {
            string[] param;
            bool help = args.Length == 0, stdOut = false, numeration = false;
            string[] files = { };
            string mask = "*.*", resultFile = "", filesPath = "", firstFile = "", fileList = "";
            string separator = "";
            int codepage;
            //Console.WriteLine(Program.exePath);
            try {
                foreach (string  arg  in  args) {
                    param = arg.Split(new char[] { ':' }, 2);
                    switch (param[0].ToLower()) {
                        case "/path":
                        case "/p":
                            filesPath = AppDomain.CurrentDomain.BaseDirectory;
                            if (param.Length > 1) {
                                if (param[1].StartsWith("@"))
                                {
                                    fileList = param[1].TrimStart(new []{'@'});
                                    if (fileList != "") {
                                        fileList  = new FileInfo(fileList).FullName;
                                        files = File.ReadAllLines(fileList);
                                    } else {
                                        throw new Exception("The path to the file list is not specified or does not exist");
                                    }
                                    break;
                                }
                                // ����� ���������� � ��������� ������ ������ ������ �� ������� 
                                if (param[1].StartsWith("+"))
                                {
                                    fileList = param[1].TrimStart(new char[]{'+'});
                                    files = fileList.Split(new char[]{',','+','|'});
                                    break;
                                }
                                
                                // ����� ��������� ����� /p: �� �������
                                string[] tmp = param[1].Split(new char[]{','} , 2);
                                filesPath = tmp[0] != "" ? tmp[0] : filesPath;
                                
                                FileInfo fsi = new FileInfo(filesPath);
                                // ���� �������� �� ���������� - ��������� 
                                if ((fsi.Attributes & FileAttributes.Directory) != FileAttributes.Directory )
                                {
                                    // � ���� ������� ��� ���������
                                    firstFile = fsi.FullName;
                                    filesPath = fsi.DirectoryName;
                                    mask = "*" + fsi.Extension;
                                }
                                if (tmp.Length > 1) {
                                    mask = tmp[1] != "" ? tmp[1].Trim() : mask;
                                }   
                                
                                files = Directory.GetFiles(filesPath, mask);
                            // ���� ��  ������ - ���������� ������� ���������
                            } else {
                                files = Directory.GetFiles(filesPath, mask);
                            }
                            break;
                        case "/result":
                        case "/r":
                            resultFile = param.Length > 1 ? param[1] : resultFile;
                            break;
                        case "/sep":
                        case "/s":
                            separator = param.Length > 1 ? param[1] : separator;
                            break;
                        case "/stdout":
                            stdOut = true;
                            numeration = param.Length > 1 && param[1]=="n";
                            break;
                        case "/cpin":   
                            if (param.Length > 1 && param[1] != "") {
                                if (Int32.TryParse(param[1], out codepage)) {
                                        encodingIn = Encoding.GetEncoding(codepage);
                                } else {
                                        encodingIn = Encoding.GetEncoding(param[1]);
                                }
                            } else {
                                encodingIn = Encoding.Default;
                                }
                            break;
                        case "/cpout":  
                            if (param.Length > 1) {
                                if (Int32.TryParse(param[1], out codepage)) {
                                        encodingOut = Encoding.GetEncoding(codepage);
                                } else {
                                        encodingOut = Encoding.GetEncoding(param[1]);
                                }
                            } else {
                                encodingOut = Encoding.Default;
                                }
                            break;
                        case "/help":
                        case "/?":
                            help = true;
                            break;
                        default:
                            Console.WriteLine("Invalid option: " + param[0]);
                            return 1;
                    }
                }
            
            
            } catch (Exception ex) {
                Console.WriteLine("Error parsing arguments:{0}\n{1}|{2}", ex.Message, filesPath, mask);
                return 1;
            }
            
            
            if (help) {
                HelpMe();
                return 0;
            }
            // ���� �� ���������� ���� ������ �� ����� - ����� ������
            if (files.Length == 0) {
                Console.WriteLine("Files are not found: {0},{1}", filesPath, mask);
                return 1;
            }
            
            Dict.Zip(firstFile, files, resultFile, separator, stdOut, numeration, fileList);
            return 0;
        }
    
    
        internal static void HelpMe()
        {
        
        Console.WriteLine(
        "\nZip, ver. 1.0.0.3: (C) Garry Galler, 2015\n" +
        "Line-by-line merge of files.\n" +
        "������������ ������� ����� �� ������ ������.\n\n" +
        "������������ ��� �������� ������ ������������������ � ���������, ������ ��������\n" + 
        "�������� ����� �� �� ������ ������������������. ���� ������������������ �����\n" +
        "�� ���������� ����� ���������, ����������� ���������� �� ��� ���, ���� �� �����\n" + 
        "��������� ����� ����� �� ���.\n\n" +
        "zip [/path:<�������,�����|���� �� �����,�����>] [/result:<�������������� ����>] [/sep:<�����������>\n" +
        "zip /p:c:\\test,*.txt  /����� *.txt � ��������� ��������/\n" +
        "zip /p:1.txt,*.txt    /����� *.txt ������� � ����� 1.txt/\n" +
        "zip /p:,*.dic         /����� *.dic � �������� ���������/\n" +
        "zip /p                /��� ����� � �������� ���������/\n" +
        "zip /p:+5.txt,4.txt   /����� �� ������ ������� � ����� 5.txt/\n" +
        "zip /p:@list.ini      /����� �� ������  ������ � list.ini/\n" +
        "zip /p /s:-           /����������� ����� �� �������� ����������� '�����'/\n\n" +
        " /p[ath]:    ������� ������������ ������ ��� �����������. ���� ������� ������ ����\n" +
        "             �� ����� - �� ����� ��������� ��������� ������ ��� ������ �������.\n" +
        "             ����� �������� ���������� �������.\n" +
        " /p[ath]:@   ���� �� ����� �� ������� ������ ��� �������.\n" +
        " /p[ath]:+   ������ ������ ����� ������� ��� ���� + ��� �������.\n" +
        " /r[esult]:  ���� �� ��������������� �����.\n" +
        "             ��� �������� ������ ����� �� ����� ������ � ����� �������� ������.\n" +
        " /s[ep]:     ����������� ��� ������������ �����.\n" +
        "             �� ��������� - ������ ������.\n" +
        " /stdout:[n] ����� ���������� � �������, � �� � ����.\n" +
        "             ����� n �������� ��������� �����.\n" +
        " /cpin:      ��������� ��� �������� �����.\n"+
        "             �� ��������� - ������� ��������� �������.\n" +
        " /cpout:     ��������� ��� ������ �����.\n" +
        "             �� ��������� - ������� ��������� �������.\n" +
        "             ������ ���������: 866 ��� cp866; 1251 ��� Windows-1251; UTF-8 ��� 65001 � �.�.\n" +
        " /?          ������� �� ������ ��������� ������.\n\n" +
        "��������� ����������: .NET Framework �� ���� 4.0."
            );
        }
    
    }
    
    public static class Dict
    {
        public static int Zip(string firstFile, string[] args, string resultFile, string separator, bool stdOut, bool numeration, string fileList)
        {  
            try {
                string outputDir = new FileInfo(args[0]).DirectoryName;
                string workingdDir  = AppDomain.CurrentDomain.BaseDirectory;
                //Encoding encodingFile;
                //using (StreamReader sr = new StreamReader(args[0], true)) {
                    //encodingFile = sr.CurrentEncoding;
                    //}
                if (Program.encodingIn==null) {
                    Program.encodingIn = Encoding.Default;
                }
                if (Program.encodingOut==null) {
                    Program.encodingOut= Encoding.Default;
                }
                    
                if (resultFile == "") resultFile = outputDir + "\\result.txt";
                
                Console.WriteLine("\r\nWorking directory:[{0}]", workingdDir);
                Console.WriteLine("Output directory: [{0}]\r\n", outputDir);
                var excludeFiles = new List<string>() 
                {
                    Program.exePath,
                    firstFile,
                    fileList
                };
                
                //var files = args.Where(w=>File.Exists(w)).Except(excludeFiles).ToList();
                var files2 = args.Where(w=>File.Exists(w)).ToList();
                List<string> files = new List<string>();
                foreach (string file in files2) {
                    if (!excludeFiles.Contains(file)) {
                        //Console.WriteLine(file);
                        files.Add(file);
                    }
                }
                
                if (!string.IsNullOrEmpty(firstFile)) {files.Insert(0,firstFile);}
                
                switch (files.Count)
                {
                    case 0:
                        Console.WriteLine("Files from the list {0} aren't found",fileList);
                        return 1;
                    case 1:
                        Console.WriteLine("The number of files to merge to be more than one");
                        return 1;
                }
                
                
                Console.WriteLine("List Files:[{0}]\r\n",files.Count);
                foreach (string file in files) Console.WriteLine(file);
                
                Console.WriteLine(new String('=', 50));
                Console.WriteLine("Encoding default:   {0}|page:{1}",Encoding.Default.EncodingName,Encoding.Default.CodePage);
                //Console.WriteLine("Encoding file:   {0}|page:{1}",    encodingFile.EncodingName,encodingFile.CodePage);
                Console.WriteLine("Encoding set input: {0}|page:{1}",   Program.encodingIn.EncodingName,Program.encodingIn.CodePage);
                Console.WriteLine("Encoding set output:{0}|page:{1}",   Program.encodingOut.EncodingName,Program.encodingOut.CodePage);
                
                
                string startTime = DateTime.Now.ToString();
                long startTiks = DateTime.Now.Ticks;//���������� ���-�� ����� � ������ ������.
                
                var words = File.ReadLines(files[0], Program.encodingIn);
                for (int i = 1; i < files.Count; i++) {
                    words = 
                        words.Zip(
                            File.ReadLines(files[i], Program.encodingIn), 
                                (first, second) => first + separator + second)
                                    .AsParallel()
                                        .AsOrdered();
                }
                long count = 0;
                if (stdOut) {
                    Console.WriteLine(new String('=', 50));
                    if (numeration) {
                        foreach (string w in words) {Console.WriteLine("[{0}]{1}",count,w);count++;}
                    } else {
                        foreach (string w in words) {Console.WriteLine(w);count++;}}
                } else {
                    File.WriteAllLines(resultFile, words, Program.encodingOut);         
                }
                words = null;
                files = null;
                files2 =null;
        // --------����� ���������� ����--------------------
                Console.WriteLine(new String('=', 50));
                Console.WriteLine("Start time: {0,-15}", startTime);
                TimeSpan totalTimeSpan = new TimeSpan(DateTime.Now.Ticks - startTiks);
                Console.WriteLine("Total time: {0}", totalTimeSpan);
                Console.WriteLine("End time:   {0}", DateTime.Now);
                Console.WriteLine("Combine files completed.");
                Console.WriteLine(new String('=', 50));
                //--------------------------------------------------
                if (!stdOut) {
                    long fileSize = new FileInfo(resultFile).Length;
                    string fileSize2 =  fileSize > 1024 ? (fileSize/1024) +" kb" : fileSize +" byte";
                    Console.WriteLine(
                    "Result: {0}\nSize:   {1}\nRows:   {2}",
                        resultFile, 
                            fileSize2,
                                File.ReadLines(resultFile).Count());
                } else {Console.WriteLine("Rows:   {0}",count);}
            } catch (Exception ex) {
                Console.WriteLine("Error:{0}", ex.Message);
                return 1;
            }
            
            return 0;
        }
        
    }
}
