using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Lib
{
    public class Utilities
    {
        public static void SerializeItem(object obj, string path)
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(path, FileMode.Create, FileAccess.Write);

            formatter.Serialize(stream, obj);
            stream.Close();
        }


        public static object DeserializeItem(string path)
        {
            object output;
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            output = formatter.Deserialize(stream);
            stream.Close();
            return output;
        }
    }
}
