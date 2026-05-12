using System;
using System.Collections.Generic;
using System.Text;

namespace VNSpace
{
    internal class VisualNovel
    {
        private string name; //primary title
        private string company; //company that produced/developed the work
        private string writer; //writer that wrote scenario

        //the following is largely only relevant for VNDB linked VNs
        private string? vndbid; //string for vndb ID variable
        private string? coverArtURL; //change to Cover coverArt
        private float? averageRating;
        private int? voteCount; //number of votes
        private string? altName; //alternate name

        //constructor with vndb info
        public VisualNovel(string name, string company, string writer, string? coverArt, string? id){
            this.name = name;
            this.company = company;
            this.writer = writer;
            this.coverArtURL = coverArt;
            this.vndbid = id;
        }

        //constructor without vndb info
        public VisualNovel(string name, string company, string writer)
        {
            this.name = name;
            this.company = company;
            this.writer = writer;
        }

        //get methods
        public string GetName()
        {
            return this.name;
        }
        public string GetCompany()
        {
            return this.company;
        }
        public string GetWriter()
        {
            return this.writer;
        }
        public string? GetVNDBID()
        {
            return this.vndbid;
        }
        public string? GetCoverArtURL()
        {
            return this.coverArtURL;
        }
        public float? GetAverageRating()
        {
            return this.averageRating;
        }
        public int? GetVoteCount()
        {
            return this.voteCount;
        }
        public string? GetAltName()
        {
            return this.altName;
        }
        //set methods
        public void SetCoverArtURL(string url)
        {
            this.coverArtURL = url;
        }
        public void SetAverageRating(float rating)
        {
            this.averageRating = rating;
        }
        public void SetVoteCount(int count)
        {
            this.voteCount = count;
        }
        public void SetAltName(string name)
        {
            this.altName = name;
        }
        public void LinkToVNDB(string id)
        {
            this.vndbid = id;
        }

        //other methods

        //function to get display title
        public string GetDisplayTitle()
        {
            return altName != null ? altName : name;
        }
        public override string ToString()
        {
            return this.name;
        }

        //function to check if visual novel is linked to VNDB
        public bool IsLinkedToVNDB()
        {
            return vndbid != null;
        }
    }
}
