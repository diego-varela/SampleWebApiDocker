using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using ServiceStack.Text;
using WebApi.AddressBook;
using Google.Protobuf;
using System.IO;
using static WebApi.AddressBook.Person.Types;

namespace WebApi.Controllers
{    
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        string databaseUrl = "redis-12405.c62.us-east-1-4.ec2.cloud.redislabs.com:12405,password=vWYFmUEv7fWZweJRO3YdaBlbzc09Hy9l";

        IDatabase database;

        public ValuesController()
        {
            try
            {
                var redis = ConnectionMultiplexer.Connect(databaseUrl);
                database = redis.GetDatabase();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to connect to Redis Database at {databaseUrl}");
                Debug.WriteLine($"Exception {ex}");
            }
        }
        
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            Person john = new Person
            {
                Id = 1234,
                Name = "John Doe",
                Email = "jdoe@example.com",
                Phones = { new PhoneNumber { Number = "555-4321", Type = Person.Types.PhoneType.Home } }
            };

            // Use of ServiceStack libraries
            var json = JsonSerializer.SerializeToString(john);

            // Use of Google Protocol Buffer
            using (MemoryStream stream = new MemoryStream())
            {
                john.WriteTo(stream);
                john = Person.Parser.ParseFrom(stream.ToArray());
            }

            return new string[] { "Hello", "from", "Docker", john.Name, json };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(string id)
        {
            return database != null
                ? $"Value {database.StringGet(id)} stored in Redis as {id}"
                : $"Unable to connect to database to get data with key {id}";
        }

        // GET api/values/mykey/value
        [HttpGet("{key}/{value}")]
        public ActionResult<string> Get(string key, string value)
        {
            if (database != null)
            {
                database.StringSet(key, value);
                return $"Stored value {database.StringGet(key)} in Redis with key {key}";
            }
            else
            {
                return $"Unable to connect to database to store data with key {key}";
            }
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
