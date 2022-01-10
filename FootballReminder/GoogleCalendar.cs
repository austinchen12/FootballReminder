using System;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using FootballReminder.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using FootballReminder.Services;

namespace FootballReminder
{
    public class GoogleCalendar : ICalendarService
    {
        private readonly IConfiguration _config;
        private readonly CalendarService _calendarService;

        public GoogleCalendar(IConfiguration config)
        {
            _config = config;
            _calendarService = createCalendarService();
        }

        public IEnumerable<CalendarEvent> GetEvents()
        {
            return _calendarService.Events.List(_config.GetValue<string>("GoogleCalendarId")).Execute().Items
                .Where(e => e.Start.DateTime > DateTime.Today && e.Start.DateTime < DateTime.Today.AddMonths(3))
                .Select(e => new CalendarEvent()
                {
                    Summary = e.Summary,
                    Location = e.Location,
                    Description = e.Description,
                    Datetime = e.Start.DateTime.Value,
                    Duration = e.End.DateTime.Value.Subtract(e.Start.DateTime.Value),
                    ExtraData = new Dictionary<string, string>() {
                        { "EventId", e.Id },
                        { "MatchId", e.ExtendedProperties.Private__["MatchId"] }
                    }
                });
        }

        public void AddEvent(CalendarEvent calendarEvent)
        {
            var newEvent = new Event()
            {
                Summary = calendarEvent.Summary,
                Location = calendarEvent.Location,
                Description = calendarEvent.Description,
                Start = new EventDateTime()
                {
                    DateTime = calendarEvent.Datetime
                },
                End = new EventDateTime()
                {
                    DateTime = calendarEvent.Datetime.Add(calendarEvent.Duration)
                },
                Reminders = new Event.RemindersData()
                {
                    UseDefault = true
                },
                ExtendedProperties = new Event.ExtendedPropertiesData()
                {
                    Private__ = new Dictionary<string, string>() {
                        { "MatchId", calendarEvent.ExtraData["MatchId"] }
                    }
                }
            };

            _calendarService.Events.Insert(newEvent, _config.GetValue<string>("GoogleCalendarId")).Execute();
        }

        public void AddEvents(IEnumerable<CalendarEvent> calendarEvents)
        {
            foreach (CalendarEvent match in calendarEvents)
            {
                AddEvent(match);
            }
        }

        public void UpdateEvent(CalendarEvent calendarEvent)
        {
            var newEvent = new Event()
            {
                Summary = calendarEvent.Summary,
                Location = calendarEvent.Location,
                Description = calendarEvent.Description,
                Start = new EventDateTime()
                {
                    DateTime = calendarEvent.Datetime
                },
                End = new EventDateTime()
                {
                    DateTime = calendarEvent.Datetime.AddHours(2)
                },
                Reminders = new Event.RemindersData()
                {
                    UseDefault = true
                },
                ExtendedProperties = new Event.ExtendedPropertiesData()
                {
                    Private__ = new Dictionary<string, string>()
                    {
                        { "MatchId", calendarEvent.ExtraData["MatchId"] }
                    }
                }
            };

            _calendarService.Events.Update(newEvent, _config.GetValue<string>("GoogleCalendarId"), calendarEvent.ExtraData["EventId"]).Execute();
        }

        public void UpdateEvents(IEnumerable<CalendarEvent> calendarEvents)
        {
            foreach (CalendarEvent calendarEvent in calendarEvents)
            {
                UpdateEvent(calendarEvent);
            }
        }

        private ServiceAccountCredential createCredential()
        {
            ServiceAccountCredential credential;
            string[] Scopes = { CalendarService.Scope.Calendar };

            using (var stream =
                new FileStream($"{Directory.GetCurrentDirectory()}/{_config.GetValue<string>("GoogleCalendarKeyFile")}", FileMode.Open, FileAccess.Read))
            {
                var confg = Google.Apis.Json.NewtonsoftJsonSerializer.Instance.Deserialize<JsonCredentialParameters>(stream);
                credential = new ServiceAccountCredential(
                   new ServiceAccountCredential.Initializer(confg.ClientEmail)
                   {
                       Scopes = Scopes
                   }.FromPrivateKey(confg.PrivateKey));
            }

            return credential;
        }

        private CalendarService createCalendarService()
        {
            ServiceAccountCredential credential = createCredential();
            var service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "FootballReminder",
            });

            return service;
        }
    }
}
