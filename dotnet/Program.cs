using System.Net.Sockets;
using System.Text;


var lines = File.ReadAllLines("instructions.txt");
if(lines.Length < 3 || !lines[0].StartsWith("IP") || !lines[1].StartsWith("PORT"))
{
    Console.WriteLine("Invalid file");
    return;
}
string ipAddress = lines[0].Split(" ")[1];
int port = Convert.ToInt32(lines[1].Split(" ")[1]);

var waitMap = new Dictionary<string, int>();

// Connect to the instrument
TcpClient client = new TcpClient(ipAddress, port);
NetworkStream stream = client.GetStream();

foreach (var line in lines.Skip(2))
{
    SendCommand(stream, line, waitMap);
}

// Close the connection
stream.Close();
client.Close();

static void SendCommand(NetworkStream stream, string command, Dictionary<string, int> waitMap)
{
    if (string.IsNullOrWhiteSpace(command))
    {
        return;
    }
    Console.WriteLine(command);
    if (command.StartsWith("DEF"))
    {
        var parts = command.Split(" ");
        if (parts.Length == 3 && parts[1].StartsWith("WAIT") && !"WAIT".Equals(parts[1]))
        {            
            int ms = Convert.ToInt32(parts[2]);
            waitMap[parts[1]] = ms;
        }
    }
    else if (command.StartsWith("WAIT"))
    {
        var parts = command.Split(" ");
        if (!waitMap.TryGetValue(parts[0], out int ms))
        {
            if ("WAIT".Equals(parts[0]))
            {
                ms = Convert.ToInt32(parts[1]);
            }
            else
            {
                Console.WriteLine($"WARNING: wait not recognized - {parts[0]}");
                return;
            }            
        }
        Thread.Sleep(ms);

    }
    else
    {
        // Convert the command to bytes and send it over the network stream
        byte[] data = Encoding.ASCII.GetBytes(command + "\n");
        stream.Write(data, 0, data.Length);
    }
}