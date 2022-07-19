using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace MagicGirlWeb.Service
{
  public class FileService : IFileService
  {
    private readonly ILogger _logger;
    private DriveService _drivceService;

    public FileService(ILoggerFactory loggerFactory, IConfiguration config)
    {
      string className = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name;
      this._logger = loggerFactory.CreateLogger(className);

      // If modifying these scopes, delete your previously saved credentials
      string[] scopes = { DriveService.Scope.DriveFile,
                          DriveService.Scope.Drive,
                          DriveService.Scope.DriveMetadata
                         };
      Credential credential = new Credential(config, scopes);

      // Create the service using the service account credentials.
      this._drivceService = new DriveService(new BaseClientService.Initializer()
      {
        HttpClientInitializer = credential.GoogleCredential,
        ApplicationName = config.GetValue<string>("Authentication:Google:ApplicationName")
      });
    }

    /// <summary>
    /// 根據fileId從雲端儲存空間下載檔案至指定的儲存位置
    /// </summary>
    /// <param name="fileId">雲端儲存空間的檔案Id</param>
    /// <param name="filePath">下載檔案儲存位置</param>
    /// <returns></returns>
    // https://googleapis.dev/dotnet/Google.Apis.Drive.v3/latest/api/Google.Apis.Drive.v3.FilesResource.GetRequest.html#Google_Apis_Drive_v3_FilesResource_GetRequest_MediaDownloader
    public bool Download(
        string fileId,
        string filePath)
    {
      _logger.LogInformation("Download");
      _logger.LogInformation("fileId: {0}",fileId);
      _logger.LogInformation("filePath: {0}",filePath);
      // Get the media get request object.
      try
      {
        using (var stream = new FileStream(filePath, FileMode.OpenOrCreate))
        {
          FilesResource.GetRequest getRequest = _drivceService.Files.Get(fileId);
          getRequest.Download(stream);
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex.ToString());
        return false;
      }

      return true;
    }

    /// <summary>
    /// 將指定檔案上傳至雲端儲存空間
    /// </summary>
    /// <param name="filePath">上傳檔案位置</param>
    /// <param name="mimeType">檔案格式</param>
    /// <param name="description">該檔案上傳至雲端儲存空間的備註內容</param>
    /// <param name="cloudFolderIds">上傳目的地資料夾Id list</param>
    /// <returns></returns>
    public string Upload(
      string filePath,
      string mimeType,
      string description,
      List<string> cloudFolderIds)
    {
      _logger.LogInformation("Upload");
      _logger.LogInformation("filePath: {0}",filePath);
      _logger.LogInformation("mimeType: {0}",mimeType);
      _logger.LogInformation("description: {0}",description);
      _logger.LogInformation("cloudFolderIds: {0}",cloudFolderIds.ToString());


      string fileName = Path.GetFileName(filePath);
      var fileMetadata = new Google.Apis.Drive.v3.Data.File();
      fileMetadata.Name = fileName;
      fileMetadata.Description = description;

      string query = "mimeType!='application/vnd.google-apps.folder' and trashed = false ";
      query += String.Format("and name contains '{0}' ", fileName);
      query += String.Format("and fullText contains '{0}' ", description);

      string queryParent = String.Format("'{0}' in parents ", cloudFolderIds[0]);
      for (int i = 1; i < cloudFolderIds.Count; i++)
      {
        queryParent += String.Format("or '{0}' in parents ", cloudFolderIds[i]);
      }
      query += String.Format("and ({0})", queryParent);


      FilesResource.ListRequest req;
      req = _drivceService.Files.List();
      req.Q = query;
      req.Fields = "files(id, name, description)";
      _logger.LogTrace("ListQuest.Q: {0}", req.Q);
      _logger.LogTrace("ListQuest.Fields: {0}", req.Fields);

      var result = req.Execute();
      _logger.LogInformation("ListQuest.Execute().Files.Count: {0}", result.Files.Count);

      if (result.Files.Count == 1)
      {
        FilesResource.UpdateMediaUpload updateRequest;
        string fileId = result.Files[0].Id;

        using (var stream = new FileStream(filePath, FileMode.OpenOrCreate))
        {
          updateRequest = _drivceService.Files.Update(fileMetadata, fileId, stream, mimeType);
          updateRequest.Upload();
          var file = updateRequest.ResponseBody;
          return file.Id;
        };
      }
      else
      {
        FilesResource.CreateMediaUpload request;
        fileMetadata.Parents = cloudFolderIds;
        using (var stream = new FileStream(filePath, FileMode.Open))
        {
          request = _drivceService.Files.Create(fileMetadata, stream, mimeType);
          request.Fields = "id";
          var reqResult = request.Upload();
          if (reqResult.Exception != null)
          {
            _logger.LogWarning(reqResult.Exception.Message);
          }
          var file = request.ResponseBody;
          return file.Id;
        };
      }
    }

    
    /// <summary>
    /// 根據指定的雲端資料夾Id，回傳該資料夾下所有檔案資料(包含檔名、檔案大小、備註)
    /// </summary>
    /// <param name="cloudFolderId">雲端資料夾Id</param>
    public List<IFileService.CloudFile> GetFileList(string cloudFolderId)
    {
      _logger.LogInformation("GetFileList");
      _logger.LogInformation("cloudFolderId: {0}",cloudFolderId);

      // Define parameters of request.
      FilesResource.ListRequest listRequest = _drivceService.Files.List();
      // listRequest.PageSize = 10;
      listRequest.Fields = "nextPageToken, files(id, name, description, size)";
      listRequest.Q = String.Format("parents='{0}'",cloudFolderId);      
      _logger.LogTrace("ListQuest.Q: {0}", listRequest.Q);
      _logger.LogTrace("ListQuest.Fields: {0}", listRequest.Fields);

      // List files.
      IList<Google.Apis.Drive.v3.Data.File> files = listRequest.Execute().Files;
      _logger.LogInformation("ListQuest.Execute().Files.Count: {0}", files.Count);

      List<IFileService.CloudFile> cloudFiles = new List<IFileService.CloudFile>();
      if (files == null || files.Count == 0)
      {
        return null;
      }
      foreach (var file in files)
      {
        IFileService.CloudFile cf = new IFileService.CloudFile();
        cf.Id = file.Id;
        cf.Name = file.Name;
        cf.Size = (int) file.Size;
        cf.Description = file.Description;
        cloudFiles.Add(cf);
      }                    
      return cloudFiles;
    }


    /// <summary>
    /// 刪除目標資料夾下的所有檔案
    /// </summary>
    /// <param name="folderPath">資料夾路徑</param>
    public void ClearLocalFolder(string folderPath)
    {
      DirectoryInfo directory = new DirectoryInfo(folderPath);
      FileInfo[] files = directory.GetFiles();
      foreach (FileInfo file in files)
      {
        file.Delete();
      }
    }


    private class Credential
    {
      public GoogleCredential GoogleCredential;

      public Credential(IConfiguration config, string[] scopes)
      {
        string credFilePath = config.GetValue<string>("Authentication:Google:DriveApiKey");
        this.GoogleCredential = GoogleCredential.FromFile(credFilePath).CreateScoped(scopes);
      }
    }




  }
}