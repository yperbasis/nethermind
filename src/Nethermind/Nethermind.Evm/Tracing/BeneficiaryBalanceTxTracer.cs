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
using System.Collections.Generic;
using Nethermind.Core;
using Nethermind.Core.Crypto;
using Nethermind.Evm;
using Nethermind.Evm.Tracing;
using Nethermind.Int256;

namespace Nethermind.MevSearcher
{
    public class BeneficiaryBalanceTxTracer : ITxTracer
    {
        public BeneficiaryBalanceTxTracer(Address beneficiary)
        {
            _beneficiary = beneficiary;
        }

        public bool IsTracingState => true;
        public void ReportBalanceChange(Address address, UInt256? before, UInt256? after)
        {
            if (address == _beneficiary)
            {
                BeneficiaryBalanceBefore ??= before;
                BeneficiaryBalanceAfter = after;
            }
        }

        public UInt256? BeneficiaryBalanceAfter { get; set; }

        public UInt256? BeneficiaryBalanceBefore { get; set; }

        public void ReportCodeChange(Address address, byte[]? before, byte[]? after)
        {
            throw new NotImplementedException();
        }

        public void ReportNonceChange(Address address, UInt256? before, UInt256? after)
        {
            throw new NotImplementedException();
        }

        public void ReportAccountRead(Address address)
        {
            throw new NotImplementedException();
        }

        public bool IsTracingStorage => false;
        public void ReportStorageChange(StorageCell storageCell, byte[] before, byte[] after)
        {
            throw new NotImplementedException();
        }

        public void ReportStorageRead(StorageCell storageCell)
        {
            throw new NotImplementedException();
        }

        public bool IsTracingReceipt => false;
        public bool IsTracingActions => false;
        public bool IsTracingOpLevelStorage => false;
        public bool IsTracingMemory => false;
        public bool IsTracingInstructions => false;
        public bool IsTracingRefunds => false;
        public bool IsTracingCode => false;
        public bool IsTracingStack => false;
        public bool IsTracingBlockHash => false;
        public bool IsTracingAccess => false;
        public void MarkAsSuccess(Address recipient, long gasSpent, byte[] output, LogEntry[] logs, Keccak? stateRoot = null)
        {
            GasSpent = gasSpent;
            Success = true;
        }

        public bool Success { get; set; }

        public long GasSpent { get; set; }
        
        public Address _beneficiary { get; set; }
        

        public void MarkAsFailed(Address recipient, long gasSpent, byte[] output, string error, Keccak? stateRoot = null)
        {
            GasSpent = gasSpent;
            Success = false;
        }
        
        public void StartOperation(int depth, long gas, Instruction opcode, int pc)
        {
            throw new NotImplementedException();
        }

        public void ReportOperationError(EvmExceptionType error)
        {
            throw new NotImplementedException();
        }

        public void ReportOperationRemainingGas(long gas)
        {
            throw new NotImplementedException();
        }

        public void SetOperationStack(List<string> stackTrace)
        {
            throw new NotImplementedException();
        }

        public void ReportStackPush(in ReadOnlySpan<byte> stackItem)
        {
            throw new NotImplementedException();
        }

        public void SetOperationMemory(List<string> memoryTrace)
        {
            throw new NotImplementedException();
        }

        public void SetOperationMemorySize(ulong newSize)
        {
            throw new NotImplementedException();
        }

        public void ReportMemoryChange(long offset, in ReadOnlySpan<byte> data)
        {
            throw new NotImplementedException();
        }

        public void ReportStorageChange(in ReadOnlySpan<byte> key, in ReadOnlySpan<byte> value)
        {
            throw new NotImplementedException();
        }

        public void SetOperationStorage(Address address, UInt256 storageIndex, ReadOnlySpan<byte> newValue, ReadOnlySpan<byte> currentValue)
        {
            throw new NotImplementedException();
        }

        public void ReportSelfDestruct(Address address, UInt256 balance, Address refundAddress)
        {
            throw new NotImplementedException();
        }

        public void ReportAction(long gas, UInt256 value, Address @from, Address to, ReadOnlyMemory<byte> input, ExecutionType callType,
            bool isPrecompileCall = false)
        {
            throw new NotImplementedException();
        }

        public void ReportActionEnd(long gas, ReadOnlyMemory<byte> output)
        {
            throw new NotImplementedException();
        }

        public void ReportActionError(EvmExceptionType evmExceptionType)
        {
            throw new NotImplementedException();
        }

        public void ReportActionEnd(long gas, Address deploymentAddress, ReadOnlyMemory<byte> deployedCode)
        {
            throw new NotImplementedException();
        }

        public void ReportBlockHash(Keccak blockHash)
        {
            throw new NotImplementedException();
        }

        public void ReportByteCode(byte[] byteCode)
        {
            throw new NotImplementedException();
        }

        public void ReportGasUpdateForVmTrace(long refund, long gasAvailable)
        {
            throw new NotImplementedException();
        }

        public void ReportRefund(long refund)
        {
            throw new NotImplementedException();
        }

        public void ReportExtraGasPressure(long extraGasPressure)
        {
            throw new NotImplementedException();
        }

        public void ReportAccess(IReadOnlySet<Address> accessedAddresses, IReadOnlySet<StorageCell> accessedStorageCells)
        {
            throw new NotImplementedException();
        }
    }
}
