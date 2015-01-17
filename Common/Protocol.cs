using System;
using System.Collections;
using System.Collections.Generic;

namespace Common
{
    public class Response
    {
        public Status Status { get; set; }
        public string Message { get; set; }
    }

    public class Request
    {
        public string Login { get; set; }
    }

    public class Notification
    {

    }

    public class AuthRequest : Request
    {
        public string Password { get; set; }
    }

    //to determine
    public enum Status
    {
        OK,
        Error,
        NotAuthenticated,
    }



    public class AuthResponse : Response
    {
        public bool IsAuthenticated { get; set; }
    }

    public class UserListReq : Request
    {
    }

    //to determine
    public enum PresenceStatus
    {
        Online,
        Offline,
        Afk,//away from keyboard
    }

    public class User
    {
        public string Login { get; set; }
        public PresenceStatus Status { get; set; }
    }

    public class UserListResponse : Response
    {
        public List<User> Users { get; set; }
    }

    public class CreateUserReq : Request
    {
        public string Password { get; set; }
    }

    public class CreateUserResponse : Response
    {
        public bool CreatedSuccessfully { get; set; }
    }

    public class Attachment
    {
        public byte[] Data { get; set; }
        public string Name { get; set; }
        public string MimeType { get; set; }
    }

    public class MessageReq : Request
    {
        public string Recipient { get; set; }
        public string Message { get; set; }
        public Attachment Attachment { get; set; }
        //Server
        public DateTimeOffset SendTime { get; set; }
    }

    public class MessageResponse : Response
    {
        public string Recipient { get; set; }
        public Attachment Attachment { get; set; }
        //Server
        public DateTimeOffset SendTime { get; set; }
    }


    public class MessageNotification : Notification
    {
        public string Sender { get; set; }
        public string Recipient { get; set; }
        public string Message { get; set; }
        public Attachment Attachment { get; set; }
        public DateTimeOffset SendTime { get; set; }
    }

    public class PresenceStatusNotification : Notification
    {
        public string Login { get; set; }
        public PresenceStatus PresenceStatus { get; set; }
        public string Recipient { get; set; }
    }

    public class ActivityNotification : Notification
    {
        public string Sender { get; set; }
        public bool IsWriting { get; set; }
    }

    public class ActivityReq : Request
    {
        public bool IsWriting { get; set; }
        public string Recipient { get; set; }
    }

    public class ActivityResponse : Response
    {
        public bool IsWriting { get; set; }
        public string Recipient { get; set; }
    }

    public class AddFriendReq : Request
    {
        public string FriendLogin { get; set; }
    }

    public class AddFriendResponse : Response {}
    public class DeleteFriendReq : Request
    {
        public string FriendLogin { get; set; }
    }

    public class DeleteFriendResponse : Response {}
}
