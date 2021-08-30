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
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Nethermind.Serialization.Json;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;



namespace Nethermind.Pipeline.Publishers
{
    public class DiscordPublisher : IPublisher
    {
        private readonly IJsonSerializer _serializer;
        private readonly string _webhookUrl;
        private readonly HttpClient _httpClient;
        private bool _isEnabled;

        public DiscordPublisher(IJsonSerializer serializer, string webhookUrl)
        {
            _serializer = serializer;
            _webhookUrl = $"{webhookUrl}";
            _httpClient = new HttpClient();

            Start();
        }

        public void SubscribeToData<T>(T data)
        {
            if (!_isEnabled) return;
            var task = Task.Run(() => SendMessageAsync(data));
            task.Wait();
        }

        public void Stop()
        {
            _isEnabled = false;
        }

        public void Start()
        {
            _isEnabled = true;
        }

        private async Task SendMessageAsync(object data)
        {
            var uri = new Uri(_webhookUrl);

            var values = new Dictionary<string, string>
            {
                { "content", _serializer.Serialize(data.ToString().Replace("\n", string.Empty)) }
            };

            var content = new FormUrlEncodedContent(values);

            await _httpClient.PostAsync(uri, content);
        }
    }
}
