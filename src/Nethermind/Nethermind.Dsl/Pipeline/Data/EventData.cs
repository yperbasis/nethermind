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
using System.Linq;
using Microsoft.Extensions.Logging;
using Nethermind.Core;
using Nethermind.Core.Crypto;
using Nethermind.Core.Extensions;

namespace Nethermind.Dsl.Pipeline.Data
{
    public class EventData : LogEntry
    {
        public EventData(Address address, byte[] data, Keccak[] topics) : base(address, data, topics)
        {
        }

        public static EventData FromLogEntry(LogEntry log)
        {
            return new(log.LoggersAddress, log.Data, log.Topics);
        }
        

        public override string ToString()
        {
            var result = $"Found new event at {LoggersAddress}. \n";

            if (Topics.Any())
            {
                result += "Topics: \n";

                var i = 0;
                foreach (var topic in Topics)
                {
                    result += $"[{i}] {topic}\n";
                    i++;
                }
            }

            if (Data.Length != 0)
            {
                result += $"Data: {Data.ToHexString()} \n";
            }

            return result;
        }
    }
}