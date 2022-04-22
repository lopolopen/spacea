using System;

namespace SpaceA.Model
{
    public class ConnectionUser
    {
        // 连接ID
        public string ConnectionID { get; set; }

        // 用户名称
        public string Name { get; set; }

        public ConnectionUser(string name, string connectionId)
        {
            this.Name = name;
            this.ConnectionID = connectionId;
        }
    }
}