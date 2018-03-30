using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ServerJob;

namespace ServerJob
{
    class main
    {
        static void Main(string[] args)
        {
            while (true) {
                LoadData d = new LoadData();
                d.WebParsing();
                Databazka db = new Databazka();
                db.InsertData(d.tabulkaZWebStranky);
                db.odpoj();
                System.Threading.Thread.Sleep(150000);
            }
        }
    }
    
}
