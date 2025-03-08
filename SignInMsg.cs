public class SignInMsg : BaseNetMsg
{
    public SignData signData;
    public override int GetBytesNum()
    {
        return 4 //消息类型的ID
            + 4 //消息体的长度
            + signData.GetBytesNum();
    }
    public override byte[] Writing()
    {
        int index = 0;
        int byteNum = GetBytesNum();
        byte[] bytes = new byte[byteNum];
        //写入消息体ID
        WriteInt(bytes, GetID(), ref index);
        //写入消息体的长度
        WriteInt(bytes, byteNum - 8, ref index);

        //写入消息体的成员变量
        WriteData(bytes, signData, ref index);
        return bytes;
    }
    //反序列化不用读消息类型ID 会统一区分
    public override int Reading(byte[] bytes, int beginIndex = 0)
    {
        int index = beginIndex;
        signData = ReadData<SignData>(bytes, ref index);
        return index - beginIndex;
    }
    public override int GetID()
    {
        return 1010;
    }
}


