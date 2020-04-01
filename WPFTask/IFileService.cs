using System.Collections.Generic;

namespace WPFTask
{
    public interface IFileService
    {
        List<Person> Open(string filename);
        void Save(string filename, List<Person> personsList);
    }
}
