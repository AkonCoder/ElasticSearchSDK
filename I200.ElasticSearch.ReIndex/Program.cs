using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using I200.ElasticSearch.ReIndex.Models;
using I200.ElasticSearch.ReIndex.Utils;

namespace I200.ElasticSearch.ReIndex
{
    class Program
    {

        private static ElasticSearch _Search = new ElasticSearch(ConfigurationManager.AppSettings["Elasticsearch"]);

        static void Main(string[] args)
        {
            Show("1.输入 init 初始化全部店铺");
            Show("2.输入 one 初始化部分店铺");
            Show("3.输入 file 初始化部分店铺(来源文件)");
            Show("4.输入 exit 退出");
            Stream inputStream = Console.OpenStandardInput(5000);
            Console.SetIn(new StreamReader(inputStream));
            var key = Console.ReadLine();
            while (key != "exit")
            {
                switch (key)
                {
                    case "init":
                        //初始化全部店铺
                        InitAllProc();
                        break;
                    case "one":
                        //初始化部分店铺
                        InitOne();
                        break;
                    case "file":
                        //初始化部分店铺(从文件)
                        InitFile();
                        break;
                }
            }
        }


        public static void Show(string word)
        {
            Console.WriteLine(word);
            WriteLog("info", word);
        }

        public static void Error(string word)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(word);
            Console.ForegroundColor = originalColor;
            WriteLog("error", word);
        }

        protected static void WriteLog(string type, string content)
        {
            var path = string.Format("{0}/logs", Environment.CurrentDirectory);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"); //获取当前系统时间
            string filename = path + "/" + DateTime.Now.ToString("yyyy-MM-dd") + ".log"; //用日期对日志文件命名

            //创建或打开日志文件，向日志文件末尾追加记录
            StreamWriter mySw = File.AppendText(filename);

            //向日志文件写入内容
            string write_content = time + " " + type + " : " + content;
            mySw.WriteLine(write_content);

            //关闭日志文件
            mySw.Close();
        }

        /// <summary>
        /// 初始化全部
        /// </summary>
        public static void InitAllProc()
        {
            Show("-请输入 index 初始化会员索引");
            Show("-请输入 delindex 删除会员索引");
            Show("-请输入 all 初始化店铺数据");
            var key = Console.ReadLine();
            if (key != null && key.ToLower().Trim() == "index")
            {
                InitUserIndex();
            }
            else if (key != null && key.ToLower().Trim() == "delindex")
            {
                DeleteUserIndex();
            }
            else if (key != null && key.ToLower().Trim() == "all")
            {
                InitDoc();
            }
            else if (key != null && key.ToLower().Trim() == "exit")
            {
                Environment.Exit(0);
            }

            Console.ReadLine();
        }


        /// <summary>
        /// 初始化部分
        /// </summary>
        public static void InitOne()
        {
            Show("-输入店铺id 例如 397 初始化单个店铺");
            Show("-输入店铺id 例如 397,119  初始化多个店铺");
            Stream inputStream = Console.OpenStandardInput(5000);
            Console.SetIn(new StreamReader(inputStream));
            var key = Console.ReadLine();
            if (key != null && key.ToLower().Trim() == "exit")
            {
                Environment.Exit(0);
            }
            else
            {
                if (key != null)
                {
                    List<long> ids = key.TrimEnd(',').Split(',').Select(s => Convert.ToInt64(s)).ToList();
                    //删除数据
                    DeleteUserBasic(ids);
                    //初始化数据
                    InitDoc(ids);
                }
            }

            Console.ReadLine();
        }

        /// <summary>
        /// 初始化部分店铺(从文件)
        /// </summary>
        public static void InitFile()
        {
            Show("-输入当前文件夹下文件名 例:account.txt");
            Show("-文件内容格式：397,119 ");

            var key = Console.ReadLine();
            if (key != null && key.ToLower().Trim() == "exit")
            {
                Environment.Exit(0);
            }
            else
            {
                if (key != null)
                {

                    var path = string.Format("{0}/{1}", Environment.CurrentDirectory, key);

                    if (File.Exists(path))
                    {
                        //读取文件
                        var streamReader = new StreamReader(path);
                        string fileContent = streamReader.ReadToEnd();
                        streamReader.Close();

                        List<long> ids = fileContent.TrimEnd(',').Split(',').Select(s => Convert.ToInt64(s)).ToList();
                        //删除数据
                        DeleteUserBasic(ids);
                        //初始化数据
                        InitDoc(ids);
                    }
                    else
                    {
                        Error(string.Format("未找到文件{0}", key));
                    }
                }
            }

            Console.ReadLine();
        }

        /// <summary>
        /// 初始化索引
        /// </summary>
        public static void InitUserIndex()
        {
            if (!_Search.IndexExists())
            {
                var oResult = _Search.IndexCreate();
                if (oResult)
                {
                    Show("索引(users)创建成功！");
                }
                else
                {
                    Error("索引(users)创建失败！");
                }
            }
            else
            {
                Error("索引(users)已存在！");
            }
        }

        /// <summary>
        /// 删除会员索引
        /// </summary>
        public static void DeleteUserIndex()
        {
            if (_Search.IndexExists())
            {
                var oResult = _Search.IndexDelete();
                if (oResult)
                {
                    Show("索引(users)删除成功！");
                }
                else
                {
                    Error("索引(users)删除失败！");
                }
            }
            else
            {
                Error("索引(users)不存在！");
            }
        }

        /// <summary>
        /// 初始化店铺数据
        /// </summary>
        public static void InitDoc(List<long> ids = null)
        {
            var allWatch = new Stopwatch();
            allWatch.Start();

            Dictionary<long, long> shopList;

            if (ids == null)
            {
                //全部店铺
                shopList = GetShopList();
            }
            else
            {
                //部分店铺
                shopList = GetShopList(ids);
            }

            if (shopList != null && shopList.Count > 0)
            {
                Show(string.Format("获取店铺列表成功，总数{0}", shopList.Count));

                var iCount = 0;
                var accountIds = new List<long>();
                foreach (var shopItem in shopList)
                {
                    accountIds.Add(shopItem.Key);
                    iCount++;
                    if (iCount == 200)
                    {
                        InitUserProc(shopList, accountIds);
                        accountIds.Clear();
                        iCount = 0;
                    }
                }
                if (accountIds.Count > 0)
                {
                    InitUserProc(shopList, accountIds);
                }
            }

            allWatch.Stop();
            if (shopList != null)
                Show(string.Format("店铺数据初始化完成,店铺总数{0} - {1}ms", shopList.Count, allWatch.ElapsedMilliseconds));
        }

        /// <summary>
        /// 批量初始化会员数据
        /// </summary>
        /// <param name="shopMapper"></param>
        /// <param name="ids"></param>
        public static void InitUserProc(Dictionary<long,long> shopMapper,List<long> ids)
        {
            var userList = GetUserBasic(shopMapper,ids);
            if (userList != null && userList.Count > 0)
            {
                Show(string.Format("获取会员列表成功，总数{0}", userList.Count));

                var iUserCount = userList.Count;
                var shopWatch = new Stopwatch();
                shopWatch.Start();

                if (iUserCount <= 5000)
                {
                    InitUserBasic(userList);
                }
                else
                {
                    var iCount = 0;
                    var userPart = new List<UserBasic>();
                    foreach (var userItem in userList)
                    {
                        userPart.Add(userItem);
                        iCount++;
                        if (iCount == 5000)
                        {
                            InitUserBasic(userPart);
                            userPart.Clear();
                            iCount = 0;
                        }
                    }
                    if (userPart.Count > 0)
                    {
                        InitUserBasic(userPart);
                    }
                }

                shopWatch.Stop();

                Show(string.Format("添加完成,会员{0} - {1}ms",iUserCount,shopWatch.ElapsedMilliseconds));
            }
        }
        
        /// <summary>
        /// 批量写入数据
        /// </summary>
        /// <param name="users"></param>
        public static void InitUserBasic(List<UserBasic> users)
        {
            if (_Search.BlukDocumentPut(users))
            {
                //Show(string.Format("数据初始化成功 {0}",users.Count));
            }
            else
            {
                Error(string.Format("数据初始化失败 {0}", users.Count));
            }
        }

        /// <summary>
        /// 批量删除
        /// </summary>
        /// <param name="ids"></param>
        public static void DeleteUserBasic(List<long> ids)
        {
            if (ids != null && ids.Count > 0)
            {
                var masterIds = GetShopList(ids);
                if (masterIds != null && masterIds.Count > 0)
                {
                    foreach (var item in masterIds)
                    {   
                        var delWatch=new Stopwatch();
                        delWatch.Start();
                        var oResult = _Search.BlukDocumentDelete(item.Key);
                        if (oResult != null && oResult.ContainsKey("deleted"))
                        {
                            delWatch.Stop();
                            Show(string.Format("成功删除{0}-{1}总店会员{2} - {3}ms",item.Key, item.Value, oResult["deleted"],
                                delWatch.ElapsedMilliseconds));
                        }
                    }
                }
            }
        }



        /// <summary>
        /// 获得店铺信息
        /// </summary>
        /// <returns>店铺id-总店id</returns>
        public static Dictionary<long,long> GetShopList()
        {
            var strSql = new StringBuilder();
            strSql.Append(" SELECT T_Account.id,max_shop from T_Account WITH (NOLOCK) left outer join T_Business WITH (NOLOCK) on T_Business.accountid=T_Account.id where T_Business.active<>-3 ");

            var model = DapperHelper.Query<AccountBase>(strSql.ToString()).ToList();

            var dict=new Dictionary<long,long>();
            if (model != null && model.Count > 0)
            {
                foreach (var item in model)
                {
                    dict.Add(item.Id,item.max_shop);
                }
            }
            return dict;
        }

        /// <summary>
        /// 获得部分店铺信息
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static Dictionary<long, long> GetShopList(List<long> ids)
        {
            var strShopId = string.Join(",", ids);

            var strSql = new StringBuilder();
            strSql.Append(" SELECT T_Account.id,max_shop from T_Account WITH (NOLOCK) where ");
            strSql.Append(string.Format(" max_shop in (select max_shop from T_Account where T_Account.id in ({0}) group by max_shop)", strShopId));

            var model =
                DapperHelper.Query<AccountBase>(strSql.ToString())
                    .ToList();

            var dict = new Dictionary<long, long>();
            if (model != null && model.Count > 0)
            {
                foreach (var item in model)
                {
                    dict.Add(item.Id, item.max_shop);
                }
            }
            return dict;
        }

        /// <summary>
        /// 获得会员信息
        /// </summary>
        /// <param name="shopMapper"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static List<UserBasic> GetUserBasic(Dictionary<long, long> shopMapper, List<long> ids)
        {
            var oList = new List<UserBasic>();

            var strShopId = string.Join(",", ids);

            var strSql = new StringBuilder();
            strSql.Append(" SELECT uid, uNumber, uName, uPhone, accID, uPY, uPinYin from T_UserInfo WITH (NOLOCK) where ");
            strSql.Append(string.Format(" accid in ({0}) ", strShopId));
 
            var model =DapperHelper.Query<UserBasicModel>(strSql.ToString()).ToList();

            if (model != null && model.Count > 0)
            {
                foreach (var item in model)
                {
                    if (shopMapper.ContainsKey(item.accId))
                    {
                        var masterId = shopMapper[item.accId];
                        var oItem = new UserBasic();
                        oItem.account_id = item.accId;
                        oItem.user_cardno = string.IsNullOrEmpty(item.uNumber) ? "" : item.uNumber.ToLower();
                        oItem.user_initials = string.IsNullOrEmpty(item.uPY) ? "" : item.uPY.ToLower();
                        oItem.user_name = string.IsNullOrEmpty(item.uName) ? "" : item.uName.ToLower();
                        oItem.user_phone = string.IsNullOrEmpty(item.uPhone) ? "" : item.uPhone.ToLower();
                        oItem.user_pinyin = string.IsNullOrEmpty(item.uPinYin) ? "" : item.uPinYin.ToLower();
                        oItem.user_id = item.uid;
                        oItem.master_id = masterId;
                        oList.Add(oItem);
                    }
                    else
                    {
                        Error(string.Format("未找到匹配的总店id [uid:{0},shopid:{1}]",item.uid,item.accId));
                    }
                }
            }

            return oList;
        }
    }
}
