using System;
using System.Threading.Tasks;

namespace mqtt.net
{
    public interface IMqttService
    {
        /// <summary>
        /// 连接mqtt服务器
        /// </summary>
        /// <returns></returns>
        Task<Boolean> Connect();

        /// <summary>
        /// 订阅mqtt消息
        /// </summary>
        /// <param name="topics">主题</param>
        /// <returns></returns>
        Boolean Subscribe(string[] topics);

        /// <summary>
        /// mqtt断连重连的线程
        /// </summary>
        void Reconnect();
    }
}
