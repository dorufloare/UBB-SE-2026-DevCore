using System;
using DevCoreHospital.Models;
using DevCoreHospital.Repositories;
using DevCoreHospital.Services;
using DevCoreHospital.Tests.Repositories;
using Xunit;

namespace DevCoreHospital.Tests.Integration
{
    // HangoutRepository.HasConflictsOnDate queries the real Appointments table.
    // Using InMemoryHangoutRepository with AddConflictForStaff would bypass that
    // SQL entirely. These tests use a real DB so the conflict detection is exercised
    // end-to-end, not just the service branching logic.
    public class HangoutFlowIntegrationTests : IClassFixture<SqlTestFixture>
    {
        private readonly SqlTestFixture db;
        private static readonly DateTime HangoutDate = DateTime.Today.AddDays(14);

        public HangoutFlowIntegrationTests(SqlTestFixture db) => this.db = db;

        // -------------------------------------------------------------------
        // Test 1: creator + joiner, no conflicts → both end up in participant list
        // -------------------------------------------------------------------
        [Fact]
        public void CreateThenJoin_WhenNoConflicts_StoresHangoutWithBothParticipants()
        {
            using var conn = db.OpenConnection();
            var creatorId = db.InsertStaff(conn, "Doctor", "HgCreate", "Creator",  "Cardiology");
            var joinerId  = db.InsertStaff(conn, "Doctor", "HgCreate", "Joiner",   "Cardiology");
            var hangoutId = 0;
            try
            {
                var repo    = new HangoutRepository(db.ConnectionString);
                var service = new HangoutService(repo);
                var creator = new Doctor { StaffID = creatorId };
                var joiner  = new Doctor { StaffID = joinerId };

                hangoutId = service.CreateHangout("Team hangout", "Monthly meetup", HangoutDate, 5, creator);
                service.JoinHangout(hangoutId, joiner);

                var hangout = repo.GetHangoutById(hangoutId);
                Assert.NotNull(hangout);
                Assert.Equal(2, hangout!.ParticipantList.Count);
                Assert.Contains(hangout.ParticipantList, p => p.StaffID == creatorId);
                Assert.Contains(hangout.ParticipantList, p => p.StaffID == joinerId);
            }
            finally
            {
                if (hangoutId > 0) db.DeleteHangoutParticipants(conn, hangoutId);
                if (hangoutId > 0) db.DeleteHangout(conn, hangoutId);
                db.DeleteStaff(conn, creatorId);
                db.DeleteStaff(conn, joinerId);
            }
        }

        // -------------------------------------------------------------------
        // Test 2: creator has a real appointment on hangout day → creation blocked
        // The conflict is detected via a real SQL query on the Appointments table.
        // -------------------------------------------------------------------
        [Fact]
        public void CreateHangout_WhenCreatorHasRealAppointmentOnHangoutDay_ThrowsAndPersistsNothing()
        {
            using var conn = db.OpenConnection();
            var creatorId = db.InsertStaff(conn, "Doctor", "HgConflict", "Creator", "Cardiology");
            // A "Scheduled" appointment on the same calendar day as the hangout
            var apptId = db.InsertAppointment(conn, 0, creatorId,
                HangoutDate.AddHours(9), HangoutDate.AddHours(10));
            try
            {
                var repo    = new HangoutRepository(db.ConnectionString);
                var service = new HangoutService(repo);
                var creator = new Doctor { StaffID = creatorId };

                Assert.Throws<InvalidOperationException>(
                    () => service.CreateHangout("Team hangout", "Desc", HangoutDate, 5, creator));

                // Nothing should have been written to Hangouts
                Assert.DoesNotContain(repo.GetAllHangouts(),
                    h => h.Date.Date == HangoutDate.Date);
            }
            finally
            {
                db.DeleteAppointment(conn, apptId);
                db.DeleteStaff(conn, creatorId);
            }
        }

        // -------------------------------------------------------------------
        // Test 3: joiner has a real appointment on hangout day → join blocked,
        //         creator remains the only participant
        // -------------------------------------------------------------------
        [Fact]
        public void JoinHangout_WhenJoinerHasRealAppointmentOnHangoutDay_ThrowsAndParticipantListUnchanged()
        {
            using var conn = db.OpenConnection();
            var creatorId = db.InsertStaff(conn, "Doctor", "HgJoinConf", "Creator", "Cardiology");
            var joinerId  = db.InsertStaff(conn, "Doctor", "HgJoinConf", "Joiner",  "Cardiology");
            var hangoutId = 0;
            var apptId    = 0;
            try
            {
                var repo    = new HangoutRepository(db.ConnectionString);
                var service = new HangoutService(repo);
                var creator = new Doctor { StaffID = creatorId };
                var joiner  = new Doctor { StaffID = joinerId };

                hangoutId = service.CreateHangout("Team hangout", "Desc", HangoutDate, 5, creator);

                // Give the joiner an appointment on hangout day after the hangout is created
                apptId = db.InsertAppointment(conn, 0, joinerId,
                    HangoutDate.AddHours(11), HangoutDate.AddHours(12));

                Assert.Throws<InvalidOperationException>(
                    () => service.JoinHangout(hangoutId, joiner));

                Assert.Single(repo.GetHangoutById(hangoutId)!.ParticipantList);
            }
            finally
            {
                if (apptId    > 0) db.DeleteAppointment(conn, apptId);
                if (hangoutId > 0) db.DeleteHangoutParticipants(conn, hangoutId);
                if (hangoutId > 0) db.DeleteHangout(conn, hangoutId);
                db.DeleteStaff(conn, creatorId);
                db.DeleteStaff(conn, joinerId);
            }
        }

        // -------------------------------------------------------------------
        // Test 4: hangout fills up → late-comer is rejected with the right message
        // -------------------------------------------------------------------
        [Fact]
        public void JoinHangout_WhenHangoutReachesCapacity_LaterJoinerIsRejected()
        {
            using var conn = db.OpenConnection();
            var creatorId = db.InsertStaff(conn, "Doctor", "HgFull", "Creator",  "Cardiology");
            var doctor2Id = db.InsertStaff(conn, "Doctor", "HgFull", "Doctor2",  "Cardiology");
            var doctor3Id = db.InsertStaff(conn, "Doctor", "HgFull", "Doctor3",  "Cardiology");
            var hangoutId = 0;
            try
            {
                var repo    = new HangoutRepository(db.ConnectionString);
                var service = new HangoutService(repo);

                hangoutId = service.CreateHangout("Team hangout", "Desc", HangoutDate, 2,
                    new Doctor { StaffID = creatorId });

                service.JoinHangout(hangoutId, new Doctor { StaffID = doctor2Id });

                var ex = Assert.Throws<InvalidOperationException>(
                    () => service.JoinHangout(hangoutId, new Doctor { StaffID = doctor3Id }));

                Assert.Equal("This hangout is already full.", ex.Message);
                Assert.Equal(2, repo.GetHangoutById(hangoutId)!.ParticipantList.Count);
            }
            finally
            {
                if (hangoutId > 0) db.DeleteHangoutParticipants(conn, hangoutId);
                if (hangoutId > 0) db.DeleteHangout(conn, hangoutId);
                db.DeleteStaff(conn, creatorId);
                db.DeleteStaff(conn, doctor2Id);
                db.DeleteStaff(conn, doctor3Id);
            }
        }
    }
}
