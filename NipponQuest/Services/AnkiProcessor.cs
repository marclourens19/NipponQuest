using System.IO.Compression;
using Microsoft.Data.Sqlite;
using NipponQuest.Models;

namespace NipponQuest.Services
{
    public static class AnkiProcessor
    {
        public static List<Flashcard> GetCardsFromPackage(string packagePath, string workDir)
        {
            // CRITICAL: Initialize the SQLite provider manually to stop the crash
            // This ensures the native e_sqlite3 library is wired to the connection.
            SQLitePCL.Batteries_V2.Init();

            string dbFile = Path.Combine(workDir, "collection.anki2");
            var cards = new List<Flashcard>();

            try
            {
                // 1. EXTRACT
                using (ZipArchive archive = ZipFile.OpenRead(packagePath))
                {
                    var entry = archive.GetEntry("collection.anki2");
                    if (entry != null)
                    {
                        entry.ExtractToFile(dbFile, true);
                    }
                }

                // 2. READ
                // Using Pooling=False is vital to ensure the file handle is released immediately.
                string connStr = $"Data Source={dbFile};Mode=ReadOnly;Pooling=False;";

                using (var sqlite = new SqliteConnection(connStr))
                {
                    try
                    {
                        sqlite.Open();
                    }
                    catch (Exception ex)
                    {
                        // Wrap the error so the controller can log exactly why SQLite failed
                        throw new Exception($"SQLite open failed. Inner: {ex.GetType().Name}: {ex.Message}", ex);
                    }

                    using var cmd = sqlite.CreateCommand();
                    cmd.CommandText = "SELECT flds FROM notes";
                    using var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        var fields = reader.GetString(0).Split((char)0x1f);
                        if (fields.Length >= 2)
                        {
                            cards.Add(new Flashcard
                            {
                                FrontText = fields[0] ?? "",
                                BackText = fields[1] ?? ""
                            });
                        }
                    }
                    sqlite.Close();
                }
            }
            finally
            {
                // Ensure handles are released even if an error occurs
                SqliteConnection.ClearAllPools();
            }

            return cards;
        }
    }
}