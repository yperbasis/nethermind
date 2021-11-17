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

using DotNetty.Buffers;

namespace Nethermind.Network.P2P.Subprotocols.Snap.Messages
{
    public class AccountRangeMessageSerializer : IZeroInnerMessageSerializer<AccountRangeMessage>
    {
        public void Serialize(IByteBuffer byteBuffer, AccountRangeMessage message)
        {
            throw new System.NotImplementedException();
        }

        public AccountRangeMessage Deserialize(IByteBuffer byteBuffer)
        {
            throw new System.NotImplementedException();
        }

        public int GetLength(AccountRangeMessage message, out int contentLength)
        {
            throw new System.NotImplementedException();
        }
    }
}
