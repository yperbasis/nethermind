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

using System;
using System.Diagnostics;
using Nethermind.Core.Crypto;

namespace Nethermind.Synchronization.FastSync
{
    [DebuggerDisplay("{Level} {NodeDataType} {Hash}")]
    public class StateSyncItem
    {
        public StateSyncItem(Keccak hash, byte[]? accountPathNibbles, byte[]? pathNibbles, NodeDataType nodeType, int level = 0, uint rightness = 0)
        {
            Hash = hash;
            AccountPathNibbles = accountPathNibbles ?? Array.Empty<byte>();
            PathNibbles = pathNibbles ?? Array.Empty<byte>();
            NodeDataType = nodeType;
            Level = (byte)level;
            Rightness = rightness;
        }

        public Keccak Hash { get; }

        public byte[] AccountPathNibbles { get; }

        public byte[] PathNibbles { get; }

        public NodeDataType NodeDataType { get; }

        public byte Level { get; }

        public short ParentBranchChildIndex { get; set; } = -1;

        public short BranchChildIndex { get; set; } = -1;

        public uint Rightness { get; }

        public bool IsRoot => Level == 0 && NodeDataType == NodeDataType.State;
    }
}
