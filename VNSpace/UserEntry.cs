using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;

namespace VNSpace
{
    enum statusLabel
    {
        Playing,
        Finished,
        Stalled,
        Dropped,
        Planning
    }
    class UserEntry
    {
        private VisualNovel vn; //visual novel this personal entry applies to

        private float? rating; //user rating
        private string? notes; //user notes
        private float? playTime; //play time of vn
        private DateTime? startDate; //date starting the vn
        private DateTime? finishDate; //date finishing vn
        private string? category; //user assigned category
        private statusLabel? status; //tag for status of vn

        //constructor
        public UserEntry(VisualNovel vn, statusLabel status)
        {
            this.vn = vn;
            this.status = status;
        }

        //set methods
        public void SetVN(VisualNovel vn)
        {
            this.vn = vn;
        }

        public void SetRating(float rating)
        {
            this.rating = rating;
        }

        public void SetNotes(string notes)
        {
            this.notes = notes;
        }

        public void SetPlayTime(float playTime)
        {
            this.playTime = playTime; 
        }

        public void SetStartDate(DateTime start)
        {
            this.startDate = start; 
        }

        public void SetFinishDate(DateTime finish)
        {
            this.finishDate = finish;
        }

        public void SetCategory(string category)
        {
            this.category = category;
        }

        public void SetStatus(statusLabel label)
        {
            this.status = label;
        }

        //get methods
        public VisualNovel GetVN()
        {
            return this.vn;
        }

        public float? GetRating()
        {
            return this.rating;
        }

        public string? GetNotes()
        {
            return this.notes;
        }

        public float? GetPlayTime()
        {
            return this.playTime;
        }

        public DateTime? GetStartDate()
        {
            return this.startDate;
        }

        public DateTime? GetFinishDate()
        {
            return this.finishDate;
        }

        public string? GetCategory()
        {
            return this.category;
        }

        public statusLabel? GetStatus()
        {
            return this.status;
        }

        //function to see whether a user's vn entry is marked as finished or not
        public bool IsComplete()
        {
            if (this.status == statusLabel.Finished)
            {
                return true;
            }
            return false;
        }

        public string GetStatusLabel()
        {
            switch (status)
            {
                case statusLabel.Playing:
                    return "Currently Playing";
                case statusLabel.Finished:
                    return "Finished";
                case statusLabel.Stalled:
                    return "On Hold";
                case statusLabel.Dropped:
                    return "Dropped";
                case statusLabel.Planning:
                    return "Plan to Read";
                default:
                    return "Unknown";
            }
        }

        public override string ToString()
        {
            return $"{vn.GetName()} — {GetStatusLabel()}";
        }
    }
}
