using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace i200.ElasticSearch.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var search = new I200.ElasticSearch.ElasticSearch("http://192.168.20.100:9200");

            var usebasic=new UserBasic();
            usebasic.account_id = 397;
            usebasic.user_initials = "mq";
            usebasic.user_name = "马强";
            usebasic.user_phone = "18509915185";
            usebasic.user_pinyin = "maqiang";
            usebasic.user_id = 99999;
            usebasic.user_cardno = "99999";

            var usebasic2 = new UserBasic();
            usebasic2.account_id = 119;
            usebasic2.user_initials = "my";
            usebasic2.user_name = "马云";
            usebasic2.user_phone = "18509915988";
            usebasic2.user_pinyin = "mayun";
            usebasic2.user_id = 8888;
            usebasic2.user_cardno = "8888";


            var userList=new List<UserBasic>();
            userList.Add(usebasic);
            userList.Add(usebasic2);

            //var oResult = search.BlukDocumentPut(397, 2, userList);

            //search.DocumentPut(397, "user_basic", "9999999", usebasic);

            //Search
            //Console.WriteLine("输入搜索词：");
            //var key = "";
            //do
            //{
            //    key = Console.ReadLine();
            //    var oResult = search.Search<UserBasic>(50, key, 397, 397);
            //    var oItem = oResult.FirstOrDefault();
            //    if (oItem == null)
            //    {
            //        Console.WriteLine("无结果");
            //    }
            //    else
            //    {
            //        foreach (var item in oResult.ToList())
            //        {
            //            Console.WriteLine("{0}-{1}-{2}-{3}", item.user_id, item.user_name, item.user_phone, item.account_id);
            //        }
            //    }
            //} while (key != "exit");

            
            //Delete Document
            //search.BlukDocumentDelete(119);

            //Delete Bluk
            List<string> ids=new List<string>();
            ids.Add("7346320");
            ids.Add("7304970");
            search.BlukDocumentDelete(ids);



            Console.ReadLine();
        }


        public class UserBasic
        {
            public long user_id { get; set; }

            public string user_name { get; set; }

            public string user_cardno { get; set; }

            public string user_phone { get; set; }

            public string user_initials { get; set; }

            public string user_pinyin { get; set; }

            public long account_id { get; set; }
        }
    }
}
