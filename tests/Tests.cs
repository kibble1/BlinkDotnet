using BlinkDotnet;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Moq;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Tests
{

    public class Tests
    {readonly Mock<ILogger<BlinkCam>> loggerMock;
       
        private IConfigurationRoot configuration;
        private IBlinkCam blinkCam;

        public Tests()
        {
             var services = new ServiceCollection();

            var builder = new ConfigurationBuilder()
                .AddUserSecrets<Tests>();
            
            this.configuration = builder.Build();
            
            this.loggerMock = new Mock<ILogger<BlinkCam>>();

            services.AddSingleton<ILogger<BlinkCam>>(this.loggerMock.Object);

            services
                .AddOptions<BlinkCamOptions>()
                .Configure(options => getOpts(options));

            services.TryAddScoped<IBlinkCam, BlinkCam>();

            var serviceProvider = services.BuildServiceProvider(); 
            this.blinkCam = serviceProvider.GetRequiredService<IBlinkCam>();
        }

        private void getOpts(BlinkCamOptions opt)
        {
                this.configuration.Bind("BlinkCam", opt);
        }

        [Fact]
        public async Task CanLoginAndGetToken()
        {
            var loginResponse = await this.blinkCam.LoginAsync();
          
            Assert.NotEmpty(loginResponse.Networks);
            Assert.NotEqual(string.Empty, loginResponse.Authtoken);
        }

        [Fact]
        public async Task CanQueryNetworks()
        {
            var networksResponse = await this.blinkCam.GetNetworksAsync();
            var networks = networksResponse.ToList();
            Assert.NotEmpty(networks);
        }

        [Fact]
        public async Task CanQueryEvents()
        {
            var eventsResponse = await this.blinkCam.GetEventsAsync();
            var events = eventsResponse.ToList();
            var groupedEvents = events.GroupBy(e => e.type, e => e, (e, d) => new {EventType = e, Events = d.ToList()}).ToList();;
            Assert.NotEmpty(events);
        }

        [Fact]
        public async Task CanQueryVideos()
        {
            var videosResponse = await this.blinkCam.GetAllVideosListAsync();
            var videos = videosResponse.ToList();
            Assert.NotEmpty(videos);
        }

        [Fact]
        public async Task CanDownloadAVideo()
        {
            var video = (await this.blinkCam.GetAllVideosListAsync()).First();

            using(var stream = await this.blinkCam.GetVideoAsMemoryStreamAsync(video.address)){
                Assert.True(stream.Length > 0);
            }
        }
    }
}