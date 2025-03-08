public class QuitMsg : BaseNetMsg
{
    public override int GetBytesNum()
    {
        return 4//消息ID
            + 4;//消息长度
    }

    public override int GetID()
    {
        return 1003;
    }

    public override byte[] Writing()
    {
        byte[] bytes = new byte[GetBytesNum()];
        int index = 0;
        WriteInt(bytes, GetID(), ref index);
        WriteInt(bytes, 0, ref index);
        return bytes;
    }

    public override int Reading(byte[] bytes, int beginIndex = 0)
    {

        return 0;
    }
}
