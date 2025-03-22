using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RepositoryLayer.AddressBookDbContext;
using RepositoryLayer.Models;
using RepositoryLayer.DTOs;

namespace AddressBook.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AddressBookController : ControllerBase
    {
        private readonly AddressBookContext _context;

        public AddressBookController(AddressBookContext context)
        {
            _context = context;
        }

        // GET: api/addressbook
        [HttpGet]
        public ActionResult<IEnumerable<AddressBookEntryDto>> GetAddressBookEntries()
        {
            var entries = _context.Contacts.ToList();
            var entryDtos = entries.Select(e => new AddressBookEntryDto
            {
                Id = e.Id,
                Name = e.Name,
                Email = e.Email,
                Phone = e.Phone
            }).ToList();
            return Ok(entryDtos);
        }

        // GET: api/addressbook/{id}
        [HttpGet("{id}")]
        public ActionResult<AddressBookEntryDto> GetAddressBookEntry(int id)
        {
            var entry = _context.Contacts.Find(id);
            if (entry == null)
            {
                return NotFound();
            }

            var entryDto = new AddressBookEntryDto
            {
                Id = entry.Id,
                Name = entry.Name,
                Email = entry.Email,
                Phone = entry.Phone
            };
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

            var entry = new AddressBookEntry
            {
                Name = entryDto.Name,
                Email = entryDto.Email,
                Phone = entryDto.Phone
            };

            _context.Contacts.Add(entry);
            _context.SaveChanges();

            entryDto.Id = entry.Id; // Update DTO with the generated ID
            return CreatedAtAction(nameof(GetAddressBookEntry), new { id = entry.Id }, entryDto);
        }

        // PUT: api/addressbook/{id}
        [HttpPut("{id}")]
        public ActionResult<AddressBookEntryDto> UpdateAddressBookEntry(int id, [FromBody] AddressBookEntryDto entryDto)
        {
            if (id != entryDto.Id)
            {
                return BadRequest("ID in URL must match ID in body");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingEntry = _context.Contacts.Find(id);
            if (existingEntry == null)
            {
                return NotFound();
            }

            existingEntry.Name = entryDto.Name;
            existingEntry.Email = entryDto.Email;
            existingEntry.Phone = entryDto.Phone;

            _context.SaveChanges();
            return Ok(entryDto);
        }

        // DELETE: api/addressbook/{id}
        [HttpDelete("{id}")]
        public ActionResult DeleteAddressBookEntry(int id)
        {
            var entry = _context.Contacts.Find(id);
            if (entry == null)
            {
                return NotFound();
            }

            _context.Contacts.Remove(entry);
            _context.SaveChanges();
            return NoContent();
        }
    }
}
