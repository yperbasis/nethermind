//  Copyright (c) 2018 Demerzel Solutions Limited
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

namespace Nethermind.BeaconNode.Eth1Bridge.Bridge
{
    public struct Block
    {
        public static string Latest = "latest";
        public static string ToHexString(ulong blockNumber) => blockNumber.ToString("X");

        public ulong Number { get; set; }
        public string Hash { get; set; }
        public string ParentHash { get; set; }
        public ulong Timestamp { get; set; }
    }
}
