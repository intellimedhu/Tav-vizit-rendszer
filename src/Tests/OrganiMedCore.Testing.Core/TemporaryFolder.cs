using System;
using System.IO;

namespace OrganiMedCore.Testing.Core
{
    public class TemporaryFolder : IDisposable
    {
        private readonly bool _deleteOnDispose;


        public TemporaryFolder(bool deleteOnDispose = true)
        {
            Folder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            if (!Directory.Exists(Folder))
            {
                Directory.CreateDirectory(Folder);
            }

            _deleteOnDispose = deleteOnDispose;
        }


        public string Folder { get; }

        public void Dispose()
        {
            if (_deleteOnDispose)
            {
                if (Directory.Exists(Folder))
                {
                    Directory.Delete(Folder, true);
                }
            }
        }

        public override string ToString()
        {
            return Folder;
        }
    }
}
