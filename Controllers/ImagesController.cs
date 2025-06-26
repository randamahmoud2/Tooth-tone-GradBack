//using DentalManagementAPI.Data;
//using DentalManagementAPI.Models;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Threading.Tasks;

//namespace DentalManagementAPI.Controllers
//{
//    [Route("api/patients/{patientId}/images")]
//    [ApiController]
//    public class ImagesController : ControllerBase
//    {
//        private readonly AppDbContext _context;
//        private readonly string _storagePath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads/Images");

//        public ImagesController(AppDbContext context)
//        {
//            _context = context;
//            if (!Directory.Exists(_storagePath))
//            {
//                Directory.CreateDirectory(_storagePath);
//            }
//        }

//        [HttpGet]
//        public async Task<ActionResult<List<Image>>> GetImages(int patientId)
//        {
//            var images = await _context.Images
//                .Where(i => i.PatientId == patientId)
//                .ToListAsync();

//            if (images == null || images.Count == 0)
//            {
//                return NotFound("No images found for this patient.");
//            }

//            return Ok(images);
//        }

//        [HttpPost]
//        public async Task<ActionResult<Image>> UploadImage(int patientId, [FromForm] IFormFile file, [FromForm] string uploadedBy)
//        {
//            if (file == null || file.Length == 0)
//            {
//                return BadRequest("No file uploaded.");
//            }

//            var patient = await _context.Patients.FindAsync(patientId);
//            if (patient == null)
//            {
//                return NotFound("Patient not found.");
//            }

//            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
//            var filePath = Path.Combine(_storagePath, fileName);

//            using (var stream = new FileStream(filePath, FileMode.Create))
//            {
//                await file.CopyToAsync(stream);
//            }

//            var image = new Image
//            {
//                PatientId = patientId,
//                Url = $"/Uploads/Images/{fileName}",
//                Size = file.Length,
//                Type = Path.GetExtension(file.FileName).Replace(".", "").ToUpper(), // JPG, PNG, etc.
//                UploadedDate = DateTime.Now,
//                UploadedBy = uploadedBy ?? "Unknown"
//            };

//            _context.Images.Add(image);
//            await _context.SaveChangesAsync();

//            return CreatedAtAction(nameof(GetImages), new { patientId = patientId }, image);
//        }
//    }
//}