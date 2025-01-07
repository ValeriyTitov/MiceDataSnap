using System;
using DAC.ObjectModels;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAC.Authorization
{
    public class TAuthorization
    {
        public static bool AuthorizeUser(TMiceUser User, string Password)
        {
            if (User.PasswordHash == Password)
                return true;
            else
                return false;
        }
        public static string RefreshToken(string Token, string RefreshToken)
        {
            if (TSessionList.DefaultInstance.ContainsKey(Token))
            {
                var OldSession = TSessionList.DefaultInstance[Token];
                if (OldSession.Token.RefreshToken == RefreshToken)
                {
                    TSessionList.DefaultInstance.Remove(Token);
                    var NewSession = TSessionList.DefaultInstance.NewSession(OldSession.User);
                    return NewSession.Token.ToJsonString();
                }
            }
            throw new Exception("Invalid refresh token");
        }

        public static TMiceAuthorizationResponse Authorize(string UserName, string Password)
        {
            var User = TMiceUserList.DefaultInstance.FindUser(UserName);

            if (User != null && AuthorizeUser(User, Password))
            {
                var Session = TSessionList.DefaultInstance.NewSession(User);
                var Response = new TMiceAuthorizationResponse();
                Response.Token = Session.Token;
                Response.MiceUser = User;
                return Response;
            }

            throw new Exception("Invalid username or password");
        }
        public static TMiceUser UserFromToken(string Token)
        {
            if (TSessionList.DefaultInstance.ContainsKey(Token))
            {
                var Session = TSessionList.DefaultInstance[Token];
                if (Session.Token.Expired())
                    throw new Exception("Session is expired");
                if (Session.User.Active == false)
                    throw new Exception("This user is no longer available for requests");

                return TSessionList.DefaultInstance[Token].User;
            }
            throw new Exception("Cannot find token. Authorization failed");
        }
    }
}
