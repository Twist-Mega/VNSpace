/*Michael Randazzo*/
/*Handles all communication with the VNDB Kana API*/

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace VNSpace
{
    class VNDBI_Api_Manager
    {
        /*base URL for all VNDB API calls*/
        private const string BASE_URL = "https://api.vndb.org/kana";

        /*shared HttpClient -- reusing one instance is best practice*/
        private static readonly HttpClient client = new HttpClient();

        /*fields to request from POST /vn -- covers all data we need*/
        private const string VN_FIELDS =
            "title, alttitle, image.thumbnail, image.sexual, image.violence, " +
            "average, rating, votecount, " +
            "developers.name, " +
            "staff.role, staff.name, " +
            "va.staff.name, va.character.name, " +
            "description, released, languages, platforms";

        /*fields to request for search results -- lighter than full fetch*/
        /*additional tags so flagged thumbnails can be filtered out*/
        private const string SEARCH_FIELDS =
            "title, alttitle, image.thumbnail, image.sexual, image.violence, developers.name, released";

        /*searches VNDB for VNs matching a title string -- returns list of results*/
        public async Task<List<VNDBResult>> SearchByTitle(string title)
        {
            List<VNDBResult> results = new List<VNDBResult>();

            try
            {
                /*build search query -- uses search filter with title string*/
                string query = JsonSerializer.Serialize(new
                {
                    filters = new object[] { "search", "=", title },
                    fields = SEARCH_FIELDS,
                    sort = "searchrank",
                    results = 10
                });

                HttpResponseMessage response = await PostQuery("/vn", query);
                if (!response.IsSuccessStatusCode) return results;

                string json = await response.Content.ReadAsStringAsync();
                using JsonDocument doc = JsonDocument.Parse(json);

                /*parse each result into a VNDBResult object*/
                foreach (JsonElement vn in doc.RootElement.GetProperty("results").EnumerateArray())
                {
                    VNDBResult result = ParseSearchResult(vn);
                    results.Add(result);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"VNDB SearchByTitle error: {ex.Message}");
            }

            return results;
        }

        /*fetches full metadata for a VN by its VNDB ID -- populates a VisualNovel object*/
        public async Task<bool> FetchById(string vndbId, VisualNovel vn)
        {
            try
            {
                /*build id query -- fetches all fields we care about*/
                string query = JsonSerializer.Serialize(new
                {
                    filters = new object[] { "id", "=", vndbId },
                    fields = VN_FIELDS,
                    results = 1
                });

                HttpResponseMessage response = await PostQuery("/vn", query);
                if (!response.IsSuccessStatusCode) return false;

                string json = await response.Content.ReadAsStringAsync();
                using JsonDocument doc = JsonDocument.Parse(json);

                JsonElement results = doc.RootElement.GetProperty("results");
                if (results.GetArrayLength() == 0) return false;

                /*populate VisualNovel with fetched data*/
                JsonElement entry = results[0];
                PopulateVisualNovel(entry, vn);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"VNDB FetchById error: {ex.Message}");
                return false;
            }
        }

        /*loads a cover art image from a URL or local file path and returns a BitmapImage for WPF display*/
        public async Task<BitmapImage?> LoadCoverArt(string path)
        {
            try
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();

                /*if path is a web URL, download bytes first*/
                if (path.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    byte[] imageBytes = await client.GetByteArrayAsync(path);
                    bitmap.StreamSource = new System.IO.MemoryStream(imageBytes);
                }
                else
                {
                    /*otherwise treat as local file path from file explorer*/
                    bitmap.UriSource = new Uri(path, UriKind.Absolute);
                }

                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"VNDB LoadCoverArt error: {ex.Message}");
                return null;
            }
        }

        /*checks if API is reachable -- uses lightweight stats endpoint*/
        public async Task<bool> IsApiReachable()
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync($"{BASE_URL}/stats");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        /*sends a POST request to the given VNDB endpoint with JSON body*/
        private async Task<HttpResponseMessage> PostQuery(string endpoint, string jsonBody)
        {
            StringContent content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            return await client.PostAsync($"{BASE_URL}{endpoint}", content);
        }

        /*parses a single VN search result JSON element into a VNDBResult*/
        private VNDBResult ParseSearchResult(JsonElement vn)
        {
            VNDBResult result = new VNDBResult();

            result.VndbId = vn.GetProperty("id").GetString() ?? "";
            result.Title = vn.GetProperty("title").GetString() ?? "";

            /*alttitle can be null*/
            if (vn.TryGetProperty("alttitle", out JsonElement alt) && alt.ValueKind != JsonValueKind.Null)
                result.AltTitle = alt.GetString();

            /*released can be null*/
            if (vn.TryGetProperty("released", out JsonElement rel) && rel.ValueKind != JsonValueKind.Null)
                result.Released = rel.GetString();

            /*get first developer name if present*/
            if (vn.TryGetProperty("developers", out JsonElement devs) && devs.GetArrayLength() > 0)
                result.Developer = devs[0].GetProperty("name").GetString();

            /*check flags before storing thumbnail -- same threshold as FetchById*/
            if (vn.TryGetProperty("image", out JsonElement img) && img.ValueKind != JsonValueKind.Null)
            {
                float sexual = img.TryGetProperty("sexual", out JsonElement s) ? s.GetSingle() : 0;
                float violence = img.TryGetProperty("violence", out JsonElement v) ? v.GetSingle() : 0;

                if (sexual < 1.0f && violence < 1.0f)
                {
                    if (img.TryGetProperty("thumbnail", out JsonElement thumb) && thumb.ValueKind != JsonValueKind.Null)
                        result.ThumbnailUrl = thumb.GetString();
                }
            }

            return result;
        }

        /*populates a VisualNovel object with data from a full VNDB entry JSON element*/
        private void PopulateVisualNovel(JsonElement entry, VisualNovel vn)
        {
            /*alt title*/
            if (entry.TryGetProperty("alttitle", out JsonElement alt) && alt.ValueKind != JsonValueKind.Null)
                vn.SetAltName(alt.GetString()!);

            /*community rating and vote count*/
            if (entry.TryGetProperty("rating", out JsonElement rating) && rating.ValueKind != JsonValueKind.Null)
                vn.SetAverageRating(rating.GetSingle());

            if (entry.TryGetProperty("votecount", out JsonElement votes) && votes.ValueKind != JsonValueKind.Null)
                vn.SetVoteCount(votes.GetInt32());

            /*cover art thumbnail URL -- only use if not flagged for sexual/violence content*/
            if (entry.TryGetProperty("image", out JsonElement img) && img.ValueKind != JsonValueKind.Null)
            {
                float sexual = img.TryGetProperty("sexual", out JsonElement s) ? s.GetSingle() : 0;
                float violence = img.TryGetProperty("violence", out JsonElement v) ? v.GetSingle() : 0;

                /*only use image if it is not flagged -- 1.0 is the threshold*/
                if (sexual < 1.0f && violence < 1.0f)
                {
                    if (img.TryGetProperty("thumbnail", out JsonElement thumb) && thumb.ValueKind != JsonValueKind.Null)
                        vn.SetCoverArtURL(thumb.GetString()!);
                }
            }

            /*voice actors -- collect unique names from va array*/
            if (entry.TryGetProperty("va", out JsonElement vaList))
            {
                List<string> voiceActors = new List<string>();
                foreach (JsonElement va in vaList.EnumerateArray())
                {
                    if (va.TryGetProperty("staff", out JsonElement staff))
                    {
                        string? name = staff.GetProperty("name").GetString();
                        if (name != null && !voiceActors.Contains(name))
                            voiceActors.Add(name);
                    }
                }
                vn.SetVoiceActors(string.Join(", ", voiceActors));
            }

            /*writer -- look for staff with scenario role*/
            if (entry.TryGetProperty("staff", out JsonElement staffList))
            {
                foreach (JsonElement staff in staffList.EnumerateArray())
                {
                    if (staff.TryGetProperty("role", out JsonElement role) &&
                        role.GetString() == "scenario")
                    {
                        vn.SetWriter(staff.GetProperty("name").GetString() ?? vn.GetWriter());
                        break;
                    }
                }
            }

            /*developer -- first in developers array*/
            if (entry.TryGetProperty("developers", out JsonElement devs) && devs.GetArrayLength() > 0)
                vn.SetCompany(devs[0].GetProperty("name").GetString() ?? vn.GetCompany());
        }
    }

    /*simple data class to hold search result display info before user picks one*/
    class VNDBResult
    {
        public string VndbId { get; set; } = "";
        public string Title { get; set; } = "";
        public string? AltTitle { get; set; }
        public string? Released { get; set; }
        public string? Developer { get; set; }
        public string? ThumbnailUrl { get; set; }

        /*display string for showing in a list*/
        public override string ToString()
        {
            string display = Title;
            if (!string.IsNullOrEmpty(Released))
                display += $" ({Released})";
            if (!string.IsNullOrEmpty(Developer))
                display += $" — {Developer}";
            return display;
        }
    }
}