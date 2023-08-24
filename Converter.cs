using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Runtime.InteropServices;
using System.Net;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace FileLister
{
    public class Converter
    {
        private DataDoc? DataToSerialize;
        private Dictionary<FileLayoutOptions, bool> FOptions;
        private bool IsTpl = false;

        public Converter(Dictionary<FileLayoutOptions, bool> fOptions)
        {FOptions = fOptions;}

        public void AddData(object _FileData, bool _IsTplList)
        {
            IsTpl = _IsTplList;
            DataToSerialize.AddData(_FileData, _IsTplList);
        }

        public void Serialise(FileStream _FStream, FileStructure _FStructure)
        {
            switch (_FStructure)
            {
                case FileStructure.CSV:
                {
                    ToCSV(_FStream);
                    break; 
                }
                case FileStructure.JSON:
                {
                    ToJSON(_FStream);
                    break; 
                }
                case FileStructure.XML:
                {
                    ToXML(_FStream);
                    break; 
                }
                case FileStructure.PT:
                default:
                {
                    ToPT(_FStream);
                    break; 
                }
            }
        }

        private void ToJSON(FileStream _FStream)
        {
            if (DataToSerialize == null)
            {throw new Exception("Please use AddData() before using a To...() function");}

            //return JsonSerializer.Serialize(DataToSerialize, new JsonSerializerOptions 
            //{ DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, WriteIndented = true });

            JsonSerializer.Serialize((Stream)_FStream, DataToSerialize, new JsonSerializerOptions
            { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, WriteIndented = true });
        }

        private void ToXML(FileStream _FStream) 
        {
            if (DataToSerialize == null)
            { throw new Exception("Please use AddData() before using a To...() function"); }

            XmlSerializer XMLer = new XmlSerializer(typeof(DataDoc));

            XMLer.Serialize((Stream)_FStream, DataToSerialize);

            //using (StringWriter Writer = new StringWriter())
            //{
            //    XMLer.Serialize(Writer, DataToSerialize);
            //    return Writer.ToString();
            //}
        }

        private void ToCSV(FileStream _FStream)
        {
            if (DataToSerialize == null)
            { throw new Exception("Please use AddData() before using a To...() function"); }

            String Separator = ",";

            using (StreamWriter Writer = new StreamWriter(_FStream))
            {
                if (IsTpl)
                {
                    Writer.WriteLine("File Path \t File Name");

                    foreach (var _File in DataToSerialize.FilesTpl)
                    {Writer.WriteLine($"{_File.Item1}{Separator}{_File.Item2}");}
                }
                else
                {
                    if (FOptions[FileLayoutOptions.Path])
                    {Writer.WriteLine("Path");}
                    else if (FOptions[FileLayoutOptions.Name])
                    {Writer.WriteLine("File Name");}

                    foreach (var _File in DataToSerialize.Files)
                    {Writer.WriteLine($"{_File}{Separator}");}
                }
            }
        }

        private void ToPT(FileStream _FStream)
        {
            if (DataToSerialize == null)
            { throw new Exception("Please use AddData() before using a To...() function"); }

            string Separator = "";

            if (FOptions[FileLayoutOptions.C])
            {Separator += ",";}
            if (FOptions[FileLayoutOptions.NL])
            {Separator += "\n";}
            if (FOptions[FileLayoutOptions.CS])
            {Separator += ", ";}

            using (StreamWriter Writer = new StreamWriter(_FStream))
            {
                if (IsTpl)
                {
                    foreach (var _File in DataToSerialize.FilesTpl)
                    {Writer.WriteLine($"{_File.Item1},{_File.Item2}{Separator}");}
                }
                else
                {
                    foreach (var _File in DataToSerialize.Files)
                    {Writer.WriteLine($"{_File}{Separator}");}
                }
            }
        }
    }

    [Serializable]
    internal class DataDoc
    {
        [JsonPropertyName("Files")]
        public List<string> Files { get; set; } = null;

        public List<(string, string)> FilesTpl { get; set; } = null;

        public void AddData(object _FileData, bool _IsTplList)
        {
            if (_IsTplList)
            {
                FilesTpl = new List<(string, string)>();
                FilesTpl = (List<(string, string)>)_FileData; 
            }
            else
            {
                Files = new List<string>();
                Files = (List<string>)_FileData; 
            }
        }
    }


}
