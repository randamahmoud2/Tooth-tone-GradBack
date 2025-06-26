using DentalManagementAPI.Data;
namespace DentalManagementAPI.Models
{
    public class Image
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string Url { get; set; }
        public long Size { get; set; }
        public string Type { get; set; }
        public DateTime UploadedDate { get; set; }
        public string UploadedBy { get; set; }
    }
}