using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace I200.ElasticSearch
{
    public interface IElasticSearch
    {
        bool IndexCreate();
        bool IndexDelete();
        bool IndexExists();
        bool DocumentPut<T>(string documentId, T model) where T : class;
        bool DocumentDelete(string documentId);
        IEnumerable<T> Search<T>(int size, string queryWord, long accountId, long masterId) where T : class;
        bool BlukDocumentPut<T>(IEnumerable<T> model) where T : class;
        bool BlukDocumentDelete(IEnumerable<string> documentIds);
        Dictionary<string, long> BlukDocumentDelete(long accountId, long masterId);
    }
}
