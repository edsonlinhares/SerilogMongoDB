using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace TesteSeriLogMongo.Controllers
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

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();

            var range = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();

            _logger.LogInformation($"GET WeatherForecast -> {Newtonsoft.Json.JsonConvert.SerializeObject(range)}");

            return range;
        }

        [HttpGet("TesteLogs")]
        public string TesteLogs()
        {
            _logger.LogInformation(new EventId(22, "TesteLog"), $"Log Information.");
            _logger.LogWarning(new EventId(22, "TesteLog"), $"Log Warning.");
            _logger.LogError(new EventId(22, "TesteLog"), $"Log Error.");
            _logger.LogCritical(new EventId(22, "TesteLog"), $"Log Critica.");
            _logger.LogDebug(new EventId(22, "TesteLog"), $"Log Debug.");            
            _logger.LogTrace(new EventId(22, "TesteLog"), $"Log Trace.");

            _logger.LogInformation( new EventId(22,"TesteLog"), "Testando", new TesteLog { Mensagem = "Testando", Codigo = "A045" });

            //throw new ArgumentOutOfRangeException(message: "teste",innerException:new Exception("Muita zica mesmo"));

            return "Oi";
        }

        [HttpGet("TesteMongo")]
        public string TesteMongo()
        {
            MongoClient dbClient = new MongoClient("mongodb://root:testE_1234x@localhost:27017");

            var database = dbClient.GetDatabase("sample_training");
            var collection = database.GetCollection<BsonDocument>("grades");

            var document = new BsonDocument { { "student_id", 10000 }, {
                "scores",
                new BsonArray {
                new BsonDocument { { "type", "exam" }, { "score", 88.12334193287023 } },
                new BsonDocument { { "type", "quiz" }, { "score", 74.92381029342834 } },
                new BsonDocument { { "type", "homework" }, { "score", 89.97929384290324 } },
                new BsonDocument { { "type", "homework" }, { "score", 82.12931030513218 } }
                }
                }, { "class_id", 480 }
        };

            collection.InsertOne(document);
            //await collection.InsertOneAsync (document);

            return "Oi";
        }
    }

    public class TesteLog
    {
        public string Mensagem { get; set; }
        public string Codigo { get; set; }
    }
}
