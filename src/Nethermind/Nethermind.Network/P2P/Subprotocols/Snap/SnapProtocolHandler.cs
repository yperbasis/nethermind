//  Copyright (c) 2021 Demerzel Solutions Limited
//  This file is part of the Nethermind library.
// 
//  The Nethermind library is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  The Nethermind library is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU Lesser General Public License for more details.
// 
//  You should have received a copy of the GNU Lesser General Public License
//  along with the Nethermind. If not, see <http://www.gnu.org/licenses/>.
// 

using System;
using System.Threading;
using Nethermind.Core.Specs;
using Nethermind.Logging;
using Nethermind.Network.P2P.ProtocolHandlers;
using Nethermind.Network.P2P.Subprotocols.Eth.V62;
using Nethermind.Network.P2P.Subprotocols.Eth.V65;
using Nethermind.Network.P2P.Subprotocols.Eth.V66;
using Nethermind.Network.P2P.Subprotocols.Snap.Messages;
using Nethermind.Network.Rlpx;
using Nethermind.Stats;
using Nethermind.Stats.Model;
using Nethermind.Synchronization;
using Nethermind.TxPool;

namespace Nethermind.Network.P2P.Subprotocols.Snap
{
    public class SnapProtocolHandler : ZeroProtocolHandlerBase
    {
        public override string Name => "snap1";
        protected override TimeSpan InitTimeout => Timeouts.Eth;

        public override byte ProtocolVersion => 1;
        public override string ProtocolCode => Protocol.Snap;
        public override int MessageIdSpaceSize => 8;

        /// <summary>
        /// Currently we use ETH Status msg but it's probable that SNAP will get own Status msg in the future
        /// </summary>
        private bool _ethStatusReceived;
        
        public SnapProtocolHandler(ISession session, 
            INodeStatsManager nodeStats, 
            IMessageSerializationService serializer, 
            ILogManager logManager) 
            : base(session, nodeStats, serializer, logManager)
        {
        }

        public override event EventHandler<ProtocolInitializedEventArgs> ProtocolInitialized;
        public override event EventHandler<ProtocolEventArgs>? SubprotocolRequested
        {
            add { }
            remove { }
        }

        public override void Init()
        {
            ProtocolInitialized?.Invoke(this, new ProtocolInitializedEventArgs(this));
        }

        public override void Dispose()
        {
        }
        
        public override void HandleMessage(ZeroPacket message)
        {
            //TODO: add unit test
            if (!CheckIfEthStatusMsgReceived())
            {
                throw new SubprotocolException(
                    $"No {nameof(StatusMessage)} received prior to communication with {Session?.Node:c}.");
            }
            
            int packetType = message.PacketType;
            if (Logger.IsTrace)
                Logger.Trace(
                    $"{Counter:D5} {Eth62MessageCode.GetDescription(packetType)} from {Session?.Node:c}");

            switch (packetType)
            {
                case SnapMessageCode.GetAccountRange:
                    GetAccountRangeMessage getAccRangeMsg = Deserialize<GetAccountRangeMessage>(message.Content);
                    ReportIn(getAccRangeMsg);
                    Handle(getAccRangeMsg);
                    break;
                case SnapMessageCode.AccountRange:
                    AccountRangeMessage accRangeMsg = Deserialize<AccountRangeMessage>(message.Content);
                    ReportIn(accRangeMsg);
                    Handle(accRangeMsg);
                    break;
                case SnapMessageCode.GetStorageRanges:
                    GetStorageRangesMessage getStorageRangesMsg = Deserialize<GetStorageRangesMessage>(message.Content);
                    ReportIn(getStorageRangesMsg);
                    Handle(getStorageRangesMsg);
                    break;
                case SnapMessageCode.StorageRanges:
                    StorageRangesMessage storageRangesMsg = Deserialize<StorageRangesMessage>(message.Content);
                    ReportIn(storageRangesMsg);
                    Handle(storageRangesMsg);
                    break;
                case SnapMessageCode.GetByteCodes:
                    GetByteCodesMessage getByteCodesMsg = Deserialize<GetByteCodesMessage>(message.Content);
                    ReportIn(getByteCodesMsg);
                    Handle(getByteCodesMsg);
                    break;
                case SnapMessageCode.ByteCodes:
                    ByteCodesMessage byteCodesMsg = Deserialize<ByteCodesMessage>(message.Content);
                    ReportIn(byteCodesMsg);
                    Handle(byteCodesMsg);
                    break;
                case SnapMessageCode.GetTrieNodes:
                    GetTrieNodesMessage getTrieNodesMsg = Deserialize<GetTrieNodesMessage>(message.Content);
                    ReportIn(getTrieNodesMsg);
                    Handle(getTrieNodesMsg);
                    break;
                case SnapMessageCode.TrieNodes:
                    TrieNodesMessage trieNodesMsg = Deserialize<TrieNodesMessage>(message.Content);
                    ReportIn(trieNodesMsg);
                    Handle(trieNodesMsg);
                    break;
            }
        }

        private void Handle(GetAccountRangeMessage msg)
        {
            throw new NotImplementedException();
        }
        private void Handle(AccountRangeMessage msg)
        {
            throw new NotImplementedException();
        }
        private void Handle(GetStorageRangesMessage msg)
        {
            throw new NotImplementedException();
        }private void Handle(StorageRangesMessage msg)
        {
            throw new NotImplementedException();
        }private void Handle(GetByteCodesMessage msg)
        {
            throw new NotImplementedException();
        }private void Handle(ByteCodesMessage msg)
        {
            throw new NotImplementedException();
        }private void Handle(GetTrieNodesMessage msg)
        {
            throw new NotImplementedException();
        }private void Handle(TrieNodesMessage msg)
        {
            throw new NotImplementedException();
        }
        
        private bool CheckIfEthStatusMsgReceived()
        {
            if (_ethStatusReceived)
            {
                return true;
            }
            
            if (Session.TryGetProtocolHandler(Protocol.Eth, out IProtocolHandler handler))
            {
                if (handler is Eth62ProtocolHandler { StatusReceived: true })
                {
                    _ethStatusReceived = true;
                    return true;
                }
            }

            return false;
        }

        public override void DisconnectProtocol(DisconnectReason disconnectReason, string details)
        {
            Dispose();
        }
    }
}
