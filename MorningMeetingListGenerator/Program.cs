using CommandLine;
using Microsoft.Extensions.Configuration;
using org.random.JSONRPC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MorningMeetingListGenerator
{
    public class Program
    {
        public class MeetingMember
        {
            public string Name { get; set; }
            public string AttendeeType { get; set; }
        }

        #region Private Variables
        private static IConfiguration Configuration;
        public static string APIKey;
        public static string URI;
        public static bool _WithBoss = false;                               // Include the boss in the randomized list when true
        public static bool _WithSometimes = false;                          // Include special guests in the randomized list when true
        public static string _ConfigFile = "appsettings.json";              // Name of the configuration file being used during this run
        public static List<string> _SpecialGuests = new List<string>();     // List of guests to the meeting
        public static List<string> _ExcludedMembers = new List<string>();   // List of regular members not attending this meeting
        #endregion

        #region Option handling using Command Line Parser Library from NuGet
        /// <summary>
        /// Special Options class used by the CommandLineParser NuGet package to 
        /// establish the known options, their parameter types, aliases, default values
        /// and help text
        /// </summary>
        public class Options
        {
            /// <summary>
            /// Include the boss
            /// </summary>
            [Option('b', "withboss", Required = false, Default = false, HelpText = "Include anyone listed as the boss attendee type in the list")]
            public bool withBoss {
                get { return _WithBoss; }
                set { _WithBoss = value; }
            }

            /// <summary>
            /// Set a different configuration file
            /// </summary>
            [Option('c', "configfile", Required = false, Default = "appsettings.json", HelpText = "Change the configuration file for this run from the default")]
            public string configFile {
                get { return _ConfigFile; }
                set { _ConfigFile = value; }
            }

            /// <summary>
            /// Allows the user to add guests to the meeting list
            /// </summary>
            [Option('g', "addguest", Required = false, HelpText = "Add guest(s) to the meeting", Separator = ',')]
            public IEnumerable<string> Guests
            {
                get { return _SpecialGuests; }
                set
                {
                    foreach (string guest in value)
                    {
                        _SpecialGuests.Add(guest);
                    }
                }
            }

            /// <summary>
            /// Allows the user to add guests to the meeting list
            /// </summary>
            [Option('r', "exclude", Required = false, HelpText = "Exclude regular member(s) from this meeting")]
            public IEnumerable<string> Exclude
            {
                get { return _ExcludedMembers; }
                set
                {
                    foreach(string excluded in value)
                    {
                        _ExcludedMembers.Add(excluded);
                    }
                }
            }
        }
        #endregion

        static void Main(string[] args)
        {
            Console.WriteLine("Morning Stand-Up Meeting Name Randomizer");
            Parser.Default.ParseArguments<Options>(args);
            bool again = true;

            GetAppSettings(_ConfigFile);
            GetAPIInfo();

            while (again)
            {
                // Retrieve the regular attendees of the meeting
                List<MeetingMember> knownAttendees = GetKnownMeetingMembers();

                // Generate the attendee list
                List<string> todaysRandomizedAttendees = GenerateTodaysAttendeeList(knownAttendees);

                int counter = 1;
                foreach (string attendee in todaysRandomizedAttendees)
                    Console.WriteLine($"{counter++}. {attendee}");

                Console.Write("Again? y/[N] >");
                string another = Console.ReadLine();

                if (another.ToLower() != "y")
                    again = false;
            }

            Environment.Exit(0);
        }

        /// <summary>
        /// Retrieves the API key and the random.org JSONRPC endpoint
        /// from the configuration file and places them in the global
        /// APIKey and URI variables.
        /// </summary>
        public static void GetAPIInfo()
        {
            // Retrieve the API key and URI
            APIKey = GetAPIKey();
            URI = GetURI();
        }

        /// <summary>
        /// Accepts a list of attendees, gets a randomized list of integers from
        /// random.org, and returns the attendee list in the randomized order
        /// </summary>
        /// <param name="attendees">List<string> of attendees</param>
        /// <returns></returns>
        public static List<string> RandomizeTodaysAttendeeList(List<string> attendees)
        {
            // Randomize meeting participant list
            var Randomizer = new RandomJSONRPC(APIKey, URI);
            var randomOrder = Randomizer.GenerateIntegers(attendees.Count, 0, attendees.Count - 1, false);

            List<string> Random = new List<string>();
            string[] arrAttend = attendees.ToArray();
            foreach (var index in randomOrder)
                Random.Add(arrAttend[index]);

            return Random;
        }

        /// <summary>
        /// Gets configuration items from the designated JSON-formatted configuration file
        /// </summary>
        /// <param name="fileName"></param>
        public static void GetAppSettings(string fileName)
        {
            // Get configurations from appsettings.json or a user-supplied JSON configuration file
            try
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile(fileName, optional: false, reloadOnChange: true);
                Configuration = builder.Build();
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine($"Error: Could not find settings file {fileName}");
                Environment.Exit(-1);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: caught exception {ex.Message}\r\nStack Trace:\r\n{ex.StackTrace}\r\naborting");
                Environment.Exit(-1);
            }
        }

        /// <summary>
        /// Returns the API key from the configuration file
        /// </summary>
        /// <returns>API key string</returns>
        public static string GetAPIKey()
        {
            return Configuration["APIKey"].ToString();
        }

        /// <summary>
        /// Returns the URI to random.org for the JSONRPC call
        /// </summary>
        /// <returns>URI string</returns>
        public static string GetURI()
        {
            return Configuration["APIURI"];
        }

        public static List<MeetingMember> GetKnownMeetingMembers()
        {
            // Retrieve the MeetingMembers section from the configuration file
            var participantSection = Configuration.GetSection("MeetingMembers"); // Top-level property in appsettings.json
            var attendees = participantSection.GetChildren();

            // Break out each known regular attendee from the config file into an instance of the MeetingMember class
            List<MeetingMember> knownAttendees = new List<MeetingMember>();
            foreach (var attendee in attendees)
            {
                var excluded = from x in _ExcludedMembers
                               where x == attendee.Value
                               select x;
                if (!excluded.Any())
                    knownAttendees.Add(new MeetingMember() { Name = attendee["Name"], AttendeeType = attendee["AttendeeType"] });
            }

            return knownAttendees;
        }

        public static List<MeetingMember> AddSpecialGuests(List<MeetingMember> meetingMembers)
        {
            if (_SpecialGuests.Count > 0)
            {
                foreach (var guest in _SpecialGuests)
                    meetingMembers.Add(new MeetingMember() { Name = guest, AttendeeType = "sometimes" });

                _WithSometimes = true;
            }

            return meetingMembers;
        }


        /// <summary>
        /// Retrieves a list of attendees from the configuration file
        /// and scrubs it with the command line configuration options 
        /// to return today's list of attendees.
        /// </summary>
        /// <returns>list of attendees</returns>
        public static List<string> GenerateTodaysAttendeeList(List<MeetingMember> knownAttendees)
        {

            // Add any special guests specified on the command line and automagically turn on
            // the _WithSometimes flag so they are included
            knownAttendees = AddSpecialGuests(knownAttendees);

            List<string> filteredAttendees = new List<string>();

            // Filter out the attendees by their AttendeeType
            // according to the way the command line options were set
            foreach (var possibleAttendee in knownAttendees)
            {
                bool test = (!(possibleAttendee.AttendeeType.ToLower() == "sometimes") && !_WithSometimes);
                if (possibleAttendee.AttendeeType.ToLower() == "boss" && !_WithBoss)
                {
                    continue;
                }
                else if ((possibleAttendee.AttendeeType.ToLower() == "sometimes") && !_WithSometimes)
                {
                    continue;
                }
                else
                {
                    filteredAttendees.Add(possibleAttendee.Name);
                }
            }

            return RandomizeTodaysAttendeeList(filteredAttendees);
        }
    }
}
