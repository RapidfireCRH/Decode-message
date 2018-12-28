using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Decode_message
{
    class Program
    {

        static void Main(string[] args)
        {
            decode d = new decode();
            decode.message_st temp = new decode.message_st();
            temp.message = "{\"$schemaRef\": \"https://eddn.edcd.io/schemas/journal/1/beta\", \"header\": {\"gatewayTimestamp\": \"2018-11-13T07:58:52.860072Z\", \"softwareName\": \"E:D Market Connector [Windows]\", \"softwareVersion\": \"3.2.0.0\", \"uploaderID\": \"4128817eafa8f3b961301b7f49ff8d623eb816de\"}, \"message\": {\"Body\": \"IC 2391 Sector FL-X b1-7 A 2\", \"BodyID\": 14, \"BodyType\": \"Planet\", \"Docked\": false, \"Population\": 0, \"StarPos\": [611.34375, -78.40625, -51.6875], \"StarSystem\": \"IC 2391 Sector FL-X b1-7\", \"SystemAddress\": 16072170087825, \"SystemAllegiance\": \"Guardian\", \"SystemEconomy\": \"$economy_None;\", \"SystemGovernment\": \"$government_None;\", \"SystemSecondEconomy\": \"$economy_None;\", \"SystemSecurity\": \"$GAlAXY_MAP_INFO_state_anarchy;\", ";
            temp.schemainfo = d.getschema(temp.message);
            temp.header = d.decodeheader(temp.message);
        }
    }
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
        public schema_st getschema(string line)
        {
            schema_st ret = new schema_st();
            string schemaline = line.Substring(line.IndexOf("{\"$schemaRef\": \"") + "{\"$schemaRef\": \"".Length, line.IndexOf("\"", line.IndexOf("{\"$schemaRef\": \"") + "{\"$schemaRef\": \"".Length) - (line.IndexOf("{\"$schemaRef\": \"") + "{\"$schemaRef\": \"".Length));
            if (schemaline.Contains("beta") || schemaline.Contains("test"))
                ret.beta = true;
            else
                ret.beta = false;
            ret.schemavernum = Int32.Parse(schemaline.Substring(schemaline.Length - 1 - (ret.beta ? 5 : 0), 1));
            switch(schemaline.Substring("https://eddn.edcd.io/schemas/".Length, schemaline.Length - (schemaline.Length - 2 - (ret.beta ? 5 : 0))))
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
                    throw new InvalidCastException("Unknown schema: " + schemaline +". Tried parcing: " + schemaline.Substring("https://eddn.edcd.io/schemas/".Length, (schemaline.Length - 1 - (ret.beta ? 5 : 0))));
                        
            }
            return ret;
        }
        public header_st decodeheader(string line)
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
            throw new NotImplementedException();
        }
    }
}
