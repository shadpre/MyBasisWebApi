using DAL;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using MyBasisWebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TESTS.Utility;
using Xunit.Abstractions;

namespace TESTS.Tests
{
    public class WebAppFactoryWorks : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly TestUtils<MyDbContext> _testUtils;
        private readonly HttpClient _client;

        public WebAppFactoryWorks(
            ITestOutputHelper outputHelper,
            WebApplicationFactory<Program> factory)

        {
            _client = factory.CreateClient();
            _testUtils = new TestUtils<MyDbContext>();

        }


        [Fact]
        public async Task CanGetHelloWorldWhenUsingHttpClient()
        {
            var response = await _client.GetAsync("/api/test");
            var responseString = await response.Content.ReadAsStringAsync();
            Assert.Equal("Hello World!", responseString);
            Assert.Equal(200, (int)response.StatusCode);
        }

    }
}
