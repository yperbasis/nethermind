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

using Nethermind.Core2.Containers;

namespace Nethermind.BeaconNode.Eth1Bridge.Bridge
{
    public class DepositLog
    {
        public DepositData DepositData { get; }
        public ulong BlockNumber { get; }
        public ulong Index { get; }
        // public bool SignatureValid { get; } // Is it needed?

        public DepositLog(DepositData depositData, ulong blockNumber, ulong index, bool signatureValid = true)
        {
            DepositData = depositData;
            BlockNumber = blockNumber;
            Index = index;
            // SignatureValid = signatureValid;
        }
    }
}
