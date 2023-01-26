using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NetworkingTest1;
internal class Program
{
	static void Main()
	{
		Console.WriteLine("Start as server or client?");

		var mode = Console.ReadLine();
		if (mode == "Server")
		{
			var tcpListener = new TcpListener(IPAddress.Any, 42069);
			tcpListener.Start();
			Console.WriteLine("Listening for connection...");

			var tcpClient = tcpListener.AcceptTcpClient();

			tcpClient.GetStream().Write(Encoding.UTF8.GetBytes("Hello from Cory!"));

			var outputThread = new Thread(new ParameterizedThreadStart(OutputThreadMethod));
			outputThread.Start(tcpClient);

			var inputThread = new Thread(new ParameterizedThreadStart(InputThreadMethod));
			inputThread.Start(tcpClient);
		}
		else if (mode == "Client")
		{
			var tcpClient = new TcpClient();
			while (!tcpClient.Connected)
			{
				Console.WriteLine("IP Address to connect to: ");
				var targetIP = Console.ReadLine();

				if (targetIP == "localhost")
					tcpClient.Connect(targetIP, 42069);
				else
				{
					tcpClient.Connect(IPAddress.Parse(targetIP!), 42069);
				}
			}

			var outputThread = new Thread(new ParameterizedThreadStart(OutputThreadMethod));
			outputThread.Start(tcpClient);

			var inputThread = new Thread(new ParameterizedThreadStart(InputThreadMethod));
			inputThread.Start(tcpClient);
		}
	}

	static void OutputThreadMethod(object? data)
	{
		if (data != null)
		{
			var tcpClient = (TcpClient) data;
			while (true)
			{
				var inputSpan = new Span<byte>(new byte[1024]);
				lock (tcpClient)
					tcpClient.GetStream().Read(inputSpan);

				string Output = Encoding.UTF8.GetString(inputSpan.ToArray());
				if (Output != "")
					Console.Write(Output);
			}
		}
	}

	static void InputThreadMethod(object? data)
	{
		if (data != null)
		{
			var tcpClient = (TcpClient) data;
			while (true)
			{
				var Input = Console.ReadLine() + "\n";
				lock (tcpClient)
					tcpClient.GetStream().Write(Encoding.UTF8.GetBytes(Input!));
			}
		}
	}
}
