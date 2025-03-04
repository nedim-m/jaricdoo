﻿using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{


    [ApiController]
    [Route("[controller]")]
    public class ContactMessageController : ControllerBase
    {
        private readonly IContactMessageService _contactMessageService;


        public ContactMessageController(IContactMessageService contactMessageService)
        {
            _contactMessageService = contactMessageService;
        }

        [HttpPost]
        public async Task<IActionResult> Insert(ContactMessageInsertRequest request)
        {
            var result = await _contactMessageService.InsertAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var messages = await _contactMessageService.GetAllAsync();
            return Ok(messages);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var message = await _contactMessageService.GetByIdAsync(id);

            if (message == null) return NotFound();

            return Ok(message);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _contactMessageService.DeleteAsync(id);

            if (!result) return NotFound();

            return NoContent();
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] bool isRead)
        {
            var result = await _contactMessageService.UpdateStatusAsync(id, isRead);

            if (!result) return NotFound();

            return NoContent();
        }
        [HttpGet("unread-message-count")]
        public async Task<ActionResult<int>> GetUnreadMessageCount()
        {
            int unreadCount = await _contactMessageService.GeUnreadMessageCount();
            return Ok(unreadCount);
        }

    }
}
