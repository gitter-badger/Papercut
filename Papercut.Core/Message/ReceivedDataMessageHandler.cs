﻿// Papercut
// 
// Copyright © 2008 - 2012 Ken Robertson
// Copyright © 2013 - 2015 Jaben Cargman
//  
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//  
// http://www.apache.org/licenses/LICENSE-2.0
//  
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License. 

namespace Papercut.Core.Message
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Papercut.Core.Annotations;
    using Papercut.Core.Events;
    using Papercut.Core.Helper;
    using Papercut.Core.Network;

    using Serilog;

    public class ReceivedDataMessageHandler : IReceivedDataHandler
    {
        readonly ILogger _logger;

        readonly MessageRepository _messageRepository;

        readonly IPublishEvent _publishEvent;

        public ReceivedDataMessageHandler(MessageRepository messageRepository,
            IPublishEvent publishEvent,
            ILogger logger)
        {
            _messageRepository = messageRepository;
            _publishEvent = publishEvent;
            _logger = logger;
        }

        public void HandleReceived([CanBeNull] IEnumerable<string> data)
        {
            var file = _messageRepository.SaveMessage(data.IfNullEmpty().ToList());
            try
            {
                if (!string.IsNullOrWhiteSpace(file))
                    _publishEvent.Publish(new NewMessageEvent(new MessageEntry(file)));
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex, "Unable to publish new message event for message file: {MessageFile}", file);
            }
        }
    }
}