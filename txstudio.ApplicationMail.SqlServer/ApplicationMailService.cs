using System;
using System.Data;
using System.Net;
using System.Net.Mail;

namespace txstudio.ApplicationMail.SqlServer
{
    public class ApplicationMailService : IApplicationMailService
    {
        private IApplicationMailRepository _applicationMailRepository;

        private SmtpClient _smtpClient;
        private ApplicationMailConfig _config;

        public ApplicationMailService()
        {
            this._applicationMailRepository = new ApplicationMailRepository();

            this._config = this._applicationMailRepository.GetConfig();

            //取得 SMTP 設定內容
            this._smtpClient = new SmtpClient();
            this._smtpClient.Host = _config.Host;
            this._smtpClient.Port = _config.Port;
            this._smtpClient.Credentials
                = new NetworkCredential(_config.UserName, _config.Password);

        }


        /// <summary>
        /// 寄送位於待處理電子郵件清單
        /// </summary>
        public void Send()
        {
            var _items = this._applicationMailRepository.GetMailMessage();

            if (_items == null)
            {
                return;
            }

            var _isSuccess = false;
            var _message = string.Empty;

            foreach (var item in _items)
            {
                try
                {
                    var _mailMessage = item.MailMessage;

                    _mailMessage.From
                        = new MailAddress(this._config.From, this._config.FromDisplayName);

                    //寄送電子郵件內容
                    this._smtpClient.Send(_mailMessage);

                    _isSuccess = true;
                }
                catch (Exception ex)
                {
                    _isSuccess = false;
                    _message = ex.Message;
                }


                if (string.IsNullOrWhiteSpace(_message) == true)
                {
                    _message = "--";
                }

                this._applicationMailRepository.UpdateMailMessage(item.No
                    , _isSuccess
                    , _message);
            }
        }

        /// <summary>
        /// 寄送測試的電子郵件內容
        /// </summary>
        public void SendTestMail(string to)
        {
            ApplicationMailMessage _mailMessage;
            MailMessage _message;

            _mailMessage = new ApplicationMailMessage();
            _mailMessage.Subject = "ApplicationMail Service Demo Mail";
            _mailMessage.Body = string.Format("<h3>this is Demo Mail</h3><p>mail send successful in {0:yyyy-MM-dd HH:mm} !</p>"
                , DateTime.Now);

            _message = _mailMessage.MailMessage;
            _message.From = new MailAddress(this._config.From, this._config.FromDisplayName);
            _message.To.Add(to);

            try
            {
                this._smtpClient.Send(_message);
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// 將要寄送的電子郵件物件放到待寄送清單
        /// </summary>
        /// <param name="item">電子郵件物件</param>
        /// <returns>新建立電子郵件識別碼</returns>
        public int Save(ApplicationMailMessage item)
        {
            DataTable _address;
            DataRow _row;
            int _index = 0;

            _address = new DataTable();
            _address.Columns.Add("Index", typeof(byte));
            _address.Columns.Add("Address", typeof(string));
            _address.Columns.Add("DisplayName", typeof(string));

            foreach (var address in item.MailAddress)
            {
                _row = _address.NewRow();

                _row["Index"] = _index;
                _row["Address"] = address.Address;

                if (string.IsNullOrEmpty(address.DisplayName) == true)
                {
                    _row["DisplayName"] = "--";
                }
                else
                {
                    _row["DisplayName"] = address.DisplayName;
                }
                _address.Rows.Add(_row);
            }

            return this._applicationMailRepository.AddMailMessage(_address
                , item.Subject
                , item.Body
                , item.Checksum);
        }
    }
}
