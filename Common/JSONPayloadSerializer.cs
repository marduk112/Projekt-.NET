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
        public static byte[] Serialize(Object dto)
        {
            using (var stream = new MemoryStream())
            {
                JsonSerializer.SerializeToStream(dto, stream);
                return stream.GetBuffer();
            }
        }

        public static Object Deserialize(byte[] data)
        {
            using (var stream = new MemoryStream(data))
            {
                var serializer = JsonSerializer.DeserializeFromStream<Object>(stream);
                return serializer;
            }
        }
    }
}
