using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace Decode_message
{
    class db
    {
        private static List<decode.message_st> cache = new List<decode.message_st>();
        private readonly object lck = new object();
        public static bool loading_complete = false;//This will be used to indicate that there will be no more messages loaded into cache
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
            catch(Exception e) { return false; }
        }
        private List<decode.message_st> popcache()
        {
            List<decode.message_st> ret;
            lock(lck)
            {
                ret = cache;
                cache = new List<decode.message_st>();
            }
            return ret;
        }
        public void writetodb(object dbpath)
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
                    cmd.CommandText = "CREATE TABLE lines (hash CARCHAR(32) PRIMARY KEY, schemanum INTERGER, message TEXT)";
                    cmd.ExecuteNonQuery();
                }
                tr.Commit();
            }
            //Write db till cache is cleared
            List<decode.message_st> localcache = new List<decode.message_st>();
            while (!(cache.Count == 0 && loading_complete))
            {
                localcache = popcache();
                using (SQLiteTransaction tr = m_dbconnection.BeginTransaction())
                {
                    using (SQLiteCommand cmd = m_dbconnection.CreateCommand())
                    {
                        foreach (decode.message_st x in localcache)
                        {
                            string sql = "Select hash from lines where hash = " + x.header.hash;
                            SQLiteCommand reader = new SQLiteCommand(sql, m_dbconnection);
                            SQLiteDataReader read = reader.ExecuteReader();
                            if (read.HasRows)
                                continue;
                            cmd.CommandText = @"INSERT INTO lines (hash, schemanum, message) VALUES (@hash, @schemanum, @message)";
                            cmd.Parameters.Add(new SQLiteParameter("@hash", x.header.hash));
                            cmd.Parameters.Add(new SQLiteParameter("@schemanum", x.schemainfo.schema));
                            cmd.Parameters.Add(new SQLiteParameter("@message", x.message));
                        }
                    }
                    tr.Commit();
                }
            }
        }

        //thread manager
        //db creater
        //db writer
    }
}
