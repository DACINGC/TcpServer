using System.Text;
public class SignData : BaseNetData
{
    public string username = "";
    public string password = "";
    public override int GetBytesNum()
    {
        return 8 + Encoding.UTF8.GetBytes(username).Length + Encoding.UTF8.GetBytes(password).Length;
    }

    public override int Reading(byte[] bytes, int beginIndex = 0)
    {
        int index = beginIndex;
        username = ReadString(bytes, ref index);
        password = ReadString(bytes, ref index);
        return index - beginIndex;
    }
    //序列化和反序列化的顺序要一致
    public override byte[] Writing()
    {
        byte[] bytes = new byte[GetBytesNum()];
        int index = 0;
        WriteString(bytes, username, ref index);
        WriteString(bytes, password, ref index);
        return bytes;
    }
}

