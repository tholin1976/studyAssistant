using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Threading.Tasks;
using ChartJSCore.Models;
using studyAssistant.Core.Domain;
using studyAssistant.Data;

namespace studyAssistant.Models
{
    public enum ChartType
    {
        [Display(Name = "Progresjon")]
        Progression,
        [Display(Name = "Arbeidsmengde")]
        Workload
    }

    public class ProgressionData
    {
        public List<double> dsRealProgression { get; }
        public string lblRealProgression { get; set; }
        public List<double> dsReferenceProgression { get; }
        public string lblReferenceProgression { get; set; }
        public List<string> labelsProgression { get; }
        


       public ProgressionData(List<double> RealProgression, string RealProgressionLabel, List<double> ReferenceProgression, string ReferenceProgressionLabel, List<string> ProgressionLabels)
        {
            dsRealProgression = RealProgression;
            lblRealProgression = RealProgressionLabel;
            dsReferenceProgression = ReferenceProgression;
            lblReferenceProgression = ReferenceProgressionLabel;
            labelsProgression = ProgressionLabels;
        }

        public ProgressionData(List<double> RealProgression, List<double> ReferenceProgression, List<string> ProgressionLabels)
        {
            dsRealProgression = RealProgression;
            dsReferenceProgression = ReferenceProgression;
            labelsProgression = ProgressionLabels;
        }
    }

    public class ChartMaker
    {
        private readonly ApplicationDbContext _context;

        public ChartMaker(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Chart> GenerateCourseChart(Course course, ChartType chartType, string title)
        {
            Chart chart = new Chart();

            switch (chartType)
            {
                case ChartType.Progression:
                    var progressionData = await GenerateProgressionData(course);
                    progressionData.lblRealProgression = "Reell progresjon";
                    progressionData.lblReferenceProgression = "Anbefalt progresjon";
                    chart = GenerateLineChart(progressionData);
                    break;
                case ChartType.Workload:
                    var studyHours = await _context.GetStudyTimeByCourse(course.Id);
                    chart = GeneratePieChart(title,
                        new List<string>() {"Gjennomførte arbeidstimer", "Gjenværende arbeidstimer"},
                        new List<double>() {studyHours, course.WorkLoad - studyHours});
                    break;
                default:
                    break;
        }
            return chart;
        }

        public Chart GeneratePieChart(string title, List<string> labels, List<double> inputData)
        {
            var chart = new Chart()
            {
                Type = "pie",
                Data =
                    new ChartJSCore.Models.Data
                    {
                        Labels = labels,
                        Datasets = new List<Dataset>
                        {
                            new PieDataset()
                            {
                                Label = title,
                                BackgroundColor = new List<string>() { "#85CE36", "#36A2EB", "#FFCE56" },
                                HoverBackgroundColor = new List<string>() { "#85CE36", "#36A2EB", "#FFCE56" },
                                Data = inputData
                            }
                        }
                    },
                Options = new Options()
                {
                    Responsive = true,
                    Title = new Title
                    {
                        Display = true,
                        Text = title,
                        FontSize = 16
                    },
                    MaintainAspectRatio = true
                }
            };
            
            return chart;
        }

        public Chart GenerateLineChart(ProgressionData progressionData)
        {
            var chart = new Chart
            {
                Type = "line",
                Data = new ChartJSCore.Models.Data
                {
                    Labels = progressionData.labelsProgression,
                    Datasets = new List<Dataset>()
                    {
                        new LineDataset()
                        {
                            Label = progressionData.lblReferenceProgression,
                            Data = progressionData.dsReferenceProgression,
                            Fill = "+1",
                            LineTension = 0.1,
                            BackgroundColor = "rgba(75, 192, 192, 0.4)",
                            BorderColor = "rgba(75,192,192,1)",
                            BorderCapStyle = "butt",
                            BorderDash = new List<int> { },
                            BorderDashOffset = 0.0,
                            BorderJoinStyle = "miter",
                            PointBorderColor = new List<string>() { "rgba(75,192,192,1)" },
                            PointBackgroundColor = new List<string>() { "#fff" },
                            PointBorderWidth = new List<int> { 1 },
                            PointHoverRadius = new List<int> { 5 },
                            PointHoverBackgroundColor = new List<string>() { "rgba(75,192,192,1)" },
                            PointHoverBorderColor = new List<string>() { "rgba(220,220,220,1)" },
                            PointHoverBorderWidth = new List<int> { 2 },
                            PointRadius = new List<int> { 1 },
                            PointHitRadius = new List<int> { 10 },
                            SpanGaps = false
                        },
                        new LineDataset()
                        {
                            Label = progressionData.lblRealProgression,
                            Data = progressionData.dsRealProgression,
                            Fill = "start",
                            LineTension = 0.1,
                            BackgroundColor = "rgba(133, 206, 54, 0.4)",
                            BorderColor = "rgba(133, 206, 54,1)",
                            BorderCapStyle = "butt",
                            BorderDash = new List<int> { },
                            BorderDashOffset = 0.0,
                            BorderJoinStyle = "miter",
                            PointBorderColor = new List<string>() {"rgba(133, 206, 54,1)"},
                            PointBackgroundColor = new List<string>() {"#fff"},
                            PointBorderWidth = new List<int> {1},
                            PointHoverRadius = new List<int> {5},
                            PointHoverBackgroundColor = new List<string>() {"rgba(200, 150, 150,1)"},
                            PointHoverBorderColor = new List<string>() {"rgba(220,220,220,1)"},
                            PointHoverBorderWidth = new List<int> {2},
                            PointRadius = new List<int> {1},
                            PointHitRadius = new List<int> {10},
                            SpanGaps = false
                        }
                    }

                }
            };

            return chart;
        }

        public int GetWeekNumber(DateTime date)
        {
            CultureInfo ciCurr = CultureInfo.CurrentCulture;
            int weekNum = ciCurr.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            return weekNum;
        }

        internal async Task<ProgressionData> GenerateProgressionData(Course course)
        {
            var studyTime = await _context.GetCompletedStudySessionDurationsByCourse(course.Id);

            double aggRealStudyTime = 0;
            double aggReferenceStudyTime = 0;

            int startWeek = GetWeekNumber(course.DateFrom);
            int endWeek = course.IsActive ? GetWeekNumber(DateTime.Now) : GetWeekNumber(course.DateTo);

            double referenceProgressPerWeek = course.WorkLoad / (GetWeekNumber(course.DateTo) - GetWeekNumber(course.DateFrom));

            var referenceProgression = new List<double>();
            var realProgression = new List<double>();
            var labels = new List<string>();

            referenceProgression.Add(0);
            realProgression.Add(0);
            labels.Add("");

            while (startWeek <= endWeek)
            {
                foreach (var v in studyTime)
                {
                    if (GetWeekNumber(v.StartDate) == startWeek)
                    {
                        aggRealStudyTime += v.Duration;
                    }
                }

                aggReferenceStudyTime += referenceProgressPerWeek;

                    referenceProgression.Add(aggReferenceStudyTime);
                    realProgression.Add(aggRealStudyTime);
                    labels.Add($"Uke {startWeek.ToString()}");

                startWeek++;
            }


            var progressionData = new ProgressionData(realProgression, referenceProgression, labels);
            
            return progressionData;
        }
    }
}
