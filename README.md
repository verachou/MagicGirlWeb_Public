# MagicGrilWeb
It's a web for 30+ years old magic girls. Enjoy it.


## To Do List
- [x] create web projec
- [x] import web theme
- [x] trans functions from CSNCOnLine to MGWeb
- [ ] CI: create test case and auto test it
- [x] CD: deploy project to Azure|AWS|Google

## Install on Azure

### Setting Environment
1. @Azure Web App
	1. Configuration, see [azure portal - configure connection strings](https://docs.microsoft.com/zh-tw/azure/app-service/configure-common?tabs=portal#configure-connection-strings)
		* ASPNETCORE_ENVIRONMENT = Production

### Upload Database File
1. @Azure Web App
	1. upload initial db to azure by ftp, path: `/site/database`
	1. Configuration, see [azure portal - configure connection strings](https://docs.microsoft.com/zh-tw/azure/app-service/configure-common?tabs=portal#configure-connection-strings)
		* ConnectionStrings__MagicConnection = {db connection string} `e.g. DataSource=D:\home\site\database\MagicDatabase.db`

### Using Google OAuth2
1. @Google API Console:
	1. create client id & password, see [Google Sign-In for server-side apps](https://developers.google.com/identity/sign-in/web/server-side-flow)
	1. callback uri will be
	`https://{example.com}/signin-google`
1. @Azure Web App
	1. Configuration, see [azure portal - configure connection strings](https://docs.microsoft.com/zh-tw/azure/app-service/configure-common?tabs=portal#configure-connection-strings)
		* Authentication__GOOGLE_CLIENT_ID = {google client id}
		* Authentication__GOOGLE_CLIENT_SECRET = {google client id's password}

### Using Google Drive
1. @Google API Console:
	1. create service account, see [Using OAuth 2.0 for Server to Server Applications](https://developers.google.com/identity/protocols/oauth2/service-account)
	1. get the service account's json format auth key
	1. enable googe drive api, see [Enable the Google Drive API](https://developers.google.com/drive/api/guides/enable-drive-api)
	1. go to google drvie, create a folder, get the [folderID](https://ploi.io/documentation/database/where-do-i-get-google-drive-folder-id)
	1. share the service account to access the folder
1. @Azure Web App
	1. upload the google service account auth key to azure by ftp, path: `/site/wwwroot/keystore`
	1. Configuration, see [azure portal - configure connection strings](https://docs.microsoft.com/zh-tw/azure/app-service/configure-common?tabs=portal#configure-connection-strings)
		* Authentication__GOOGLE__DriveFolderId = {the google drive folder id}
		* Authentication__GOOGLE__DriveApiKey = {auth key real path} `e.g. D:\home\site\wwwroot\keystore\magicgirlweb-6957f033bd9b.json`


---
## Thanks for these resources
novel download: <https://github.com/rngmontoli/CSNovelCrawler/>

novel download+: <https://dev.azure.com/qin89/CSNovelCrawler>

theme: <https://github.com/themefisher/focus-admin-dashboard-theme>

【Google API 教學】如何申請 OAuth 2.0 憑證？ 使用Google Drive API 做示範給你看！: <https://learn.markteaching.com/google-drive-api-%e6%95%99%e5%ad%b8-oauth-2-0-%e6%86%91%e8%ad%89/>
