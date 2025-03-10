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

using System.Net;
using Nethermind.Core.Crypto;
using Nethermind.Core.Extensions;

namespace Nethermind.Network.Discovery.Messages;

public class PongMsg : DiscoveryMsg
{
    public byte[] PingMdc { get; init; }

    public PongMsg(IPEndPoint farAddress, long expirationTime, byte[] pingMdc) : base(farAddress, expirationTime)
    {
        PingMdc = pingMdc ?? throw new ArgumentNullException(nameof(pingMdc));
    }

    public PongMsg(PublicKey farPublicKey, long expirationTime, byte[] pingMdc) : base(farPublicKey, expirationTime)
    {
        PingMdc = pingMdc ?? throw new ArgumentNullException(nameof(pingMdc));
    }

    public override string ToString()
    {
        return base.ToString() + $", PingMdc: {PingMdc?.ToHexString() ?? "empty"}";
    }

    public override MsgType MsgType => MsgType.Pong;
}
