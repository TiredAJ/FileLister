
using System.Runtime.CompilerServices;

namespace FileLister
{
    public class Program
    {
        static public ScanStates ScanState 
        {
            get { return ScanState; }
            set {Task.Run(() => { Console.WriteLine(ScanState); });}
        }
        static List<FileInfo> Files = new List<FileInfo>();
        static string CurrentPath = "";
        static public Dictionary<FileLayoutOptions,bool> FOptions = new Dictionary<FileLayoutOptions, bool>()
        { 
            {FileLayoutOptions.Path, false },
            {FileLayoutOptions.Name, false },
            {FileLayoutOptions.PN, false },
            {FileLayoutOptions.C, false },
            {FileLayoutOptions.CS, false },
            {FileLayoutOptions.NL, false }
        };
        static FileStructure FStructure = FileStructure.PT;
        static SearchOption SOption = SearchOption.TopDirectoryOnly;
        static Converter Conv;

        static void Main(string[] args)
        {
            if (args.Length > 0)
            {GetArguments(args);}

            Conv = new Converter(FOptions);

            Console.WriteLine("ScanState Directory!");

            CurrentPath = Directory.GetCurrentDirectory();

            Task.Run(() =>
            {
                ScanState = ScanStates.Started;
            
                //Gets files from the current directory
                Files = GetFiles(SOption);

                ScanState = ScanStates.FilesScanned;

                object Temp = null;

                if (FOptions[FileLayoutOptions.PN])
                {Temp = GetPN(Files);}
                else
                {Temp = GetFileData(Files);}

                ScanState = ScanStates.CreatingFileList;
                using (FileStream FStream = File.Create(Path.Combine(CurrentPath, "FileList.txt")))
                {
                    Conv.AddData(Temp, FOptions[FileLayoutOptions.PN]);
                    ScanState = ScanStates.SerialisingData;
                    Conv.Serialise(FStream, FStructure);
                    ScanState = ScanStates.Finished;
                }
            });
        }

        public static void GetArguments(string[] _Args)
        {
            bool FSDone = false; //makes sure that File Structure can only be set once

            foreach (string arg in _Args)
            {
                if (arg[0] == '-' && !FSDone) //File structure
                {
                    switch (arg.ToUpper())
                    {
                        case "-CSV":
                        {
                            FStructure = FileStructure.CSV;
                            break;
                        }
                        case "-JSON":
                        {
                            FStructure = FileStructure.JSON;
                            break;
                        }
                        case "-XML":
                        {
                            FStructure = FileStructure.XML;
                            break;
                        }
                        case "-PT":
                        default:
                        {
                            FStructure = FileStructure.PT;
                            break;
                        }
                    }
                }
                else if (arg[0] == '/') //File option
                {
                    switch (arg.ToUpper())
                    {
                        case "/PATH": 
                        {
                            FOptions[FileLayoutOptions.Path] = true;
                            break;
                        }
                        case "/NAME":
                        {
                            FOptions[FileLayoutOptions.Name] = true;
                            break;
                        }
                        case "/PN":
                        {
                            FOptions[FileLayoutOptions.PN] = true;
                            break;
                        }
                        case "/C":
                        {
                            FOptions[FileLayoutOptions.C] = true;
                            break;
                        }
                        case "/CS":
                        {
                            FOptions[FileLayoutOptions.CS] = true;
                            break;
                        }
                        case "/NL":
                        default:
                        {
                            FOptions[FileLayoutOptions.NL] = true;
                            break;
                        }
                    }
                }
                else if (arg[0] == '+')
                {
                    switch (arg.ToUpper())
                    {                        
                        case "+AD":
                        {
                            SOption = SearchOption.AllDirectories;
                            break;
                        }
                        case "+TD":
                        default:
                        {
                            SOption = SearchOption.TopDirectoryOnly;
                            break;
                        }
                    }
                }
            }
        }

        public static List<FileInfo> GetFiles(SearchOption _SO)
        {return new DirectoryInfo(CurrentPath).GetFiles("",_SO).ToList<FileInfo>();}

        public static List<(string, string)> GetPN(List<FileInfo> _Files)
        {
            List<(string, string)> Temp = new List<(string, string)>();

            foreach (var _File in _Files)
            {Temp.Add(new (_File.Name, GetPath(_File.FullName)));}

            return Temp;
        }

        public static string GetPath(string _FullPath)
        {
            int Position = _FullPath.LastIndexOf('/');

            return _FullPath.Substring(0, Position);
        }

        public static List<string> GetFileData(List<FileInfo> _Files)
        {
            List<string> Temp = new List<string>();

            if (FOptions[FileLayoutOptions.Name])
            {
                foreach (var _File in _Files)
                {Temp.Add(_File.Name);}
            }
            else if (FOptions[FileLayoutOptions.Path])
            {
                foreach (var _File in _Files)
                {Temp.Add(_File.FullName);}
            }

            return Temp;
        }

        public enum ScanStates
        {
            Started,
            FilesScanned,
            CreatingFileList,
            SerialisingData,
            Broke,
            Finished,
            NotStarted
        }
    }

    public enum FileStructure
    {
        CSV,
        JSON,
        XML,
        PT
    }

    public enum FileLayoutOptions : int
    {
        Path,   //path and name (as one)
        Name,   //just name
        PN,     //name and path  (as separate)
        C,      //comma separated
        CS,     //comma with trailing space
        NL      //newline            
    }
}
