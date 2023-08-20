using System.IO.Ports;

Console.WriteLine("EEPROM Programmer for Arduino MEGA");
Console.WriteLine("Arduino Project: http://danceswithferrets.org/geekblog/?page_id=903");

if(args.Length != 1)
{
    Console.WriteLine("Usage: eeprom_programmer ROM_FILE_NAME");
    return;
}
if(!File.Exists(args[0]))
{
    Console.WriteLine("Error: File not found.");
    return;
}

FileStream fs = File.Open(args[0], FileMode.Open);
SerialPort sp = new SerialPort("/dev/ttyACM0", 9600);
Console.Write($"Writing {args[0]}");
sp.NewLine = "\r\n";
int pos = 0;
var write = (List<byte> bytes) => 
{
    while(bytes.Count < 16)
    {
        bytes.Add(0); 
    }
    var sendData = "";
    foreach(var by in bytes)
    {
        sendData += $"{by:X2}";
    }
    Thread.Sleep(100);
    sp.WriteLine($"W{pos:X4}:{sendData}");
    if(sp.ReadLine() != "OK") Console.WriteLine("Write error");
    sp.WriteLine($"R{pos:X4}");
    var rawData = sp.ReadLine();
    var recvData = rawData.Split(':')[1].Substring(0, sendData.Length);
    if(sp.ReadLine() != "OK") Console.WriteLine("Read error");
    if(sendData.ToUpper() == recvData.ToUpper())
    {
        Console.Write(".");
    }
    else
    {
        Console.WriteLine("[ Data corrupted ]");
        Console.WriteLine($"Send: [{sendData}]");
        Console.WriteLine($"Recv: [{recvData}]");
        Environment.Exit(1);
    }

    pos += 16;
};
int b;
List<byte> bloco = new List<byte>();
sp.Open();
sp.WriteLine($"W0000:00000000000000000000000000000000");
Thread.Sleep(1000);
sp.DiscardInBuffer();
sp.DiscardOutBuffer();
while((b = fs.ReadByte()) >= 0 )
{
    bloco.Add((byte)b);
    if(bloco.Count() == 16)
    {
        write(bloco);
        bloco.Clear();
    }
}

if(bloco.Any())
{
    write(bloco);
}
sp.Close();

Console.WriteLine("[ OK ]");