// GET BULK request message type.
// Copyright (C) 2008-2010 Malcolm Crowe, Lex Li, and other contributors.
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

/*
 * Created by SharpDevelop.
 * User: lextm
 * Date: 2008/8/3
 * Time: 15:13
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using Lextm.SharpSnmpLib.Security;

namespace Lextm.SharpSnmpLib.Messaging
{
    /// <summary>
    /// GETBULK request message.
    /// </summary>
    public class GetBulkRequestMessage : ISnmpMessage
    {
        private readonly byte[] _bytes;

        /// <summary>
        /// Creates a <see cref="GetBulkRequestMessage"/> with all contents.
        /// </summary>
        /// <param name="requestId">The request ID.</param>
        /// <param name="version">Protocol version.</param>
        /// <param name="community">Community name.</param>
        /// <param name="nonRepeaters">Non-repeaters.</param>
        /// <param name="maxRepetitions">Max repetitions.</param>
        /// <param name="variables">Variables.</param>
        public GetBulkRequestMessage(int requestId, VersionCode version, OctetString community, int nonRepeaters, int maxRepetitions, IList<Variable> variables)
        {
            if (variables == null)
            {
                throw new ArgumentNullException("variables");
            }
            
            if (community == null)
            {
                throw new ArgumentNullException("community");
            }
            
            if (version != VersionCode.V2)
            {
                throw new ArgumentException("only v2c are supported", "version");
            }

            if (nonRepeaters > variables.Count)
            {
                throw new ArgumentException("nonRepeaters should not be greater than variable count", "nonRepeaters");
            }

            if (maxRepetitions < 1)
            {
                throw new ArgumentException("maxRepetitions should be greater than 0", "maxRepetitions");
            }

            Version = version;
            Header = Header.Empty;
            Parameters = new SecurityParameters(null, null, null, community, null, null);
            GetBulkRequestPdu pdu = new GetBulkRequestPdu(
                requestId,
                nonRepeaters,
                maxRepetitions,
                variables);
            Scope = new Scope(pdu);
            Privacy = DefaultPrivacyProvider.DefaultPair;

            _bytes = SnmpMessageExtension.PackMessage(Version, Header, Parameters, Scope, Privacy).ToBytes();
        }

        /// <summary>
        /// Creates a <see cref="GetBulkRequestMessage"/> with a specific <see cref="Sequence"/>.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <param name="messageId">The message id.</param>
        /// <param name="requestId">The request id.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="nonRepeaters">The non repeaters.</param>
        /// <param name="maxRepetitions">The max repetitions.</param>
        /// <param name="variables">The variables.</param>
        /// <param name="privacy">The privacy provider.</param>
        /// <param name="report">The report.</param>
        [Obsolete("Please use other overloading ones.")]
        public GetBulkRequestMessage(VersionCode version, int messageId, int requestId, OctetString userName, int nonRepeaters, int maxRepetitions, IList<Variable> variables, IPrivacyProvider privacy, ISnmpMessage report)
            : this(version, messageId, requestId, userName, nonRepeaters, maxRepetitions, variables, privacy, 0xFFE3, report)
        {
        }

        /// <summary>
        /// Creates a <see cref="GetBulkRequestMessage"/> with a specific <see cref="Sequence"/>.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <param name="messageId">The message id.</param>
        /// <param name="requestId">The request id.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="nonRepeaters">The non repeaters.</param>
        /// <param name="maxRepetitions">The max repetitions.</param>
        /// <param name="variables">The variables.</param>
        /// <param name="privacy">The privacy provider.</param>
        /// <param name="maxMessageSize">Size of the max message.</param>
        /// <param name="report">The report.</param>
        public GetBulkRequestMessage(VersionCode version, int messageId, int requestId, OctetString userName, int nonRepeaters, int maxRepetitions, IList<Variable> variables, IPrivacyProvider privacy, int maxMessageSize, ISnmpMessage report)
        {
            if (variables == null)
            {
                throw new ArgumentNullException("variables");
            }
            
            if (userName == null)
            {
                throw new ArgumentNullException("userName");
            }
            
            if (version != VersionCode.V3)
            {
                throw new ArgumentException("only v3 is supported", "version");
            }

            if (report == null)
            {
                throw new ArgumentNullException("report");
            }
            
            if (privacy == null)
            {
                throw new ArgumentNullException("privacy");
            }

            if (nonRepeaters > variables.Count)
            {
                throw new ArgumentException("nonRepeaters should not be greater than variable count", "nonRepeaters");
            }

            if (maxRepetitions < 1)
            {
                throw new ArgumentException("maxRepetitions should be greater than 0", "maxRepetitions");
            }

            Version = version;
            Privacy = privacy;
            Levels recordToSecurityLevel = PrivacyProviderExtension.ToSecurityLevel(privacy);
            recordToSecurityLevel |= Levels.Reportable;
            byte b = (byte)recordToSecurityLevel;
            
            // TODO: define more constants.
            Header = new Header(new Integer32(messageId), new Integer32(maxMessageSize), new OctetString(new[] { b }), new Integer32(3));
            var parameters = report.Parameters;
            var authenticationProvider = Privacy.AuthenticationProvider;
            Parameters = new SecurityParameters(
                parameters.EngineId,
                parameters.EngineBoots,
                parameters.EngineTime,
                userName,
                authenticationProvider.CleanDigest,
                Privacy.Salt);
            GetBulkRequestPdu pdu = new GetBulkRequestPdu(
                requestId,
                nonRepeaters,
                maxRepetitions,
                variables);
            var scope = report.Scope;
            Scope = new Scope(scope.ContextEngineId, scope.ContextName, pdu);

            Parameters.AuthenticationParameters = authenticationProvider.ComputeHash(Version, Header, Parameters, Scope, Privacy);
            _bytes = SnmpMessageExtension.PackMessage(Version, Header, Parameters, Scope, Privacy).ToBytes();
        }

        internal GetBulkRequestMessage(VersionCode version, Header header, SecurityParameters parameters, Scope scope, IPrivacyProvider privacy)
        {
            if (scope == null)
            {
                throw new ArgumentNullException("scope");
            }
            
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }
            
            if (header == null)
            {
                throw new ArgumentNullException("header");
            }
            
            if (privacy == null)
            {
                throw new ArgumentNullException("privacy");
            }

            Version = version;
            Header = header;
            Parameters = parameters;
            Scope = scope;
            Privacy = privacy;

            _bytes = SnmpMessageExtension.PackMessage(Version, Header, Parameters, Scope, Privacy).ToBytes();
        }

        /// <summary>
        /// Gets the header.
        /// </summary>
        public Header Header { get; private set; }
        
        /// <summary>
        /// Gets the privacy provider.
        /// </summary>
        /// <value>The privacy provider.</value>
        public IPrivacyProvider Privacy { get; private set; }

        /// <summary>
        /// Variables.
        /// </summary>
        public IList<Variable> Variables
        {
            get { return Scope.Pdu.Variables; }
        }

        /// <summary>
        /// Request ID.
        /// </summary>
        public int RequestId
        {
            get { return Scope.Pdu.RequestId.ToInt32(); }
        }
        
        /// <summary>
        /// Gets the message ID.
        /// </summary>
        /// <value>The message ID.</value>
        /// <remarks>For v3, message ID is different from request ID. For v1 and v2c, they are the same.</remarks>
        public int MessageId
        {
            get
            {
                return Header == Header.Empty ? RequestId : Header.MessageId;
            }
        }

        /// <summary>
        /// Converts to byte format.
        /// </summary>
        /// <returns></returns>
        public byte[] ToBytes()
        {
            return _bytes;
        }

        /// <summary>
        /// PDU.
        /// </summary>
        public ISnmpPdu Pdu
        {
            get { return Scope.Pdu; }
        }

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        /// <value>The parameters.</value>
        public SecurityParameters Parameters { get; private set; }

        /// <summary>
        /// Gets the scope.
        /// </summary>
        /// <value>The scope.</value>
        public Scope Scope { get; private set; }

        /// <summary>
        /// Gets the version.
        /// </summary>
        /// <value>The version.</value>
        public VersionCode Version { get; private set; }

        /// <summary>
        /// Community name.
        /// </summary>
        public OctetString Community
        {
            get { return Parameters.UserName; }
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents this <see cref="GetBulkRequestMessage"/>.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "GET BULK request message: version: {0}; {1}; {2}", Version, Parameters.UserName, Scope.Pdu);
        }

        /// <summary>
        /// Sends this <see cref="GetNextRequestMessage"/> and handles the response from agent.
        /// </summary>
        /// <param name="timeout">The time-out value, in milliseconds. The default value is 0, which indicates an infinite time-out period. Specifying -1 also indicates an infinite time-out period.</param>
        /// <param name="receiver">Port number.</param>
        /// <returns></returns>
        public ISnmpMessage GetResponse(int timeout, IPEndPoint receiver)
        {
            if (receiver == null)
            {
                throw new ArgumentNullException("receiver");
            }
            
            using (Socket socket = SnmpMessageExtension.GetSocket(receiver))
            {
                return GetResponse(timeout, receiver, socket);
            }
        }

        /// <summary>
        /// Sends this <see cref="GetNextRequestMessage"/> and handles the response from agent.
        /// </summary>
        /// <param name="timeout">The time-out value, in milliseconds. The default value is 0, which indicates an infinite time-out period. Specifying -1 also indicates an infinite time-out period.</param>
        /// <param name="receiver">Agent.</param>
        /// <param name="udpSocket">The UDP <see cref="Socket"/> to use to send/receive.</param>
        /// <returns></returns>
        public ISnmpMessage GetResponse(int timeout, IPEndPoint receiver, Socket udpSocket)
        {
            if (udpSocket == null)
            {
                throw new ArgumentNullException("udpSocket");
            }
            
            if (receiver == null)
            {
                throw new ArgumentNullException("receiver");
            }

            UserRegistry registry = new UserRegistry();
            if (Version == VersionCode.V3)
            {
                registry.Add(Parameters.UserName, Privacy);
            }

            return MessageFactory.GetResponse(receiver, ToBytes(), MessageId, timeout, registry, udpSocket);
        }
    }
}
