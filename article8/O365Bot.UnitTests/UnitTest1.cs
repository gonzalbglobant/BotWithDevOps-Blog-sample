﻿using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Tests;
using Microsoft.Bot.Connector;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using Autofac;
using O365Bot.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Base;
using System.Threading;
using System.Collections.Generic;
using Microsoft.QualityTools.Testing.Fakes;
using Moq;
using Microsoft.Graph;
using O365Bot.Services;
namespace O365Bot.UnitTests
{
    [TestClass]
    public class SampleDialogTest : DialogTestBase
    {
        [TestMethod]
        public async Task ShouldReturnEvents()
        {
            // Instantiate ShimsContext to use Fakes 
            using (ShimsContext.Create())
            {
                // Return "dummyToken" when calling GetAccessToken method 
                AuthBot.Fakes.ShimContextExtensions.GetAccessTokenIBotContextString =
                    async (a, e) => { return "dummyToken"; };

                var mockEventService = new Mock<IEventService>();
                mockEventService.Setup(x => x.GetEvents()).ReturnsAsync(new List<Event>()
                {
                    new Event
                    {
                        Subject = "dummy event",
                        Start = new DateTimeTimeZone()
                        {
                            DateTime = "2017-05-31 12:00",
                            TimeZone = "Standard Tokyo Time"
                        },
                        End = new DateTimeTimeZone()
                        {
                            DateTime = "2017-05-31 13:00",
                            TimeZone = "Standard Tokyo Time"
                        }
                    }
                });
                var builder = new ContainerBuilder();
                builder.RegisterInstance(mockEventService.Object).As<IEventService>();
                WebApiApplication.Container = builder.Build();

                // Instantiate dialog to test
                IDialog<object> rootDialog = new RootDialog();

                // Create in-memory bot environment
                Func<IDialog<object>> MakeRoot = () => rootDialog;
                using (new FiberTestBase.ResolveMoqAssembly(rootDialog))
                using (var container = Build(Options.MockConnectorFactory | Options.ScopedQueue, rootDialog))
                {
                    // Create a message to send to bot
                    var toBot = DialogTestBase.MakeTestMessage();
                    toBot.From.Id = Guid.NewGuid().ToString();
                    toBot.Text = "get events";

                    // Send message and check the answer.
                    IMessageActivity toUser = await GetResponse(container, MakeRoot, toBot);

                    // Verify the result
                    Assert.IsTrue(toUser.Text.Equals("2017-05-31 12:00-2017-05-31 13:00: dummy event"));
                }
            }
        }

        [TestMethod]
        public async Task ShouldCreateAllDayEvent()
        {
            // Instantiate ShimsContext to use Fakes 
            using (ShimsContext.Create())
            {
                // Return "dummyToken" when calling GetAccessToken method 
                AuthBot.Fakes.ShimContextExtensions.GetAccessTokenIBotContextString =
                    async (a, e) => { return "dummyToken"; };

                // Mock the service and register
                var mockEventService = new Mock<IEventService>();
                mockEventService.Setup(x => x.CreateEvent(It.IsAny<Event>())).Returns(Task.FromResult(true));

                var builder = new ContainerBuilder();
                builder.RegisterInstance(mockEventService.Object).As<IEventService>();
                WebApiApplication.Container = builder.Build();

                // Instantiate dialog to test
                IDialog<object> rootDialog = new RootDialog();

                // Create in-memory bot environment
                Func<IDialog<object>> MakeRoot = () => rootDialog;
                using (new FiberTestBase.ResolveMoqAssembly(rootDialog))
                using (var container = Build(Options.MockConnectorFactory | Options.ScopedQueue, rootDialog))
                {
                    // Create a message to send to bot
                    var toBot = DialogTestBase.MakeTestMessage();
                    // Specify local as US English
                    toBot.Locale = "en-US";
                    toBot.From.Id = Guid.NewGuid().ToString();
                    toBot.Text = "add appointment";

                    // Send message and check the answer.
                    var toUser = await GetResponses(container, MakeRoot, toBot);

                    // Verify the result
                    Assert.IsTrue(toUser[0].Text.Equals("Creating an event."));
                    Assert.IsTrue(toUser[1].Text.Equals("What is the title?"));

                    toBot.Text = "Learn BotFramework";
                    toUser = await GetResponses(container, MakeRoot, toBot);
                    Assert.IsTrue(toUser[0].Text.Equals("What is the detail?"));

                    toBot.Text = "Implement O365Bot";
                    toUser = await GetResponses(container, MakeRoot, toBot);
                    Assert.IsTrue(toUser[0].Text.Equals("When do you start? Use dd/MM/yyyy HH:mm format."));

                    toBot.Text = "01/07/2017 13:00";
                    toUser = await GetResponses(container, MakeRoot, toBot);
                    Assert.IsTrue((toUser[0].Attachments[0].Content as HeroCard).Text.Equals("Is this all day event?"));

                    toBot.Text = "Yes";
                    toUser = await GetResponses(container, MakeRoot, toBot);
                    Assert.IsTrue(toUser[0].Text.Equals("The event is created."));
                }
            }
        }

        [TestMethod]
        public async Task ShouldCreateEvent()
        {
            // Instantiate ShimsContext to use Fakes 
            using (ShimsContext.Create())
            {
                // Return "dummyToken" when calling GetAccessToken method 
                AuthBot.Fakes.ShimContextExtensions.GetAccessTokenIBotContextString =
                    async (a, e) => { return "dummyToken"; };

                // Mock the service and register
                var mockEventService = new Mock<IEventService>();
                mockEventService.Setup(x => x.CreateEvent(It.IsAny<Event>())).Returns(Task.FromResult(true));

                // IEventService 解決時にモックが返るよう設定
                var builder = new ContainerBuilder();
                builder.RegisterInstance(mockEventService.Object).As<IEventService>();
                WebApiApplication.Container = builder.Build();

                // Instantiate dialog to test
                IDialog<object> rootDialog = new RootDialog();

                // Create in-memory bot environment
                Func<IDialog<object>> MakeRoot = () => rootDialog;
                using (new FiberTestBase.ResolveMoqAssembly(rootDialog))
                using (var container = Build(Options.MockConnectorFactory | Options.ScopedQueue, rootDialog))
                {
                    // Create a message to send to bot
                    var toBot = DialogTestBase.MakeTestMessage();
                    // Specify local as US English
                    toBot.Locale = "en-US";
                    toBot.From.Id = Guid.NewGuid().ToString();
                    toBot.Text = "add appointment";

                    // Send message and check the answer.
                    var toUser = await GetResponses(container, MakeRoot, toBot);

                    // Verify the result
                    Assert.IsTrue(toUser[0].Text.Equals("Creating an event."));
                    Assert.IsTrue(toUser[1].Text.Equals("What is the title?"));

                    toBot.Text = "Learn BotFramework";
                    toUser = await GetResponses(container, MakeRoot, toBot);
                    Assert.IsTrue(toUser[0].Text.Equals("What is the detail?"));

                    toBot.Text = "Implement O365Bot";
                    toUser = await GetResponses(container, MakeRoot, toBot);
                    Assert.IsTrue(toUser[0].Text.Equals("When do you start? Use dd/MM/yyyy HH:mm format."));

                    toBot.Text = "01/07/2017 13:00";
                    toUser = await GetResponses(container, MakeRoot, toBot);
                    Assert.IsTrue((toUser[0].Attachments[0].Content as HeroCard).Text.Equals("Is this all day event?"));

                    toBot.Text = "No";
                    toUser = await GetResponses(container, MakeRoot, toBot);
                    Assert.IsTrue(toUser[0].Text.Equals("How many hours?"));


                    toBot.Text = "4";
                    toUser = await GetResponses(container, MakeRoot, toBot);
                    Assert.IsTrue(toUser[0].Text.Equals("The event is created."));
                }
            }
        }

        /// <summary>
        /// Send a message to the bot and get repsponse.
        /// </summary>
        public async Task<IMessageActivity> GetResponse(IContainer container, Func<IDialog<object>> makeRoot, IMessageActivity toBot)
        {
            using (var scope = DialogModule.BeginLifetimeScope(container, toBot))
            {
                DialogModule_MakeRoot.Register(scope, makeRoot);

                // act: sending the message
                using (new LocalizedScope(toBot.Locale))
                {
                    var task = scope.Resolve<IPostToBot>();
                    await task.PostAsync(toBot, CancellationToken.None);
                }
                //await Conversation.SendAsync(toBot, makeRoot, CancellationToken.None);
                return scope.Resolve<Queue<IMessageActivity>>().Dequeue();
            }
        }

        /// <summary>
        /// Send a message to the bot and get all repsponses.
        /// </summary>
        public async Task<List<IMessageActivity>> GetResponses(IContainer container, Func<IDialog<object>> makeRoot, IMessageActivity toBot)
        {
            using (var scope = DialogModule.BeginLifetimeScope(container, toBot))
            {
                var results = new List<IMessageActivity>();
                DialogModule_MakeRoot.Register(scope, makeRoot);

                // act: sending the message
                using (new LocalizedScope(toBot.Locale))
                {
                    var task = scope.Resolve<IPostToBot>();
                    await task.PostAsync(toBot, CancellationToken.None);
                }
                //await Conversation.SendAsync(toBot, makeRoot, CancellationToken.None);
                var queue = scope.Resolve<Queue<IMessageActivity>>();
                while (queue.Count != 0)
                {
                    results.Add(queue.Dequeue());
                }

                return results;
            }
        }
    }
}