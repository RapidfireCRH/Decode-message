using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Threading;

namespace Decode_message
{
    class db
    {
        private static List<decode.message_st> cache = new List<decode.message_st>();
        private static List<string> hashlist = new List<string>();
        private readonly static object lck = new object();
        public static bool loading_complete = false;//This will be used to indicate that there will be no more messages loaded into cache
        public static bool queue_empty = false;
        public bool addcache(decode.message_st message)
        {
            try
            {
                lock (lck)
                {
                    cache.Add(message);
                }
                return true;
            }
            catch { return false; }
        }
        private static List<decode.message_st> popcache()
        {
            List<decode.message_st> ret;
            lock(lck)
            {
                ret = cache;
                cache = new List<decode.message_st>();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            return ret;
        }
        public static void writetodb(object dbpath)
        {
            try
            {
                if (File.Exists((string)dbpath))
                    File.Delete((string)dbpath);
                //create db file
                SQLiteConnection m_dbconnection;
                SQLiteConnection.CreateFile((string)dbpath);
                m_dbconnection = new SQLiteConnection("Data Source=" + dbpath + "; Version=3;");
                m_dbconnection.Open();
                //create tables in db
                using (SQLiteTransaction tr = m_dbconnection.BeginTransaction())
                {
                    using (SQLiteCommand cmd = m_dbconnection.CreateCommand())
                    {
                        cmd.Transaction = tr;
                        cmd.CommandText = "CREATE TABLE lines (hash CARCHAR(32) PRIMARY KEY, schemanum INTERGER, datestamp INTERGER, message TEXT)";
                        cmd.ExecuteNonQuery();
                    }
                    tr.Commit();
                }
                //Write db till cache is cleared
                List<decode.message_st> localcache = new List<decode.message_st>();
                while (!(cache.Count == 0 && loading_complete))
                {
                    localcache = popcache();
                    queue_empty = false;
                    if(cache.Count == 0 && localcache.Count == 0)
                    {
                        queue_empty = true;
                        Thread.Sleep(500);
                        continue;
                    }
                    using (SQLiteTransaction tr = m_dbconnection.BeginTransaction())
                    {
                        using (SQLiteCommand cmd = m_dbconnection.CreateCommand())
                        {
                            foreach (decode.message_st x in localcache)
                            {
                                string sql = "Select * from lines where hash = \"" + x.header.hash + "\"";
                                SQLiteCommand reader = new SQLiteCommand(sql, m_dbconnection);
                                SQLiteDataReader read = reader.ExecuteReader();
                                if (read.HasRows)
                                    continue;
                                cmd.CommandText = @"INSERT INTO lines (hash, schemanum, datestamp, message) VALUES (@hash, @schemanum, @datestamp, @message)";
                                cmd.Parameters.Add(new SQLiteParameter("@hash", x.header.hash));
                                cmd.Parameters.Add(new SQLiteParameter("@schemanum", x.schemainfo.schema));
                                cmd.Parameters.Add(new SQLiteParameter("@datestamp", x.header.timestamp.Ticks));
                                cmd.Parameters.Add(new SQLiteParameter("@message", x.message));
                                cmd.ExecuteNonQuery();
                            }
                        }
                        tr.Commit();
                    }
                }
                m_dbconnection.Close();
            }
            catch (Exception e)
            {
                Console.Write("");
            }
        }

        //thread manager
        //db creater
        //db writer
    }
}
