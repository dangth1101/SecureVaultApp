using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SecureVaultApp.Data;
using SecureVaultApp.DTOs;
using SecureVaultApp.Helpers;
using SecureVaultApp.Interfaces;
using SecureVaultApp.Models;
using SecureVaultApp.Services;

namespace SecureVaultApp.Controllers.API;

[ApiController]
[Route("api/documents")]
[Authorize(Policy = "AnyRole")]
public class DocumentsController(
    AppDbContext context,
    IEncryptionService encryptionService,
    IAuditService auditService,
    UserManager<IdentityUser> userManager) : ControllerBase
{
    private readonly AppDbContext _context = context;
    private readonly IEncryptionService _encryptionService = encryptionService;
    private readonly IAuditService _auditService = auditService;
    private readonly UserManager<IdentityUser> _userManager = userManager;

    // POST /api/documents
    [HttpPost]
    [Authorize(Policy = "AnyRole")]
    public async Task<IActionResult> Create([FromBody] DocumentCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userDepartment = User.FindFirstValue("department");

        // User can only create documents in their department
        if (!User.IsInRole("Admin") && dto.Department != userDepartment)
            return Forbid();

        var document = new Document
        {
            Title = dto.Title,
            EncryptedContent = _encryptionService.Encrypt(dto.Content),
            OwnerId = userId!,
            Classification = dto.Classification,
            Department = dto.Department
        };

        await _context.Documents.AddAsync(document);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync(
            userId!,
            "DOCUMENT_CREATED",
            document.Id,
            IpAddressHelper.GetIpAddress(HttpContext));

        return CreatedAtAction(nameof(GetById), new { id = document.Id }, new DocumentResponseDto
        {
            Id = document.Id,
            Title = document.Title,
            Content = dto.Content,
            Classification = document.Classification,
            Department = document.Department,
            OwnerId = document.OwnerId,
            CreatedAt = document.CreatedAt
        });
    }

    // GET /api/documents
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userDepartment = User.FindFirstValue("department");

        // Admin sees all, User sees only their department
        var documents = User.IsInRole("Admin")
            ? _context.Documents.ToList()
            : _context.Documents.Where(d => d.Department == userDepartment).ToList();

        if (!documents.Any())
            return NotFound("No documents found.");

        var result = documents.Select(d => new DocumentMaskedDto
        {
            Id = d.Id,
            Title = d.Title,
            Content = MaskingHelper.MaskContent(d.EncryptedContent, d.Classification),
            Classification = d.Classification,
            Department = d.Department,
            OwnerId = MaskingHelper.MaskOwnerId(d.OwnerId),
            CreatedAt = d.CreatedAt
        });

        return Ok(result);
    }

    // GET /api/documents/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userDepartment = User.FindFirstValue("department");

        var document = await _context.Documents.FindAsync(id);
        if (document == null)
            return NotFound($"Document {id} not found.");

        // User can only view documents in their department
        if (!User.IsInRole("Admin") && document.Department != userDepartment)
            return Forbid();

        var decryptedContent = _encryptionService.Decrypt(document.EncryptedContent);

        await _auditService.LogAsync(
            userId!,
            "DOCUMENT_VIEWED",
            document.Id,
            IpAddressHelper.GetIpAddress(HttpContext));

        return Ok(new DocumentResponseDto
        {
            Id = document.Id,
            Title = document.Title,
            Content = decryptedContent ?? "Unable to decrypt content.",
            Classification = document.Classification,
            Department = document.Department,
            OwnerId = document.OwnerId,
            CreatedAt = document.CreatedAt
        });
    }

    // PUT /api/documents/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] DocumentCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userDepartment = User.FindFirstValue("department");

        var document = await _context.Documents.FindAsync(id);
        if (document == null)
            return NotFound($"Document {id} not found.");

        // User can only update documents in their department
        if (!User.IsInRole("Admin") && document.Department != userDepartment)
            return Forbid();

        document.Title = dto.Title;
        document.EncryptedContent = _encryptionService.Encrypt(dto.Content);
        document.Classification = dto.Classification;
        document.Department = dto.Department;
        document.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditService.LogAsync(
            userId!,
            "DOCUMENT_UPDATED",
            document.Id,
            IpAddressHelper.GetIpAddress(HttpContext));

        return Ok(new DocumentResponseDto
        {
            Id = document.Id,
            Title = document.Title,
            Content = dto.Content,
            Classification = document.Classification,
            Department = document.Department,
            OwnerId = document.OwnerId,
            CreatedAt = document.CreatedAt
        });
    }

    // DELETE /api/documents/{id}
    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(string id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var document = await _context.Documents.FindAsync(id);
        if (document == null)
            return NotFound($"Document {id} not found.");

        _context.Documents.Remove(document);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync(
            userId!,
            "DOCUMENT_DELETED",
            id,
            IpAddressHelper.GetIpAddress(HttpContext));

        return Ok($"Document {id} deleted successfully.");
    }

    // GET /api/documents/{id}/audit
    [HttpGet("{id}/audit")]
    [Authorize(Policy = "AdminOnly")]
    public IActionResult GetAuditLog(string id)
    {
        var logs = _context.AuditLogs
            .Where(l => l.ResourceId == id)
            .OrderByDescending(l => l.Timestamp)
            .ToList();

        if (!logs.Any())
            return NotFound($"No audit logs found for document {id}.");

        return Ok(logs);
    }
}