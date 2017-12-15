using System;
using System.Collections.Generic;
using System.Text;
using LangExt;
using Microsoft.WindowsAzure;
using Gbook.Base.Configuration;
using Gbook.Base.IO;
using Gbook.Base.TransientFaultHandling;
using Gbook.Base.IO.WindowsAzure;
using Microsoft.WindowsAzure.Storage;

namespace StubServer.Core
{
    public interface IRepository
    {
        Option<string> TryReadFile(string path);
        IEnumerable<string> ListFiles(string path);
        void WriteFile(string path, string content);
    }

    public class AzureReposiroty : IRepository
    {
        public static readonly string ContainerName = "stub";

        public Option<string> TryReadFile(string path)
        {
            var file = GetFile(path);

            if (file.Exists)
            {
                using (var reader = new System.IO.StreamReader(file.OpenRead()))
                {
                    return Option.Some(reader.ReadToEnd());
                }
            }
            else
            {
                return Option.None;
            }
        }

        static File GetFile(string path)
        {
            return BlobFactory.File(path);
        }

        static FileFactory blobFactory = null;

        static FileFactory BlobFactory
        {
            get
            {
                if (blobFactory == null)
                {
                    var account = CloudStorageAccount.Parse(Config.Get<string>("Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString"));
                    var urlRoot = new Path(account.BlobEndpoint);

                    var retry = RetryPolicies.Retry(3, TimeSpan.FromSeconds(3));
                    blobFactory = new FileFactory(new Blob(account, urlRoot / ContainerName, retry));
                }
                return blobFactory;
            }
        }

        public void WriteFile(string path, string content)
        {
            var file = GetFile(path);

            using (var ms = new System.IO.MemoryStream(Encoding.UTF8.GetBytes(content)))
            {
                if (file.Exists)
                {
                    file.Append(ms);
                }
                else
                {
                    file.Save(ms);
                }
            }
        }


        public IEnumerable<string> ListFiles(string path)
        {
            return AzureReposiroty.BlobFactory.Directory(path).List().Select(f => f.Name);
        }
    }

    public class MemoryReposiroty : IRepository
    {
        readonly string content;

        public MemoryReposiroty(string content)
        {
            this.content = content;
        }

        public Option<string> TryReadFile(string path)
        {
            return Option.Some(content);
        }

        public void WriteFile(string path, string content)
        {
            throw new NotImplementedException();
        }


        public IEnumerable<string> ListFiles(string path)
        {
            throw new NotImplementedException();
        }
    }

    public class MemoryFolderReposiroty : IRepository
    {
        readonly IDictionary<string, string> files;

        public MemoryFolderReposiroty(IDictionary<string, string> files)
        {
            this.files = files;
        }

        public Option<string> TryReadFile(string path)
        {
            var name = System.Linq.Enumerable.LastOrDefault(path.Split('/'));
            if (files.ContainsKey(name))
            {
                return Option.Some(this.files[name]);
            }
            else
            {
                return Option.None;
            }
        }

        public void WriteFile(string path, string content)
        {
            throw new NotImplementedException();
        }


        public IEnumerable<string> ListFiles(string path)
        {
            return ((IEnumerable<string>)files.Keys);
        }
    }

}
