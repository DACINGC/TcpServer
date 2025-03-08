using System.Text;
public class BroadMsg : BaseNetMsg
{
    public string msg;
    public override int GetBytesNum()
    {
        return 4 //消息ID
            + 4//消息长度
            + 4 //字符串长度
            + Encoding.UTF8.GetBytes(msg).Length;
    }
    public override byte[] Writing()
    {
        byte[] bytes = new byte[GetBytesNum()];
        int index = 0;
        //写入消息类型
        WriteInt(bytes, GetID(), ref index);
        //写入消息长度
        WriteInt(bytes, GetBytesNum() - 8, ref index);

        //写入消息体
        WriteString(bytes, msg, ref index);
        return bytes;
    }
    public override int Reading(byte[] bytes, int beginIndex = 0)
    {
        int index = beginIndex;
        msg = ReadString(bytes, ref index);
        return index - beginIndex;
    }
    public override int GetID()
    {
        return 1000;
    }
}

