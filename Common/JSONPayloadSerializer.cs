using System;
using System.Collections.Generic;
using System.IO;
using ServiceStack.Text;

namespace Common
{
    public static class JSONPayloadSerializer
    {
        public static byte[] Serialize(this Object dto)
        {
            using (var stream = new MemoryStream())
            {
                JsonSerializer.SerializeToStream(dto, stream);
                return stream.GetBuffer();
            }
        }
        public static MessageReq DeserializeMessageReq(this byte[] data)
        {
            using (var stream = new MemoryStream(data))
            {
                return JsonSerializer.DeserializeFromStream<MessageReq>(stream);
            }
        }
        public static AuthResponse DeserializeAuthResponse(this byte[] data)
        {
            using (var stream = new MemoryStream(data))
            {
                return JsonSerializer.DeserializeFromStream<AuthResponse>(stream);
            }
        }
        public static ActivityReq DeserializeActivityReq(this byte[] data)
        {
            using (var stream = new MemoryStream(data))
            {
                return JsonSerializer.DeserializeFromStream<ActivityReq>(stream);
            }
        }
        public static CreateUserResponse DeserializeCreateUserResponse(this byte[] data)
        {
            using (var stream = new MemoryStream(data))
            {
                return JsonSerializer.DeserializeFromStream<CreateUserResponse>(stream);
            }
        }
        public static UserListResponse DeserializeUserListResponse(this byte[] data)
        {
            using (var stream = new MemoryStream(data))
            {
                return JsonSerializer.DeserializeFromStream<UserListResponse>(stream);
            }
        }
        public static AuthRequest DeserializeAuthRequest(this byte[] data)
        {
            using (var stream = new MemoryStream(data))
            {
                return JsonSerializer.DeserializeFromStream<AuthRequest>(stream);
            }
        }
        public static CreateUserReq DeserializeCreateUserReq(this byte[] data)
        {
            using (var stream = new MemoryStream(data))
            {
                return JsonSerializer.DeserializeFromStream<CreateUserReq>(stream);
            }
        }
        public static UserListReq DeserializeUserListReq(this byte[] data)
        {
            using (var stream = new MemoryStream(data))
            {
                return JsonSerializer.DeserializeFromStream<UserListReq>(stream);
            }
        }
        public static PresenceStatusNotification DeserializePresenceStatusNotification(this byte[] data)
        {
            using (var stream = new MemoryStream(data))
            {
                return JsonSerializer.DeserializeFromStream<PresenceStatusNotification>(stream);
            }
        }
    }
}
