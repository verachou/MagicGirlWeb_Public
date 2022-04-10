using System.Collections.Generic;

namespace MagicGirlWeb.Service
{
  public interface INotificationService
  {
    void SendMail(
      List<string> mails,
      string subject,
      string body,
      string filePath);

  }
}