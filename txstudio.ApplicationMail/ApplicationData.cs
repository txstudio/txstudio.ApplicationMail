using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using System.Xml.Serialization;

namespace txstudio.ApplicationMail
{
    /// <summary>
    /// 應用程式電子郵件服務的設定內容
    /// </summary>
    public class ApplicationMailConfig
    {
        public int No { get; set; }

        /// <summary>主機名稱</summary>
        public string Host { get; set; }
        /// <summary>帳號</summary>
        public string UserName { get; set; }
        /// <summary>密碼</summary>
        public string Password { get; set; }
        /// <summary>連接埠</summary>
        public int Port { get; set; }

        /// <summary>寄件者電子郵件</summary>
        public string From { get; set; }
        /// <summary>寄件者名稱</summary>
        public string FromDisplayName { get; set; }
    }

    /// <summary>
    /// 應用程式電子郵件訊息內容
    /// </summary>
	[XmlRoot(ElementName = "MailMessage")]
    public class ApplicationMailMessage
    {
        private MailMessage _mailMessage;
        private Encoding _encoding;
        private List<ApplicationMailAddress> _mailAddress;

        private int _no;
        private string _body;
        private string _subject;
        private string _checksum;

        public ApplicationMailMessage()
        {
            this._encoding = Encoding.GetEncoding("utf-8");

            //設定電子郵件寄送內容
            this._mailMessage = new MailMessage();
            this._mailMessage.IsBodyHtml = true;

            this._mailMessage.BodyEncoding = _encoding;
            this._mailMessage.SubjectEncoding = _encoding;
            this._mailMessage.HeadersEncoding = _encoding;
        }



        [XmlElement(ElementName = "No")]
        public int No
        {
            get { return _no; }
            set { _no = value; }
        }

        [XmlElement(ElementName = "Body")]
        public string Body
        {
            get { return _body; }
            set { _body = value; }
        }

        [XmlElement(ElementName = "Subject")]
        public string Subject
        {
            get { return _subject; }
            set { _subject = value; }
        }

        [XmlElement(ElementName = "Checksum")]
        public string Checksum
        {
            get { return _checksum; }
            set { _checksum = value; }
        }

        [XmlElement(ElementName = "MailAddress")]
        public List<ApplicationMailAddress> MailAddress
        {
            get
            {
                return this._mailAddress;
            }
            set
            {
                this._mailAddress = value;
            }
        }

        /// <summary>
        /// 取得寄送電子郵件使用的 MailMessage 物件內容 / 唯讀
        /// </summary>
        [JsonIgnore]
        [XmlIgnore]
        public MailMessage MailMessage
        {
            get
            {
                if (this.MailAddress != null)
                {
                    foreach (var mailAddress in this.MailAddress)
                    {
                        this._mailMessage.To.Add(mailAddress.MailAddress);
                    }
                }
                this._mailMessage.Body = this._body;
                this._mailMessage.Subject = this._subject;

                return this._mailMessage;
            }
        }
    }

    [XmlRoot(ElementName = "MailAddress")]
    public class ApplicationMailAddress
    {
        [XmlElement(ElementName = "Address")]
        public string Address { get; set; }

        [XmlElement(ElementName = "DisplayName")]
        public string DisplayName { get; set; }

        /// <summary>
        /// 取得寄送電子郵件使用的 MailAddress 物件內容 / 唯讀
        /// </summary>
        [JsonIgnore]
        [XmlIgnore]
        public MailAddress MailAddress
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this.DisplayName) == true)
                {
                    return new MailAddress(this.Address);
                }
                if (this.DisplayName == "--")
                {
                    return new MailAddress(this.Address);
                }
                return new MailAddress(this.Address, this.DisplayName);
            }
        }
    }

    [XmlRoot(ElementName = "MailMessages")]
    public class ApplicationMailMessages
    {
        [XmlElement(ElementName = "MailMessage")]
        public List<ApplicationMailMessage> MailMessage { get; set; }
    }
}