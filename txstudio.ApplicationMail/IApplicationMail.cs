using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace txstudio.ApplicationMail
{
    /// <summary>
    /// 定義應用程式電子郵件服務
    /// </summary>
    public interface IApplicationMailService
    {
        /// <summary>
        /// 寄送位於待處理電子郵件清單
        /// </summary>
        void Send();

        /// <summary>
        /// 寄送測試的電子郵件內容
        /// </summary>
        void SendTestMail(string to);

        /// <summary>
        /// 將要寄送的電子郵件物件放到待寄送清單
        /// </summary>
        /// <param name="item">電子郵件物件</param>
        /// <returns>新建立電子郵件識別碼</returns>
        int Save(ApplicationMailMessage item);
    }

    /// <summary>
    /// 定義應用程式電子郵件的資料存取層內容
    /// </summary>
    public interface IApplicationMailRepository
    {
        /// <summary>
        /// 取得應用程式電子郵件設定檔
        /// </summary>
        /// <returns></returns>
        ApplicationMailConfig GetConfig();

        /// <summary>
        /// 取得位於待寄送的電子郵件清單
        /// </summary>
        /// <returns>待寄送的電子郵件清單</returns>
        IEnumerable<ApplicationMailMessage> GetMailMessage();

        /// <summary>
        /// 更新指定電子郵件寄送結果
        /// </summary>
        /// <param name="no">要更新的識別碼</param>
        /// <param name="isSuccess">寄送成功</param>
        /// <param name="message">寄送失敗儲存的訊息</param>
        /// <returns>更新結果</returns>
        bool UpdateMailMessage(int no, bool isSuccess, string message);

        /// <summary>
        /// 新增指定電子郵件訊息到待寄送清單
        /// </summary>
        /// <param name="address">電子郵件位址</param>
        /// <param name="subject">主旨</param>
        /// <param name="body">內文</param>
        /// <param name="checksum">檢查碼（若待處裡清單有相同檢查碼不進行儲存）</param>
        /// <returns>新建立的待處裡電子郵件內容</returns>
        int AddMailMessage(DataTable address
            , string subject
            , string body
            , string checksum);
    }
}
