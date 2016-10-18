using System;
using System.Collections.Generic;
using I200.ElasticSearch.Models.User;
using Nest;

namespace I200.ElasticSearch
{
    public class ElasticSearch : IElasticSearch
    {
        private readonly ElasticClient _client;
        private string _indexName = "users";
        private string _typeName = "user_basic";

        public ElasticSearch(string connectionString)
        {
            if (String.IsNullOrEmpty(connectionString))
                throw new Exception("Elasticsearch connection string is empty");

            var settings = new ConnectionSettings(new Uri(connectionString));
            _client = new ElasticClient(settings);
        }

        /// <summary>
        /// 创建索引
        /// </summary>
        /// <returns></returns>
        public bool IndexCreate()
        {
            bool bResult = false;

            var oIndexDescriptor = new CreateIndexDescriptor(_indexName).Mappings(map => map
                .Map<UserBasic>(md => md
                    .AutoMap()
                    .AllField(all => all.Analyzer("charSplit"))
                ))
                .Settings(s => s
                    .Analysis(sis => sis
                        .Analyzers(a => a.Custom("charSplit", c => c.Tokenizer("ngram_tokenizer")))
                        .Tokenizers(token => token.NGram("ngram_tokenizer", ng => ng
                            .MinGram(1)
                            .MaxGram(1)
                            .TokenChars(TokenChar.Letter, TokenChar.Digit, TokenChar.Punctuation)
                            ))
                    )
                    .NumberOfReplicas(0)
                    .NumberOfShards(3));

            var oResult = _client.CreateIndex(oIndexDescriptor);

            if (oResult != null && oResult.Acknowledged)
            {
                bResult = true;
            }

            return bResult;
        }

        /// <summary>
        /// 删除索引
        /// </summary>
        /// <returns></returns>
        public bool IndexDelete()
        {
            var bResult = false;

            var oResult = _client.DeleteIndex(_indexName);

            if (oResult != null && oResult.Acknowledged)
            {
                bResult = true;
            }

            return bResult;
        }

        /// <summary>
        /// 索引是否存在
        /// </summary>
        /// <returns></returns>
        public bool IndexExists()
        {
            bool bResult = false;

            var oResult = _client.IndexExists(_indexName);
            if (oResult != null && oResult.Exists)
            {
                bResult = oResult.Exists;
            }
            return bResult;
        }

        /// <summary>
        /// 添加更新文档
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="documentId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool DocumentPut<T>(string documentId, T model) where T : class
        {
            bool bResult = false;

            var oResult = _client.Index(model, i => i
                .Index(_indexName)
                .Type(_typeName)
                .Id(documentId));

            if (oResult != null && oResult.Created)
            {
                bResult = oResult.Created;
            }
            return bResult;
        }

        /// <summary>
        /// 删除文档
        /// </summary>
        /// <param name="documentId"></param>
        /// <returns></returns>
        public bool DocumentDelete(string documentId)
        {
            bool bResult = false;

            var oResult = _client.Delete(new DeleteRequest(_indexName, _typeName, documentId));

            if (oResult != null && oResult.Found)
            {
                bResult = oResult.Found;
            }
            return bResult;
        }

        /// <summary>
        /// 搜索文档
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="size"></param>
        /// <param name="queryWord"></param>
        /// <param name="accountId"></param>
        /// <param name="masterId"></param>
        /// <returns></returns>
        public IEnumerable<T> Search<T>(int size, string queryWord, long accountId = 0, long masterId = 0)
            where T : class
        {
            var matchPhraseQuery = new MatchPhraseQuery()
            {
                Field = "_all",
                Query = queryWord.ToLower(),
                Slop = 0,
                Analyzer = "charSplit",
                MaxExpansions = 1
            };

            var termQuery = new TermQuery();
            if (masterId != 0)
            {
                //全部分店
                termQuery.Field = "master_id";
                termQuery.Value = masterId;
            }
            else
            {
                //限制店铺
                termQuery.Field = "account_id";
                termQuery.Value = accountId;
            }

            var queryContainer = new QueryContainer[]
            {
                termQuery,
                matchPhraseQuery
            };

            return _client.Search<T>(s => s
                .Type(_typeName)
                .Index(_indexName)
                .Size(size)
                .Query(q => q
                    .Bool(b => b
                        .Must(queryContainer))
                )
                .Sort(sort => sort.Ascending("_score"))
                ).Documents;
        }

        /// <summary>
        /// 批量添加文档
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool BlukDocumentPut<T>(IEnumerable<T> model) where T : class
        {
            bool bResult = false;

            var oResult = _client.Bulk(bk => bk
                .Index(_indexName)
                .Type(_typeName)
                .CreateMany(model, (x, y) => x.Id(GetTypeValue(y)))
                );

            //var oResult = _client.Bulk(bk => bk
            //    .Index(indexName)
            //    .Type(_typeName)
            //    .CreateMany(model)
            //    );

            if (oResult != null && !oResult.Errors)
            {
                bResult = !oResult.Errors;
            }
            return bResult;
        }

        /// <summary>
        /// 批量删除文档
        /// </summary>
        /// <param name="documentIds"></param>
        /// <returns></returns>
        public bool BlukDocumentDelete(IEnumerable<string> documentIds)
        {
            bool bResult = false;

            var oResult = _client.Bulk(bk => bk
                .Index(_indexName)
                .Type(_typeName)
                .DeleteMany(documentIds,(x,y)=>x.Id(y))
                );

            if (oResult != null && !oResult.Errors)
            {
                bResult = !oResult.Errors;
            }
            return bResult;
        }

        /// <summary>
        /// 批量删除文档
        /// </summary>
        /// <param name="accountId">店铺id</param>
        /// <param name="masterId">总店id</param>
        /// <returns></returns>
        public Dictionary<string, long> BlukDocumentDelete(long accountId = 0, long masterId = 0)
        {
            var dict = new Dictionary<string, long>();

            string strField;
            long longValue;

            if (masterId != 0)
            {
                //全部分店
                strField = "master_id";
                longValue = masterId;
            }
            else
            {
                //限制店铺
                strField = "account_id";
                longValue = accountId;
            }

            var oResult = _client.DeleteByQuery<UserBasic>(_indexName, _typeName, d => d
                .Query(q => q
                    .Term(strField, longValue)));
            if (oResult != null && !oResult.TimedOut)
            {
                if (oResult.Indices.ContainsKey("_all"))
                {
                    var oItem = oResult.Indices["_all"];
                    dict.Add("deleted", oItem.Deleted);
                    dict.Add("failed", oItem.Failed);
                    dict.Add("found", oItem.Found);
                    dict.Add("missing", oItem.Missing);
                }
            }

            return dict;
        }

        /// <summary>
        /// 获得字段内容
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private string GetTypeValue(object obj)
        {
            dynamic oType = obj;
            return oType.user_id.ToString();
        }
    }
}
