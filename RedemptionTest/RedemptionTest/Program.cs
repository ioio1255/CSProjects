using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Redemption;

namespace RedemptionTest
{
    class Program
    {
        static void Main(string[] args)
        {
            RDOSession session = new RDOSession();
            RDOPstStore store = session.LogonPstStore(@"c:\backup.pst");
            //RDOPstStore store = session.Stores.AddPSTStore(@"c:\backup.pst");

        }
    }
}
