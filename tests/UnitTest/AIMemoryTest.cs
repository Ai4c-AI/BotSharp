using BotSharp.Abstraction.AIContext;
using BotSharp.Abstraction.Agents.Models;
using BotSharp.Abstraction.Conversations.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace UnitTest
{
    [TestClass]
    public class AIMemoryTest
    {
        [TestMethod]
        public void TestAIContextProviderPriority()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();

            services.AddSingleton<IAIContextProvider, TestProviderC>();
            services.AddSingleton<IAIContextProvider, TestProviderA>();
            services.AddSingleton<IAIContextProvider, TestProviderB>();

            var serviceProvider = services.BuildServiceProvider();

            // Act
            var providers = serviceProvider.GetServices<IAIContextProvider>()
                .OrderBy(p => p.Priority)
                .ToList();

            // Assert
            Assert.AreEqual(3, providers.Count);
            Assert.AreEqual(1, providers[0].Priority);
            Assert.AreEqual(2, providers[1].Priority);
            Assert.AreEqual(3, providers[2].Priority);
        }

        [TestMethod]
        public async Task TestAIContextProviderInvoking()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<IAIContextProvider, TestProviderA>();

            var serviceProvider = services.BuildServiceProvider();
            var provider = serviceProvider.GetRequiredService<IAIContextProvider>();

            var context = new InvokingContext
            {
                Agent = new Agent { Id = "test-agent", Name = "Test Agent" },
                Dialogs = new List<RoleDialogModel>(),
                ConversationId = "test-conversation"
            };

            // Act
            var aiContext = await provider.InvokingAsync(context);

            // Assert
            Assert.IsNotNull(aiContext);
            Assert.IsNotNull(aiContext.SystemInstruction);
            Assert.IsTrue(aiContext.SystemInstruction.Contains("Test"));
        }

        [TestMethod]
        public async Task TestAIContextProviderInvoked()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<IAIContextProvider, TestProviderA>();

            var serviceProvider = services.BuildServiceProvider();
            var provider = serviceProvider.GetRequiredService<IAIContextProvider>();

            var context = new InvokedContext
            {
                Agent = new Agent { Id = "test-agent", Name = "Test Agent" },
                RequestDialogs = new List<RoleDialogModel>(),
                Response = new RoleDialogModel { Content = "Test response" },
                ConversationId = "test-conversation"
            };

            // Act
            await provider.InvokedAsync(context);

            // Assert - no exception means success
            Assert.IsTrue(true);
        }

        class TestProviderA : AIContextProviderBase
        {
            public TestProviderA()
            {
                Priority = 1;
            }

            public override int Priority { get; }

            public override async Task<AIContext?> InvokingAsync(InvokingContext context)
            {
                return await Task.FromResult(new AIContext
                {
                    SystemInstruction = "Test system instruction"
                });
            }
        }

        class TestProviderB : AIContextProviderBase
        {
            public TestProviderB()
            {
                Priority = 2;
            }

            public override int Priority { get; }
        }

        class TestProviderC : AIContextProviderBase
        {
            public TestProviderC()
            {
                Priority = 3;
            }

            public override int Priority { get; }
        }
    }
}
