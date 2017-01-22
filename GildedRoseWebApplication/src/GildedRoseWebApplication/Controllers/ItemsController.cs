using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GildedRoseWebApplication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GildedRoseWebApplication.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class ItemsController : Controller
    {
        private IItemsRepository itemsRepository;

        public ItemsController(IItemsRepository itemsRepository)
        {
            this.itemsRepository = itemsRepository;
        }

        // GET api/items
        [HttpGet]
        public IEnumerable<Item> Get()
        {
            return itemsRepository.GetAll();
        }

        // GET api/items/5
        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            var item = itemsRepository.Find(id);
            if (item == null)
                return NotFound();
            return new ObjectResult(item);
        }

        // POST api/items
        [HttpPost]
        public IActionResult Post([FromBody]string value)
        {
            throw new NotImplementedException();
        }

        // PUT api/items/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody]string value)
        {
            throw new NotImplementedException();
        }

        // DELETE api/items/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            throw new NotImplementedException();
        }
    }
}
