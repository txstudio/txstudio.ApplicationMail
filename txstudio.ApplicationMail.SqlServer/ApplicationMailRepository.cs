using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Xml.Serialization;

namespace txstudio.ApplicationMail.SqlServer
{
    public class ApplicationMailRepository : IApplicationMailRepository
    {
        private SqlConnection _conn;
        private SqlCommand _cmd;

        public ApplicationMailRepository()
        {
            this._conn = new SqlConnection();
            this._conn.ConnectionString = @"
                    Data Source=(localdb)\ProjectsV12;
                    Initial Catalog=ApplicationMail;
                    Integrated Security=True;
                    Connect Timeout=15;
                    Encrypt=False;
                    TrustServerCertificate=False;
                    ApplicationIntent=ReadWrite;
                    MultiSubnetFailover=False";
        }

        /// <summary>
        /// 取得應用程式電子郵件設定檔
        /// </summary>
        /// <returns></returns>
        public ApplicationMailConfig GetConfig()
        {
            var _configs = this._conn.Query<ApplicationMailConfig>(
                "ApplicationMail.GetConfig",
                commandType: CommandType.StoredProcedure
                );

            return _configs.FirstOrDefault();
        }


        /// <summary>
        /// 取得位於待寄送的電子郵件清單
        /// </summary>
        /// <returns>待寄送的電子郵件清單</returns>
        public IEnumerable<ApplicationMailMessage> GetMailMessage()
        {
            XmlSerializer _serializer;

            _serializer = new XmlSerializer(typeof(ApplicationMailMessages));

            this._cmd = this._conn.CreateCommand();
            this._cmd.CommandText = "ApplicationMail.GetMailMessageSending";
            this._cmd.CommandType = CommandType.StoredProcedure;

            this._conn.Open();
            var _xmlReader = this._cmd.ExecuteXmlReader();
            var _messages = (ApplicationMailMessages)_serializer.Deserialize(_xmlReader);
            this._conn.Close();

            if (_messages == null)
                return null;

            return _messages.MailMessage;
        }


        /// <summary>
        /// 更新指定電子郵件寄送結果
        /// </summary>
        /// <param name="no">要更新的識別碼</param>
        /// <param name="isSuccess">寄送成功</param>
        /// <param name="message">寄送失敗儲存的訊息</param>
        /// <returns>更新結果</returns>
        public bool UpdateMailMessage(int no
            , bool isSuccess
            , string message)
        {
            var _parameters = new DynamicParameters();
            _parameters.Add("@No", no);
            _parameters.Add("@IsSuccess", isSuccess);
            _parameters.Add("@Message", message);
            _parameters.Add("@Result",
                 dbType: DbType.Boolean,
                 direction: ParameterDirection.Output);

            this._conn.Execute("ApplicationMail.UpdateMailMessage",
                _parameters,
                commandType: CommandType.StoredProcedure
                );

            return _parameters.Get<Boolean>("@Result");
        }


        /// <summary>
        /// 新增指定電子郵件訊息到待寄送清單
        /// </summary>
        /// <param name="address">電子郵件位址</param>
        /// <param name="subject">主旨</param>
        /// <param name="body">內文</param>
        /// <param name="checksum">檢查碼（若待處裡清單有相同檢查碼不進行儲存）</param>
        /// <returns>新建立的待處裡電子郵件內容</returns>
        public int AddMailMessage(DataTable address
            , string subject
            , string body
            , string checksum)
        {
            this._cmd = this._conn.CreateCommand();
            this._cmd.CommandText = "ApplicationMail.AddMailMessage";
            this._cmd.CommandType = CommandType.StoredProcedure;

            this._cmd.Parameters.Add("@Address", SqlDbType.Structured);
            this._cmd.Parameters.Add("@Subject", SqlDbType.NVarChar, 150);
            this._cmd.Parameters.Add("@Body", SqlDbType.NVarChar, 500);
            this._cmd.Parameters.Add("@Checksum", SqlDbType.VarChar, 100);
            this._cmd.Parameters.Add("@No", SqlDbType.Int);

            this._cmd.Parameters["@Address"].TypeName = "ApplicationMail.ApplicationMailAddress";
            this._cmd.Parameters["@No"].Direction = ParameterDirection.Output;

            this._cmd.Parameters["@Address"].Value = address;
            this._cmd.Parameters["@Subject"].Value = subject;
            this._cmd.Parameters["@Body"].Value = body;
            this._cmd.Parameters["@Checksum"].Value = checksum;
            this._cmd.Parameters["@No"].Value = DBNull.Value;

            this._conn.Open();
            this._cmd.ExecuteNonQuery();
            this._conn.Close();

            var _result = this._cmd.Parameters["@No"].Value;

            if (_result == DBNull.Value)
                return (-1);

            return Convert.ToInt32(_result);
        }
    }
}
