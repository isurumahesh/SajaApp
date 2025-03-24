using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Saj.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HousesController : ControllerBase
    {
        private readonly IHouseRepository houseRepository;
        public HousesController(IHouseRepository houseRepository)
        {
            this.houseRepository = houseRepository;
        }

        // GET: api/<HousesController>
        [HttpGet]
        public async Task<IActionResult> Get() 
        {
            var houses = await houseRepository.GetAll();
            return Ok(houses);
        }

        // GET api/<HousesController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<HousesController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<HousesController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<HousesController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
