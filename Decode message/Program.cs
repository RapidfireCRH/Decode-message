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
            db db = new db();
            Thread dbmgr = new Thread(db.writetodb);
            dbmgr.Start(@"C:\Users\Rapid\Desktop\db.sqlite");

            DateTime start = DateTime.Now;
            string line;
            System.IO.StreamReader file = new System.IO.StreamReader(@"C:\Users\Rapid\Downloads\2018-12-30.jsonl");
            int num = 0;
            while (!db.queue_empty) Thread.Sleep(1000);
            while ((line = file.ReadLine()) != null)
            {
                num++;
                ThreadPool.QueueUserWorkItem(runner,line);
            }
            file.Close();
            while (!db.queue_empty) Thread.Sleep(1000);
            file = new System.IO.StreamReader(@"C:\Users\Rapid\Downloads\2018-12-30_1.jsonl");
            while ((line = file.ReadLine()) != null)
            {
                num++;
                ThreadPool.QueueUserWorkItem(runner,line);
            }
            file.Close();
            while (!db.queue_empty) Thread.Sleep(1000);
            file = new System.IO.StreamReader(@"C:\Users\Rapid\Downloads\2018-12-30_2.jsonl");
            while ((line = file.ReadLine()) != null)
            {
                num++;
                ThreadPool.QueueUserWorkItem(runner, line);
            }
            file.Close();
            while(!db.queue_empty) Thread.Sleep(1000);
            db.loading_complete = true;
            Console.Write("Decoded " + num + " lines in " + (DateTime.Now - start).TotalMilliseconds + " Milliseconds");
            Console.Read();
        }
        public static void runner(object line)
        {
            decode de = new decode();
            db db = new db();
            decode.message_st temp;
            temp = de.decodemsg((string)line);
            if (!temp.header.Equals(new decode.header_st()))
                db.addcache(temp);
        }
    }   
}
