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

using System.Collections.Generic;
using Nethermind.Core;
using Nethermind.Core.Extensions;
using Nethermind.JsonRpc;
using NUnit.Framework;

namespace Nethermind.Evm.Test
{
    public class TransactionSubstateTests
    {
        [Test]
        public void ShouldSetRevertErrorProperly()
        {
            byte[] bytes = new byte[100];
            bytes[0] = 8;
            bytes[1] = 195;
            bytes[2] = 121;
            bytes[3] = 160;
            bytes[35] = 32;
            bytes[67] = 32;
            bytes[98] = 210;
            bytes[99] = 251;
            TransactionSubstate txSubstate = new TransactionSubstate(bytes, 0, new Address[0], new List<LogEntry>(), true, true);
            Assert.AreEqual("",txSubstate.Error);
        }
    }
}
