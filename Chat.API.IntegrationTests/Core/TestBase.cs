namespace Chat.API.IntegrationTests.Core
{
    public abstract class TestBase
    {
        private readonly ChatApiFactory _chatApiFactory;

        public TestBase(ChatApiFactory chatApiFactory)
        {
            _chatApiFactory = chatApiFactory;
        }
    }
}


