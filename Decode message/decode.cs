using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Decode_message
{
    class decode
    {
        public enum schemaref { blackmarket, commodity, journal, outfitting, shipyard }
        public struct message_st
        {
            public schema_st schemainfo;
            public header_st header;
            public string message;
        }
        public struct header_st
        {
            public DateTime timestamp;
            public string softname;
            public string softver;
            public string uploaderid;
            public string hash;
        }
        public struct schema_st
        {
            public schemaref schema;
            public bool beta;
            public int schemavernum;
        }
        public message_st decodemsg(string message)
        {
            message_st ret = new message_st();
            ret.message = message;
            ret.schemainfo = getschema(ret.message);
            ret.header = decodeheader(ret.message);
            ret.header.hash = CalculateMD5Hash(ret.header.timestamp.Ticks + ret.header.uploaderid + ret.schemainfo.schema);
            return ret;
        }

        private schema_st getschema(string line)
        {
            try
            {
                schema_st ret = new schema_st();
                string schemaline = line.Substring(line.IndexOf("{\'$schemaRef\': \'") + "{\'$schemaRef\': \'".Length, line.IndexOf("\'", line.IndexOf("{\'$schemaRef\': \'") + "{\'$schemaRef\': \'".Length) - (line.IndexOf("{\'$schemaRef\': \'") + "{\'$schemaRef\': \'".Length));
                if (schemaline.Contains("beta") || schemaline.Contains("test"))
                    ret.beta = true;
                else
                    ret.beta = false;
                ret.schemavernum = Int32.Parse(schemaline.Substring(schemaline.Length - 1 - (ret.beta ? 5 : 0), 1));
                switch (schemaline.Substring("https://eddn.edcd.io/schemas/".Length, (schemaline.Length - 2 - (ret.beta ? 5 : 0)) - "https://eddn.edcd.io/schemas/".Length))
                {
                    case "journal":
                        ret.schema = schemaref.journal;
                        break;
                    case "blackmarket":
                        ret.schema = schemaref.blackmarket;
                        break;
                    case "commodity":
                        ret.schema = schemaref.commodity;
                        break;
                    case "outfitting":
                        ret.schema = schemaref.outfitting;
                        break;
                    case "shipyard":
                        ret.schema = schemaref.shipyard;
                        break;
                    default:
                        throw new InvalidCastException("Unknown schema: " + schemaline + ". Tried parcing: " + schemaline.Substring("https://eddn.edcd.io/schemas/".Length, (schemaline.Length - 1 - (ret.beta ? 5 : 0))));

                }
                return ret;
            }
            catch(Exception e) { throw e; } // for future error catching

        }
        private header_st decodeheader(string line)
        {
            try
            {
                header_st ret = new header_st();
                dynamic message = JObject.Parse(line);
                ret.timestamp = DateTime.Parse((string)message.header.gatewayTimestamp, new CultureInfo("EN-US", false));
                ret.softname = message.header.softwareName;
                ret.softver = message.header.softwareVersion;
                ret.uploaderid = message.header.uploaderID;
                return ret;
            }
            catch { return new header_st(); }
        }

        //thanks to https://blogs.msdn.microsoft.com/csharpfaq/2006/10/09/how-do-i-calculate-a-md5-hash-from-a-string/
        public string CalculateMD5Hash(string input)

        {

            // step 1, calculate MD5 hash from input
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);
            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();

        }
    }
}
