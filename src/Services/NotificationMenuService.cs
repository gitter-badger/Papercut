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

namespace Papercut.Services
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Windows.Threading;

    using Papercut.Core.Events;
    using Papercut.Events;

    public class NotificationMenuService : IDisposable,
        IHandleEvent<PapercutClientReadyEvent>,
        IHandleEvent<ShowBallonTip>
    {
        readonly IPublishEvent _publishEvent;

        readonly AppResourceLocator _resourceLocator;

        NotifyIcon _notification;

        public NotificationMenuService(
            AppResourceLocator resourceLocator,
            IPublishEvent publishEvent)
        {
            _resourceLocator = resourceLocator;
            _publishEvent = publishEvent;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_notification != null)
                {
                    _notification.Dispose();
                    _notification = null;
                }
            }
        }

        public void Handle(PapercutClientReadyEvent message)
        {
            if (_notification != null) return;

            // Set up the notification icon
            _notification = new NotifyIcon
            {
                Icon = new Icon(_resourceLocator.GetResource("App.ico").Stream),
                Text = "Papercut",
                Visible = true
            };

            _notification.Click +=
                (sender, args) => _publishEvent.Publish(new ShowMainWindowEvent());

            _notification.BalloonTipClicked +=
                (sender, args) =>
                _publishEvent.Publish(new ShowMainWindowEvent { SelectMostRecentMessage = true });

            var options = new MenuItem(
                "Options",
                (sender, args) => _publishEvent.Publish(new ShowOptionWindowEvent()))
            {
                DefaultItem = false,
            };

            var menuItems = new[]
            {
                new MenuItem(
                    "Show",
                    (sender, args) => _publishEvent.Publish(new ShowMainWindowEvent()))
                {
                    DefaultItem = true
                },
                options,
                new MenuItem(
                    "Exit",
                    (sender, args) => _publishEvent.Publish(new AppForceShutdownEvent()))
            };

            _notification.ContextMenu = new ContextMenu(menuItems);
        }

        public void Handle(ShowBallonTip @event)
        {
            Dispatcher.CurrentDispatcher.BeginInvoke(
                new Action(
                    () =>
                    _notification.ShowBalloonTip(
                        @event.Timeout,
                        @event.TipTitle,
                        @event.TipText,
                        @event.ToolTipIcon)));
        }
    }
}