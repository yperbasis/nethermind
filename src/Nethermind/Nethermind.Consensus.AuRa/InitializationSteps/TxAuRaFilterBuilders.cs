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

using Nethermind.Consensus.AuRa.Config;
using Nethermind.Consensus.AuRa.Contracts;
using Nethermind.Consensus.AuRa.Contracts.DataStore;
using Nethermind.Consensus.AuRa.Transactions;
using Nethermind.Consensus.Transactions;
using Nethermind.Core;
using Nethermind.Core.Specs;
using Nethermind.Evm.TransactionProcessing;
using Nethermind.Init.Steps;

namespace Nethermind.Consensus.AuRa.InitializationSteps
{
    public static class TxAuRaFilterBuilders
    {
        /// <summary>
        /// Filter decorator.
        /// <remarks>
        /// Allow to create new filter based on original filter and a potential fallbackFilter if original filter was not used.
        /// </remarks>
        /// </summary>
        public delegate ITxFilter FilterDecorator(ITxFilter originalFilter, ITxFilter? fallbackFilter = null);

        /// <summary>
        /// Delegate factory method to create final filter for AuRa.
        /// </summary>
        /// <remarks>
        /// This is used to decorate original filter with <see cref="AuRaMergeTxFilter"/> in order to disable it post-merge.
        /// </remarks>
        public static FilterDecorator CreateFilter { get; set; } = (x, _) => x;

        private static ITxFilter CreateBaseAuRaTxFilter(
            IMiningConfig miningConfig,
            AuRaNethermindApi api,
            IReadOnlyTxProcessorSource readOnlyTxProcessorSource,
            IDictionaryContractDataStore<TxPriorityContract.Destination>? minGasPricesContractDataStore,
            ISpecProvider specProvider)
        {
            IMinGasPriceTxFilter minGasPriceTxFilter = TxFilterBuilders.CreateStandardMinGasPriceTxFilter(miningConfig, specProvider);
            ITxFilter gasPriceTxFilter = minGasPriceTxFilter;
            if (minGasPricesContractDataStore != null)
            {
                gasPriceTxFilter = CreateFilter(new MinGasPriceContractTxFilter(minGasPriceTxFilter, minGasPricesContractDataStore), minGasPriceTxFilter);
            }

            Address? registrar = api.ChainSpec?.Parameters.Registrar;
            if (registrar != null)
            {
                RegisterContract registerContract = new(api.AbiEncoder, registrar, readOnlyTxProcessorSource);
                CertifierContract certifierContract = new(api.AbiEncoder, registerContract, readOnlyTxProcessorSource);
                return CreateFilter(new TxCertifierFilter(certifierContract, gasPriceTxFilter, specProvider, api.LogManager), gasPriceTxFilter);
            }

            return gasPriceTxFilter;
        }

        private static ITxFilter CreateBaseAuRaTxFilter(
            AuRaNethermindApi api,
            IReadOnlyTxProcessorSource readOnlyTxProcessorSource,
            ISpecProvider specProvider,
            ITxFilter baseTxFilter)
        {
            Address? registrar = api.ChainSpec?.Parameters.Registrar;
            if (registrar != null)
            {
                RegisterContract registerContract = new(api.AbiEncoder, registrar, readOnlyTxProcessorSource);
                CertifierContract certifierContract = new(api.AbiEncoder, registerContract, readOnlyTxProcessorSource);
                return CreateFilter(new TxCertifierFilter(certifierContract, baseTxFilter, specProvider, api.LogManager));
            }

            return baseTxFilter;
        }


        public static ITxFilter? CreateTxPermissionFilter(AuRaNethermindApi api, IReadOnlyTxProcessorSource readOnlyTxProcessorSource)
        {
            if (api.ChainSpec == null) throw new StepDependencyException(nameof(api.ChainSpec));
            if (api.SpecProvider == null) throw new StepDependencyException(nameof(api.SpecProvider));

            if (api.ChainSpec.Parameters.TransactionPermissionContract != null)
            {
                api.TxFilterCache ??= new PermissionBasedTxFilter.Cache();

                var txPermissionFilter = CreateFilter(new PermissionBasedTxFilter(
                    new VersionedTransactionPermissionContract(api.AbiEncoder,
                        api.ChainSpec.Parameters.TransactionPermissionContract,
                        api.ChainSpec.Parameters.TransactionPermissionContractTransition ?? 0,
                        readOnlyTxProcessorSource,
                        api.TransactionPermissionContractVersions,
                        api.LogManager,
                        api.SpecProvider),
                    api.TxFilterCache,
                    api.LogManager));

                return txPermissionFilter;
            }

            return null;
        }

        public static ITxFilter CreateAuRaTxFilterForProducer(
            IMiningConfig miningConfig,
            AuRaNethermindApi api,
            IReadOnlyTxProcessorSource readOnlyTxProcessorSource,
            IDictionaryContractDataStore<TxPriorityContract.Destination>? minGasPricesContractDataStore,
            ISpecProvider specProvider)
        {
            ITxFilter baseAuRaTxFilter = CreateBaseAuRaTxFilter(miningConfig, api, readOnlyTxProcessorSource, minGasPricesContractDataStore, specProvider);
            ITxFilter? txPermissionFilter = CreateTxPermissionFilter(api, readOnlyTxProcessorSource);
            return txPermissionFilter != null
                ? new CompositeTxFilter(baseAuRaTxFilter, txPermissionFilter)
                : baseAuRaTxFilter;
        }

        public static ITxFilter CreateAuRaTxFilter(
            AuRaNethermindApi api,
            IReadOnlyTxProcessorSource readOnlyTxProcessorSource,
            ISpecProvider specProvider,
            ITxFilter baseTxFilter)
        {
            ITxFilter baseAuRaTxFilter = CreateBaseAuRaTxFilter(api, readOnlyTxProcessorSource, specProvider, baseTxFilter);
            ITxFilter? txPermissionFilter = CreateTxPermissionFilter(api, readOnlyTxProcessorSource);
            return txPermissionFilter != null
                ? new CompositeTxFilter(baseAuRaTxFilter, txPermissionFilter)
                : baseAuRaTxFilter;
        }

        public static (TxPriorityContract? Contract, TxPriorityContract.LocalDataSource? DataSource) CreateTxPrioritySources(
            IAuraConfig config,
            AuRaNethermindApi api,
            IReadOnlyTxProcessorSource readOnlyTxProcessorSource)
        {
            Address.TryParse(config.TxPriorityContractAddress, out Address? txPriorityContractAddress);
            bool usesTxPriorityContract = txPriorityContractAddress != null;

            TxPriorityContract? txPriorityContract = null;
            if (usesTxPriorityContract)
            {
                txPriorityContract = new TxPriorityContract(api.AbiEncoder, txPriorityContractAddress, readOnlyTxProcessorSource);
            }

            string? auraConfigTxPriorityConfigFilePath = config.TxPriorityConfigFilePath;
            bool usesTxPriorityLocalData = auraConfigTxPriorityConfigFilePath != null;
            if (usesTxPriorityLocalData)
            {
                api.TxPriorityContractLocalDataSource ??= new TxPriorityContract.LocalDataSource(auraConfigTxPriorityConfigFilePath, api.EthereumJsonSerializer, api.FileSystem, api.LogManager);
            }

            return (txPriorityContract, api.TxPriorityContractLocalDataSource);
        }

        public static DictionaryContractDataStore<TxPriorityContract.Destination>? CreateMinGasPricesDataStore(
            AuRaNethermindApi api,
            TxPriorityContract? txPriorityContract,
            TxPriorityContract.LocalDataSource? localDataSource)
        {
            return txPriorityContract != null || localDataSource != null
                ? new DictionaryContractDataStore<TxPriorityContract.Destination>(
                    new TxPriorityContract.DestinationSortedListContractDataStoreCollection(),
                    txPriorityContract?.MinGasPrices,
                    api.BlockTree,
                    api.ReceiptFinder,
                    api.LogManager,
                    localDataSource?.GetMinGasPricesLocalDataSource())
                : null;
        }
    }
}
