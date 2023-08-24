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
                    ToCSV();
                    break; 
                }
                case FileStructure.JSON:
                {
                    ToJSON();
                    break; 
                }
                case FileStructure.XML:
                {
                    ToXML();
                    break; 
                }
                case FileStructure.PT:
                default:
                {
                    ToPT();
                    break; 
                }
            }
        }

        private string ToJSON()
        {
            if (DataToSerialize == null)
            {throw new Exception("Please use AddData() before using a To...() function");}

            return JsonSerializer.Serialize(DataToSerialize, new JsonSerializerOptions 
            { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, WriteIndented = true });
        }

        private string ToXML() 
        {
            if (DataToSerialize == null)
            { throw new Exception("Please use AddData() before using a To...() function"); }

            XmlSerializer XMLer = new XmlSerializer(typeof(DataDoc));
            using (StringWriter Writer = new StringWriter())
            {
                XMLer.Serialize(Writer, DataToSerialize);
                return Writer.ToString();
            }
        }

        private string ToCSV()
        {
            if (DataToSerialize == null)
            { throw new Exception("Please use AddData() before using a To...() function"); }

        }

        private string ToPT()
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

            if (IsTpl)
            {

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
