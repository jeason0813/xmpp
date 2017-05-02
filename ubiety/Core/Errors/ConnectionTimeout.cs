﻿// ConnectionTimeout.cs
//
//Ubiety XMPP Library Copyright (C) 2017 Dieter Lunn
//
//This library is free software; you can redistribute it and/or modify it under
//the terms of the GNU Lesser General Public License as published by the Free
//Software Foundation; either version 3 of the License, or (at your option)
//any later version.
//
//This library is distributed in the hope that it will be useful, but WITHOUT
//ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
//FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more
//
//You should have received a copy of the GNU Lesser General Public License along
//with this library; if not, write to the Free Software Foundation, Inc., 59
//Temple Place, Suite 330, Boston, MA 02111-1307 USA

using System.Xml;
using Ubiety.Common;
using Ubiety.Infrastructure.Attributes;

namespace Ubiety.Core.Errors
{
    /// <summary>
    ///     One party is closing the stream because it has reason to believe that
    /// the other party has permanently lost the ability to communicate over the
    /// stream.
    /// </summary>
    [XmppTag("connection-timeout", Namespaces.XmppStreams, typeof(ConnectionTimeout))]
    public class ConnectionTimeout : Tag
    {
        /// <summary>
        ///     Instantiate a new instance
        /// </summary>
        public ConnectionTimeout() : base("", new XmlQualifiedName("connection-timeout", Namespaces.XmppStreams)) { }
    }
}
