using System.Text;
using System.Net;
using System.Net.Sockets;


class NetworkManager
{
    private int port;
    TcpClient? client;
    private NetworkStream? NetStream;

    public NetworkManager(int port)
    {
        this.port = port;
    }

    public void StartServer()
    {
        TcpListener listener = new TcpListener(IPAddress.Any, this.port);
        listener.Start();
        Console.WriteLine($"[+] Server listening on {IPAddress.Any}:{this.port}...");

        while (true) {
            TcpClient client = listener.AcceptTcpClient();
            Console.WriteLine($"[+] Client connected: {client.Client.RemoteEndPoint}");
            HandleClient(client);
        }
    }

    private void HandleClient(TcpClient client)
    {
        Console.WriteLine("[+] Handling client...");

        this.client = client;
        this.NetStream = client.GetStream();

        try {
            while (true) {
                Packet packet = ReceivePacket();

                switch (packet.opcode) {
                    case Opcode.FullSync:
                        HandleFullSyncRequest();
                        break;

                    case Opcode.UpdateBuffer:
                        //HandleUpdateBuffer(packet);
                        break;

                    default:
                        Console.WriteLine($"[-] Unknow opcode: {packet.opcode}");
                        break;
                }
            }
        } catch (Exception ex) {
            Console.WriteLine($"[-] Client disconnected: {ex.Message}");
        } finally {
            this.NetStream?.Close();
            client.Close();

            this.NetStream = null;
            this.client = null;
        }
    }

    public void Connect(string ip)
    {
        this.client = new TcpClient();
        Console.WriteLine($"[i] Connecting to {ip}:{this.port}...");
        this.client.Connect(ip, this.port);

        this.NetStream = this.client.GetStream();
    }

    public void SendFullSyncRequest(byte node_id)
    {
        Packet packet = new Packet(node_id, Opcode.FullSync);
        SendPacket(packet);
    }

    public void HandleFullSyncRequest()
    {
        Console.WriteLine("Recibida la FullSyncRequest, preparando paquetes...");
    }

    private void SendPacket(Packet packet)
    {

        if (this.NetStream == null)
            throw new Exception("[-] Not connected");

        byte[] packetBytes = packet.ToBytes();
        byte[] lenBytes    = BitConverter.GetBytes(packetBytes.Length);

        this.NetStream.Write(lenBytes, 0, lenBytes.Length); // Send a header for packet total len
        this.NetStream.Write(packetBytes, 0, packetBytes.Length);

    }

    private Packet ReceivePacket()
    {
        if (this.NetStream == null)
            throw new Exception("[-] Not connected");

        byte[] lenBytes = new byte[sizeof(int)];
        ReadExact(lenBytes, sizeof(int)); // Read packet total len
        
        int packetLen = BitConverter.ToInt32(lenBytes);
        byte[] packetBytes = new byte[packetLen];
        ReadExact(packetBytes, packetLen);

        return Packet.FromBytes(packetBytes);

    }

    private void ReadExact(byte[] buffer, int len)
    {
        int totalRead = 0;

        while (totalRead < len) {
            int bytesRead = this.NetStream!.Read(buffer, totalRead, len-totalRead);
            
            if (bytesRead == 0)
                throw new Exception("Connection closed");

            totalRead+=bytesRead;
        }
    }


}

enum Opcode : byte
{
    UpdateBuffer  = 1, // Update buffer
    FullSync      = 2  // When a new machine is connected
}


// Protocol
class Packet
{
    public byte     node_id;      // Machine that send the Packet (maybe change for IP)
    public Opcode   opcode;       // Intruction
    public byte     id_buf;       // Buffer ID to operate
    public int      len;          // Length of content
    public string   content = ""; // Content to copy/paste


    public Packet(byte node_id, Opcode opcode, byte id_buf, string content)
    {
        this.node_id  = node_id;
        this.opcode   = opcode;
        this.id_buf   = id_buf;
        this.len      = Encoding.UTF8.GetByteCount(content);;
        this.content  = content;
    }

    // Constructor for FullSync packets
    public Packet(byte node_id, Opcode opcode)
    {
        this.node_id = node_id;
        this.opcode  = opcode;
        this.id_buf  = 0;
        this.len     = 0;
        this.content = "";
    }


    public byte[] ToBytes()
    {
        byte[] packetBytes  = new byte[sizeof(byte)+sizeof(Opcode)+sizeof(byte)+sizeof(int)+this.len]; // Create new byte array of size received packet

        byte[] lenBytes     = BitConverter.GetBytes(this.len);        // Get bytes for packet.len;
        byte[] contentBytes = Encoding.UTF8.GetBytes(this.content);   // Get bytes for packet.content;

        // Fill up byte array
        int idx = 0;

        packetBytes[idx] = this.node_id;  // node_id
        idx++;

        packetBytes[idx] = (byte)this.opcode; // opcode
        idx++;

        packetBytes[idx] = this.id_buf;   // id_buf
        idx++;

        Array.Copy(lenBytes, 0, packetBytes, idx, lenBytes.Length); // len
        idx += lenBytes.Length;

        Array.Copy(contentBytes, 0, packetBytes, idx, contentBytes.Length); // content

        return packetBytes;
    }


    static public Packet FromBytes(byte[] packetBytes)
    {
        int idx = 0;

        byte node_id = packetBytes[idx];
        idx++;

        Opcode opcode = (Opcode)packetBytes[idx];
        idx++;

        byte id_buf = packetBytes[idx];
        idx++;

        int len = BitConverter.ToInt32(packetBytes, idx);
        idx += sizeof(int);

        string content = Encoding.UTF8.GetString(packetBytes, idx, len);

        return new Packet(node_id, opcode, id_buf, content);
    }

}
