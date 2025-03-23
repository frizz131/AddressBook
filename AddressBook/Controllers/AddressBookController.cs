using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RepositoryLayer.AddressBookDbContext;
using RepositoryLayer.Models;
using RepositoryLayer.DTOs;
using BusinessLayer.Services;

namespace AddressBook.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AddressBookController : ControllerBase
    {
        private readonly IAddressBookBL _businessLayer;

        public AddressBookController(IAddressBookBL businessLayer)
        {
            _businessLayer = businessLayer;
        }

        // GET: api/addressbook
        [HttpGet]
        public ActionResult<IEnumerable<AddressBookEntryDto>> GetAddressBookEntries()
        {
            var entryDtos = _businessLayer.GetAllEntries();
            return Ok(entryDtos);
        }

        // GET: api/addressbook/{id}
        [HttpGet("{id}")]
        public ActionResult<AddressBookEntryDto> GetAddressBookEntry(int id)
        {
            var entryDto = _businessLayer.GetEntryById(id);
            if (entryDto == null)
            {
                return NotFound();
            }
            return Ok(entryDto);
        }

        // POST: api/addressbook
        [HttpPost]
        public ActionResult<AddressBookEntryDto> AddAddressBookEntry([FromBody] AddressBookEntryDto entryDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdDto = _businessLayer.AddEntry(entryDto);
            return CreatedAtAction(nameof(GetAddressBookEntry), new { id = createdDto.Id }, createdDto);
        }

        // PUT: api/addressbook/{id}
        [HttpPut("{id}")]
        public ActionResult<AddressBookEntryDto> UpdateAddressBookEntry(int id, [FromBody] AddressBookEntryDto entryDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updatedDto = _businessLayer.UpdateEntry(id, entryDto);
                if (updatedDto == null)
                {
                    return NotFound();
                }
                return Ok(updatedDto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE: api/addressbook/{id}
        [HttpDelete("{id}")]
        public ActionResult DeleteAddressBookEntry(int id)
        {
            _businessLayer.DeleteEntry(id);
            return NoContent();
        }
    }
}