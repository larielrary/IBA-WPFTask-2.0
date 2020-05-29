using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;

namespace WPFTask
{
    public class JsonFileService : IFileService
    {
        public List<Person> Open(string filename)
        {
            List<Person> phones = new List<Person>();
            DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(List<Person>));
            using (FileStream fs = new FileStream(filename, FileMode.OpenOrCreate))
            {
                phones = jsonFormatter.ReadObject(fs) as List<Person>;
            }

            return phones;
        }

        public void Save(string filename, List<Person> personsList)
        {
            DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(List<Person>));
            using (FileStream fs = new FileStream(filename, FileMode.Create))
            {
                jsonFormatter.WriteObject(fs, personsList);
            }
        }
    }
}
