using DAC.XDataSet;
using System.Web;
using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net;
using System.Data;

namespace DAC.DataService.DocFlow.Actions
{
    public class TSendMessageAction : TAbstractDocFlowAction
    {
       
        private string MailServerAddress { get; set; }
        private int MailServerPort { get; set; }
        private string MailLogin { get; set; }
        private string MailPassword { get; set; }
       
        public string MessageTo { get; set; }
        public string MessageSubject { get; set; }
        public string MessageBody { get; set; }
        public int MessageType { get; set; }
        private TDocFlowEntity _Entity { get; set; }
        private void ReadSettings()
        {
            MailLogin = "mice6robot";
            MailPassword = "Kcd3HEqprY";
            MailServerAddress = "smtp";
            MailServerPort = 25;
        }

        private void LoadFromDataRow(DataRow Row)
        {
            MessageTo = Row["MessageTo"].ToString();
            MessageSubject = Row["MessageSubject"].ToString();
            MessageBody = Row["MessageBody"].ToString();
            MessageType = Convert.ToInt32(Row["MessageType"]);

            if (String.IsNullOrEmpty(MessageTo))
            {
                throw new Exception("DocFlow Engine: Send message action. Recipient is empty");
            }
            if (String.IsNullOrEmpty(MessageSubject))
            {
                throw new Exception("DocFlow Engine: Send message action. Message body is empty");
            }
        }
        private void GetMessageDetails()
        {
            var Tmp = new TxDataSet();
            Tmp.ProviderName = "sysdf_ActionDetailsSendMessage";
            Tmp.SetParameter("dfPathFolderActionsId", dfPathFolderActionsId);
            Tmp.DBName = this.DBName;
            Tmp.Open();
            if (Tmp.Rows.Count == 0)
            {
                throw new Exception("DocFlow Engine: Send message action. [sysdf_ActionDetailsSendMessage] could not find any action details");
            }
            LoadFromDataRow(Tmp.Rows[0]);
        }
        public TSendMessageAction(TDocFlowEntity AEntity, DataRow AAction):base (AEntity, AAction)
        {
            _Entity = AEntity;
            this.ReadSettings();
        }
        public override void Execute()
        {
            GetMessageDetails();
            switch (MessageType)
            {
                case 0: SendMail(); break;
                case 1: SendSms(); break;
                case 2: SendOnlineMessage(); break;
                default: throw new Exception($"Doc action: Unknow message type: {MessageType}");
            }
        }
        private MailMessage CreateMailMessage()
        {
            MailMessage m = new MailMessage();
            m.IsBodyHtml = true;
            m.From = new MailAddress(MailLogin);
            m.To.Add(TDocFlowStringUtils.ReplaceVariablesInText(MessageTo, _Entity.Document.Rows[0]));
            m.Subject = TDocFlowStringUtils.ReplaceVariablesInText(MessageSubject, _Entity.Document.Rows[0]);
            m.Body = TDocFlowStringUtils.ReplaceVariablesInText(MessageBody, _Entity.Document.Rows[0]);
            return m;
        }
        private void SendMail()
        {
            var m = CreateMailMessage();

            SmtpClient smtp = new SmtpClient(MailServerAddress, MailServerPort);
            smtp.UseDefaultCredentials = false;
            smtp.EnableSsl = true;
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.Credentials = new NetworkCredential(MailLogin, MailPassword);
            smtp.Send(m);
        }

        private void SendSms()
        {
            throw new Exception("SendSms is not implemented");
        }

        private void SendOnlineMessage()
        {
            throw new Exception("Send online message is not implemented");
        }



    }
}
