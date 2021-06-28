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
using Nethermind.State;

namespace Nethermind.Mev.Execution
{
    public class BeneficiaryBalanceBlockTracer : IBlockTracer, IBeneficiaryBalanceSource
    {
        private readonly IStateProvider _stateProvider;
        private Block? _block;

        public BeneficiaryBalanceBlockTracer(IStateProvider stateProvider)
        {
            _stateProvider = stateProvider;
        }

        public bool IsTracingRewards { get; } = true;
        
        public UInt256 Reward { get; private set; }

        public void ReportReward(Address author, string rewardType, UInt256 rewardValue)
        {
            if (author == _block!.Header.GasBeneficiary)
            {
                Reward = rewardValue;
            }
        }

        public void StartNewBlockTrace(Block block)
        {
            _block = block;
        }

        public ITxTracer StartNewTxTrace(Transaction? tx)
        {
            throw new NotImplementedException();
        }

        public void EndTxTrace()
        {
            throw new NotImplementedException();
        }

        public void EndBlockTrace()
        {
            BeneficiaryBalance = _stateProvider.GetBalance(_block!.Header.GasBeneficiary!);
        }

        public UInt256 BeneficiaryBalance { get; private set; }
    }
    
    public class BeneficiaryBalanceTxTracer : ITxTracer
        {
            public Transaction? Transaction { get; }
            public int Index { get; }
            private readonly Address _beneficiary;

            public BeneficiaryBalanceTxTracer(Address beneficiary, Transaction? transaction, int index)
            {
                Transaction = transaction;
                Index = index;
                _beneficiary = beneficiary;
            }

            public bool IsTracingReceipt => true;
            public bool IsTracingActions => false;
            public bool IsTracingOpLevelStorage => false;
            public bool IsTracingMemory => false;
            public bool IsTracingInstructions => false;
            public bool IsTracingRefunds => false;
            public bool IsTracingCode => false;
            public bool IsTracingStack => false;
            public bool IsTracingState => true;
            public bool IsTracingStorage => false;
            public bool IsTracingBlockHash => false;
            public bool IsTracingAccess => false;
            public long GasSpent { get; set; }
            public UInt256? BeneficiaryBalanceBefore { get; private set; }
            public UInt256? BeneficiaryBalanceAfter { get; private set; }
            
            public bool Success { get; private set; }
            
            public void MarkAsSuccess(Address recipient, long gasSpent, byte[] output, LogEntry[] logs, Keccak? stateRoot = null)
            {
                GasSpent = gasSpent;
                Success = true;
            }

            public void MarkAsFailed(Address recipient, long gasSpent, byte[] output, string error, Keccak? stateRoot = null)
            {
                GasSpent = gasSpent;
                Success = false;
            }

            public void StartOperation(int depth, long gas, Instruction opcode, int pc)
            {
                throw new NotSupportedException();
            }

            public void ReportOperationError(EvmExceptionType error)
            {
                throw new NotSupportedException();
            }

            public void ReportOperationRemainingGas(long gas)
            {
                throw new NotSupportedException();
            }

            public void SetOperationStack(List<string> stackTrace)
            {
                throw new NotSupportedException();
            }

            public void ReportStackPush(in ReadOnlySpan<byte> stackItem)
            {
                throw new NotSupportedException();
            }

            public void SetOperationMemory(List<string> memoryTrace)
            {
                throw new NotSupportedException();
            }

            public void SetOperationMemorySize(ulong newSize)
            {
                throw new NotSupportedException();
            }

            public void ReportMemoryChange(long offset, in ReadOnlySpan<byte> data)
            {
                throw new NotSupportedException();
            }

            public void ReportStorageChange(in ReadOnlySpan<byte> key, in ReadOnlySpan<byte> value)
            {
                throw new NotSupportedException();
            }

            public void SetOperationStorage(Address address, UInt256 storageIndex, ReadOnlySpan<byte> newValue, ReadOnlySpan<byte> currentValue)
            {
                throw new NotSupportedException();
            }

            public void ReportSelfDestruct(Address address, UInt256 balance, Address refundAddress)
            {
                throw new NotSupportedException();
            }

            public void ReportBalanceChange(Address address, UInt256? before, UInt256? after)
            {
                if (address == _beneficiary)
                {
                    BeneficiaryBalanceBefore ??= before;
                    BeneficiaryBalanceAfter = after;
                }
            }

            public void ReportCodeChange(Address address, byte[]? before, byte[]? after)
            {
                throw new NotSupportedException();
            }

            public void ReportNonceChange(Address address, UInt256? before, UInt256? after)
            {
            }

            public void ReportAccountRead(Address address)
            {
            }

            public void ReportStorageChange(StorageCell storageCell, byte[] before, byte[] after)
            {
                throw new NotSupportedException();
            }

            public void ReportStorageRead(StorageCell storageCell)
            {
                throw new NotImplementedException();
            }

            public void ReportAction(long gas, UInt256 value, Address from, Address to, ReadOnlyMemory<byte> input, ExecutionType callType, bool isPrecompileCall = false)
            {
                throw new NotSupportedException();
            }

            public void ReportActionEnd(long gas, ReadOnlyMemory<byte> output)
            {
                throw new NotSupportedException();
            }

            public void ReportActionError(EvmExceptionType exceptionType)
            {
                throw new NotSupportedException();
            }

            public void ReportActionEnd(long gas, Address deploymentAddress, ReadOnlyMemory<byte> deployedCode)
            {
                throw new NotSupportedException();
            }

            public void ReportBlockHash(Keccak blockHash)
            {
                throw new NotSupportedException();
            }

            public void ReportByteCode(byte[] byteCode)
            {
                throw new NotSupportedException();
            }

            public void ReportGasUpdateForVmTrace(long refund, long gasAvailable)
            {
                throw new NotSupportedException();
            }

            public void ReportRefund(long refund)
            {
                throw new NotSupportedException();
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
