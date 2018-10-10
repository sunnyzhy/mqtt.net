using System;
using System.Configuration;
using System.Text;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace mqtt.net
{
    class Program
    {
        static void Main(string[] args)
        {
            Start();
            Console.Read();
        }

        private static async void Start()
        {
            MqttService mqtt = new MqttService();
            mqtt.MqttMsgPublishReceived += MqttMsgPublishReceived;
            await mqtt.Connect();
            string[] topics = ConfigurationManager.AppSettings["MqttTopic"].Split(',');
            mqtt.Subscribe(topics);
            mqtt.Reconnect();
        }

        private static void MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            string msg = "Topic:" + e.Topic + "   Message:" + Encoding.UTF8.GetString(e.Message);
            Console.WriteLine(msg);
        }
    }
}
