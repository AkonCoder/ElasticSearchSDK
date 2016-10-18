using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace I200.ElasticSearch.ReIndex.Models
{
    public class UserBasic
    {
        public long user_id { get; set; }

        public string user_name { get; set; }

        public string user_cardno { get; set; }

        public string user_phone { get; set; }

        public string user_initials { get; set; }

        public string user_pinyin { get; set; }

        public long account_id { get; set; }

        public long master_id { get; set; }
    }

    public class UserBasicModel
    {
        public long uid { get; set; }
        public string uNumber { get; set; }
        public string uName { get; set; }
        public string uPhone { get; set; }
        public long accId { get; set; }
        public string uPY { get; set; }
        public string uPinYin { get; set; }
    }
}
