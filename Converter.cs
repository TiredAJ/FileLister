﻿using System.Text.Json;
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

            DataToSerialize = new DataDoc();

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
            { throw new Exception("Please use AddData() before using a To...() function"); }

            if
            ((IsTpl && DataToSerialize.FilesTpl == null) ||
                (IsTpl && DataToSerialize.Files == null))
            { throw new NullReferenceException("DataToSerialise.Files(Tpl) was null!"); }

            //return JsonSerializer.Serialize(DataToSerialize, new JsonSerializerOptions 
            //{ DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, WriteIndented = true });

            JsonSerializer.Serialize((Stream)_FStream, DataToSerialize, new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = true,
                IncludeFields = true
            });
        }

        private void ToXML(FileStream _FStream)
        {
            if (DataToSerialize == null)
            { throw new Exception("Please use AddData() before using a To...() function"); }

            if
            ((IsTpl && DataToSerialize.FilesTpl == null) ||
                (IsTpl && DataToSerialize.Files == null))
            { throw new NullReferenceException("DataToSerialise.Files(Tpl) was null!"); }

            XmlSerializer XMLer = new XmlSerializer(typeof(DataDoc));

            XMLer.Serialize(_FStream, DataToSerialize);

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
                    if (DataToSerialize.FilesTpl == null)
                    { throw new NullReferenceException("DataToSerialise.FilesTpl was null!"); }

                    Writer.WriteLine("File Path \t File Name");

                    foreach (var _File in DataToSerialize.FilesTpl)
                    { Writer.WriteLine($"{_File.Item1}{Separator}{_File.Item2}"); }

                }
                else
                {
                    if (DataToSerialize.Files == null)
                    { throw new NullReferenceException("DataToSerialise.Files was null!"); }

                    if (FOptions[FileLayoutOptions.Path])
                    { Writer.WriteLine("Path"); }
                    else if (FOptions[FileLayoutOptions.Name])
                    { Writer.WriteLine("File Name"); }

                    foreach (var _File in DataToSerialize.Files)
                    { Writer.WriteLine($"{_File}"); }
                }
            }
        }

        private void ToPT(FileStream _FStream)
        {
            if (DataToSerialize == null)
            { throw new Exception("Please use AddData() before using a To...() function"); }

            string Separator = "";

            if (FOptions[FileLayoutOptions.C])
            { Separator += ","; }
            if (FOptions[FileLayoutOptions.NL])
            { Separator += "\n"; }
            if (FOptions[FileLayoutOptions.CS])
            { Separator += ", "; }

            using (StreamWriter Writer = new StreamWriter(_FStream))
            {
                if (IsTpl)
                {
                    if (DataToSerialize.FilesTpl == null)
                    { throw new NullReferenceException("DataToSerialise.FilesTpl was null!"); }

                    foreach (var _File in DataToSerialize.FilesTpl)
                    { Writer.WriteLine($"{_File.Item1},{_File.Item2}{Separator}"); }
                }
                else
                {
                    if (DataToSerialize.Files != null)
                    { throw new NullReferenceException("DataToSerialise.Files was null!"); }

                    foreach (var _File in DataToSerialize.Files)
                    { Writer.WriteLine($"{_File}{Separator}"); }
                }
            }
        }
    }

    [Serializable, XmlRoot("Root")]
    public class DataDoc
    {
        [JsonPropertyName("Files")]
        public List<string>? Files { get; set; } = null;

        [XmlArray("File-Path")]
        [XmlArrayItem("File")]
        public List<(string Filename, string Path)>? FilesTpl { get; set; } = null;

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
