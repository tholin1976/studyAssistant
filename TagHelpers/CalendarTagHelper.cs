using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;
using studyAssistant.Core.Domain;
using studyAssistant.Models;

namespace studyAssistant.TagHelpers
{

   public static class EnumExtensions
    {
        /// <summary>
        ///     A generic extension method that aids in reflecting 
        ///     and retrieving any attribute that is applied to an `Enum`.
        /// </summary>
        public static TAttribute GetAttribute<TAttribute>(this Enum enumValue) 
            where TAttribute : Attribute
        {
            return enumValue.GetType()
                .GetMember(enumValue.ToString())
                .First()
                .GetCustomAttribute<TAttribute>();
        }
    }
	/// <summary>
	/// Adds Norwegian display notation to the days of the week
	/// </summary>
    public enum DaysOfWeek
    {
        [Display(Name = "Mandag")]
        Monday, 
        [Display(Name = "Tirsdag")]
        Tuesday,
        [Display(Name = "Onsdag")]
        Wednesday,
        [Display(Name = "Torsdag")]
        Thursday,
        [Display(Name = "Fredag")]
        Friday,
        [Display(Name = "Lørdag")]
        Saturday,
        [Display(Name = "Søndag")]
        Sunday
   }

	/// <summary>
	/// Generates and displays the month and year specified by <see cref="Month"/> and <see cref="Year"/>.
	/// </summary>
    [HtmlTargetElement("calendar", TagStructure = TagStructure.NormalOrSelfClosing)]
	public class CalendarTagHelper : TagHelper
	{
		public int Month { get; set; }

		public int Year { get; set; }

		public List<CalendarEvent> Events { get; set; }

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			output.TagName = "section";
			output.Attributes.Add("class", "calendar");
			output.Content.SetHtmlContent(GetHtml());
			output.TagMode = TagMode.StartTagAndEndTag;
		}

		private string GetHtml()
		{
			var monthStart = new DateTime(Year, Month, 1);
			var events = Events?.OrderBy(e => e.Date).ToList();


			var html = new XDocument( 
				new XElement("div",
					new XAttribute("class", "container-fluid"),
				new XElement("header",
					new XElement("h4",
						new XAttribute("class", "display-4 mb-2 text-center"),
						monthStart.ToString("MMMM yyyy")),
						new XElement("div",
							new XAttribute("class", "row d-none d-lg-flex p-1 bg-dark text-white"),
                        Enum.GetValues(typeof(DayOfWeek)).Cast<DaysOfWeek>().Select(d =>
                            new XElement("h5", new XAttribute("class", "col-lg p-1 text-center"), d.GetAttribute<DisplayAttribute>().Name)))),
                    new XElement("div",
                        new XAttribute("class", "row border border-right-0 border-bottom-0"), GetDatesHtml())));
		    
			return html.ToString();

			IEnumerable<XElement> GetDatesHtml()
			{
			    var dayOfWeek = (int) monthStart.DayOfWeek;
				var startDate = monthStart.AddDays(-dayOfWeek+1);
				var dates = Enumerable.Range(0, 42).Select(i => startDate.AddDays(i));

				foreach (var d in dates)
				{
					if (d.DayOfWeek == DayOfWeek.Monday && d != startDate)
					{
						yield return new XElement("div",
							new XAttribute("class", "w-100"),
							String.Empty
						);
					}

					var mutedClasses = "d-none d-lg-inline-block bg-light text-muted";
					yield return new XElement("div",
						new XAttribute("class", $"day col-lg p-2 border border-left-0 border-top-0 text-truncate {(d.Month != monthStart.Month ? mutedClasses : null)}"),
						new XElement("h5",
							new XAttribute("class", "row align-items-center"),
							new XElement("span",
								new XAttribute("class", "date col-1"),
								d.Day
							),
							new XElement("small",
								new XAttribute("class", "col d-lg-none text-center text-muted"),
								d.DayOfWeek
							),
							new XElement("span",
								new XAttribute("class", "col-1"),
								String.Empty
							)
						),
						GetEventHtml(d)
					);
				}
			}

			IEnumerable<XElement> GetEventHtml(DateTime d)
			{
				var xElements = new List<XElement>();
                bool dayHasEvents = false;

				if (events != null)
				{
				    string eventClassName, eventInfoUrl = "";

					foreach (CalendarEvent calEvent in events.ToList())
					{
					    switch (calEvent.Type)
					    {
                            case "StudySession":
                                eventClassName = "StudySession";
                                eventInfoUrl = "/StudySession";
                                break;
                            case "Assignment":
                                eventClassName = "Assignment";
                                eventInfoUrl = "/Assignment";
                                break;
                            default:
                                eventClassName = "DefaultEvent";
                                break;
					    }
						if (calEvent.Date.Date == d.Date)
						{
                            dayHasEvents = true;
							xElements.Add(
								new XElement("a",
									new XAttribute("class", $"event d-block p-1 pl-2 pr-2 mb-1 rounded text-truncate small {eventClassName} text-white"),
									new XAttribute("title", calEvent.Title),
                                    new XAttribute("href", $"{eventInfoUrl}/Info/{calEvent.Id}"),
									$"Kl {calEvent.Date.ToShortTimeString()} : {calEvent.Title}"
								));
							events.Remove(calEvent);
						}
					}
				}
			    if (!dayHasEvents)
				{
					xElements.Add(new XElement("p", new XAttribute("class", "d-lg-none"), "Ingen aktiviteter"));
				}
				return xElements;
			}
		}
	}
}
