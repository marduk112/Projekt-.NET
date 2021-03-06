﻿namespace Common
{
    public static class Const
    {
        public const string HostName = "150.254.79.254";
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
        public static string ServerFriendAddRequestRoute = "server.request.addfriend";
        public static string ServerFriendDeleteRequestRoute = "server.request.deletefriend";
        public static string ClientExchange = "ClientExchange";

    }
}
