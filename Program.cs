using System.Diagnostics;

namespace FileLister
{
    public class Program
    {
        #region Fields and shit
        static private ScanStates _ScanState { get; set; } = ScanStates.NotStarted;
        static private ScanStates ScanState
        {
            get => _ScanState;
            set
            {
                if (value != _ScanState)
                {
                    _ScanState = value;
                    Console.WriteLine($"Status: {_ScanState}\n");
                }
            }
        }
        static private List<FileInfo> Files = new List<FileInfo>();
        static private string CurrentPath = "";
        static private Dictionary<FileLayoutOptions, bool> FOptions = new Dictionary<FileLayoutOptions, bool>()
        {
            {FileLayoutOptions.Path, false },
            {FileLayoutOptions.Name, false },
            {FileLayoutOptions.PN, false },
            {FileLayoutOptions.C, false },
            {FileLayoutOptions.CS, false },
            {FileLayoutOptions.NL, false }
        };
        static private FileStructure FStructure = FileStructure.PT;
        static private SearchOption SOption = SearchOption.TopDirectoryOnly;
        static private Converter Conv;
        static private List<string> FileTypes = new List<string>();
        #endregion

        public static void Main(string[] args)
        {
            if (args.Length > 0)
            { GetArguments(args); }

            Conv = new Converter(FOptions);

            CurrentPath = Directory.GetCurrentDirectory();

            ScanState = ScanStates.Started;

            CollectFiles();

            ScanState = ScanStates.FilesScanned;

            //ToDo will rename eventually
            object? Temp = null;

            if (FOptions[FileLayoutOptions.PN])
            { Temp = GetPN(Files); }
            else
            { Temp = GetFileData(Files); }

            if (Temp == null)
            { throw new NullReferenceException("Temp was null!"); }

            ScanState = ScanStates.CreatingFileList;

            if (File.Exists(Path.Combine(CurrentPath, "FileList.txt")))
            {
                ScanState = ScanStates.Waiting;

                bool TempInputValid;

                do
                {
                    TempInputValid = true;
                    Console.WriteLine("FileList.txt already exists. What would you like to do?");
                    Console.WriteLine("[O]verwrite, [N]ew File, [C}ancel");

                    ConsoleKeyInfo Input = Console.ReadKey();

                    Console.WriteLine();

                    switch (Input.KeyChar)
                    {
                        case 'O':
                        case 'o':
                        {
                            CreateFile(Temp, false);
                            break;
                        }
                        case 'N':
                        case 'n':
                        {
                            CreateFile(Temp, true);
                            break;
                        }
                        case 'c':
                        case 'C':
                        { return; }
                        default:
                        {
                            Console.WriteLine("Please enter a valid option");
                            TempInputValid = false;
                            break;
                        }
                    }
                } while (!TempInputValid);
            }
            else
            { CreateFile(Temp, false); }
        }

        private static void CollectFiles()
        {
            //Gets files from the current directory
            try
            { Files = GetFiles(SOption); }
            catch (Exception EXC)
            { Debug.WriteLine(EXC.Message); }
        }

        private static void CreateFile(object _Temp, bool _IncludeDate)
        {
            string StrPath = string.Empty;

            if (_IncludeDate)
            { StrPath = Path.Combine(CurrentPath, $"FileList{DateTime.Now.Date.ToString("dd-MM-YYYY")}.txt"); }
            else
            { StrPath = Path.Combine(CurrentPath, "FileList.txt"); }

            using (FileStream FStream = File.Create(StrPath))
            {
                Conv.AddData(_Temp, FOptions[FileLayoutOptions.PN]);
                ScanState = ScanStates.SerialisingData;
                Conv.Serialise(FStream, FStructure);
                ScanState = ScanStates.Finished;
                FStream.Close();
            }
        }

        private static void GetArguments(string[] _Args)
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
                else
                {
                    string Temp = arg.Trim('\"');

                    FileTypes = Temp.Split(' ').ToList();
                }
            }
        }

        private static List<FileInfo> GetFiles(SearchOption _SO)
        {
            Debug.WriteLine("Hmm?");

            List<FileInfo> TempFiles = new List<FileInfo>();

            try
            {
                if (FileTypes.Count <= 0)
                {TempFiles = new DirectoryInfo(CurrentPath).GetFiles("", _SO).ToList<FileInfo>();}
                else
                {
                    TempFiles = new DirectoryInfo(CurrentPath).GetFiles("", _SO)
                                        .Where(X => FileTypes.Contains(Path.GetExtension(X.Name)))
                                        .ToList();
                }
            
            }
            catch (Exception EXC)
            { Console.WriteLine(EXC.Message); }

            return TempFiles;
        }

        private static List<(string, string)> GetPN(List<FileInfo> _Files)
        {
            List<(string, string)> Temp = new List<(string, string)>();

            foreach (var _File in _Files)
            { Temp.Add(new(_File.Name, GetPath(_File.FullName))); }

            return Temp;
        }

        private static string GetPath(string _FullPath)
        {
            int Position = _FullPath.LastIndexOf("\\");

            return _FullPath.Substring(0, Position);
        }

        private static List<string> GetFileData(List<FileInfo> _Files)
        {
            List<string> Temp = new List<string>();

            if (FOptions[FileLayoutOptions.Name])
            {
                foreach (var _File in _Files)
                { Temp.Add(_File.Name); }
            }
            else if (FOptions[FileLayoutOptions.Path])
            {
                foreach (var _File in _Files)
                { Temp.Add(_File.FullName); }
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
            NotStarted,
            Waiting
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
