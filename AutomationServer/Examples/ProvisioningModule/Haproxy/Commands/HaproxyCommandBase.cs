using System;
using Atomia.Provisioning.Base;
using Atomia.Provisioning.Base.Module;
using Atomia.Provisioning.Modules.Common;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System.Web.Script.Serialization;
using System.Text;
using System.Web;

namespace Atomia.Provisioning.Modules.Haproxy.Commands
{
    abstract class HaproxyCommandBase : ModuleCommandSimpleBase
    {
        /// <summary>
        /// Serializer used to serialize/deserialize JSON data from the REST server.
        /// </summary>
        protected JavaScriptSerializer jsonSerializer;

        public HaproxyCommandBase(ModuleService service, ResourceDescription resource, ModuleService newServiceSettings, ModuleCommandType commandType, int listDepth)
            : base(service, resource, newServiceSettings, commandType, listDepth)
        {
            this.jsonSerializer = new JavaScriptSerializer();
        }

        private WebRequest REST_GetClient(string uri)
        {
            string baseuri = this.Resource["AgentURL"];
            if (string.IsNullOrEmpty(baseuri))
            {
                throw ExceptionHelper.GetModuleException("ID422001", null, null);
            }
            else if (!baseuri.EndsWith("/"))
            {
                baseuri = baseuri + "/";
            }

            if (uri.StartsWith("/"))
            {
                uri = uri.TrimStart('/');
            }

            WebRequest client = WebRequest.Create(baseuri + uri);

            if (this.Resource.GetListOfProperties().Contains("AgentUser") && this.Resource.GetListOfProperties().Contains("AgentPass"))
            {
                string credential = String.Format("{0}:{1}", this.Resource["AgentUser"], this.Resource["AgentPass"]);
                client.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(credential)));
            }

            return client;
        }

        protected void REST_Execute_PUT(string uri, Dictionary<string, string> postdata)
        {
            this.REST_Execute_WithPostData(uri, "PUT", postdata);
        }

        protected Dictionary<string, string> REST_Execute_POST(string uri, Dictionary<string, string> postdata)
        {
            return this.REST_Execute_WithPostData(uri, "POST", postdata);
        }

        protected string REST_Execute_GET(string uri)
        {
            return this.REST_Execute_WithPostData_JSON(uri, "GET", new Dictionary<string, string>());
        }

        private Dictionary<string, string> REST_Execute_WithPostData(string uri, string method, Dictionary<string, string> postdata)
        {
            return this.jsonSerializer.Deserialize<Dictionary<string, string>>(this.REST_Execute_WithPostData_JSON(uri, method, postdata));
        }
                
        private string REST_Execute_WithPostData_JSON(string uri, string method, Dictionary<string, string> postdata)
        {
            HttpWebRequest restclient = (HttpWebRequest)this.REST_GetClient(uri);
            restclient.Method = method;
            restclient.Accept = "application/json";

            List<string> encoded_postdata_pairs = new List<string>();
            foreach (string key in postdata.Keys)
            {
                encoded_postdata_pairs.Add(key + "=" + HttpUtility.UrlPathEncode(postdata[key]));
            }

            if (encoded_postdata_pairs.Count() > 0)
            {
                byte[] byteArray = Encoding.UTF8.GetBytes(string.Join("&", encoded_postdata_pairs.ToArray()));
                restclient.ContentType = "application/x-www-form-urlencoded";
                restclient.ContentLength = byteArray.Length;

                Stream dataStream = restclient.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
            }

            try
            {
                HttpWebResponse response = (HttpWebResponse)restclient.GetResponse();
                if ((int)response.StatusCode < 200 || (int)response.StatusCode > 299)
                {
                    throw new Exception("Bad status code: " + response.StatusCode);
                }

                StreamReader reader = new StreamReader(response.GetResponseStream());
                string json = reader.ReadToEnd();

                reader.Close();
                response.Close();

                return json;
            }
            catch (WebException e)
            {
                HttpWebResponse response = (HttpWebResponse)e.Response;

                StreamReader reader = new StreamReader(response.GetResponseStream());
                string json = reader.ReadToEnd();
                reader.Close();
                response.Close();

                throw new Exception("REST request for " + uri + " failed with response: " + (int)response.StatusCode + " " + response.StatusDescription + ", the body of the response was: " + json);
            }
        }

        protected void REST_Execute_DELETE(string uri)
        {
            WebRequest restclient = this.REST_GetClient(uri);
            restclient.Method = "DELETE";

            try
            {

                HttpWebResponse response = (HttpWebResponse)restclient.GetResponse();
                if ((int)response.StatusCode < 200 || (int)response.StatusCode > 299)
                {
                    throw new Exception("Bad status code: " + response.StatusCode);
                }
                response.Close();
            }
            catch (WebException e)
            {
                HttpWebResponse response = (HttpWebResponse)e.Response;

                StreamReader reader = new StreamReader(response.GetResponseStream());
                string json = reader.ReadToEnd();
                reader.Close();
                response.Close();

                throw new Exception("REST request for " + uri + " failed with response: " + (int)response.StatusCode + " " + response.StatusDescription + ", the body of the response was: " + json);
            }
        }

        protected override void ExecuteRemove(ModuleService moduleService)
        {
            this.REST_Execute_DELETE(this.GetResourceURL(moduleService));
        }

        protected override void ExecuteModify(ModuleService oldService, Base.Module.ModuleService service)
        {
            this.REST_Execute_PUT(this.GetResourceURL(service), this.GetPostData(service));
        }

        protected override void ExecuteAdd(ModuleService moduleService)
        {
            Dictionary<string,string> response = this.REST_Execute_POST(this.GetResourceURL(service), this.GetPostData(service));
            if (response.ContainsKey("id"))
            {
                moduleService["Id"] = response["id"];
            }
            else
            {
                throw new Exception("id not found in response, which contained " + string.Join(", ", response.Select(pair => pair.Key + "='" + pair.Value + "'").ToArray()));
            }
        }

        protected override void ValidateService(ModuleService moduleService)
        {
        }

        protected abstract string GetResourceURL(ModuleService moduleService);


        protected virtual Dictionary<string, string> GetPostData(ModuleService service)
        {
            var post_data = new Dictionary<string, string>();

            foreach (ModuleServiceProperty prop in service.Properties)
            {
                if (!string.IsNullOrEmpty(prop.Value))
                {
                    post_data[prop.Name.ToLower()] = prop.Value;
                }
            }

            return post_data;
        }

    }
}
