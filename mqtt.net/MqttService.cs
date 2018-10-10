using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace mqtt.net
{
    public class MqttService : IMqttService
    {
        private string hostName;
        private int port;
        private string ca;
        private string username;
        private string password;
        private string[] topics;
        private byte[] qosLevels;
        private MqttClient client;
        public MqttClient.MqttMsgPublishEventHandler MqttMsgPublishReceived;

        public MqttService()
        {
            this.hostName = ConfigurationManager.AppSettings["MqttHostName"];
            this.port = int.Parse(ConfigurationManager.AppSettings["MqttPort"]);
            this.username = ConfigurationManager.AppSettings["MqttUserName"];
            this.password = ConfigurationManager.AppSettings["MqttPassword"];
            this.ca = ConfigurationManager.AppSettings["MqttCA"];
        }

        /// <summary>
        /// 连接mqtt服务器
        /// </summary>
        /// <returns></returns>
        public async Task<Boolean> Connect()
        {
            try
            {
                X509Certificate caCert = new X509Certificate(this.ca);
                client = new MqttClient(this.hostName,
                                     this.port,
                                     true,
                                     caCert,
                                     null,
                                     MqttSslProtocols.TLSv1_0,
                                     new RemoteCertificateValidationCallback(cafileValidCallback));
                client.ProtocolVersion = MqttProtocolVersion.Version_3_1;
                client.MqttMsgPublishReceived += MqttMsgPublishReceived;
                bool isConnected = false;
                await Task.Run(() =>
                {
                    try {
                        if (this.username.Length > 0 && this.password.Length > 0)
                        {
                            this.client.Connect(Guid.NewGuid().ToString(), this.username, this.password);
                        }
                        else
                        {
                            this.client.Connect(Guid.NewGuid().ToString());
                        }
                        isConnected = true;
                    }
                    catch(Exception e)
                    {
                        isConnected = false;
                        Console.WriteLine(e.Message);
                    }
                });
                return isConnected;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        private bool cafileValidCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        /// <summary>
        /// 订阅mqtt消息
        /// </summary>
        /// <param name="topics">主题</param>
        /// <returns></returns>
        public Boolean Subscribe(string[] topics)
        {
            if (this.topics == null)
            {
                this.topics = topics;
            }
            if (this.qosLevels == null)
            {
                //qosLevels.Length一定要等于topics.Length
                List<byte> qosLevels = new List<byte>();
                for (int i = 0; i < topics.Length; i++)
                {
                    qosLevels.Add(MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE);
                }
                this.qosLevels = qosLevels.ToArray();
            }
            try
            {
                client.Subscribe(this.topics, this.qosLevels);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        /// <summary>
        /// mqtt断连重连的线程
        /// </summary>
        public async void Reconnect()
        {
            while (true)
            {
                await StartReconnect();
                try
                {
                    Thread.Sleep(3000);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        private async Task<Boolean> StartReconnect()
        {
            if (client != null && client.IsConnected)
            {
                return true;
            }
            if (client != null)
            {
                try
                {
                    client.Disconnect();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return false;
                }
                finally
                {
                    client = null;
                }
            }

            Boolean isConnected = await Connect();
            if (!isConnected)
            {
                return false;
            }
            Boolean isSubscribed = Subscribe(this.topics);
            if (!isSubscribed)
            {
                return false;
            }
            return true;
        }
    }
}
