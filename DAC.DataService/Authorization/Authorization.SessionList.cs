using System;
using System.Collections.Generic;
using DAC.ObjectModels;

namespace DAC.Authorization
{
    public class TMiceSession
    {
        public TMiceToken Token { get; private set; }
        public TMiceUser User { get; set; }
        public TMiceSession(TMiceUser User)
        {
            this.User = User;
            Token = new TMiceToken();
        }

    }
    
    public class TSessionList : Dictionary<string, TMiceSession>
    {
        private object LockObject;
        private string CreateToken(TMiceUser User)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(Guid.NewGuid().ToString());
            return System.Convert.ToBase64String(plainTextBytes);
          
        }
        public TMiceSession NewSession(TMiceUser User)
        {
            var Result = new TMiceSession(User);
            Result.Token.Token = CreateToken(User);
            Result.Token.RefreshToken = CreateToken(User);
            Add(Result.Token.Token, Result);
            return Result;
        }
        public TSessionList()
        {
            LockObject = new object();
        }
        
        public static TSessionList DefaultInstance = new TSessionList();

    }
}
