using System;
using Xunit;
using Xunit.Abstractions;
using MorningMeetingListGenerator;
using System.Collections.Generic;
using System.Diagnostics;

namespace MorningMeetingListGenerator_TESTS
{
    public class ConfigurationTests
    {
        private readonly ITestOutputHelper output;

        public ConfigurationTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void getAPIKey_DefaultKey_Match()
        {
            // Arrange
            Program.GetAppSettings("appsettings.json");

            // Act
            var APIKEY = Program.GetAPIKey();

            //Assert
            Assert.Equal("6067a572-038e-4e1d-9b94-7a178517055a", APIKEY);
        }

        [Fact]
        public void getURI_DefaultURI_Match()
        {
            // Arrange
            Program.GetAppSettings("appsettings.json");

            // Act
            string URI = Program.GetURI();

            //Assert
            Assert.Equal("https://api.random.org/json-rpc/2/invoke", URI);
        }

        [Fact]
        public void getKnownMeetingMembers_DefaultAttendees_MatchConfig()
        {
            // Arrange
            output.WriteLine("TestMemberTypeRetrieval: ");
            Program.GetAppSettings("appsettings.json");

            // Act
            List<Program.MeetingMember> members = Program.GetKnownMeetingMembers();

            //Assert
            foreach (var member in members)
            {
                Assert.NotNull(member);
                Assert.True(member.Name.Length > 0);
                Assert.True(member.AttendeeType.Length > 0);
                output.WriteLine($"\tRaw data: {member.Name} : {member.AttendeeType}");
                if (member.Name == "Lucas Carder")
                {
                    Assert.True(member.AttendeeType.ToLower() == "boss");
                    output.WriteLine("\tLucas Carder is the boss!");
                }
                else
                {
                    Assert.True(member.AttendeeType.ToLower() == "worker");
                    output.WriteLine($"\t{member.Name} is a worker!");
                }
            }
        }
    }
    public class FunctionTests
    {
        private readonly ITestOutputHelper output;

        public FunctionTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void randomizeTodaysAttendeeList_SampleAttendees_AreRandomized()
        {
            output.WriteLine($"Test randomizeTodaysAttendeeList_SampleAttendees_AreRandomized:");

            // Arrange
            Program.GetAppSettings("appsettings.json");
            Program.URI = Program.GetURI();
            Program.APIKey = Program.GetAPIKey();

            // Act
            List<string> attendees = new List<string>() { "A", "B", "C", "D", "E" };
            List<string> randomattendees = Program.RandomizeTodaysAttendeeList(attendees);

            // Assert
            Assert.NotEqual<List<string>>(attendees, randomattendees);
            output.WriteLine($"List of attendees:");
            foreach (string attend in attendees)
            {
                output.WriteLine(attend);
            }

            output.WriteLine($"List of randomized attendees:");
            foreach (string attend in randomattendees)
            {
                output.WriteLine(attend);
            }
        }

        [Fact]
        public void generateTodaysAttendeeList_BossAttendee_IsExcluded()
        {
            output.WriteLine($"Test randomizeTodaysAttendeeList_BossAttendee_IsExcluded:");

            // Arrange
            Program.GetAppSettings("appsettings.json");
            Program.URI = Program.GetURI();
            Program.APIKey = Program.GetAPIKey();
            Program._WithBoss = false;

            // Act
            List<Program.MeetingMember> members = Program.GetKnownMeetingMembers();
            List<string> todaysAttendees = Program.GenerateTodaysAttendeeList(members);
            bool bossIncluded = false;
            foreach (var attendee in todaysAttendees)
            {
                if (attendee == "Lucas Carder")
                {
                    bossIncluded = true;
                }
            }


            // Assert
            Assert.False(bossIncluded);
            output.WriteLine($"List of attendees:");
            foreach (string attend in todaysAttendees)
            {
                output.WriteLine(attend);
            }
        }

        [Fact]
        public void generateTodaysAttendeeList_BossAttendee_IsIncluded()
        {
            output.WriteLine($"Test randomizeTodaysAttendeeList_BossAttendee_IsIncluded:");

            // Arrange
            Program.GetAppSettings("appsettings.json");
            Program.URI = Program.GetURI();
            Program.APIKey = Program.GetAPIKey();
            Program._WithBoss = true;

            // Act
            List<Program.MeetingMember> members = Program.GetKnownMeetingMembers();
            List<string> todaysAttendees = Program.GenerateTodaysAttendeeList(members);
            bool bossIncluded = false;
            foreach (var attendee in todaysAttendees)
            {
                if (attendee == "Lucas Carder")
                {
                    bossIncluded = true;
                }
            }


            // Assert
            Assert.True(bossIncluded);
            output.WriteLine($"List of attendees:");
            foreach (string attend in todaysAttendees)
            {
                output.WriteLine(attend);
            }
        }

        [Fact]
        public void generateTodaysAttendeeList_GuestAttendee_IsIncluded()
        {
            output.WriteLine($"Test generateTodaysAttendeeList_GuestAttendee_IsIncluded:");

            // Arrange
            Program.GetAppSettings("appsettings.json");
            Program.URI = Program.GetURI();
            Program.APIKey = Program.GetAPIKey();
            Program._WithSometimes = true;
            Program._SpecialGuests.Add("Bob Newhart");

            // Act
            List<Program.MeetingMember> members = Program.GetKnownMeetingMembers();
            List<string> todaysAttendees = Program.GenerateTodaysAttendeeList(members);
            bool guestIncluded = false;
            foreach (var attendee in todaysAttendees)
            {
                if (attendee == "Bob Newhart")
                {
                    guestIncluded = true;
                }
            }


            // Assert
            Assert.True(guestIncluded);
            output.WriteLine($"List of attendees:");
            foreach (string attend in todaysAttendees)
            {
                output.WriteLine(attend);
            }
        }

        [Fact]
        public void generateTodaysAttendeeList_SometimesAttendee_IsExcluded()
        {
            output.WriteLine($"Test generateTodaysAttendeeList_SometimesAttendee_IsExcluded:");

            // Arrange
            Program.GetAppSettings("appsettings.json");
            Program.URI = Program.GetURI();
            Program.APIKey = Program.GetAPIKey();
            Program._WithSometimes = false;

            // Act
            List<Program.MeetingMember> members = Program.GetKnownMeetingMembers();
            members.Add(new Program.MeetingMember() { Name = "Zaphod Beeblebrox", AttendeeType = "sometimes" });           
            List<string> todaysAttendees = Program.GenerateTodaysAttendeeList(members);
            bool guestIncluded = false;
            foreach (var attendee in todaysAttendees)
            {
                if (attendee == "Zaphod Beeblebrox")
                {
                    guestIncluded = true;
                }
            }

            // Assert
            Assert.False(guestIncluded);
            output.WriteLine($"List of attendees:");
            foreach (string attend in todaysAttendees)
            {
                output.WriteLine(attend);
            }
        }

    }

}