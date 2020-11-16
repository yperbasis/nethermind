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

using System.Threading;
using Nethermind.Core;
using Nethermind.Core.Extensions;
using Nethermind.Core.Specs;
using Nethermind.Evm.Tracing;

namespace Nethermind.Evm.Precompiles.Bls.Shamatar
{
    /// <summary>
    /// Just for diagnostic
    /// </summary>
    public class FatPrecompile : IPrecompile
    {
        public static IPrecompile Instance = new FatPrecompile();

        private FatPrecompile() { }

        public Address Address { get; } = Address.FromNumber(19);

        public long BaseGasCost(IReleaseSpec releaseSpec)
        {
            return 100000;
        }

        public long DataGasCost(byte[] inputData, IReleaseSpec releaseSpec)
        {
            return 0L;
        }

        public (byte[], bool) Run(byte[] inputData, ITxTracer tracer = null)
        {
            for (int i = 0; i < 100000; i++)
            {
                Thread.Sleep(10);
                tracer?.StartOperation(1, 100000 - i, Instruction.EQ, 0);
                tracer?.ReportOperationRemainingGas(100000 - i - 1);
            }

            return (Bytes.Empty, true);
        }
    }
}