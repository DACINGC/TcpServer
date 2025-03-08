public class HeartMsg : BaseNetMsg
{
    public override int GetBytesNum()
    {
        return 8;
    }

    public override byte[] Writing()
    {
        byte[] bytes = new byte[8];
        int index = 0;
        WriteInt(bytes, GetID(), ref index);
        WriteInt(bytes, 0, ref index);
        return bytes;
    }

    public override int Reading(byte[] bytes, int beginIndex = 0)
    {
        return 0;
    }

    public override int GetID()
    {
        return 999;
    }
}
