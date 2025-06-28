using DentalManagementAPI.Models;
using Microsoft.EntityFrameworkCore;
using DentalManagementAPI.Models.DTOs;

namespace DentalManagementAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<AdminEarnings> AdminEarnings { get; set; }
        public DbSet<DoctorsEarnings> DoctorsEarnings { get; set; }
        public DbSet<DoctorEarningDTO> DoctorEarningDTO { get; set; }

        public DbSet<PatientDto> Patients { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Drug> Drugs { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<DoctorSchedule> DoctorSchedules { get; set; }
        public DbSet<PerioMeasurement> PerioMeasurements { get; set; }
        public DbSet<PerioChart> PerioCharts { get; set; }
        public DbSet<PatientDocument> PatientDocuments { get; set; }
        public DbSet<PatientImage> PatientImages { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        // public DbSet<PrescriptionMedication> PrescriptionMedications { get; set; }
        public DbSet<Medication> Medications { get; set; }
        public DbSet<AttendanceRecord> AttendanceRecords { get; set; }
        
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Salary> Salaries { get; set; }

        public DbSet<Procedure> Procedures { get; set; }
        public DbSet<ToothProcedure> ToothProcedures { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Doctor>()
               .Property(d => d.Fee)
               .HasPrecision(18, 2);

            base.OnModelCreating(modelBuilder);
        }
    }
}