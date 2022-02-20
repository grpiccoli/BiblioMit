using BiblioMit.Data;
using BiblioMit.Models;
using BiblioMit.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace BiblioMit.Controllers
{
    [Authorize]
    public class ContactsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ContactsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Contacts
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            IQueryable<Contact> contacts = _context.Contacts
                .Include(c => c.ConsessionOrResearch)
                    .ThenInclude(c => c.Company);

            IQueryCollection q = Request.Query;
            string[] tmp = q["c"];

            foreach (var c in tmp)
            {
                if (int.TryParse(c, out int r))
                    contacts = contacts.Where(o =>
                    o.ConsessionOrResearch != null && o.ConsessionOrResearch.CommuneId != null &&
                    o.ConsessionOrResearch.CommuneId.Value == r);
            }

            var isAuthorized = User.IsInRole(RoleData.Administrator.ToString()) &&
                               User.HasClaim(UserClaims.Contacts.ToString(), UserClaims.Contacts.ToString());

            var currentUserId = _userManager.GetUserId(User);

            // Only approved contacts are shown UNLESS you're authorized to see them
            // or you are the owner.
            if (!isAuthorized)
            {
                contacts = contacts.Where(c => c.Status == ContactStatus.Approved
                                            || c.OwnerId == currentUserId);
            }
            var modelAsync = contacts.ToListAsync();

            ViewData["c"] = tmp;
            ViewData["comunas"] = _context.Communes
            .Select(c => new ContactCommune
            {
                Id = c.Id,
                Commune = c.Name,
                Province = c.Province.Name
            });

            return View(await modelAsync.ConfigureAwait(false));
        }

        // GET: Contacts/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contact = await _context.Contacts
                .SingleOrDefaultAsync(m => m.Id == id).ConfigureAwait(false);
            if (contact == null)
            {
                return NotFound();
            }

            return View(contact);
        }

        // GET: Contacts/Create
        [Authorize(Roles = nameof(RoleData.Administrator), Policy = nameof(UserClaims.Contacts))]
        [HttpGet]
        public IActionResult Create()
        {
            //return View();
            // TODO-Rick - remove, this is just for quick testing.
            return View(new ContactEditViewModel
            {
                Last = "Apellido",
                Email = _userManager.GetUserName(User),
                Name = "Nombre",
                Position = Position.Management,
                Description = "Descripci�n",
                Phone = 56912345678,
                OpenHr = Convert.ToDateTime("9:00", CultureInfo.InvariantCulture),
                CloseHr = Convert.ToDateTime("18:00", CultureInfo.InvariantCulture)
            });
        }

        // POST: Contacts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [Authorize(Roles = nameof(RoleData.Administrator) + "," + nameof(RoleData.Editor), Policy = nameof(UserClaims.Contacts))]
        public async Task<IActionResult> Create(ContactEditViewModel editModel)
        {
            if (editModel == null) return NotFound();
            if (!ModelState.IsValid)
            {
                return View(editModel);
            }

            var contact = ViewModelToModel(new Contact(), editModel);

            contact.OwnerId = _userManager.GetUserId(User);

            _context.Add(contact);
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return RedirectToAction("Index");
        }

        // GET: Contacts/Edit/5
        [Authorize(Roles = nameof(RoleData.Administrator) + "," + nameof(RoleData.Editor), Policy = nameof(UserClaims.Contacts))]
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contact = await _context.Contacts.SingleOrDefaultAsync(m => m.Id == id).ConfigureAwait(false);
            if (contact == null)
            {
                return NotFound();
            }

            var editModel = ModelToViewModel(contact);

            return View(editModel);
        }

        // POST: Contacts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [Authorize(Roles = nameof(RoleData.Administrator) + "," + nameof(RoleData.Editor), Policy = nameof(UserClaims.Contacts))]
        public async Task<IActionResult> Edit(int id, ContactEditViewModel editModel)
        {
            if (!ModelState.IsValid)
            {
                return View(editModel);
            }

            // Fetch Contact from DB to get OwnerId.
            var contact = await _context.Contacts.SingleOrDefaultAsync(m => m.Id == id).ConfigureAwait(false);
            if (editModel == null || contact == null)
            {
                return NotFound();
            }

            contact = ViewModelToModel(contact, editModel);

            if (contact.Status == ContactStatus.Approved)
            {
                // If the contact is updated after approval, 
                // and the user cannot approve set the status back to submitted
                var canApprove = User.IsInRole(RoleData.Administrator.ToString()) && User.HasClaim(UserClaims.Contacts.ToString(), UserClaims.Contacts.ToString());

                if (!canApprove) contact.Status = ContactStatus.Submitted;
            }

            _context.Update(contact);
            await _context.SaveChangesAsync().ConfigureAwait(false);

            return RedirectToAction("Index");
        }

        // GET: Contacts/Delete/5
        [Authorize(Roles = nameof(RoleData.Administrator), Policy = nameof(UserClaims.Contacts))]
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contact = await _context.Contacts
                .SingleOrDefaultAsync(m => m.Id == id).ConfigureAwait(false);
            if (contact == null)
            {
                return NotFound();
            }

            return View(contact);
        }

        // POST: Contacts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [Authorize(Roles = nameof(RoleData.Administrator), Policy = nameof(UserClaims.Contacts))]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contact = await _context.Contacts.SingleOrDefaultAsync(m => m.Id == id).ConfigureAwait(false);
            if (contact != null)
            {
                _context.Contacts.Remove(contact);
                await _context.SaveChangesAsync().ConfigureAwait(false);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [Authorize(Roles = nameof(RoleData.Administrator), Policy = nameof(UserClaims.Contacts))]
        public async Task<IActionResult> SetStatus(int id, ContactStatus status)
        {
            var contact = await _context.Contacts.SingleOrDefaultAsync(m => m.Id == id).ConfigureAwait(false);

            if (contact != null)
            {
                contact.Status = status;
                _context.Contacts.Update(contact);
                await _context.SaveChangesAsync().ConfigureAwait(false);
            }
            return RedirectToAction("Index");
        }

        //private bool ContactExists(int id)
        //{
        //    return _context.Contact.Any(e => e.ContactId == id);
        //}

        private static Contact ViewModelToModel(Contact contact, ContactEditViewModel editModel)
        {
            contact.Last = editModel.Last;
            contact.Email = editModel.Email;
            contact.Name = editModel.Name;
            contact.Position = editModel.Position;
            contact.Description = editModel.Description;
            contact.Phone = editModel.Phone;
            contact.OpenHr = editModel.OpenHr;
            contact.CloseHr = editModel.CloseHr;

            return contact;
        }

        [Authorize(Roles = nameof(RoleData.Administrator), Policy = nameof(UserClaims.Contacts))]
        private static ContactEditViewModel ModelToViewModel(Contact contact)
        {
            var editModel = new ContactEditViewModel()
            {
                Id = contact.Id,
                Last = contact.Last,
                Email = contact.Email,
                Name = contact.Name,
                Position = contact.Position,
                Description = contact.Description,
                Phone = contact.Phone,
                OpenHr = contact.OpenHr,
                CloseHr = contact.CloseHr
            };

            return editModel;
        }
    }
}
