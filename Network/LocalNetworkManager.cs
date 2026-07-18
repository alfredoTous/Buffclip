class LocalNetworkManager : NetworkManager
{
    public override bool IsConnected => false;

    public override void SendUpdateBuffer(byte id_buf)
    {
        // Local mode does not send updates over the network.
    }
}
