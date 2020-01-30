using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ProjectManagement.Utilities
{
    class configObj
    {
        public string name;
        public string value;

        public configObj(string name, string value)
        {
            this.name = name;
            this.value = value;
        }
    }
    public class Config
    {
        private static Config instance;
        private static List<configObj> objList;
        public static string load(string key)
        {
            reload();
            return readLocal(key);
        }
        public static void save(string key, string value)
        {
            writeLocal(key, value);
            writeToFile();
        }
        private static void reload()
        {
            if (objList == null) objList = new List<configObj>();
            
            string obj;
            using (StreamReader r = new StreamReader(HttpRuntime.AppDomainAppPath + "\\Configuration.txt"))
            {
                obj = r.ReadToEnd();
                objList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<configObj>>(obj);
                if (objList == null) objList = new List<configObj>();
            }
        }
        private static string readLocal(string key)
        {
            foreach(configObj obj in objList)
            {
                if (obj.name.Equals(key)) return obj.value;
            }
            return "";
        }
        private static void writeLocal(string key, string value)
        {
            if (objList == null) reload();
            foreach(configObj obj in objList)
            {
                if (obj.name.Equals(key))
                {
                    obj.value = value;
                    return;
                }
            }
            objList.Add(new configObj(key, value));
        }
        private static void writeToFile()
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(objList.ToArray());
            System.IO.File.WriteAllText(HttpRuntime.AppDomainAppPath + "\\Configuration.txt", json);
        }
    }
}
