using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.Text;

namespace Common
{
    public static class JSONPayloadSerializer
    {
        public static byte[] Serialize(MessageDTO dto)
        {
            using (var stream = new MemoryStream())
            {
                JsonSerializer.SerializeToStream(dto, stream);
                return stream.GetBuffer();
            }
        }

        public static MessageDTO Deserialize(byte[] data)
        {
            using (var stream = new MemoryStream(data))
            {
                var serializer = JsonSerializer.DeserializeFromStream<MessageDTO>(stream);
                return serializer;
            }
        }
    }
}
