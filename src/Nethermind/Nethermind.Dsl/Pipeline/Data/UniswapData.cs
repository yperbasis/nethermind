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

using Nethermind.Core;
using Nethermind.Core.Crypto;

namespace Nethermind.Dsl.Pipeline.Data
{
    public class UniswapData
    {
        public Keccak? Transaction { get; set; }
        public Address? Swapper { get; set; }
        public Address? Pool { get; set; }
        public Address? Token0 { get; set; }
        public Address? Token1 { get; set; }
        public string? Token0V2Price { get; set; }
        public string? Token1V2Price { get; set; }
        public string? Token0V3Price { get; set; }
        public string? Token1V3Price { get; set; }
        public string? Token0In { get; set; }
        public string? Token0Out { get; set; }
        public string? Token1In { get; set; }
        public string? Token1Out { get; set; }

        public override string ToString()
        {
            return $"Found a swap on pool {Pool} \n" +
                   $"Token0 is {Token0} \n" +
                   $"Token1 is {Token1} \n" +
                   $"Token0Out {Token0Out} \n" +
                   $"Token0In {Token0In} \n " +
                   $"Token1Out {Token1Out} \n" +
                   $"Token1In {Token1In} \n" +
                   $"Token0 price on uniswap V2 is {Token0V2Price}$ \n" +
                   $"Token0 price on uniswap V3 is {Token0V3Price}$ \n" +
                   $"Token1 price on uniswap V2 is {Token1V2Price}$ \n" +
                   $"Token1 price on uniswap V3 is {Token1V3Price}$ \n" +
                   $"Swapper address is {Swapper}";
        }
    }
}