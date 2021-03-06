﻿using System;
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
            System.IO.StreamReader file = new System.IO.StreamReader(@"C:\Users\Rapid\Downloads\2018-12-28_json");
            int num = 0;
            while ((line = file.ReadLine()) != null)
            {
                num++;
                decode.message_st temp;
                temp = de.decodemsg(line);
                if (!temp.header.Equals(new decode.header_st()))
                    db.addcache(temp);
                else
                    Console.Read();
            }
            file = new System.IO.StreamReader(@"C:\Users\Rapid\Downloads\2018-12-28_json.1");
            while ((line = file.ReadLine()) != null)
            {
                num++;
                decode.message_st temp;
                temp = de.decodemsg(line);
                if (!temp.header.Equals(new decode.header_st()))
                    db.addcache(temp);
                else
                    Console.Read();
            }
            file.Close();
            db.loading_complete = true;
            Console.Write("Decoded " + num + " lines in " + (DateTime.Now - start).TotalMilliseconds + " Milliseconds");
            Console.Read();
        }


    }
    
}
