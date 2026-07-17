using System.Net.Sockets;

class BuffclipClient : NetworkManager
{

    private string ip = "";

    public BuffclipClient(string ip, int port)
    {
        this.ip      = ip;
        this.port    = port;
        this.node_id = 2; // 2 for now
    }

    public void Connect()
    {
        this.client = new TcpClient();
        Console.WriteLine($"[i] Connecting to {ip}:{this.port}...");
        this.client.Connect(this.ip, this.port);

        this.NetStream = this.client.GetStream();
    }

    public void SendFullSyncRequest(byte node_id)
    {
        Packet packet = new Packet(node_id, Opcode.FullSync);
        SendPacket(packet);
    }

}


