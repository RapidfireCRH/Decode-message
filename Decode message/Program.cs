using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Decode_message
{
    class Program
    {

        static void Main(string[] args)
        {
            decode de = new decode();
            db db = new db();
            Thread dbmgr = new Thread(db.writetodb);
            dbmgr.Start(@"C:\Users\Rapid\Desktop\db.sqlite");
            
            DateTime start = DateTime.Now;
            string line;
            System.IO.StreamReader file = new System.IO.StreamReader(@"C:\Users\Rapid\Downloads\2018-12-25_json");
            while ((line = file.ReadLine()) != null)
                db.addcache(de.decodemsg(line));
            file.Close();
            db.loading_complete = true;
            Console.WriteLine((DateTime.Now - start).TotalMilliseconds);
            Console.Read();
        }


    }
    
}
