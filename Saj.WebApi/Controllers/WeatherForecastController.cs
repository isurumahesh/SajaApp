using Azure.Identity;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Queues;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using StackExchange.Redis;
using System.ComponentModel;
using System.Reflection.Metadata;
using System.Text;

namespace Saj.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private IDatabase _database;
        private IConfiguration _configurationBuilder;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IConfiguration configurationBuilder, IConnectionMultiplexer connectionMultiplexer)
        {
            _logger = logger;
            _configurationBuilder = configurationBuilder;
            _database = connectionMultiplexer.GetDatabase();
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {

            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.UtcNow,
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }


        [HttpPost(Name = "SaveWeatherForecast")]
        public async Task<IActionResult> Post(WeatherForecast data)
        {

            var client = new ServiceBusClient(_configurationBuilder["ServiceBus:QueueConnectionString"].ToString());
            var sender = client.CreateSender("add-weather-data");
            var body = JsonConvert.SerializeObject(data);
            var message = new ServiceBusMessage(body);
            await sender.SendMessageAsync(message);

            var receiver = client.CreateReceiver("add-weather-data");
            var peekMessage = await receiver.PeekMessagesAsync(5);
            return Ok();
        }

        [HttpPost]
        [Route("topic")]
        public async Task<IActionResult> PostTopicData(WeatherForecast data)
        {
            var client = new ServiceBusClient(_configurationBuilder["ServiceBus:TopicConnectionString"].ToString());
            var sender = client.CreateSender("weather-data-topic");
            var body = JsonConvert.SerializeObject(data);
            var message = new ServiceBusMessage(body);
            message.ApplicationProperties.Add("Month", "December");
            await sender.SendMessageAsync(message);

            return Ok();
        }

        [HttpPost]
        [Route("queue")]
        public async Task<IActionResult> PostQueueData(WeatherForecast data)
        {
            var client = new QueueClient(_configurationBuilder["StorageAccount:ConnectionString"].ToString(), "sajqueue");

            //for (int i = 0; i <= 3; i++)
            //{
            //    data.TemperatureC = i;
            //    var body = JsonConvert.SerializeObject(data);
            //    await client.SendMessageAsync(body);
            //}

            var peekMessages = await client.PeekMessagesAsync();
            var receiveMessages = await client.ReceiveMessagesAsync();
            return Ok();
        }

        [HttpPost]
        [Route("eventhubsenddata")]
        public async Task<IActionResult> PostEventHub(WeatherForecast data)
        {
            var cn = _configurationBuilder["EventHub:ConnectionString"].ToString();
            var name = _configurationBuilder["EventHub:Name"].ToString();

            var producerClient = new EventHubProducerClient(cn, name);
            var eventDataBatch = await producerClient.CreateBatchAsync();

            eventDataBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes("First Event")));
            eventDataBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes("Second Event")));
            eventDataBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes("Third Event")));

            await producerClient.SendAsync(eventDataBatch);

            return Ok();
        }

        [HttpPost]
        [Route("eventhubreaddata")]
        public async Task<IActionResult> RedEventHub(WeatherForecast data)
        {
            var cn = _configurationBuilder["EventHub:ConnectionString"].ToString();
            var name = _configurationBuilder["EventHub:Name"].ToString();

            var producerClient = new EventHubProducerClient(cn, name);
            var eventDataBatch = await producerClient.CreateBatchAsync();

            eventDataBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes("First Event")));
            eventDataBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes("Second Event")));
            eventDataBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes("Third Event")));

            await producerClient.SendAsync(eventDataBatch);

            return Ok();
        }

        [HttpPost]
        [Route("blob")]
        public async Task<IActionResult> AddBlobData()
        {
            string containerEndpoint = string.Format("https://sajstorage204.blob.core.windows.net/photos");
            var endpoint = _configurationBuilder["StorageAccount:ConnectionString"];

            // Get a credential and create a client object for the blob container.
            BlobServiceClient blobServiceClient = new BlobServiceClient(endpoint.ToString());
            var containerClient = blobServiceClient.GetBlobContainerClient("photos");
            try
            {
                var formCollection = await Request.ReadFormAsync();
                var file = formCollection.Files.First();

                // Create the container if it does not exist.
                await containerClient.CreateIfNotExistsAsync();
                var blob = containerClient.GetBlobClient(file.FileName);

                using (var fileStream = file.OpenReadStream())
                {
                    await blob.UploadAsync(fileStream, new BlobHttpHeaders { ContentType = file.ContentType, ContentLanguage = "en-us" });
                }

                IDictionary<string, string> metadata =
          new Dictionary<string, string>();

                // Add some metadata to the container.
                metadata.Add("docType", "saja");
                metadata.Add("category", "guidance");

                // Set the container's metadata.
                await blob.SetMetadataAsync(metadata);

                return Ok(blob.Uri.ToString());
            }
            catch (Exception e)
            {
                throw e;
            }


        }

        [HttpGet]
        [Route("Getpeople")]
        public async Task<IActionResult> GetPeople(string id, string partitionKey)
        {
            try
            {
                //    var cachedData = await _database.StringGetAsync("instagram");
                //    var cachedValue = cachedData.ToString();

                var endpoint = _configurationBuilder["CosmosDb:Endpoint"];
                var primarykey = _configurationBuilder["CosmosDb:PrimaryKey"];
                var database = _configurationBuilder["CosmosDb:Database"];

                var endpointstr = endpoint.ToString();
                var primaryKeystr = primarykey.ToString();

                CosmosClient cosmosClient = new CosmosClient(endpoint.ToString(), primarykey.ToString());
                Database db = cosmosClient.GetDatabase(database.ToString());
                Microsoft.Azure.Cosmos.Container container = db.GetContainer("people");

                var response = await container.ReadItemAsync<People>(id, new PartitionKey(partitionKey));
                return Ok(response.Resource);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        [HttpPost]
        [Route("addpeople")]
        public async Task<IActionResult> AddPeople()
        {
            try
            {
                //    var cachedData = await _database.StringGetAsync("instagram");
                //    var cachedValue = cachedData.ToString();

                var endpoint = _configurationBuilder["CosmosDb:Endpoint"];
                var primarykey = _configurationBuilder["CosmosDb:PrimaryKey"];
                var database = _configurationBuilder["CosmosDb:Database"];

                var endpointstr = endpoint.ToString();
                var primaryKeystr = primarykey.ToString();

                CosmosClient cosmosClient = new CosmosClient(endpoint.ToString(), primarykey.ToString());
                Database db = cosmosClient.GetDatabase(database.ToString());
                Microsoft.Azure.Cosmos.Container container = db.GetContainer("people");

                var person = new People
                {
                    FirstName = "Isuru",
                    LastName = "Mahesh",
                    City = "Gampaha"
                };

                ItemResponse<People> response = await container.CreateItemAsync(person);
                return Ok(response.Resource);
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }

    public class People
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [JsonProperty(PropertyName = "city")]
        public string City { get; set; }
    }
}
