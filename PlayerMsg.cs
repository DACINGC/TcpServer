public class PlayerMsg : BaseNetMsg
{
    public int playerID;
    public PlayerData playerData;
    public override int GetBytesNum()
    {
        return 4 //��Ϣ���͵�ID
            + 4 //��Ϣ��ĳ���
            + 4 //playerID
            + playerData.GetBytesNum();
    }
    public override byte[] Writing()
    {
        int index = 0;
        int byteNum = GetBytesNum();
        byte[] bytes = new byte[byteNum];
        //д����Ϣ��ID
        WriteInt(bytes, GetID(), ref index);
        //д����Ϣ��ĳ���
        WriteInt(bytes, byteNum - 8, ref index);

        //д����Ϣ��ĳ�Ա����
        WriteInt(bytes, playerID, ref index);
        WriteData(bytes, playerData, ref index);
        return bytes;
    }
    //�����л����ö���Ϣ����ID ��ͳһ����
    public override int Reading(byte[] bytes, int beginIndex = 0)
    {
        int index = beginIndex;
        playerID = ReadInt(bytes, ref index);
        playerData = ReadData<PlayerData>(bytes, ref index);
        return index - beginIndex;
    }
    public override int GetID()
    {
        return 1001;
    }
}
