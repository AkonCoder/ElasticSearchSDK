using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nest;

namespace I200.ElasticSearch.Models.User
{
    [ElasticsearchType(Name = "user_basic")]
    public class UserBasic
    {
        [Number(NumberType.Long, Name = "user_id", Index = NonStringIndexOption.No)]
        public long user_id { get; set; }

        [String(Name = "user_name", Index = FieldIndexOption.NotAnalyzed)]
        public string user_name { get; set; }

        [String(Name = "user_cardno", Index = FieldIndexOption.NotAnalyzed)]
        public string user_cardno { get; set; }

        [String(Name = "user_phone", Index = FieldIndexOption.NotAnalyzed)]
        public string user_phone { get; set; }

        [String(Name = "user_initials", Index = FieldIndexOption.NotAnalyzed)]
        public string user_initials { get; set; }

        [String(Name = "user_pinyin", Index = FieldIndexOption.NotAnalyzed)]
        public string user_pinyin { get; set; }

        [Number(NumberType.Long, Name = "account_id", IncludeInAll = false)]
        public long account_id { get; set; }

        [Number(NumberType.Long, Name = "master_id", IncludeInAll = false)]
        public long master_id { get; set; }
    }
}
