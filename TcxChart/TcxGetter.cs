using HtmlAgilityPack;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;

namespace TcxChart
{
    internal class TcxGetter
    {
        public TcxGetter()
        {
        }

        internal void Sync()
        {
            var client = new WebClient();
            var signinAddress = "https://sso.garmin.com/sso/signin?service=https%3A%2F%2Fconnect.garmin.com%2Fmodern%2F&webhost=https%3A%2F%2Fconnect.garmin.com%2Fmodern%2F&source=https%3A%2F%2Fconnect.garmin.com%2Fsignin&redirectAfterAccountLoginUrl=https%3A%2F%2Fconnect.garmin.com%2Fmodern%2F&redirectAfterAccountCreationUrl=https%3A%2F%2Fconnect.garmin.com%2Fmodern%2F&gauthHost=https%3A%2F%2Fsso.garmin.com%2Fsso&locale=en_US&id=gauth-widget&cssUrl=https%3A%2F%2Fconnect.garmin.com%2Fgauth-custom-v1.2-min.css&privacyStatementUrl=https%3A%2F%2Fwww.garmin.com%2Fen-US%2Fprivacy%2Fconnect%2F&clientId=GarminConnect&rememberMeShown=true&rememberMeChecked=false&createAccountShown=true&openCreateAccount=false&displayNameShown=false&consumeServiceTicket=false&initialFocus=true&embedWidget=false&generateExtraServiceTicket=true&generateTwoExtraServiceTickets=false&generateNoServiceTicket=false&globalOptInShown=true&globalOptInChecked=false&mobile=false&connectLegalTerms=true&showTermsOfUse=false&showPrivacyPolicy=false&showConnectLegalAge=false&locationPromptShown=true&showPassword=true&useCustomHeader=false";
            NameValueCollection values = new NameValueCollection();
            using (var stream = client.OpenRead(signinAddress))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    string loginDocument = reader.ReadToEnd();
                    HtmlDocument document = new HtmlDocument();
                    document.OptionUseIdAttribute = true;
                    document.LoadHtml(loginDocument);
                    var nodes = document.DocumentNode.Descendants().ToArray();
                    var elements = nodes.Where(node => node.NodeType == HtmlNodeType.Element).ToList();
                    var forms = nodes.Where(node => node.Name == "form").ToList();
                    var loginForms = nodes.Where(node => node.Id == "login-form").ToList();

                    foreach (var loginForm in loginForms)
                    {
                        var inputs = loginForm.Descendants().Where(node => node.Name == "input").ToList();
                        foreach (var input in inputs)
                        {
                            var attributes = input.Attributes.ToList();
                            var name = attributes.FirstOrDefault(a => a.Name == "name")?.Value;
                            if (name == "username")
                            {
                                values.Add("username", "barbara-garmin@wolffh.de");
                            }
                            else if (name == "password")
                            {
                                values.Add("password", "jadajada");
                            }
                            else
                            {
                                var value = attributes.FirstOrDefault(a => a.Name == "value")?.Value;
                                if (value != null)
                                {
                                    values.Add(name, value);
                                }
                            }
                        }
                    }

                    /*
                    values.Add("TextBox2", "value2");
                    values.Add("TextBox3", "value3");
                    string Url = urlvalue.ToLower();

                    */
                }
                using (WebClient postClient = new WebClient())
                {
                    postClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                    byte[] result = postClient.UploadValues("https://connect.garmin.com/modern", "POST", values);
                    string ResultAuthTicket = System.Text.Encoding.UTF8.GetString(result);
                }
            }

        }
    }
}