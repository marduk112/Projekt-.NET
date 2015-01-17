namespace Common
{
    public static class Const
    {
        public const string HostName = "127.0.0.1";
        /// <summary>
        /// Was used to login and register users. Deprecated.
        /// </summary>
        public const string FileNameToRegAndLogin = @"Users.xml";
        public static User User = new User();

        /// <summary>
        /// Route for user message notification handling. Should be formatted like this:
        /// ClientMessageNotificationRoute + *sender* + "." + "recipent"
        /// </summary>
        public static string ClientMessageNotificationRoute = "client.notification.message.";
        public static string ClientPresenceStatusNotificationRoute = "client.notification.presence.";
        public static string ServerMessageRequestRoute = "server.request.message";
        public static string ServerStatusRequestRoute = "server.request.status";
        public static string ServerFriendListRequestRoute = "server.request.friends";
        public static string ClientExchange = "ClientExchange";

    }
}
