using DentalManagementAPI.Data;
using DentalManagementAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DentalManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        // Allowed file extensions and size limit to match frontend
        private readonly string[] _allowedExtensions = { ".pdf", ".png", ".jpg", ".jpeg", ".doc", ".docx" };
        private readonly long _maxFileSize = 10 * 1024 * 1024; // 10 MB

        public DocumentsController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: api/Documents
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Document>>> GetAllDocuments()
        {
            var documents = await _context.Documents.ToListAsync();
            return Ok(documents);
        }

        // GET: api/Documents/patient/{patientId}
        [HttpGet("patient/{patientId}")]
        public async Task<ActionResult<IEnumerable<Document>>> GetDocumentsByPatient(int patientId)
        {
            var patient = await _context.Patients.FindAsync(patientId);
            if (patient == null)
            {
                return NotFound(new { message = "Patient not found" });
            }

            var documents = await _context.Documents
                .Where(d => d.PatientId == patientId)
                .ToListAsync();

            return Ok(documents);
        }

        // GET: api/Documents/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Document>> GetDocument(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null)
            {
                return NotFound(new { message = "Document not found" });
            }

            return Ok(document);
        }

        // POST: api/Documents
        [HttpPost]
        public async Task<ActionResult<Document>> UploadDocument([FromForm] DocumentUploadDto documentDto)
        {
            // Validate input
            if (documentDto.File == null || documentDto.File.Length == 0)
            {
                return BadRequest(new { message = "No file uploaded" });
            }

            if (documentDto.File.Length > _maxFileSize)
            {
                return BadRequest(new { message = "File size exceeds 10MB limit" });
            }

            var extension = Path.GetExtension(documentDto.File.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
            {
                return BadRequest(new { message = "Invalid file type. Allowed types: pdf, png, jpg, jpeg, doc, docx" });
            }

            var patient = await _context.Patients.FindAsync(documentDto.PatientId);
            if (patient == null)
            {
                return NotFound(new { message = "Patient not found" });
            }

            // Validate DocumentType
            var validDocumentTypes = new[] { "EOB", "Forms", "Patient Treatment", "Patient Information", "Patient Health History" };
            if (!string.IsNullOrEmpty(documentDto.DocumentType) && !validDocumentTypes.Contains(documentDto.DocumentType))
            {
                return BadRequest(new { message = $"Invalid document type. Valid types are: {string.Join(", ", validDocumentTypes)}" });
            }

            // Create uploads directory if it doesn't exist
            var uploadsPath = Path.Combine(_env.WebRootPath ?? Directory.GetCurrentDirectory(), "wwwroot", "Uploads");
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            // Generate unique filename
            var fileName = Guid.NewGuid().ToString() + extension;
            var filePath = Path.Combine(uploadsPath, fileName);

            // Save file
            try
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await documentDto.File.CopyToAsync(stream);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error saving file", error = ex.Message });
            }

            // Create document record
            var document = new Document
            {
                PatientId = documentDto.PatientId,
                Name = documentDto.File.FileName,
                Url = $"/Uploads/{fileName}",
                Size = documentDto.File.Length,
                UploadedDate = DateTime.UtcNow,
                UploadedBy = documentDto.UploadedBy ?? "system_user",
                DocumentType = documentDto.DocumentType ?? "Patient Treatment"
            };

            _context.Documents.Add(document);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Clean up file if database save fails
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
                // Log full exception details
                Console.WriteLine($"Exception: {ex}\nInner Exception: {ex.InnerException}");
                return StatusCode(500, new
                {
                    message = "Error saving document to database",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message ?? "No inner exception",
                    stackTrace = ex.InnerException?.StackTrace ?? ex.StackTrace
                });
            }

            return CreatedAtAction(nameof(GetDocument), new { id = document.Id }, document);
        }

        // PUT: api/Documents/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDocument(int id, [FromBody] DocumentUpdateDto documentDto)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null)
            {
                return NotFound(new { message = "Document not found" });
            }

            // Validate DocumentType
            var validDocumentTypes = new[] { "EOB", "Forms", "Patient Treatment", "Patient Information", "Patient Health History" };
            if (!string.IsNullOrEmpty(documentDto.DocumentType) && !validDocumentTypes.Contains(documentDto.DocumentType))
            {
                return BadRequest(new { message = $"Invalid document type. Valid types are: {string.Join(", ", validDocumentTypes)}" });
            }

            // Update fields
            document.DocumentType = documentDto.DocumentType ?? document.DocumentType;
            document.UploadedBy = documentDto.UploadedBy ?? document.UploadedBy;
            document.UploadedDate = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex}\nInner Exception: {ex.InnerException}");
                return StatusCode(500, new
                {
                    message = "Error updating document in database",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message ?? "No inner exception",
                    stackTrace = ex.InnerException?.StackTrace ?? ex.StackTrace
                });
            }

            return NoContent();
        }

        // GET: api/Documents/{id}/download
        [HttpGet("{id}/download")]
        public async Task<IActionResult> DownloadDocument(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null)
                return NotFound(new { message = "Document not found" });

            var filePath = Path.Combine(_env.WebRootPath ?? Directory.GetCurrentDirectory(), document.Url.TrimStart('/'));
            if (!System.IO.File.Exists(filePath))
                return NotFound(new { message = "File not found on server" });

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            var contentType = GetContentType(document.Url);
            return File(fileBytes, contentType, document.Name);
        }

        // DELETE: api/Documents/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null)
            {
                return NotFound(new { message = "Document not found" });
            }

            // Delete physical file
            var filePath = Path.Combine(_env.WebRootPath ?? Directory.GetCurrentDirectory(), document.Url.TrimStart('/'));
            try
            {
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting file", error = ex.Message });
            }

            _context.Documents.Remove(document);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting document from database", error = ex.Message });
            }

            return NoContent();
        }

        private string GetContentType(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                _ => "application/octet-stream"
            };
        }

        public class DocumentUploadDto
        {
            public int PatientId { get; set; }
            public IFormFile File { get; set; }
            public string? UploadedBy { get; set; }
            public string? DocumentType { get; set; }
        }

        public class DocumentUpdateDto
        {
            public string? UploadedBy { get; set; }
            public string? DocumentType { get; set; }
        }
    }
}