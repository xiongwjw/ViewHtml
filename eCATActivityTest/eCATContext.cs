using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace eCATActivityTest
{
    public class eCATContext
    {
        public string RequestName { get; set; }
        public VIP Vip { get; set; }
        public UserInfo User { get; set; }
        public UserInfo TestUser { get; set; }
        public string TestString { get; set; }
        public UserInfo TestEmptySeriUser { get; set; }
        public List<UserInfo> TestEmptyUserList { get; set; }
        public int testInt { get; set; }
        public IDataCache CardHolderDataCache { get; set; }
        public CreadCardInputInfo FormInfo { get; set; }
        public Role RoleResponse { get; set; }
        public string Image { get; set; }
        [XmlArray("links"), XmlArrayItem("link")]
        public List<UserInfo> UserList { get; set; }
        public string ID { get; set; }
        public eCatBankInterface BankInterface { get; set; }
        public eCatTransactionDataCache TransactionDataCache { get; set; }
        public TerminalConfigClass TerminalConfig { get; set; }
        public Role TestRole { get; set; }

        public FaceUserInfo FaceUserInformation { get; set; }

        public eCATContext()
        {
            InitData();
        }

        public string PrintDebugData()
        {
            StringBuilder sb = new StringBuilder();

            foreach (PropertyInfo pi in this.GetType().GetProperties())
            {
                object value = pi.GetValue(this);
                if (value is IList)
                {
                    sb.Append($"{pi.Name}:type is { value.GetType().Name}");
                    IList item = value as IList;
                    foreach (var obj in item)
                    {
                        string strValue = obj == null ? string.Empty : obj.ToString();
                        sb.Append($"{strValue}").AppendLine();
                    }
                }
                else
                {
                    string strValue = value == null ? string.Empty : value.ToString();
                    sb.Append($"{pi.Name}:{strValue}").AppendLine();
                }

            }
            return sb.ToString();
        }



        public object GetBindData(string key)
        {
            object obj = null;
            TransactionDataCache.Get(key,out obj);
            if (obj == null)
                CardHolderDataCache.Get(key, out obj);
            return obj;
        }

        private void InitData()
        {
            this.FaceUserInformation = new FaceUserInfo();
            this.FaceUserInformation.Uid = "20200828_002";
            this.FaceUserInformation.FullName = "Tester2";
            this.FaceUserInformation.ImageBase64 = "base 64";
            this.FormInfo = new CreadCardInputInfo();
            // this.Image = File.ReadAllText("image.txt");
            this.Image = "reques iamge";
            this.TerminalConfig = new TerminalConfigClass
            {
                Terminal = new TerminalClass
                {
                    ATMNumber = "sssssssssss"
                }
            };

            this.RequestName = "requestname";
            this.User = new UserInfo()
            {
                Age = "23",
                CIF_number = "232323",
                Name = "independenceuser",
                Place = "place",
                IssueDate = "20202023",
                CifType = "cnidcard"
            };

            UserInfo user1 = new UserInfo
            {
                Age = "4324",
                CIF_number = "232323",
                Name = "user1",
                Place = "place",
                IssueDate = "20202023"
            };
            UserInfo user2 = new UserInfo
            {
                Age = "333",
                CIF_number = "21321",
                Name = "user2",
                Place = "place",
                IssueDate = "3312"
            };

            this.Vip = new VIP
            {
                Name = "vip_name",
                Age = "vip_23"
            };


            this.UserList = new List<UserInfo>();
            this.UserList.Add(user1);
            this.UserList.Add(user2);

            TestRole = new Role
            {
                RoleUser = this.User,
                RoleVip = this.Vip

            };

            this.BankInterface = new eCatBankInterface();

            this.TransactionDataCache = new eCatTransactionDataCache();
            this.CardHolderDataCache = new eCatCardHolderDataCache();
            this.TransactionDataCache.Set("s_name", "datacache name", GetType());
            this.TransactionDataCache.Set("sex", "datacache sex", GetType());
            this.TransactionDataCache.Set("s_core_serial", "2222222222", GetType());

        }

    }

    public class TerminalConfigClass
    {
        public TerminalClass Terminal { get; set; }
    }

    public class TerminalClass
    {
        public string ATMNumber = string.Empty;
    }


    public class FaceUserInfo
    {
        public string Uid { get; set; }

        public string FullName { get; set; }

        public int Sex { get; set; }

        public string ImageBase64 { get; set; }
    }


    public class UserInfo
    {
        public string Name { get; set; }

        public string Age { get; set; }

        public string Place { get; set; }

        public string CIF_number { get; set; }

        public string IssueDate { get; set; }

        public string CifType { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"type is {this.GetType().Name}").AppendLine();
            sb.Append($"User.Age:{this.Age}").AppendLine();
            sb.Append($"User.CIF_number:{this.CIF_number}").AppendLine();
            sb.Append($"User.CifType:{this.CifType}").AppendLine();
            sb.Append($"User.IssueDate:{this.IssueDate}").AppendLine();
            sb.Append($"User.Name:{this.Name}").AppendLine();
            sb.Append($"User.Place:{this.Place}").AppendLine();
            return sb.ToString();
        }


    }

    [Serializable]
    [XmlRoot("ROLEAA")]
    public class Role
    {
        public UserInfo RoleUser { get; set; }
        public VIP RoleVip { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"type is {this.GetType().Name}").AppendLine();
            if (RoleUser != null)
                sb.Append(RoleUser.ToString());
            if (RoleVip != null)
                sb.Append(RoleVip.ToString());
            return sb.ToString();
        }
    }

    public class VIP
    {
        public string Name { get; set; }
        public string Age { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"type is {this.GetType().Name}").AppendLine();
            sb.Append($"VIP.Name:{Name}").AppendLine();
            sb.Append($"VIP.Age:{Age}").AppendLine();
            return sb.ToString();
        }
    }



}
