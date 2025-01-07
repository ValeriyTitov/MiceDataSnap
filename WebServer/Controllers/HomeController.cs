using System;
using System.Web.Mvc;
using DAC.ObjectModels;
using DAC.DataService;
using DAC.DataService.DocFlow.Actions;
using DAC.DataService.DocFlow;
using DAC.Authorization;
using DAC.XDataSet;

/*
  **********************************************************************
  // The JSON request was too large to be deserialized.   
  
  // c:\MiceDataSnap\WebServer\Web.config   
  <appSettings>
	<add key = "aspnet:MaxJsonDeserializerMembers" value="1500000" />
  </appSettings>

  **********************************************************************
  
  **********************************************************************
   //Включить удалёный доступ к IIS Express в Visual Studio на этой машине
   //Важно, что бы было 2 строчки:
     1.<binding protocol="http" bindingInformation="*:55062:localhost" />
     2.<binding protocol="http" bindingInformation="*:55062:*" />

   //C:\MiceDataSnap\.vs\config\applicationhost.config 
    
     <site name="MiceDataSnap" id="2">
      <bindings>
       <binding protocol="http" bindingInformation="*:55062:localhost" />
       <binding protocol="http" bindingInformation="*:55062:*" />
       <binding protocol="https" bindingInformation="*:44306:localhost" />
      </bindings>
     </site>
  **********************************************************************
*/
namespace MiceDataSnap.Controllers
{
    public class HomeController : Controller
    {

        private string InternalMiceRequest(string Token, TMiceDataRequest MiceRequest)
        {
            var Result = new TMiceDataProcess();
            try
            {
                Result.User = GetMiceUser(Token, MiceRequest, ModelState);
                Result.MiceRequest = MiceRequest;
                Result.ProceedRequest();
                return Result.StringResult;
            }
            catch (Exception e)
            {
                var ex = TMiceException.CreateException(e);
                if (Result.Query != null)
                    ex.Messages = Result.Query.ExecutionContext.Messages;
                return ex.ToString();
            }
        }
        
        [HttpPost]
        public string MiceRequest(TMiceDataRequest MiceRequest)
        {

            var Token = Request.Headers.GetValues("Authorization")[0].Replace("Bearer:", "").Trim();

            return this.InternalMiceRequest(Token, MiceRequest);
         }
        
        [HttpPost]
        public string MiceApplyUpdatesRequest(TMiceApplyUpdatesRequest UpdatesRequest)
        {
            var Token = Request.Headers.GetValues("Authorization")[0].Replace("Bearer:", "").Trim();
            try
            {
                var User = GetMiceUserApplyUpdates(Token, UpdatesRequest, ModelState);
                return TDataService.MiceApplyUpdatesRequest(User, UpdatesRequest);
            }

            catch (Exception e)
            {
              return TMiceException.CreateExceptionJsonString(e);
            }
        }

        public TMiceUser GetMiceUser(string Token, TMiceDataRequest MiceRequest, ModelStateDictionary ModelState)
        {
            if (ModelState.IsValid == false || MiceRequest == null || MiceRequest.ExecutionContext == null)
                throw new Exception("Error serializing query");

            var User = TAuthorization.UserFromToken(Token);

            if (TDataSecurity.AllowedToExecute(MiceRequest, User) == false)
                throw new Exception("You are not allowed to perform this type of queries");
            
            return User;
        }

        public TMiceUser GetMiceUserApplyUpdates(string Token, TMiceApplyUpdatesRequest UpdatesRequest, ModelStateDictionary ModelState)
        {
            if (ModelState.IsValid == false || UpdatesRequest == null || UpdatesRequest.ExecutionContext == null)
                throw new Exception("Error serializing query");

            if (String.IsNullOrEmpty(UpdatesRequest.ExecutionContext.ProviderName))
                throw new Exception("ApplyUpdates: ProviderName is empty.");

            return TAuthorization.UserFromToken(Token);
        }

        [HttpPost]
        public string Authorize(string UserName, string Password)
        {
            try
            {
                return TAuthorization.Authorize(UserName, Password).ToJsonString();
            }
            catch (Exception e)
            {
                var ex = TMiceException.CreateException(e);
                return ex.ToString();
            }
        }


        [HttpPost]
        public string AbortQuery (string Token, string QueryToken )
        {
            return TDataService.AbortQuery(Token, QueryToken);
        }
       
        [HttpPost]
        public string RefreshToken(string Token, string RefreshToken)
        {
            try
            {
                return TAuthorization.RefreshToken(Token, RefreshToken);
            }
            catch (Exception e)
            {
                var ex = TMiceException.CreateException(e);
                return ex.ToString();
            }
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Methods()
        {
            return View();
        }

        public ActionResult SessionList()
        {
            foreach (string s in TSessionList.DefaultInstance.Keys)
            {
                if (TSessionList.DefaultInstance[s].User.FullName == "")
                { }

            }
            return View(TSessionList.DefaultInstance);
        }

        public ActionResult ClearCache()
        {
            TDataService.ClearCache();
            return View();
        }

        public ActionResult SendMail()
        {
         
            return View();
        }

        public ActionResult PushDocumentWeb()
        {
            return View();
        }

        private string AuthorizeWeb(string UserName, string Password)
        {
            try
            {
                return TAuthorization.Authorize(UserName, Password).Token.Token;
            }
            catch (Exception e)
            {
                var ex = TMiceException.CreateException(e);
                return ex.ToString();
            }
        }

       // [HttpPost]
        public string InternalPushDocumentWeb(int DocumentsId, int dfPathFoldersId, int dfMethodsIdTarget, string DbName)
        {
           // return "Hello";

            var R = new TMiceDataRequest();
            R.ExecutionContext.ProviderName = "sysdf_DocumentPush";
            R.ExecutionContext.Params.SetParameter("DocumentsId", DocumentsId);
            R.ExecutionContext.Params.SetParameter("dfPathFoldersIdSource", dfPathFoldersId);
            R.ExecutionContext.Params.SetParameter("dfMethodsIdTarget", dfMethodsIdTarget);
            R.ExecutionContext.DBName = DbName;
            var Token = this.AuthorizeWeb("1", "1");

            var s = this.InternalMiceRequest(Token, R);
            return s;

        }
    }
}