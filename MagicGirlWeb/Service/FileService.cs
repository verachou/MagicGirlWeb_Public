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
    // private readonly IConfiguration _config;
    private readonly List<string> _folderIds;
    private DriveService _drivceService;

    public FileService(ILoggerFactory loggerFactory, IConfiguration config)
    {
      string className = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name;
      this._logger = loggerFactory.CreateLogger(className);
      this._folderIds = new List<string>();
      this._folderIds.Add(config.GetValue<string>("Authentication:Google:DriveFolderId"));

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

    // https://googleapis.dev/dotnet/Google.Apis.Drive.v3/latest/api/Google.Apis.Drive.v3.FilesResource.GetRequest.html#Google_Apis_Drive_v3_FilesResource_GetRequest_MediaDownloader
    public bool Download(
        string fileId,
        FileStream fileStream
    )
    {
      // Get the media get request object.
      try
      {
        using (fileStream)
        {
          FilesResource.GetRequest getRequest = _drivceService.Files.Get(fileId);
          getRequest.Download(fileStream);
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex.ToString());
        return false;
      }

      return true;
    }

    public string Upload(
      string fileName,
      string filePath,
      string mimeType,
      string description
    )
    {
      string FileFullName = filePath + @"\" + fileName;
      var fileMetadata = new Google.Apis.Drive.v3.Data.File();
      fileMetadata.Name = fileName;
      fileMetadata.Description = description;

      string query = "mimeType!='application/vnd.google-apps.folder' and trashed = false ";
      query += String.Format("and name contains '{0}' ", fileName);
      query += String.Format("and fullText contains '{0}' ", description);

      string queryParent = String.Format("'{0}' in parents ", _folderIds[0]);
      for (int i = 1; i < _folderIds.Count; i++)
      {
        queryParent += String.Format("or '{0}' in parents ", _folderIds[i]);
      }
      query += String.Format("and ({0})", queryParent);


      FilesResource.ListRequest req;
      req = _drivceService.Files.List();
      req.Q = query;
      req.Fields = "files(id, name, description)";
      var result = req.Execute();

      if (result.Files.Count == 1)
      {
        FilesResource.UpdateMediaUpload updateRequest;
        string fileId = result.Files[0].Id;

        using (var stream = new FileStream(FileFullName, FileMode.OpenOrCreate))
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
        fileMetadata.Parents = _folderIds;
        using (var stream = new FileStream(FileFullName, FileMode.Open))
        {
          request = _drivceService.Files.Create(fileMetadata, stream, mimeType);
          request.Fields = "id";
          request.Upload();
          var file = request.ResponseBody;
          return file.Id;
        };
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