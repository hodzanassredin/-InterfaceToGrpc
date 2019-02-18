using Microsoft.Hadoop.Avro;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UniformRpc
{
    public static class Serializer<T>
    {
        static readonly IAvroSerializer<T> ser;
        static Serializer()
        {
            ser = AvroSerializer.Create<T>(new AvroSerializerSettings() { UseCache = true });
        }
        public static byte[] ToBytes(T obj)
        {
            using (var buffer = new MemoryStream())
            {
                ser.Serialize(buffer, obj);
                return buffer.ToArray();
            }
        }

        public static T FromBytes(byte[] bytes)
        {
            using (var buffer = new MemoryStream(bytes))
            {
                return ser.Deserialize(buffer);
            }
        }
    }
}
