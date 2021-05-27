using AppointmentScheduler.Models;
using AppointmentScheduler.Models.ViewModels;
using AppointmentScheduler.Utility;
using Microsoft.AspNetCore.Identity.UI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppointmentScheduler.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly ApplicationDbContext _db;
        private readonly IEmailSender _emailSender;

        public AppointmentService(ApplicationDbContext db, IEmailSender emailSender)
        {
            _db = db;
            _emailSender = emailSender;
        }

        public async Task<int> AddUpdate(AppointmentVM model)
        {
            var startDate = DateTime.Parse(model.StartDate);
            var endDate = DateTime.Parse(model.StartDate).AddMinutes(Convert.ToDouble(model.Duration));
            var patient = _db.Users.FirstOrDefault(u => u.Id == model.PatientId);
            var therapist = _db.Users.FirstOrDefault(u => u.Id == model.TherapistId);

            if (model != null && model.Id > 0)
            {
                //update
                var appointment = _db.Appointments.FirstOrDefault(x => x.Id == model.Id);
                appointment.Title = model.Title;
                appointment.Description = model.Description;
                appointment.StartDate = startDate;
                appointment.EndDate = endDate;
                appointment.Duration = model.Duration;
                appointment.TherapistId = model.TherapistId;
                appointment.PatientId = model.PatientId;
                appointment.IsTherapistApproved = false;
                appointment.AdminId = model.AdminId;

                await _emailSender.SendEmailAsync(therapist.Email, "Appointment Updated",
                    $"Your appointment with {patient.FirstName} has been updated");
                await _emailSender.SendEmailAsync(patient.Email, "Appointment Updated",
                    $"Your appointment with {therapist.FirstName} has been updated");

                await _db.SaveChangesAsync();
                return 1;
            }
            else
            {
                //create
                Appointment appointment = new Appointment()
                {
                    Title = model.Title,
                    Description = model.Description,
                    StartDate = startDate,
                    EndDate = endDate,
                    Duration = model.Duration,
                    TherapistId = model.TherapistId,
                    PatientId = model.PatientId,
                    IsTherapistApproved = false,
                    AdminId = model.AdminId
                };

                await _emailSender.SendEmailAsync(therapist.Email, "Appointment Created",
                    $"Your appointment with {patient.FirstName} is created and in pending status");
                await _emailSender.SendEmailAsync(patient.Email, "Appointment Created",
                    $"Your appointment with {therapist.FirstName} is created and in pending status");

                _db.Appointments.Add(appointment);
                await _db.SaveChangesAsync();
                return 2;
            }

        }

        public async Task<int> ConfirmEvent(int id)
        {
            var appointment = _db.Appointments.FirstOrDefault(x => x.Id == id);
            if(appointment != null)
            {
                appointment.IsTherapistApproved = true;
                return await _db.SaveChangesAsync();
            }
            return 0;
        }

        public async Task<int> Delete(int id)
        {
            var appointment = _db.Appointments.FirstOrDefault(x => x.Id == id);

            if (appointment != null)
            {
                _db.Appointments.Remove(appointment);
                return await _db.SaveChangesAsync();
            }
            return 0;
        }

        public AppointmentVM GetById(int id)
        {
            return _db.Appointments.Where(x => x.Id == id).ToList().Select(c => new AppointmentVM()
            {
                Id = c.Id,
                Description = c.Description,
                StartDate = c.StartDate.ToString("yyy-MM-dd HH:mm:ss"),
                EndDate = c.EndDate.ToString("yyy-MM-dd HH:mm:ss"),
                Title = c.Title,
                Duration = c.Duration,
                IsTherapistApproved = c.IsTherapistApproved,
                PatientId = c.PatientId,
                TherapistId = c.TherapistId,
                PatientName = _db.Users.Where(x => x.Id == c.PatientId).Select(x => x.FirstName).FirstOrDefault(),
                TherapistName = _db.Users.Where(x => x.Id == c.TherapistId).Select(x => x.FirstName).FirstOrDefault(),
            }).SingleOrDefault();
        }

        public List<PatientVM> GetPatientList()
        {
            var patients = (from user in _db.Users
                              join userRoles in _db.UserRoles on user.Id equals userRoles.UserId
                              join roles in _db.Roles.Where(x => x.Name == Helper.Patient) on userRoles.RoleId equals roles.Id
                              select new PatientVM
                              {
                                  Id = user.Id,
                                  FirstName = user.FirstName,
                                  LastName = user.LastName
                              }
                              ).ToList();

            return patients;
        }

        public List<TherapistVM> GetTherapistList()
        {
            var therapists = (from user in _db.Users
                              join userRoles in _db.UserRoles on user.Id equals userRoles.UserId
                              join roles in _db.Roles.Where(x=>x.Name==Helper.Therapist) on userRoles.RoleId equals roles.Id
                              select new TherapistVM
                              {
                                  Id = user.Id,
                                  FirstName = user.FirstName,
                                  LastName = user.LastName
                              }
                              ).ToList();

            return therapists;
        }

        public List<AppointmentVM> PatientsEventsById(string patientId)
        {
            return _db.Appointments.Where(x => x.PatientId == patientId).ToList().Select(c => new AppointmentVM()
            {
                Id = c.Id,
                Description = c.Description,
                StartDate = c.StartDate.ToString("yyy-MM-dd HH:mm:ss"),
                EndDate = c.EndDate.ToString("yyy-MM-dd HH:mm:ss"),
                Title = c.Title,
                Duration = c.Duration,
                IsTherapistApproved = c.IsTherapistApproved
            }).ToList();
        }

        public List<AppointmentVM> TherapistsEventsById(string therapistId)
        {
            return _db.Appointments.Where(x => x.TherapistId == therapistId).ToList().Select(c => new AppointmentVM()
            {
                Id = c.Id,
                Description = c.Description,
                StartDate = c.StartDate.ToString("yyy-MM-dd HH:mm:ss"),
                EndDate = c.EndDate.ToString("yyy-MM-dd HH:mm:ss"),
                Title = c.Title,
                Duration = c.Duration,
                IsTherapistApproved = c.IsTherapistApproved
            }).ToList();
        }
    }
}
