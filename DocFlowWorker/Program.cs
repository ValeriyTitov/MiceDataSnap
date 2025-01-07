using System;
using DAC.DataService;
using DAC.QueueManager;
using DAC.XDataSet;
using DAC.Authorization;

namespace DocFlowWorker
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(TDataSecurity.Hash("Hello World"));
            Console.WriteLine("Begining");
            var Server = new RPCServer();
            Server.User = TAuthorization.Authorize("1", "1").MiceUser;
            Server.Start("localhost", "MiceQueue");
        }
    }
}
