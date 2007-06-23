//XMPP .NET Library Copyright (C) 2006 Dieter Lunn

//This library is free software; you can redistribute it and/or modify it under
//the terms of the GNU Lesser General Public License as published by the Free
//Software Foundation; either version 2.1 of the License, or (at your option)
//any later version.

//This library is distributed in the hope that it will be useful, but WITHOUT
//ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
//FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more
//details.

//You should have received a copy of the GNU Lesser General Public License along
//with this library; if not, write to the Free Software Foundation, Inc., 59
//Temple Place, Suite 330, Boston, MA 02111-1307 USA 

using System;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Text;

using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;
using System.Net.Security;

using log4net;
using log4net.Config;

namespace xmpp.net
{
    /// <remarks>
    /// AsyncSocket is the class that communicates with the server.
    /// </remarks>
	public class AsyncSocket
	{
		private Socket _socket;
		private Decoder _decoder = Encoding.UTF8.GetDecoder();
		private UTF8Encoding _utf = new UTF8Encoding();
		private Address _dest;
		private byte[] _buff = new byte[4096];

        private X509Certificate _cert;
        private Stream _stream;

		private static readonly ILog logger = LogManager.GetLogger(typeof(AsyncSocket));

        /// <summary>
        /// Occurs when a connection is established with a server.
        /// </summary>
		public event EventHandler Connection;

        /// <summary>
        /// Occurs when a message has been received from the server.
        /// </summary>
		public event EventHandler<MessageEventArgs> Message;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncSocket"/> class.
        /// </summary>
		public AsyncSocket()
		{
			XmlConfigurator.Configure();
			_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		}

        /// <summary>
        /// Establishes a connection to the specified remote host.
        /// </summary>
        /// <param name="hostname">Hostname to connect to</param>
        /// <param name="ssl">Is this connection to be encrypted?</param>
		public void Connect(string hostname, bool ssl)
		{
			try
			{
				_dest = new Address(5222);
				_dest.IP = IPAddress.Parse(hostname);
                _dest.Hostname = hostname;
			}
			catch (FormatException)
			{
				_dest = Address.Resolve(hostname, 5222);
			}
			_socket.Connect(_dest.EndPoint);
            if (_socket.Connected)
            {
                _stream = new NetworkStream(_socket);
            }
            _stream.BeginRead(_buff, 0, _buff.Length, new AsyncCallback(Receive), null);
			OnConnect();
		}

        /// <summary>
        /// Encrypts the connection using SSL/TLS
        /// </summary>
        public void StartSecure()
        {
			logger.Debug("Starting Secure Mode");
            Stream sslstream = new SslStream(_stream, false, new RemoteCertificateValidationCallback(RemoteValidation), null);
			logger.Debug("Authenticating as Client");
            ((SslStream)sslstream).AuthenticateAsClient(_dest.Hostname, null, SslProtocols.Tls, false);

			_stream = sslstream;
        }

        private bool RemoteValidation(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors errors)
        {
            if (errors == SslPolicyErrors.None)
            {
                return true;
            }

			logger.Debug("Policy Errors: " + errors);

            return false;
        }

        /// <summary>
        /// Closes the current socket.
        /// </summary>
        public void Close()
        {
            logger.Debug("Closing socket (Graceful Shutdown)");
            _socket.Close();
        }

        /// <summary>
        /// Writes data to the current connection.
        /// </summary>
        /// <param name="msg">Message to send</param>
		public void Write(string msg)
		{
			logger.Debug("Outgoing Message: " + msg);
            byte[] mesg = _utf.GetBytes(msg);
            _stream.Write(mesg, 0, mesg.Length);
		}

		private void OnConnect()
		{
			if (Connection != null)
			{
				Connection(this, EventArgs.Empty);
			}
		}

		private void OnMessage(String message)
		{
			if (Message != null)
			{
				Message(this, new MessageEventArgs(message));
			}
		}

		private void Receive(IAsyncResult ar)
		{
			try
			{
				int rx = _stream.EndRead(ar);
				char[] chars = new char[rx];
				_decoder.GetChars(_buff, 0, rx, chars, 0);
				string msg = new string(chars);
				logger.Debug("Incoming Message: " + msg);
				_stream.BeginRead(_buff, 0, _buff.Length, new AsyncCallback(Receive), null);
				OnMessage(msg);
			}
			catch (SocketException e)
			{
				logger.Debug("Socket Exception", e);
			}
			catch (InvalidOperationException e)
			{
				logger.Debug("Invalid Operation", e);
			}
		}

        /// <summary>
        /// Gets the current status of the socket.
        /// </summary>
		public bool Connected
		{
			get { return _socket.Connected; }
		}
	}

	/// <remarks>
	/// Provides data for the Message events.
	/// </remarks>
	public class MessageEventArgs : EventArgs
	{
		private string _message;

		/// <summary>
		/// Initializes a new instance of the <see cref="MessageEventArgs"/> class.
		/// </summary>
		/// <param name="message">The message received from the stream</param>
		public MessageEventArgs(String message)
		{
			_message = message;
		}

		/// <summary>
		/// Gets the message received from the stream.
		/// </summary>
		public String Message
		{
			get { return _message; }
			set { _message = value; }
		}
	}
}