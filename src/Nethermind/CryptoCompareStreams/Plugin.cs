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
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using CryptoCompareStreams.Contracts;
using CryptoCompareStreams.Models;
using Nethermind.Abi;
using Nethermind.Api;
using Nethermind.Api.Extensions;
using Nethermind.Blockchain.Processing;
using Nethermind.Core;
using Nethermind.Core.Extensions;
using Nethermind.Int256;
using Nethermind.Logging;
using Nethermind.WebSockets;

namespace CryptoCompareStreams
{
    public class Plugin : INethermindPlugin
    {
        private INethermindApi _api;
        private IWebSocketsModule _webSocketsModule;
        private HashSet<Address> _pairsAddresses = new();
        
        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }

        public string Name { get; } = "CryptoCompare Uniswap watcher";
        public string Description { get; } = "";
        public string Author { get; } = "Nethermind";
        
        public Task Init(INethermindApi nethermindApi)
        {
            _api = nethermindApi;

            return Task.CompletedTask;
        }

        public Task InitNetworkProtocol()
        {
            return Task.CompletedTask;
        }

        public Task InitRpcModules()
        {
            _webSocketsModule = new WebSocketsStreamer();
            _api.WebSocketsManager.AddModule(_webSocketsModule);
            GetPairs();

            _api.MainBlockProcessor.TransactionProcessed += OnTransactionProcessed;
            
            return Task.CompletedTask;
        }

        private void GetPairs()
        {
            var contractAbi = LoadContractABI("UniswapV2Factory");
            var contract = new UniswapV2Factory(new Address("0x5C69bEe701ef814a2B6a3EDD4B1652CB9cc5aA6f"), _api.CreateBlockchainBridge(), _api.BlockTree);

            var uniswapPairsLength = contract.allPairsLength();

            for (UInt256 i = 0; i < uniswapPairsLength; i++)
            {
                _pairsAddresses.Add(contract.allPairs(i));
            }
        }

        private async void OnTransactionProcessed(object? sender, TxProcessedEventArgs args)
        {
            var logs = args.TxReceipt.Logs;

            if (logs == null || logs?.Length == 0) return;

            var signature = new AbiSignature("Swap", AbiType.Address, AbiType.UInt256, AbiType.UInt256, AbiType.UInt256, AbiType.UInt256, AbiType.Address);

            var uniswapLogs = logs.Where(l => l.Topics[0].Equals(signature.Hash));

            foreach (var log in uniswapLogs)   
            {
                if (_pairsAddresses.Contains(log.LoggersAddress))
                {
                    var data = GetUniswapData(log);
                    await _webSocketsModule.SendAsync(new WebSocketsMessage("UniswapData", "", data));
                }
            }
        }

        private UniswapData GetUniswapData(LogEntry log)
        {
            var pairContract = new UniswapPair(log.LoggersAddress, _api.CreateBlockchainBridge(), _api.BlockTree);

            byte[] token0In = new byte[32];
            Array.Copy(log.Data, 0, token0In, 0, 32);
            byte[] token1In = new byte[32];
            Array.Copy(log.Data, 32, token1In, 0, 32);

            byte[] token0Out = new byte[32];
            Array.Copy(log.Data, 64, token0Out, 0, 32);
            byte[] token1Out = new byte[32];
            Array.Copy(log.Data, 96, token1Out, 0, 32);

            return new UniswapData
            {
                Id = new Guid(),
                Pair = pairContract.ContractAddress,
                Token0 = pairContract.token0(),
                Token1 = pairContract.token1(),
                Token0Out = token0Out.ToUInt256(),
                Token0In = token0In.ToUInt256(),
                Token1Out = token1Out.ToUInt256(),
                Token1In = token1In.ToUInt256()
            };
        }

        private string LoadContractABI(string contractName)
        {
            var FileSystem = new FileSystem();

            var dirPath = FileSystem.Path.Combine(PathUtils.ExecutingDirectory, "Contracts");

            if (FileSystem.Directory.Exists(dirPath))
            {
                var files = FileSystem.Directory.GetFiles("Contracts", $"{contractName}.json");

                return files.Any() ? files.First() : null;
            }

            throw new FileLoadException($"Could not find any contract ABI files at Contracts/{contractName}.json");
        }
    }
}