# MagicGrilWeb
It's a web for 30+ years old magic girls. Enjoy it.


## To Do List
- [x] create web projec
- [x] import web theme
- [x] trans functions from CSNCOnLine to MGWeb
- [ ] CI: create test case and auto test it
- [x] CD: deploy project to Azure|AWS|Google

## Install on Azure
### Using Google OAuth2
1. @Google API Console:
	1. create client id % password, see [Google Sign-In for server-side apps](https://developers.google.com/identity/sign-in/web/server-side-flow)
	1. callback uri will be
	`https://{example.com}/signin-google`
1. @Azure Web App
	1. Configuration, see [azure portal - configure connection strings](https://docs.microsoft.com/zh-tw/azure/app-service/configure-common?tabs=portal#configure-connection-strings)
		1. Authentication__GOOGLE_CLIENT_ID = {google client id}
		1. Authentication__GOOGLE_CLIENT_SECRET = {google client id's password}



---
## Thanks for these resources
novel download: <https://github.com/rngmontoli/CSNovelCrawler/>

novel download+: <https://dev.azure.com/qin89/CSNovelCrawler>

theme: <https://github.com/themefisher/focus-admin-dashboard-theme>




