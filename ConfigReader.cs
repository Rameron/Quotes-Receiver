using System;
using System.IO;
using System.Net;
using System.Xml;
using Quotes_Receiver.Models;

namespace Quotes_Receiver
{
    public class ConfigReader
    {
        public ReceiveConfig ReadReceiveConfig()
        {
            if (!File.Exists("config.xml"))
            {
                throw new Exception(
                    "XML configuration does not exist.");
            }

            var xmlDoc = new XmlDocument();
            xmlDoc.Load("config.xml");
            var configNode = GetXmlNode(xmlDoc.DocumentElement, "/configuration");

            var receiveConfig = new ReceiveConfig
            {
                ReceiveDelayMs = TryParseValue<int>(GetXmlNode(configNode, "receive-delay-ms"), int.TryParse),
                MulticastGroup = TryParseValue<IPAddress>(GetXmlNode(configNode, "multicast-group"), IPAddress.TryParse),
                MulticastPort = TryParseValue<int>(GetXmlNode(configNode, "multicast-port"), int.TryParse)
            };

            if (receiveConfig.ReceiveDelayMs >= 1000)
            {
                throw new Exception("XML configuration is invalid: 'receive-delay-ms' should be less than 1000 ms.");
            }

            return receiveConfig;
        }

        private XmlNode GetXmlNode(XmlNode xmlElement, string xPath)
        {
            var targetNode = xmlElement.SelectSingleNode(xPath);
            if (targetNode == null)
            {
                throw new Exception($"XML configuration is invalid: node '{xPath}' not found.");
            }

            return targetNode;
        }

        private T TryParseValue<T>(XmlNode targetNode, TryParseHandler<T> tryParseHandler)
        {
            if (string.IsNullOrWhiteSpace(targetNode.InnerText))
            {
                throw new Exception(
                    $"XML configuration is invalid: node '{targetNode.LocalName}' must have value.");
            }

            if (tryParseHandler(targetNode.InnerText, out var valueResult))
            {
                return valueResult;
            }

            throw new Exception(
                $"XML configuration is invalid: node '{targetNode.LocalName}' must have type '{typeof(T).FullName}'");
        }

        private delegate bool TryParseHandler<T>(string value, out T result);
    }
}