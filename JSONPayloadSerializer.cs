using System;
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

        public static Object Deserialize(this byte[] data)
        {
            using (var stream = new MemoryStream(data))
            {
                var serializer = JsonSerializer.DeserializeFromStream<Object>(stream);
                return serializer;
            }
        }
    }
}
