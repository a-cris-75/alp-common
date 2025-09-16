//using Alp.Com.Igu.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Alp.Com.Igu.Utils
{
    public class ConfigManager
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
                                        ("Alp.Com.Igu.Utils.ConfigManager");

        private IConfigurationRoot? ConfigurationManager;
        private string NamefileConfig;

        private string DirPath = string.Empty;
        // You can choose if you prefer indented or none (more space saving)
        private const Formatting MyFormatting = Formatting.Indented;
        private string InitialSection = string.Empty;

        public ConfigManager(string? dirpath, string initsection, string namefileconfig = "appsettings.json")
        {
            NamefileConfig = namefileconfig;
            if(!string.IsNullOrEmpty(dirpath))
                DirPath = dirpath;
            InitialSection = initsection;

            if (string.IsNullOrEmpty(dirpath))
                DirPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                //DirPath = Directory.GetCurrentDirectory();
            if (namefileconfig.Contains(".json"))
            {
                ConfigurationManager = new ConfigurationBuilder()
                        .SetBasePath(DirPath)
                        .AddJsonFile(namefileconfig, optional: true)
                        .Build();
            }
            else
            {

            }
            
        }

        private static JToken Serialize(IConfiguration config)
        {
            JObject obj = new JObject();
            foreach (var child in config.GetChildren())
            {
                obj.Add(child.Key, Serialize(child));
            }

            if (!obj.HasValues && config is IConfigurationSection section)
                return new JValue(section.Value);

            return obj;
        }

        public string? GetValue(string key)
        {
            string? res = string.Empty;
            try
            {
                if (NamefileConfig.Contains(".json"))
                {
                    string prefix = "";
                    if(!string.IsNullOrEmpty(InitialSection))
                        prefix = InitialSection + ":";
                    res = ConfigurationManager[prefix + key];
                }
                else
                {
                    res = System.Configuration.ConfigurationManager.AppSettings[key];
                }
            }
            catch { }
            return res;
        }

        public List<T> GetListInSectionJson<T>(string keySection)
        {
            //string? res = string.Empty;
            List<T> res = new List<T>();
            try
            {
                if (NamefileConfig.Contains(".json"))
                {
                    string prefix = "";
                    if (!string.IsNullOrEmpty(InitialSection))
                        prefix = InitialSection + ":";

                    //res = ConfigurationManager[prefix + keySection];
                    res = ConfigurationManager.GetSection(prefix + keySection).Get<List<T>>();
                }
                else
                {
                    //res = System.Configuration.ConfigurationManager.AppSettings[key];
                }
            }
            catch { }
            return res;
        }

        public T? GetSectionJson<T>(string keySection)
        {
            T? res = Activator.CreateInstance<T>();
            try
            {
                if (NamefileConfig.Contains(".json"))
                {
                    string prefix = "";
                    if (!string.IsNullOrEmpty(InitialSection))
                        prefix = InitialSection + ":";

                    //res = ConfigurationManager.GetSection(prefix + keySection).Get<T>();
                    Dictionary<string,object>? dres = ConfigurationManager.GetSection(prefix + keySection).Get<Dictionary<string, object>>();
                    bool bb = false;
                    if(dres!=null && res!=null)
                        bb = UtilsObj.CopyDictionaryToObject(dres, res);

                    if (dres == null)
                        res = default(T);
                }
                else
                {
                    //res = System.Configuration.ConfigurationManager.AppSettings[key];
                }
            }
            catch { }
            return res;
        }


        public List<string> GetAllKeys()
        {
            List<string> res = new List<string>();
            try
            {
                if (NamefileConfig.Contains(".json"))
                {
                    if (string.IsNullOrEmpty(InitialSection))
                    {
                        //ConfigurationManager.Bind(Dictionary<string, string>);
                        res = ConfigurationManager.Get<Dictionary<string, string>>().Select(X=>X.Key).ToList();
                    }
                    else 
                        res = ConfigurationManager.GetSection(InitialSection).Get<Dictionary<string,string>>().Select(X => X.Key).ToList();
                }
                else
                {
                    if(System.Configuration.ConfigurationManager.AppSettings.AllKeys.Count()>0)
                        res = System.Configuration.ConfigurationManager.AppSettings.AllKeys.ToList();
                }
            }
            catch { }
            return res;
        }

        public void SetValue(string key, string val)
        {
            string pathfile = Path.Combine(DirPath, NamefileConfig);
            if (NamefileConfig.Contains(".json"))
            {
                string fjson = File.ReadAllText(pathfile);
                dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(fjson);
                if(!string.IsNullOrEmpty(InitialSection))
                    jsonObj[InitialSection][key] = val;
                else
                    jsonObj[key] = val;

                string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(Path.Combine(DirPath, NamefileConfig), output);
            }
            else
            {
                UtilsAssembly.AddUpdateAppSettings(key, val, pathfile);
            }
            //        var json =
            //@"{
            //    ""name"": ""Ram"",
            //    ""Age"": ""25"",
            //    ""ContactDetails"": {
            //        ""MobNo"": ""1""
            //    }
            //}";

            //        var jObject = JObject.Parse(json);
            //        jObject["ContactDetails"]["Address"] = JObject.Parse(@"{""No"":""123"",""Street"":""abc""}");

            //        var resultAsJsonString = jObject.ToString();
        }

        public string GetJSon()
        {
            string json = "";
            string pathfile = Path.Combine(DirPath, NamefileConfig);
            if (NamefileConfig.Contains(".json"))
            {
                json = File.ReadAllText(pathfile);
            }
            return json;
        }

        public void DelItem(string key)
        {
            string pathfile = Path.Combine(DirPath, NamefileConfig);
            
            if (NamefileConfig.Contains(".json"))
            {
                string fjson = File.ReadAllText(pathfile);
               var jsonObj = JObject.Parse(fjson);

                if (jsonObj.ContainsKey(key))
                    jsonObj.Remove(key);

                if (!string.IsNullOrEmpty(InitialSection))
                {
                    bool removed = false;
                    if (jsonObj[InitialSection]?[key] != null)
                    {
                        // tentativi
                        //removed = jsonObj.Remove(InitialSection + ":" + key);
                        //removed = jsonObj.Remove(key);
                        //removed = jsonObj.Remove(InitialSection + "." + key);
                        //jsonObj[InitialSection]?[key].Remove();
                        foreach (var item in jsonObj[InitialSection])
                        {
                            JProperty header = (JProperty)item;
                            if (header.Name == key)
                            {
                                header.Remove();
                                break;
                            }
                        }                       
                    }
                }               
                string json = JsonConvert.SerializeObject(jsonObj, MyFormatting);
                File.WriteAllText(Path.Combine(DirPath, NamefileConfig), json);
            }
            else
            {
                UtilsAssembly.DelAppSettings(key);
            }         
        }
    }
}
