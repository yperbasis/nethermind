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

using System.Collections.Generic;
using Nethermind.Consensus;
using Nethermind.Consensus.AuRa;
using Nethermind.Core;
using Nethermind.Core.Test.Builders;
using NSubstitute;
using NUnit.Framework;

namespace Nethermind.AuRa.Test
{
    public class FaultyAuRaStepCalculatorTests
    {
        private IAuRaStepCalculator _auRaStepCalculator;
        private ISigner _signer;
        private Dictionary<Address,long> _faultyBlocksTransition;
        private Dictionary<long,Address> _reportMalicious;
        private FaultyAuRaStepCalculator _calculator;

        [SetUp]
        public void Setup()
        {
            _auRaStepCalculator = Substitute.For<IAuRaStepCalculator>();
            _auRaStepCalculator.GetCurrentStep(Arg.Any<long>()).Returns(c => c.Arg<long>());
            _signer = Substitute.For<ISigner>();
            _signer.Address.Returns(TestItem.AddressA);

            _faultyBlocksTransition = new Dictionary<Address, long>() {{TestItem.AddressB, 5}, {TestItem.AddressA, 10}};
            _reportMalicious = new Dictionary<long, Address>() {{2, TestItem.AddressB}, {3, TestItem.AddressA}};

            _calculator = new FaultyAuRaStepCalculator(_auRaStepCalculator, _signer, _faultyBlocksTransition, _reportMalicious);
        }

        [TestCase(5, ExpectedResult = 5)]
        [TestCase(8, ExpectedResult = 8)]
        [TestCase(10, ExpectedResult = 10-3)]
        [TestCase(15, ExpectedResult = 15-3)]
        public long GetCurrentStep_returns_expected(long block) => _calculator.GetCurrentStep(block);

        [TestCase(1, ExpectedResult = true)]
        [TestCase(2, ExpectedResult = true)]
        [TestCase(3, ExpectedResult = false)]
        [TestCase(4, ExpectedResult = true)]
        [TestCase(5, ExpectedResult = true)]
        [TestCase(6, ExpectedResult = true)]
        [TestCase(10, ExpectedResult = false)]
        [TestCase(20, ExpectedResult = false)]
        public bool ValidateStep_returns_expected(long block) => _calculator.ValidateStep(block);
    }
}
