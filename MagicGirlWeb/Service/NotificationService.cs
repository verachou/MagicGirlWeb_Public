using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace MagicGirlWeb.Service
{
  public class NotificationService : INotificationService
  {
    private readonly ILogger _logger;
    private readonly IConfiguration _config;

    public NotificationService(ILoggerFactory loggerFactory, IConfiguration config)
    {
      string className = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name;
      _logger = loggerFactory.CreateLogger(className);
      _config = config;
    }

    public void SendMail(
      List<string> mails,
      string subject,
      string body,
      string filePath)
    {
      string hostUrl = _config["MailSetting:SmtpHost"];
      int port = Convert.ToInt32(_config["MailSetting:SmtpPort"]);
      bool useSsl = Convert.ToBoolean(_config["MailSetting:UseSSL"]);
      var account = _config["MailSetting:SmtpAccount"];
      var password = _config["MailSetting:SmtpPassword"];

      string fromAddress = _config["MailSetting:FromAddr"];
      string fromName = _config["MailSetting:FromName"];


      var message = new MimeMessage();
      message.From.Add(new MailboxAddress(fromName, fromAddress));
      foreach (var mail in mails)
      {
        var displayName = mail.Substring(1, mail.IndexOf("@"));
        message.To.Add(new MailboxAddress(displayName, mail));
      }
      message.Subject = subject;
      var bodyText = new TextPart("plain") { Text = body };

      var attachment = new MimePart()
      {
        Content = new MimeContent(File.OpenRead(filePath), ContentEncoding.Default),
        ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
        ContentTransferEncoding = ContentEncoding.Base64,
        FileName = Path.GetFileName(filePath)
      };

      var multipart = new Multipart("mixed");
      multipart.Add(bodyText);
      multipart.Add(attachment);

      message.Body = multipart;


      using (var client = new SmtpClient())
      {
        // 連接 Mail Server (郵件伺服器網址, 連接埠, 是否使用 SSL)
        client.Connect(hostUrl, port, useSsl);
        client.Authenticate(account, password);

        // 寄出郵件
        client.Send(message);

        // 中斷連線
        client.Disconnect(true);
      }

    }

  }
}