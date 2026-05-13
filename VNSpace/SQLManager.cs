/*Michael Randazzo*/
/*Handles all database read/write operations*/

using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;

namespace VNSpace
{
    class SQLManager
    {
        /*path to local database file*/
        private string dbPath = "vnspace.db";

        /*constructor -- initializes database and creates tables if they do not exist*/
        public SQLManager()
        {
            InitializeDatabase();
        }

        /*creates the database schema if it does not already exist*/
        private void InitializeDatabase()
        {
            using (SqliteConnection connection = new SqliteConnection($"Data Source={dbPath}"))
            {
                connection.Open();

                /*create VISUAL_NOVELS table -- stores objective VN metadata*/
                string createVNTable = @"
                    CREATE TABLE IF NOT EXISTS VISUAL_NOVELS (
                        VN_Id       INTEGER     NOT NULL,
                        Name        TEXT        NOT NULL,
                        Company     TEXT,
                        Writer      TEXT,
                        CoverArtURL TEXT,
                        VNDB_Id     TEXT,
                        AltName     TEXT,
                        AvgRating   REAL,
                        VoteCount   INTEGER,
                        ExecPath    TEXT,

                        PRIMARY KEY(VN_Id)
                    )";

                /*create USER_ENTRIES table -- stores personal user data for each VN*/
                string createEntryTable = @"
                    CREATE TABLE IF NOT EXISTS USER_ENTRIES (
                        Entry_Id    INTEGER     NOT NULL,
                        VN_Id       INTEGER     NOT NULL,
                        Rating      REAL,
                        Notes       TEXT,
                        PlayTime    REAL,
                        StartDate   TEXT,
                        FinishDate  TEXT,
                        Category    TEXT,
                        Status      TEXT,

                        PRIMARY KEY(Entry_Id),
                        FOREIGN KEY(VN_Id) REFERENCES VISUAL_NOVELS(VN_Id)
                    )";

                new SqliteCommand(createVNTable, connection).ExecuteNonQuery();
                new SqliteCommand(createEntryTable, connection).ExecuteNonQuery();
            }
        }

        /*saves a new UserEntry to the database -- inserts VN first, then the user entry linked to it*/
        public long SaveEntry(UserEntry entry)
        {
            using (SqliteConnection connection = new SqliteConnection($"Data Source={dbPath}"))
            {
                connection.Open();

                /*insert VN data into VISUAL_NOVELS*/
                string insertVN = @"
                    INSERT INTO VISUAL_NOVELS (Name, Company, Writer, CoverArtURL, VNDB_Id, AltName, AvgRating, VoteCount, ExecPath)
                    VALUES (@name, @company, @writer, @coverArt, @vndbId, @altName, @avgRating, @voteCount, @execPath)";

                SqliteCommand vnCmd = new SqliteCommand(insertVN, connection);
                vnCmd.Parameters.AddWithValue("@name", entry.GetVN().GetName());
                vnCmd.Parameters.AddWithValue("@company", entry.GetVN().GetCompany());
                vnCmd.Parameters.AddWithValue("@writer", entry.GetVN().GetWriter());
                vnCmd.Parameters.AddWithValue("@coverArt", entry.GetVN().GetCoverArtURL() ?? "");
                vnCmd.Parameters.AddWithValue("@vndbId", entry.GetVN().GetVNDBID() ?? "");
                vnCmd.Parameters.AddWithValue("@altName", entry.GetVN().GetAltName() ?? "");
                vnCmd.Parameters.AddWithValue("@avgRating", entry.GetVN().GetAverageRating() ?? (object)DBNull.Value);
                vnCmd.Parameters.AddWithValue("@voteCount", entry.GetVN().GetVoteCount() ?? (object)DBNull.Value);
                vnCmd.Parameters.AddWithValue("@execPath", entry.GetVN().GetExecutablePath() ?? "");
                vnCmd.ExecuteNonQuery();

                /*get the ID of the VN just inserted*/
                long vnId = (long)new SqliteCommand("SELECT last_insert_rowid();", connection).ExecuteScalar()!;

                /*insert personal user data into USER_ENTRIES, linked to the VN above*/
                string insertEntry = @"
                    INSERT INTO USER_ENTRIES (VN_Id, Rating, Notes, PlayTime, StartDate, FinishDate, Category, Status)
                    VALUES (@vnId, @rating, @notes, @playTime, @startDate, @finishDate, @category, @status)";

                SqliteCommand entryCmd = new SqliteCommand(insertEntry, connection);
                entryCmd.Parameters.AddWithValue("@vnId", vnId);
                entryCmd.Parameters.AddWithValue("@rating", entry.GetRating() ?? (object)DBNull.Value);
                entryCmd.Parameters.AddWithValue("@notes", entry.GetNotes() ?? "");
                entryCmd.Parameters.AddWithValue("@playTime", entry.GetPlayTime() ?? (object)DBNull.Value);
                entryCmd.Parameters.AddWithValue("@startDate", entry.GetStartDate()?.ToString() ?? "");
                entryCmd.Parameters.AddWithValue("@finishDate", entry.GetFinishDate()?.ToString() ?? "");
                entryCmd.Parameters.AddWithValue("@category", entry.GetCategory() ?? "");
                entryCmd.Parameters.AddWithValue("@status", entry.GetStatus().ToString() ?? "");
                entryCmd.ExecuteNonQuery();

                long entryId = (long)new SqliteCommand("SELECT last_insert_rowid();", connection).ExecuteScalar()!;
                return entryId;
            }
        }

        /*updates an existing user entry in USER_ENTRIES -- used when user edits notes, rating, etc.*/
        public void UpdateEntry(UserEntry entry, long entryId)
        {
            using (SqliteConnection connection = new SqliteConnection($"Data Source={dbPath}"))
            {
                connection.Open();

                string updateEntry = @"
                    UPDATE USER_ENTRIES
                    SET Rating     = @rating,
                        Notes      = @notes,
                        PlayTime   = @playTime,
                        StartDate  = @startDate,
                        FinishDate = @finishDate,
                        Category   = @category,
                        Status     = @status
                    WHERE Entry_Id = @entryId";

                SqliteCommand cmd = new SqliteCommand(updateEntry, connection);
                cmd.Parameters.AddWithValue("@rating", entry.GetRating() ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@notes", entry.GetNotes() ?? "");
                cmd.Parameters.AddWithValue("@playTime", entry.GetPlayTime() ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@startDate", entry.GetStartDate()?.ToString() ?? "");
                cmd.Parameters.AddWithValue("@finishDate", entry.GetFinishDate()?.ToString() ?? "");
                cmd.Parameters.AddWithValue("@category", entry.GetCategory() ?? "");
                cmd.Parameters.AddWithValue("@status", entry.GetStatus().ToString() ?? "");
                cmd.Parameters.AddWithValue("@entryId", entryId);
                cmd.ExecuteNonQuery();
            }
        }

        /*updates the VISUAL_NOVELS row for an entry -- used after fetching VNDB data*/
        public void UpdateVN(UserEntry entry, long vnId)
        {
            using (SqliteConnection connection = new SqliteConnection($"Data Source={dbPath}"))
            {
                connection.Open();

                string updateVN = @"
                    UPDATE VISUAL_NOVELS
                    SET Company     = @company,
                        Writer      = @writer,
                        CoverArtURL = @coverArt,
                        VNDB_Id     = @vndbId,
                        AltName     = @altName,
                        AvgRating   = @avgRating,
                        VoteCount   = @voteCount,
                        ExecPath    = @execPath
                    WHERE VN_Id = @vnId";

                SqliteCommand cmd = new SqliteCommand(updateVN, connection);
                cmd.Parameters.AddWithValue("@company", entry.GetVN().GetCompany());
                cmd.Parameters.AddWithValue("@writer", entry.GetVN().GetWriter());
                cmd.Parameters.AddWithValue("@coverArt", entry.GetVN().GetCoverArtURL() ?? "");
                cmd.Parameters.AddWithValue("@vndbId", entry.GetVN().GetVNDBID() ?? "");
                cmd.Parameters.AddWithValue("@altName", entry.GetVN().GetAltName() ?? "");
                cmd.Parameters.AddWithValue("@avgRating", entry.GetVN().GetAverageRating() ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@voteCount", entry.GetVN().GetVoteCount() ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@execPath", entry.GetVN().GetExecutablePath() ?? "");
                cmd.Parameters.AddWithValue("@vnId", vnId);
                cmd.ExecuteNonQuery();
            }
        }

        /*removes a VN entry and its associated user entry from the database*/
        public void DeleteEntry(long vnId)
        {
            using (SqliteConnection connection = new SqliteConnection($"Data Source={dbPath}"))
            {
                connection.Open();

                /*delete user entry first due to foreign key constraint*/
                string deleteUserEntry = "DELETE FROM USER_ENTRIES WHERE VN_Id = @vnId";
                SqliteCommand deleteEntryCmd = new SqliteCommand(deleteUserEntry, connection);
                deleteEntryCmd.Parameters.AddWithValue("@vnId", vnId);
                deleteEntryCmd.ExecuteNonQuery();

                /*then delete the VN itself*/
                string deleteVN = "DELETE FROM VISUAL_NOVELS WHERE VN_Id = @vnId";
                SqliteCommand deleteVNCmd = new SqliteCommand(deleteVN, connection);
                deleteVNCmd.Parameters.AddWithValue("@vnId", vnId);
                deleteVNCmd.ExecuteNonQuery();
            }
        }

        /*loads all entries from database by joining VISUAL_NOVELS and USER_ENTRIES*/
        public List<UserEntry> LoadAllEntries()
        {
            List<UserEntry> entries = new List<UserEntry>();

            using (SqliteConnection connection = new SqliteConnection($"Data Source={dbPath}"))
            {
                connection.Open();

                /*join VISUAL_NOVELS and USER_ENTRIES to reconstruct full UserEntry objects*/
                string query = @"
                    SELECT V.VN_Id, V.Name, V.Company, V.Writer, V.CoverArtURL, V.VNDB_Id,
                           V.AltName, V.AvgRating, V.VoteCount, V.ExecPath,
                           E.Entry_Id, E.Rating, E.Notes, E.PlayTime, E.StartDate, E.FinishDate,
                           E.Category, E.Status
                    FROM   USER_ENTRIES  AS E
                    JOIN   VISUAL_NOVELS AS V ON E.VN_Id = V.VN_Id";

                SqliteDataReader reader = new SqliteCommand(query, connection).ExecuteReader();

                while (reader.Read())
                {
                    /*reconstruct VisualNovel object from database row*/
                    VisualNovel vn = new VisualNovel(
                        reader["Name"].ToString()!,
                        reader["Company"].ToString()!,
                        reader["Writer"].ToString()!,
                        reader["CoverArtURL"].ToString(),
                        reader["VNDB_Id"].ToString()
                    );

                    /*set optional VN fields if present*/
                    if (!string.IsNullOrEmpty(reader["AltName"].ToString()))
                        vn.SetAltName(reader["AltName"].ToString()!);

                    if (float.TryParse(reader["AvgRating"].ToString(), out float avgRating))
                        vn.SetAverageRating(avgRating);

                    if (int.TryParse(reader["VoteCount"].ToString(), out int voteCount))
                        vn.SetVoteCount(voteCount);

                    /*load executable path from database so it is available on startup*/
                    if (!string.IsNullOrEmpty(reader["ExecPath"].ToString()))
                        vn.SetExecutablePath(reader["ExecPath"].ToString()!);

                    /*reconstruct UserEntry object -- parse status enum from stored string*/
                    statusLabel status = Enum.Parse<statusLabel>(reader["Status"].ToString()!);
                    UserEntry entry = new UserEntry(vn, status);

                    /*assign Entry_Id so UpdateEntry and DeleteEntry work correctly after load*/
                    if (long.TryParse(reader["Entry_Id"].ToString(), out long entryId))
                        entry.SetEntryId(entryId);

                    /*store VN_Id on the entry so UpdateVN can target the correct row*/
                    if (long.TryParse(reader["VN_Id"].ToString(), out long vnId))
                        entry.SetVnId(vnId);

                    /*set optional user entry fields if present*/
                    if (float.TryParse(reader["Rating"].ToString(), out float rating))
                        entry.SetRating(rating);

                    if (float.TryParse(reader["PlayTime"].ToString(), out float playTime))
                        entry.SetPlayTime(playTime);

                    if (!string.IsNullOrEmpty(reader["Notes"].ToString()))
                        entry.SetNotes(reader["Notes"].ToString()!);

                    if (!string.IsNullOrEmpty(reader["Category"].ToString()))
                        entry.SetCategory(reader["Category"].ToString()!);

                    if (DateTime.TryParse(reader["StartDate"].ToString(), out DateTime startDate))
                        entry.SetStartDate(startDate);

                    if (DateTime.TryParse(reader["FinishDate"].ToString(), out DateTime finishDate))
                        entry.SetFinishDate(finishDate);

                    entries.Add(entry);
                }
            }

            return entries;
        }

        /*returns the VN_Id of the most recently inserted VISUAL_NOVELS row*/
        /*used by MainWindow after SaveEntry so the entry can store its VN_Id for UpdateVN*/
        public long GetLastVnId()
        {
            using (SqliteConnection connection = new SqliteConnection($"Data Source={dbPath}"))
            {
                connection.Open();
                return (long)new SqliteCommand("SELECT MAX(VN_Id) FROM VISUAL_NOVELS;", connection).ExecuteScalar()!;
            }
        }

        /*deletes all entries from both tables -- used to reset app to clean slate*/
        public void ResetAllData()
        {
            using (SqliteConnection connection = new SqliteConnection($"Data Source={dbPath}"))
            {
                connection.Open();

                /*delete user entries first due to foreign key constraint*/
                new SqliteCommand("DELETE FROM USER_ENTRIES;", connection).ExecuteNonQuery();

                /*then delete all visual novels*/
                new SqliteCommand("DELETE FROM VISUAL_NOVELS;", connection).ExecuteNonQuery();
            }
        }
    }
}