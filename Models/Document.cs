namespace DentalManagementAPI.Models
{
    public class Document
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public long Size { get; set; }
        public DateTime UploadedDate { get; set; }
        public string UploadedBy { get; set; }
        public string DocumentType { get; set; }
    }
}