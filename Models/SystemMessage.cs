using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using System.Xml.Linq;

namespace StudyAssistant.Web.Models
{
    public enum MessageType : byte
    {
        Critical, Warning, Success, Information
    }

    public class SystemMessage
    {
        private readonly MessageType _type;
        private readonly string _message;

        public SystemMessage(MessageType type, string message)
        {
            _type = type;
            _message = message;
        }

        public string GetSystemMessage()
        {
            string strAttributes = "alert alert-dismissable";
            string strIcon = "";

            switch (_type)
            {
                case MessageType.Critical:
                    strAttributes += " alert-danger";
                    strIcon += "fa fa-times fa-2x";
                    break;
                case MessageType.Warning:
                    strAttributes += " alert-warning";
                    strIcon += "fa fa-warning fa-2x";
                    break;
                case MessageType.Success:
                    strAttributes += " alert-success";
                    strIcon += "fa fa-thumbs-up fa-2x";
                    break;
                case MessageType.Information:
                    strAttributes += " alert-info";
                    strIcon += "fa fa-info-circle fa-2x";
                    break;
            }

            var html = new XDocument(new XElement("div", 
                new XAttribute("class", strAttributes), 
                    new XElement("a",
                        new XAttribute("class", "close"),
                        new XAttribute("data-dismiss", "alert"),
                        new XAttribute("aria-label", "lukk"),
                        new XAttribute("href", "#"),
                        "x"
                        ),
                    new XElement("i", 
                        new XAttribute("class", strIcon), ""), 
                _message));

            return html.ToString();
        }


    }
}
