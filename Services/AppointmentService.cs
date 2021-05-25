using AppointmentScheduler.Models;
using AppointmentScheduler.Models.ViewModels;
using AppointmentScheduler.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppointmentScheduler.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly ApplicationDbContext _db;

        public AppointmentService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<int> AddUpdate(AppointmentVM model)
        {
            var startDate = DateTime.Parse(model.StartDate);
            var endDate = DateTime.Parse(model.StartDate).AddMinutes(Convert.ToDouble(model.Duration));

            if (model != null && model.Id > 0)
            {
                //update
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

                _db.Appointments.Add(appointment);
                await _db.SaveChangesAsync();
                return 2;
            }

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
