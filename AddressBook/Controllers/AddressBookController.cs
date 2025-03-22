using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RepositoryLayer.AddressBookDbContext;
using RepositoryLayer.Models;

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
        public ActionResult<IEnumerable<Contact>> GetContacts()
        {
            var contacts = _context.Contacts.ToList();
            return Ok(contacts);
        }

        // GET: api/addressbook/{id}
        [HttpGet("{id}")]
        public ActionResult<Contact> GetContact(int id)
        {
            var contact = _context.Contacts.Find(id);
            if (contact == null)
            {
                return NotFound();
            }
            return Ok(contact);
        }

        // POST: api/addressbook
        [HttpPost]
        public ActionResult<Contact> AddContact([FromBody] Contact contact)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Contacts.Add(contact);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetContact), new { id = contact.Id }, contact);
        }

        // PUT: api/addressbook/{id}
        [HttpPut("{id}")]
        public ActionResult<Contact> UpdateContact(int id, [FromBody] Contact contact)
        {
            if (id != contact.Id)
            {
                return BadRequest("ID in URL must match ID in body");
            }

            var existingContact = _context.Contacts.Find(id);
            if (existingContact == null)
            {
                return NotFound();
            }

            existingContact.Name = contact.Name;
            existingContact.Email = contact.Email;
            existingContact.Phone = contact.Phone;

            _context.SaveChanges();
            return Ok(existingContact);
        }

        // DELETE: api/addressbook/{id}
        [HttpDelete("{id}")]
        public ActionResult DeleteContact(int id)
        {
            var contact = _context.Contacts.Find(id);
            if (contact == null)
            {
                return NotFound();
            }

            _context.Contacts.Remove(contact);
            _context.SaveChanges();
            return NoContent();
        }
    }
}
