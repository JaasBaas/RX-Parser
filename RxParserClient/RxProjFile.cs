using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace RxParserClient
{
    [DataContract]
    public class RxProjFile
    {
        public RxProjFile()
        {
            UserExpressions = new ObservableCollection<UserExpression>();
            InputValues = new List<string>();
        }

        [DataMember]
        public ObservableCollection<UserExpression> UserExpressions { get; set; }

        [DataMember]
        public List<string> InputValues { get; set; }

        public string FileName { get; set; }

        public void Save()
        {
            if (string.IsNullOrEmpty(FileName))
                throw new System.IO.FileNotFoundException();

            var json = "";
            var js = new DataContractJsonSerializer(typeof(RxProjFile));
            using (var ms = new MemoryStream())
            {
                js.WriteObject(ms, this);
                ms.Position = 0;

                using (var sr = new StreamReader(ms))
                {
                    json = sr.ReadToEnd();
                }
            }

            System.IO.File.WriteAllText(FileName, json);
        }

        public static RxProjFile Load(string fileName)
        {
            var json = System.IO.File.ReadAllText(fileName);
            RxProjFile result;

            using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(json)))
            {
                var d = new DataContractJsonSerializer(typeof(RxProjFile));
                result = (RxProjFile)d.ReadObject(ms);
                result.FileName = fileName;
            }

            return result;
        }
    }
}
